use customers;
ALTER TABLE `producerpromotions`
	ADD COLUMN `DateStarted` DATE NOT NULL AFTER `Name`,
	ADD COLUMN `DateFinished` DATE NOT NULL AFTER `DateStarted`;