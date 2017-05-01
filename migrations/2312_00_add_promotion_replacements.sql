use Customers;

create table PromotionReplacements (
	Id int(10) unsigned not null auto_increment,
	PromotionProductId int(10) unsigned not null,
	CatalogId int(10) unsigned not null,
	primary key (Id),
	index FK_PromotionReplacements_PromotionProducts (PromotionProductId),
	index FK_PromotionReplacements_Catalog (CatalogId),
	constraint FK_PromotionReplacements_PromotionProducts foreign key (PromotionProductId) references PromotionProducts (Id) on update cascade on delete cascade,
	constraint FK_PromotionReplacements_Catalog foreign key (CatalogId) references Catalogs.Catalog (Id)
)
collate='cp1251_general_ci'
engine=InnoDB;
