use ProducerInterface;
CREATE TABLE `promotions` (
	`Id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`UpdateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	`Enabled` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
	`AdminId` INT(10) UNSIGNED NULL DEFAULT NULL,
	`ProducerId` INT(10) UNSIGNED NOT NULL,
	`ProducerUserId` INT(10) UNSIGNED NOT NULL,
	`Annotation` MEDIUMTEXT NOT NULL,
	`PromoFile` VARCHAR(255) NULL DEFAULT NULL,
	`AgencyDisabled` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
	`Name` VARCHAR(255) NOT NULL COMMENT 'Наименование акции',
	`RegionMask` BIGINT(20) UNSIGNED NOT NULL COMMENT 'Регионы работы акции',
	`Begin` DATETIME NULL DEFAULT NULL COMMENT 'Дата начала акции',
	`End` DATETIME NULL DEFAULT NULL COMMENT 'Дата окончания акции',
	`Status` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
	PRIMARY KEY (`Id`),
	INDEX `AdminId` (`AdminId`),
	INDEX `FK_promotions_produceruser` (`ProducerUserId`),
	INDEX `promotions_ProducerId` (`ProducerId`),
	CONSTRAINT `FK_promotions_produceruser` FOREIGN KEY (`ProducerUserId`) REFERENCES `produceruser` (`Id`),
	CONSTRAINT `FK_supplierpromotions_accessright.regionaladmins` FOREIGN KEY (`AdminId`) REFERENCES `accessright`.`regionaladmins` (`RowID`) ON DELETE CASCADE,
	CONSTRAINT `promotions_ProducerId` FOREIGN KEY (`ProducerId`) REFERENCES `catalogs`.`producers` (`Id`) ON DELETE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPACT
AUTO_INCREMENT=3
;


 
CREATE TABLE `promotionToDrugs` (
	`DrugId` INT(10) UNSIGNED NOT NULL,
	`PromotionId` INT(10) UNSIGNED NOT NULL,
	INDEX `PromotionId` (`PromotionId`),
	INDEX `DrugId` (`DrugId`),
	CONSTRAINT `FK_promotioncatalogs_supplierpromotions` FOREIGN KEY (`PromotionId`) REFERENCES `promotions` (`Id`),
	CONSTRAINT `promotioncatalogs_DrugId` FOREIGN KEY (`DrugId`) REFERENCES `catalogs`.`catalog` (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPACT
;
