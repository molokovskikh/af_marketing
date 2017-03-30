using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Humanizer;
using Marketing.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Util;
using Environment = NHibernate.Cfg.Environment;

namespace Marketing
{
	public class NHibernate
	{
		private Assembly MappingAssembly;
		private ConventionModelMapper Mapper;
		private global::NHibernate.Cfg.Configuration Configuration;
		public ISessionFactory Factory;

		public void Init()
		{
			MappingAssembly = GetType().Assembly;

			Mapper = new ConventionModelMapper();
			Configuration = new global::NHibernate.Cfg.Configuration();

			Mapper.IsPersistentId((m, declared) => declared || m.Name == "Id");
			Mapper.BeforeMapBag += (inspector, member, customizer)
				=> customizer.Key(k => k.Column(member.GetContainerEntity(inspector).Name + "Id"));
			Mapper.BeforeMapManyToOne += (inspector, member, customizer)
				=> customizer.Column(member.LocalMember.Name + "Id");
			Mapper.AfterMapProperty += (inspector, member, customizer) => {
				var propertyInfo = ((PropertyInfo)member.LocalMember);
				var propertyType = propertyInfo.PropertyType;

				if (typeof(ValueType).IsAssignableFrom(propertyType) && !propertyType.IsNullable()) {
					customizer.Column(c => c.Default(GetDefaultValue(propertyInfo)));
					customizer.NotNullable(true);
				}
			};

			var simpleModelInspector = (SimpleModelInspector)Mapper.ModelInspector;
			simpleModelInspector.IsRootEntity((type, declared) => {
				var modelInspector = (IModelInspector)simpleModelInspector;
				return declared || type.IsClass
					//если наследуемся от класса который не маплен то это простое наследование
					&& (typeof(object) == type.BaseType || !modelInspector.IsEntity(type.BaseType))
					&& modelInspector.IsEntity(type);
			});
			var baseInspector = (IModelInspector)new SimpleModelInspector();
			Mapper.IsEntity((type, declared) => {
				return declared
					|| baseInspector.IsEntity(type)
					|| Configuration.ClassMappings.Any(m => m.MappedClass == type);
			});

			Configuration.AddProperties(new Dictionary<string, string> {
				{Environment.Dialect, "NHibernate.Dialect.MySQL5Dialect"},
				{Environment.ConnectionDriver, "NHibernate.Driver.MySqlDataDriver"},
				{Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider"},
				{Environment.ConnectionStringName, "db"},
				{Environment.Hbm2ddlKeyWords, "none"},
				//раскомментировать если нужно отладить запросы хибера
				//{Environment.ShowSql, "true"},
				//{Environment.FormatSql, "true"},
				{Environment.Isolation, "ReadCommitted"},
			});

			Mapper.Class<Promoter>(m => {
				m.Bag(o => o.Producers, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
				m.Bag(o => o.Members, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
			});
			Mapper.Class<PromoterProducer>(m => {
				m.Bag(o => o.Promotions, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
			});
			Mapper.Class<PromotionMember>(m => {
				m.Bag(o => o.Subscribes, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
			});
			Mapper.Class<ProducerPromotion>(m => {
				m.Bag(o => o.Products, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
				m.Bag(o => o.Suppliers, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
				m.Bag(o => o.Subscribes, c => {
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
					c.Inverse(true);
				});
			});

			var types = MappingAssembly.GetTypes().Where(t =>
				!Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute), false)
				&& Mapper.ModelInspector.IsRootEntity(t));
			var mapping = Mapper.CompileMappingFor(types.Distinct());

			var @class = Generators.Native.Class;
			foreach (var rootClass in mapping.RootClasses.Where(c => c.Id != null)) {
				if (rootClass.Id.generator == null) {
					rootClass.Id.generator = new HbmGenerator {
						@class = @class
					};
				}
			}
			Configuration.SetNamingStrategy(new PluralizeNamingStrategy());
			Configuration.AddDeserializedMapping(mapping, MappingAssembly.GetName().Name);
			Factory = Configuration.BuildSessionFactory();
		}

		private static object GetDefaultValue(PropertyInfo propertyInfo)
		{
			var instance = Activator.CreateInstance(propertyInfo.ReflectedType);
			var defaultValue = propertyInfo.GetValue(instance, null);
			if (defaultValue is bool)
				return Convert.ToInt32(defaultValue);
			if (defaultValue is Enum)
				return Convert.ToInt32(defaultValue);
			if (defaultValue is DateTime)
				return "'" + ((DateTime)defaultValue).ToString("yyyy-MM-dd") + "'";
			if (defaultValue is int || defaultValue is uint
				|| defaultValue is long || defaultValue is ulong
				|| defaultValue is float || defaultValue is double
				|| defaultValue is decimal)
				return ((IFormattable)defaultValue).ToString(null, CultureInfo.InvariantCulture);
			throw new Exception(
				$"Не знаю как получить значение по умолчанию для типа {propertyInfo.PropertyType} свойство {propertyInfo}");
		}

	public class PluralizeNamingStrategy : INamingStrategy
	{
		public string ClassToTableName(string className)
		{
			var name = DefaultNamingStrategy.Instance.ClassToTableName(className);
			return name.Pluralize();
		}

		public string PropertyToColumnName(string propertyName)
		{
			return DefaultNamingStrategy.Instance.PropertyToColumnName(propertyName);
		}

		public string TableName(string tableName)
		{
			return DefaultNamingStrategy.Instance.TableName(tableName);
		}

		public string ColumnName(string columnName)
		{
			return DefaultNamingStrategy.Instance.ColumnName(columnName);
		}

		public string PropertyToTableName(string className, string propertyName)
		{
			return DefaultNamingStrategy.Instance.PropertyToTableName(className, propertyName);
		}

		public string LogicalColumnName(string columnName, string propertyName)
		{
			return DefaultNamingStrategy.Instance.LogicalColumnName(columnName, propertyName);
		}
	}

	}
}