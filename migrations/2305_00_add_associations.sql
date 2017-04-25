use Customers;

create table Associations (
	Id int(10) unsigned not null auto_increment,
	Name varchar(255) not null,
	Comments varchar(2000) null,
	primary key (Id)
)
collate='cp1251_general_ci'
engine=InnoDB;

alter table Promoters add column LastAssociationId int(10) unsigned null;

create table AssociationRegions (
	Id int(10) unsigned not null auto_increment,
	AssociationId int(10) unsigned not null,
	RegionId bigint(20) unsigned not null,
	primary key (Id),
	index FK_AssociationRegions_Associations (AssociationId),
	index FK_AssociationRegions_Regions (RegionId),
	constraint FK_AssociationRegions_Associations foreign key (AssociationId) references Associations (Id) on update cascade on delete cascade,
	constraint FK_AssociationRegions_Regions foreign key (RegionId) references Farm.Regions (RegionCode) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromoterAssociations (
	Id int(10) unsigned not null auto_increment,
	PromoterId int(10) unsigned not null,
	AssociationId int(10) unsigned not null,
	primary key (Id),
	index FK_PromoterAssociations_Associations (AssociationId),
	index FK_PromoterAssociations_Promoters (PromoterId),
	constraint FK_PromoterAssociations_Associations foreign key (AssociationId) references Associations (Id) on update cascade on delete restrict,
	constraint FK_PromoterAssociations_Promoters foreign key (PromoterId) references Promoters (Id) on update cascade on delete restrict
)
collate='cp1251_general_ci'
engine=InnoDB;

create table AssociationContacts (
	Id int(10) unsigned not null auto_increment,
	AssociationId int(10) unsigned not null,
	ContactType tinyint unsigned not null default 0,
	Fio varchar(100) null,
	Phone varchar(50) null,
	Email varchar(255) null,
	primary key (Id),
	index FK_AssociationContacts_Associations (AssociationId),
	constraint FK_AssociationContacts_Associations foreign key (AssociationId) references Associations (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

insert into Associations (Name)
	select Login
		from Promoters;
insert into PromoterAssociations (PromoterId, AssociationId)
	select p.Id, a.Id
		from Promoters p
			inner join Associations a on p.Login = a.Name;
update Associations a
	inner join PromoterAssociations pa on a.Id = pa.AssociationId
	inner join Promoters p on pa.PromoterId = p.Id
	set a.Name = p.Name;
update Promoters p
	inner join PromoterAssociations pa on p.Id = pa.PromoterId
	set p.LastAssociationId = pa.AssociationId;

alter table MarketingEvents add column AssociationId int(10) unsigned null after PromoterId ;
update MarketingEvents me
	inner join PromoterAssociations pa on me.PromoterId = pa.PromoterId
	set me.AssociationId = pa.AssociationId;
alter table MarketingEvents modify column AssociationId int(10) unsigned not null;
alter table MarketingEvents drop foreign key FK_MarketingEvents_Promoters;
drop index FK_MarketingEvents_Promoters on MarketingEvents;
alter table MarketingEvents drop column PromoterId;
create index FK_MarketingEvents_Associations on MarketingEvents (AssociationId);
alter table MarketingEvents add constraint FK_MarketingEvents_Associations foreign key (AssociationId) references Associations (Id) on update cascade on delete restrict;

alter table PromotionMembers add column AssociationId int(10) unsigned null after PromoterId;
update PromotionMembers pm
	inner join PromoterAssociations pa on pm.PromoterId = pa.PromoterId
	set pm.AssociationId = pa.AssociationId;
alter table PromotionMembers modify column AssociationId int(10) unsigned not null;
alter table PromotionMembers drop foreign key FK_PromotionMembers_Promoters;
drop index FK_PromotionMembers_Promoters on PromotionMembers;
alter table PromotionMembers drop column PromoterId;
create index FK_PromotionMembers_Associations on PromotionMembers (AssociationId);
alter table PromotionMembers add constraint FK_PromotionMembers_Associations foreign key (AssociationId) references Associations (Id) on update cascade on delete restrict;
