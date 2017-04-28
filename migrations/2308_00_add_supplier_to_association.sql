use Customers;

alter table Associations add column SupplierId int(10) unsigned null after Name;
create index FK_Associations_Suppliers on Associations (SupplierId);
alter table Associations add constraint FK_Associations_Suppliers foreign key (SupplierId) references Suppliers (Id) on update cascade on delete set null;
