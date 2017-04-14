use customers;

alter table ProducerPromotions add column Enabled tinyint(1) not null default 1;

create table if not exists MarketingEvents (
	Id int(10) unsigned not null auto_increment,
	Name varchar(255) not null,
	PromoterId int(10) unsigned not null,
	primary key (Id),
	index FK_MarketingEvents_Promoters (PromoterId),
	constraint FK_MarketingEvents_Promoters foreign key (PromoterId) references Promoters (Id) on update cascade on delete cascade
)
collate='cp1251_general_ci'
engine=InnoDB;

insert into MarketingEvents (Name, PromoterId)
	select cp.Name, p.Id
		from Promoters p
			inner join PromoterProducers pp on p.Id = pp.PromoterId
			inner join Catalogs.Producers cp on pp.ProducerId = cp.Id;

alter table PromoterProducers add column MarketingEventId int(10) unsigned null after Id;
update PromoterProducers pr
	inner join MarketingEvents me on me.PromoterId = pr.PromoterId
	set pr.MarketingEventId = me.Id;
alter table PromoterProducers modify column MarketingEventId int(10) unsigned not null;

alter table ProducerPromotions add column MarketingEventId int(10) unsigned null after Id;
update ProducerPromotions pp
	inner join PromoterProducers pr on pp.PromoterProducerId = pr.Id
	set pp.MarketingEventId = pr.MarketingEventId;
alter table ProducerPromotions modify column MarketingEventId int(10) unsigned not null;

alter table ProducerPromotions drop foreign key FK_ProducerPromotions_PromoterProducers;
drop index FK_ProducerPromotions_PromoterProducers on ProducerPromotions;
alter table ProducerPromotions drop column PromoterProducerId;
create index FK_ProducerPromotions_MarketingEvents on ProducerPromotions (MarketingEventId);
alter table ProducerPromotions add constraint FK_ProducerPromotions_MarketingEvents
	foreign key (MarketingEventId) references MarketingEvents (Id);

alter table PromoterProducers drop foreign key FK_PromoterProducers_Promoters;
drop index FK_PromoterProducers_Promoters on PromoterProducers;
alter table PromoterProducers drop column PromoterId;
create index FK_PromoterProducers_MarketingEvents on PromoterProducers (MarketingEventId);
alter table PromoterProducers add constraint FK_PromoterProducers_MarketingEvents
	foreign key (MarketingEventId) references MarketingEvents (Id);
