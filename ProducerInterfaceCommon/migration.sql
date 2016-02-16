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


