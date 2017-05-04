use Customers;

alter table ProducerPromotions add column FeeType tinyint not null default 0;
alter table ProducerPromotions add column CalculationUnit tinyint not null default 0;
alter table ProducerPromotions add column FeeBase tinyint not null default 0;
alter table ProducerPromotions add column MinLimit tinyint not null default 0;
alter table ProducerPromotions add column SameConditions tinyint(1) not null default 0;
alter table ProducerPromotions add column Accounting tinyint not null default 0;

alter table PromotionProducts modify column Price decimal(9, 2) null;
alter table PromotionProducts add column Quantity int(10) unsigned null;
alter table PromotionProducts add column DealerSum decimal(12, 2) null;
alter table PromotionProducts add column MemberSum decimal(12, 2) null;

create table PromotionPercents (
	Id int(10) unsigned not null auto_increment,
	PromotionId int(10) unsigned not null,
	Total decimal(12, 2) not null default 0,
	DealerPercent decimal(6, 2) null,
	MemberPercent decimal(6, 2) null,
	primary key (Id),
	index FK_PromotionPercents_ProducerPromotions (PromotionId),
	constraint FK_PromotionPercents_ProducerPromotions foreign key (PromotionId) references ProducerPromotions (Id) on update cascade on delete cascade,
	constraint UK_PromotionPercents unique key (PromotionId, Total)
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromotionSums (
	Id int(10) unsigned not null auto_increment,
	PromotionId int(10) unsigned not null,
	Quantity int(10) unsigned not null default 0,
	DealerSum decimal(12, 2) null,
	MemberSum decimal(12, 2) null,
	primary key (Id),
	index FK_PromotionSums_ProducerPromotions (PromotionId),
	constraint FK_PromotionSums_ProducerPromotions foreign key (PromotionId) references ProducerPromotions (Id) on update cascade on delete cascade,
	constraint UK_PromotionSums unique key (PromotionId, Quantity)
)
collate='cp1251_general_ci'
engine=InnoDB;

alter table PromotionMembers add column MinSum decimal(12, 2) null;

create table AddressLimits (
	Id int(10) unsigned not null auto_increment,
	MemberId int(10) unsigned not null,
	AddressId int(10) unsigned not null,
	MinSum decimal(12, 2) null,
	primary key (Id),
	index FK_AddressLimits_PromotionMembers (MemberId),
	index FK_AddressLimits_Addresses (AddressId),
	constraint FK_AddressLimits_PromotionMembers foreign key (MemberId) references PromotionMembers (Id) on update cascade on delete cascade,
	constraint FK_AddressLimits_Addresses foreign key (AddressId) references Addresses (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table LegalEntityLimits (
	Id int(10) unsigned not null auto_increment,
	MemberId int(10) unsigned not null,
	LegalEntityId int(10) unsigned not null,
	MinSum decimal(12, 2) null,
	primary key (Id),
	index FK_LegalEntityLimits_PromotionMembers (MemberId),
	index FK_LegalEntityLimits_LegalEntities (LegalEntityId),
	constraint FK_LegalEntityLimits_PromotionMembers foreign key (MemberId) references PromotionMembers (Id) on update cascade on delete cascade,
	constraint FK_LegalEntityLimits_LegalEntities foreign key (LegalEntityId) references Billing.LegalEntities (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;
