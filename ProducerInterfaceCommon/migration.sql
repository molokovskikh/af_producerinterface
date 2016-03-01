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

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view PharmacyNames as
select cl.Id as PharmacyId, concat(cl.Name, ' - ', rg.Region) as PharmacyName
from Customers.Clients cl
left join farm.regions rg on rg.RegionCode = cl.RegionCode;

ALTER TABLE `promotionsimage`
 ADD COLUMN `NewsOrPromotions` TINYINT(1) NOT NULL AFTER `ImageSize`;

 insert into mailform (Id, Subject, Body, IsBodyHtml, Description)
 values (9, 'Нет данных для формировании отчета на сайте {SiteName}', 
 'Для формирования запрошенного Вами отчета {ReportName} недостаточно данных, обратитесь в АналитФармация (office@analit.net)', 
 0, 'Нет данных для формировании отчета');
 
 insert into mailform (Id, Subject, Body, IsBodyHtml, Description)
 values (10, 'Отчет с сайта {SiteName}', 
 'Отчет {ReportName} пользователя {CreatorName} производителя {ProducerName}, сформированный в {DateTimeNow}. Чтобы отписаться от автоматической рассылки, перешлите это письмо на адрес office@analit.net', 
 0, 'Автоматическая рассылка отчетов');
 
 insert into mailform (Id, Subject, Body, IsBodyHtml, Description)
 values (11, 'Отчет с сайта {SiteName}', 
 'Отчет {ReportName} пользователя {CreatorName} производителя {ProducerName}, сформированный в {DateTimeNow} пользователем {UserName} ({UserLogin})', 
 0, 'Ручная рассылка отчетов');

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view mailformwithfooter as
select m.Id, m.Subject, m.Body, mm.Body as Footer, m.IsBodyHtml, m.Description, mm.Subject as Header
from mailform m
inner join mailform mm on mm.Id = 8;

ALTER TABLE `AccountAppointment`
ADD COLUMN `IdAccountCompany` INT(10) UNSIGNED NULL AFTER `GlobalEnabled`;
 
ALTER TABLE `AccountAppointment`
ADD CONSTRAINT `FK1_Appointment_To_AccountCompany_Id` FOREIGN KEY (`IdAccountCompany`) REFERENCES `AccountCompany` (`Id`);

ALTER TABLE `Account`
 ADD COLUMN `LastUpdatePermisison` DATETIME NULL AFTER `Enabled`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER `AccountGroupToPermission_after_insert` BEFORE INSERT ON `AccountGroupToPermission` FOR EACH ROW BEGIN
 UPDATE Account ACC
    SET ACC.LastUpdatePermisison = now() 
  WHERE ACC.Id = (select AUTG.Iduser Id from AccountUserToGroup AUTG where AUTG.IdGroup = NEW.IdGroup);
END;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER `AccountGroupToPermission_before_delete` BEFORE DELETE ON `AccountGroupToPermission` FOR EACH ROW BEGIN
 UPDATE Account ACC
    SET ACC.LastUpdatePermisison = now() 
  WHERE ACC.Id = (select AUTG.Iduser Id from AccountUserToGroup AUTG where AUTG.IdGroup = OLD.IdGroup);
END;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER `AccountUserToGroup_after_insert` AFTER INSERT ON `AccountUserToGroup` FOR EACH ROW BEGIN
 UPDATE Account ACC
    SET ACC.LastUpdatePermisison = now() 
  WHERE ACC.Id = new.IdUser;
END;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER `AccountUserToGroup_before_delete` BEFORE DELETE ON `AccountUserToGroup` FOR EACH ROW BEGIN
 UPDATE Account ACC
    SET ACC.LastUpdatePermisison = now() 
  WHERE ACC.Id = OLD.IdUser;
END;

ALTER TABLE `Account`
 ADD COLUMN `RegionMask` BIGINT(20) UNSIGNED NULL DEFAULT NULL AFTER `LastUpdatePermisison`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PromotionsInRegionMask`(IN `RGM` bigint(20) UNSIGNED)
BEGIN
 select ps.Id Id, ps.RegionMask
 from promotions ps
 where RGM & ps.RegionMask;
END

ALTER TABLE `Account`
 CHANGE COLUMN `Name` `Name` VARCHAR(75) NULL DEFAULT '0' AFTER `TypeUser`,
 ADD COLUMN `LastName` VARCHAR(30) NULL DEFAULT '0' COMMENT 'Фамилия' AFTER `Name`,
 ADD COLUMN `FirstName` VARCHAR(30) NULL DEFAULT '0' COMMENT 'Имя' AFTER `LastName`,
 ADD COLUMN `OtherName` VARCHAR(30) NULL DEFAULT '0' COMMENT 'Отчество' AFTER `FirstName`;

 ALTER TABLE `AccountFeedBack`
 ADD COLUMN `UrlString` VARCHAR(250) NULL AFTER `AccountId`,
 ADD COLUMN `Type` TINYINT(4) NULL AFTER `UrlString`,
 ADD COLUMN `Contacts` VARCHAR(50) NULL AFTER `Type`;

ALTER TABLE `jobextend`
ADD COLUMN `CreatorId` INT(11) UNSIGNED NULL AFTER `ProducerId`;

update jobextend j set CreatorId = (select a.Id from Account a where a.Name = j.Creator);

ALTER TABLE `jobextend` CHANGE COLUMN `CreatorId` `CreatorId` INT(11) UNSIGNED NOT NULL;

ALTER TABLE `jobextend` drop COLUMN `Creator`;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view jobextendwithproducer as
select j.`SchedName`, j.`JobName`, j.`JobGroup`, j.`CustomName`, j.`Scheduler`, j.`ReportType`,
j.`ProducerId`, j.CreatorId, a.Name as `Creator`, j.`CreationDate`, j.`LastModified`, j.`DisplayStatus`, j.`LastRun`,
j.`Enable`, p.ProducerName
from jobextend j
LEFT JOIN producernames p ON p.ProducerId = j.ProducerId 
LEFT JOIN Account a ON a.Id = j.CreatorId;

ALTER TABLE `NewsChange`
 ADD COLUMN `IP` VARCHAR(50) NOT NULL AFTER `Id`;

 ALTER TABLE `AccountAppointment`
 DROP FOREIGN KEY `FK1_Appointment_To_AccountCompany_Id`;
ALTER TABLE `AccountAppointment`
 CHANGE COLUMN `IdAccountCompany` `IdAccount` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `GlobalEnabled`,
 ADD CONSTRAINT `FK1_Appointment_To_AccountCompany_Id` FOREIGN KEY (`IdAccount`) REFERENCES `Account` (`Id`);

 # не внесено на боевой БД

 CREATE TABLE `MediaFiles` (
	`Id` INT(10) NOT NULL AUTO_INCREMENT,
	`ImageName` VARCHAR(50) NOT NULL,
	`ImageFile` LONGBLOB NOT NULL,
	`ImageType` VARCHAR(25) NOT NULL,
	`ImageSize` VARCHAR(25) NULL DEFAULT NULL,
	`EntityType` INT(10) NOT NULL,
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;


insert into MediaFiles(	`Id`,
	`ImageName`,
	`ImageFile`,
	`ImageType`,
	`ImageSize`,
	`EntityType`)
select `Id`,
	`ImageName`,
	`ImageFile`,
	`ImageType`,
	`ImageSize`,
	`NewsOrPromotions`
	from promotionsimage;
	
	
	update MediaFiles set EntityType = 2 where EntityType = 0;
	
	update promotions set PromoFileId = null;
	
alter TABLE `promotions` drop FOREIGN KEY `fk_promotions_to_fileid`;

alter TABLE `promotions` add CONSTRAINT `fk_promotions_to_fileid` FOREIGN KEY (`PromoFileId`) REFERENCES `MediaFiles` (`Id`);

delete from MediaFiles where EntityType = 2;

drop table promotionsimage;




