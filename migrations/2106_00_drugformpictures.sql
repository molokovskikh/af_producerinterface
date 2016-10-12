use producerinterface;
CREATE TABLE `drugformpictures` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`ProducerId` INT(11) UNSIGNED NOT NULL,
	`CatalogId` INT(11) UNSIGNED NOT NULL,
	`PictureKey` INT(11) NULL DEFAULT NULL,
	PRIMARY KEY (`Id`),
	INDEX `FK_drugformpictures_catalogs.Producers` (`ProducerId`),
	INDEX `FK_drugformpictures_catalogs.Catalog` (`CatalogId`),
	INDEX `FK_drugformpictures_producerinterface.mediafiles` (`PictureKey`),
	CONSTRAINT `FK_drugformpictures_catalogs.Catalog` FOREIGN KEY (`CatalogId`) REFERENCES `catalogs`.`catalog` (`Id`) ON UPDATE NO ACTION ON DELETE CASCADE,
	CONSTRAINT `FK_drugformpictures_catalogs.Producers` FOREIGN KEY (`ProducerId`) REFERENCES `catalogs`.`producers` (`Id`) ON UPDATE NO ACTION ON DELETE CASCADE,
	CONSTRAINT `FK_drugformpictures_producerinterface.mediafiles` FOREIGN KEY (`PictureKey`) REFERENCES `mediafiles` (`Id`) ON UPDATE NO ACTION ON DELETE CASCADE
);


ALTER TABLE `cataloglog`
	ADD COLUMN `ProducerId` INT UNSIGNED NULL DEFAULT NULL AFTER `DateEdit`;