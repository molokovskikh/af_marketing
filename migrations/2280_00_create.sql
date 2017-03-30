use customers;

create table Promoters (
	Id int(10) unsigned not null auto_increment,
	Name varchar(255) not null,
	Login varchar(100) not null,
	primary key (Id),
	index IX_Promoters_Login (Login)
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromotionMembers (
	Id int(10) unsigned not null auto_increment,
	PromoterId int(10) unsigned not null,
	ClientId int(10) unsigned not null,
	primary key (Id),
	index FK_PromotionMembers_Promoters (PromoterId),
	index FK_PromotionMembers_Clients (ClientId),
	constraint FK_PromotionMembers_Promoters foreign key (PromoterId) references Promoters (Id) on update cascade on delete cascade,
	constraint FK_PromotionMembers_Clients foreign key (ClientId) references Clients (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromoterProducers (
	Id int(10) unsigned not null auto_increment,
	PromoterId int(10) unsigned not null,
	ProducerId int(10) unsigned not null,
	Contacts varchar(512) null,
	primary key (Id),
	index FK_PromoterProducers_Promoters (PromoterId),
	index FK_PromoterProducers_Producers (ProducerId),
	constraint FK_PromoterProducers_Promoters foreign key (PromoterId) references Promoters (Id) on update cascade on delete cascade,
	constraint FK_PromoterProducers_Producers foreign key (ProducerId) references catalogs.producers (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table ProducerPromotions (
	Id int(10) unsigned not null auto_increment,
	PromoterProducerId int(10) unsigned not null,
	Name varchar(255) not null,
	primary key (Id),
	index FK_ProducerPromotions_PromoterProducers (PromoterProducerId),
	constraint FK_ProducerPromotions_PromoterProducers foreign key (PromoterProducerId) references PromoterProducers (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromotionSubscribes (
	Id int(10) unsigned not null auto_increment,
	MemberId int(10) unsigned not null,
	PromotionId int(10) unsigned not null,
	primary key (Id),
	index FK_PromotionSubscribers_PromotionMembers (MemberId),
	index FK_PromotionSubscribers_ProducerPromotions (PromotionId),
	constraint FK_PromotionSubscribers_PromotionMembers foreign key (MemberId) references PromotionMembers (Id) on update cascade on delete cascade,
	constraint FK_PromotionSubscribers_ProducerPromotions foreign key (PromotionId) references ProducerPromotions (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromotionProducts (
	Id int(10) unsigned not null auto_increment,
	PromotionId int(10) unsigned not null,
	ProductId int(10) unsigned not null,
	Price decimal(9, 2) not null,
	DealerPercent decimal(6, 2) null,
	MemberPercent decimal(6, 2) null,
	primary key (Id),
	index FK_PromotionProducts_Promotions (PromotionId),
	index FK_PromotionProducts_Products (ProductId),
	constraint FK_PromotionProducts_Promotions foreign key (PromotionId) references ProducerPromotions (Id) on update cascade on delete cascade,
	constraint FK_PromotionProducts_Products foreign key (ProductId) references catalogs.products (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

create table PromotionSuppliers (
	Id int(10) unsigned not null auto_increment,
	PromotionId int(10) unsigned not null,
	SupplierId int(10) unsigned not null,
	primary key (Id),
	index FK_PromotionSuppliers_Promotions (PromotionId),
	index FK_PromotionSuppliers_Suppliers (SupplierId),
	constraint FK_PromotionSuppliers_Promotions foreign key (PromotionId) references ProducerPromotions (Id) on update cascade on delete cascade,
	constraint FK_PromotionSuppliers_Suppliers foreign key (SupplierId) references suppliers (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;
