use Customers;
alter table ProducerPromotions add column Description varchar(150) null;
alter table ProducerPromotions add column PromoRequirements varchar(2000) null;
alter table ProducerPromotions add column FeeInformation varchar(150) null;
