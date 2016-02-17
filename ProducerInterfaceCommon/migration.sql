use producerinterface;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view suppliernames as
select s.Id as SupplierId, concat(s.Name, ' - ', rg.Region) as SupplierName, s.HomeRegion
from Customers.suppliers s
left join farm.regions rg on rg.RegionCode = s.HomeRegion
where s.Name not like(CONVERT('%ассортимент%' USING CP1251)) 
and s.Disabled = 0;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view supplierregions as
select pd.FirmCode as SupplierId, pr.RegionCode
from usersettings.PricesData pd
left outer join usersettings.pricesregionaldata pr on pr.PriceCode = pd.PriceCode
where pd.Enabled = 1 and pd.IsLocal = 0;

DROP TABLE `NewsChange`;
 
CREATE TABLE `NewsChange` (
	`IdAccount` INT(11) UNSIGNED NOT NULL,
	`IdNews` INT(10) UNSIGNED NOT NULL,
	`TypeCnhange` TINYINT(4) UNSIGNED NOT NULL,
	`DateChange` DATETIME NOT NULL,
	`NewsNewTema` VARCHAR(150) NOT NULL,
	`NewsOldTema` VARCHAR(150) NOT NULL,
	`NewsOldDescription` TEXT NOT NULL,
	`NewsNewDescription` TEXT NOT NULL,
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	PRIMARY KEY (`Id`),
	INDEX `FK2_NewsChangeToNotification` (`IdNews`),
	INDEX `IdAccount_IdNews_TypeCnhange_DateChange` (`IdAccount`, `IdNews`),
	INDEX `Id` (`Id`),
	CONSTRAINT `FK1_NewsChangeToAccount` FOREIGN KEY (`IdAccount`) REFERENCES `Account` (`Id`),
	CONSTRAINT `FK2_NewsChangeToNotification` FOREIGN KEY (`IdNews`) REFERENCES `NotificationToProducers` (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

ALTER TABLE `NewsChange` DROP FOREIGN KEY `FK2_NewsChangeToNotification`;

ALTER TABLE `NewsChange` ADD CONSTRAINT `FK2_NewsChangeToNotification` 
FOREIGN KEY (`IdNews`) REFERENCES `NotificationToProducers` (`Id`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `NotificationToProducers` ADD COLUMN `Enabled` TINYINT(1) NOT NULL DEFAULT '1' AFTER `DatePublication`;

ALTER TABLE `NewsChange` ALTER `NewsNewTema` DROP DEFAULT, ALTER `NewsOldTema` DROP DEFAULT;

ALTER TABLE `NewsChange`
 CHANGE COLUMN `NewsNewTema` `NewsNewTema` VARCHAR(150) NULL AFTER `DateChange`,
 CHANGE COLUMN `NewsOldTema` `NewsOldTema` VARCHAR(150) NULL AFTER `NewsNewTema`,
 CHANGE COLUMN `NewsOldDescription` `NewsOldDescription` TEXT NULL AFTER `NewsOldTema`,
 CHANGE COLUMN `NewsNewDescription` `NewsNewDescription` TEXT NULL AFTER `NewsOldDescription`;

ALTER TABLE `NotificationToProducers`
 CHANGE COLUMN `Enabled` `Enabled` TINYINT(1) NOT NULL DEFAULT '1' AFTER `DatePublication`;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view catalognameswithuptime as
select cn.Id, cn.Name, cn.DescriptionId, cn.MnnId, COALESCE(d.UpdateTime, cn.UpdateTime) as UpdateTime 
from catalogs.catalognames cn
left join catalogs.Descriptions d on d.Id = cn.DescriptionId;

CREATE TABLE `ReportRunLog` (
	`Id` INT(10) NOT NULL AUTO_INCREMENT,
	`JobName` VARCHAR(200) NOT NULL,
	`AccountId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`Ip` VARCHAR(50) NULL DEFAULT NULL,
	`RunStartTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`RunNow` TINYINT(1) NOT NULL DEFAULT '1',
	PRIMARY KEY (`Id`),
	INDEX `IDX_JobName` (`JobName`)
);

alter table `jobextend` add index `IDX_JobName` (`JobName`);

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view reportrunlogwithuser as
select rl.Id, rl.JobName, rl.Ip, rl.RunStartTime, rl.RunNow, a.Name as UserName
from ReportRunLog rl
left outer join Account a on a.Id = rl.AccountId;




