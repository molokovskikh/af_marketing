use customers;
CREATE TABLE `promotion_producersproducts` (
	`ProductId` INT(10) UNSIGNED NOT NULL,
	`ProducerId` INT(10) UNSIGNED NOT NULL,
	INDEX `ProductId` (`ProductId`),
	INDEX `ProducerId` (`ProducerId`)
);
