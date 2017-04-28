use Customers;
alter table ProducerPromotions add column SuppliersType tinyint not null default 0;
update ProducerPromotions pp
	set SuppliersType = 1
	where pp.Id in (select PromotionId from PromotionSuppliers);
