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
	
	
alter TABLE `promotions` drop FOREIGN KEY `fk_promotions_to_fileid`;

alter TABLE `promotions` add CONSTRAINT `fk_promotions_to_fileid` FOREIGN KEY (`PromoFileId`) REFERENCES `MediaFiles` (`Id`);

drop table promotionsimage;

 ALTER TABLE `Account`
 ADD COLUMN `SecureTime` DATETIME NULL DEFAULT NULL AFTER `RegionMask`;

ALTER TABLE `Account`
 CHANGE COLUMN `Name` `Name` VARCHAR(100) NULL DEFAULT NULL,
 CHANGE COLUMN `LastName` `LastName` VARCHAR(30) NULL DEFAULT NULL COMMENT 'Фамилия',
 CHANGE COLUMN `FirstName` `FirstName` VARCHAR(30) NULL DEFAULT NULL COMMENT 'Имя',
 CHANGE COLUMN `OtherName` `OtherName` VARCHAR(30) NULL DEFAULT NULL COMMENT 'Отчество',
 CHANGE COLUMN `Password` `Password` VARCHAR(50) NULL DEFAULT NULL,
 CHANGE COLUMN `Appointment` `Appointment` VARCHAR(50) NULL DEFAULT NULL,
 CHANGE COLUMN `Enabled` `Enabled` TINYINT(4) NULL DEFAULT NULL;
 
 CREATE TABLE `AccountFeedBackComment` (
 `Id` INT(10) NOT NULL AUTO_INCREMENT,
 `IdFeedBack` INT(10) UNSIGNED NOT NULL,
 `Comment` VARCHAR(250) NULL,
 `DateAdd` DATETIME NULL DEFAULT NULL,
 PRIMARY KEY (`Id`),
 CONSTRAINT `FK1_Comment_To_Feedback` FOREIGN KEY (`IdFeedBack`) REFERENCES `AccountFeedBack` (`Id`) ON UPDATE CASCADE ON DELETE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

alter TABLE `ReportRunLog`
 add column `MailTo` VARCHAR(1000) NULL DEFAULT NULL after `RunNow`;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view reportrunlogwithuser as
select rl.Id, rl.JobName, rl.Ip, rl.RunStartTime, rl.RunNow, a.Name as UserName, p.ProducerName, rl.MailTo
from ReportRunLog rl
left outer join Account a on a.Id = rl.AccountId 
left outer join AccountCompany ac on ac.Id = a.CompanyId
left outer join ProducerNames p on p.ProducerId = ac.ProducerId;

CREATE TABLE `mailformToMediaFiles` (
	`MailFormId` INT(10) NOT NULL,
	`MediaFilesId` INT(10) NOT NULL,
	PRIMARY KEY (`MailformId`, `MediaFilesId`),
	CONSTRAINT `fk_to_mailform` FOREIGN KEY (`MailFormId`) REFERENCES `mailform` (`Id`) ON DELETE CASCADE,
	CONSTRAINT `fk_to_MediaFiles` FOREIGN KEY (`MediaFilesId`) REFERENCES `MediaFiles` (`Id`) ON DELETE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

ALTER TABLE `AccountFeedBackComment`
 ADD COLUMN `AdminId` INT(11) UNSIGNED NOT NULL,
 ADD CONSTRAINT `FK2_Comment_AccountAdminId` FOREIGN KEY (`AdminId`) REFERENCES `Account` (`Id`);

 ALTER TABLE `AccountFeedBackComment`
 ADD COLUMN `StatusOld` TINYINT(4) NOT NULL,
 ADD COLUMN `StatusNew` TINYINT(4) NOT NULL;

 CREATE TABLE `ReportDescription` (
 `Id` INT NOT NULL,
 `Name` VARCHAR(250) NOT NULL,
 `ClassName` VARCHAR(250) NOT NULL,
 `Description` VARCHAR(250) NOT NULL
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

ALTER TABLE `ReportDescription`
 ADD PRIMARY KEY (`Id`),
 ADD INDEX `Id` (`Id`);

 drop PROCEDURE `ProductRatingReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `SupplierId` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select c.CatalogName, T.ProducerId,
if(c.IsPharmacie = 1, p.ProducerName, \'Нелекарственный ассортимент\') as ProducerName,
r.RegionName, 
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, RegionCode, ProducerId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Min(Cost) as MinCost,
	Avg(Cost) as AvgCost,
	Max(Cost) as MaxCost,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and SupplierId not in (', SupplierId, ')
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId,ProducerId,RegionCode
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.ProducerNames p on p.ProducerId = T.ProducerId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

CREATE TABLE `PromotionsToSupplier` (
 `PromotionId` INT(10) UNSIGNED NOT NULL,
 `SupplierId` INT(10) UNSIGNED NOT NULL,
 INDEX `PromotionId_SupplierId` (`PromotionId`, `SupplierId`),
 CONSTRAINT `FK1_This_To_PromotionId` FOREIGN KEY (`PromotionId`) REFERENCES `promotions` (`Id`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

ALTER TABLE `PromotionsToSupplier`
 ADD PRIMARY KEY (`PromotionId`, `SupplierId`);

drop procedure SupplierRatingReport;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `SupplierRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select s.SupplierName, T.Summ
from
	(select SupplierId, 
	Sum(Cost*Quantity) as Summ
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by SupplierId
	order by Summ desc) as T
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

drop PROCEDURE `PharmacyRatingReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PharmacyRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select ph.PharmacyName, T.Summ
from
	(select PharmacyId,
	Sum(Cost*Quantity) as Summ
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by PharmacyId
	order by Summ desc) as T
left join producerinterface.PharmacyNames ph on ph.PharmacyId = T.PharmacyId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view supplierregions as 	
select distinct s.Id as SupplierId, s.RegionMask
from Customers.Suppliers s
inner join usersettings.PricesData pd on pd.FirmCode = s.Id
where pd.Enabled = 1 and pd.IsLocal = 0 and pd.AgencyEnabled = 1 and s.Disabled = 0;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PromotionsInRegionMask`(IN `RGM` bigint(20) UNSIGNED)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

 IF(RGM <= 0) THEN
 	select ps.Id, ps.RegionMask
	from promotions ps;
 ELSE  
   select ps.Id Id, ps.RegionMask
 	from promotions ps
 	where RGM & ps.RegionMask; 
 END IF;
 
END$$


drop PROCEDURE `ProductConcurentRatingReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductConcurentRatingReport`(IN `CatalogId` VARCHAR(1000), IN `RegionCode` VARCHAR(1000), IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
if(c.IsPharmacie = 1, p.ProducerName, \'Нелекарственный ассортимент\') as ProducerName,
r.RegionName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, ProducerId, RegionCode,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Min(Cost) as MinCost,
	Avg(Cost) as AvgCost,
	Max(Cost) as MaxCost,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId,ProducerId,RegionCode
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.ProducerNames p on p.ProducerId = T.ProducerId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

drop PROCEDURE `SupplierRatingReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `SupplierRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select s.SupplierName, r.RegionName, T.Summ
from
	(select SupplierId, RegionCode, 
	Sum(Cost*Quantity) as Summ
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by SupplierId,RegionCode
	order by Summ desc) as T
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

drop PROCEDURE `PharmacyRatingReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PharmacyRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select ph.PharmacyName, r.RegionName, T.Summ
from
	(select PharmacyId, RegionCode,
	Sum(Cost*Quantity) as Summ
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by PharmacyId,RegionCode
	order by Summ desc) as T
left join producerinterface.PharmacyNames ph on ph.PharmacyId = T.PharmacyId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

ALTER TABLE `AccountEmail` DROP FOREIGN KEY `Email_AccountId`;
ALTER TABLE `AccountEmail` ALTER `AccountId` DROP DEFAULT;
ALTER TABLE `AccountEmail`
 CHANGE COLUMN `AccountId` `AccountId` INT(10) UNSIGNED NOT NULL AFTER `eMail`,
 ADD CONSTRAINT `Email_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Id`) ON UPDATE NO ACTION ON DELETE NO ACTION;

drop PROCEDURE `SecondarySalesReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `SecondarySalesReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `RegionMask` BIGINT(20) UNSIGNED, IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select c.CatalogName, p.ProducerName, r.RegionName, 
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, RegionCode, ProducerId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
	where CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and SupplierId not in 
		(select s.Id
		from Customers.Suppliers s
		inner join usersettings.PricesData pd on pd.FirmCode = s.Id
		where pd.Enabled = 1 and pd.IsLocal = 1 and pd.AgencyEnabled = 1 and s.Disabled = 0 
		and s.RegionMask & ', RegionMask, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId,ProducerId,RegionCode
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.ProducerNames p on p.ProducerId = T.ProducerId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

drop PROCEDURE `SecondarySalesReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `SecondarySalesReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `RegionMask` BIGINT(20) UNSIGNED, IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select c.CatalogName, p.ProducerName, r.RegionName, s.SupplierName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, ProducerId, RegionCode, SupplierId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
	where CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and SupplierId not in 
		(select s.Id
		from Customers.Suppliers s
		inner join usersettings.PricesData pd on pd.FirmCode = s.Id
		where pd.Enabled = 1 and pd.IsLocal = 1 and pd.AgencyEnabled = 1 and s.Disabled = 0 
		and s.RegionMask & ', RegionMask, ')
	and ProducerId = ', ProducerId, '
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId,ProducerId,RegionCode,SupplierId
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.ProducerNames p on p.ProducerId = T.ProducerId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

#не внесено на боевую

drop PROCEDURE `ProductPriceDynamicsReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductPriceDynamicsReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime, IN `DateTo` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select cn.CatalogName, p.ProducerName, r.RegionName, TT.Date, TT.AvgCost from
	(select p.CatalogId, T.ProducerId, T.RegionId, T.Date, Avg(T.AvgCost) as AvgCost from
		(select ProductId, ProducerId, RegionId, Date, Avg(Cost) as AvgCost
		from reports.AverageCosts
		where Date >= \'', DateFrom, '\'
		and Date < \'', DateTo, '\'
		and RegionId in (', RegionCode, ')
		and ProducerId = ', ProducerId, '
		and ProductId in (select Id 
								from catalogs.Products pp
								where pp.CatalogId in (', CatalogId, '))
		group by ProductId, ProducerId, RegionId, Date) as T
	inner join catalogs.Products p on p.Id = T.ProductId
	group by p.CatalogId, T.ProducerId, T.RegionId, T.Date
	order by p.CatalogId, T.ProducerId, T.RegionId, T.Date) as TT
left outer join producerinterface.regionnames r on r.RegionCode = TT.RegionId
left outer join producerinterface.catalognames cn on cn.CatalogId = TT.CatalogId
left outer join producerinterface.producernames p on p.ProducerId = TT.ProducerId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

#не внесено на боевую




drop PROCEDURE `ProductResidueReportNow`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductResidueReportNow`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select cn.CatalogName, p.ProducerName, r.RegionName, s.SupplierName, null as Cost, CAST(TTT.Quantity as SIGNED INTEGER) as Quantity from
	(select p.CatalogId, TT.ProducerId, TT.RegionCode, TT.SupplierId, SUM(TT.Quantity) as Quantity from
		(select distinct c.ProductId, c.CodeFirmCr as ProducerId, T.RegionCode, T.FirmCode as SupplierId, c.Quantity from
			(select pd.PriceCode, pd.FirmCode, prd.RegionCode
			from usersettings.PricesData pd 
			inner join usersettings.PricesRegionalData prd ON prd.pricecode = pd.pricecode 
			where pd.agencyenabled = 1 and pd.enabled = 1 and pd.pricetype <> 1 and prd.enabled = 1
			and prd.RegionCode in (', RegionCode, ') 
			and pd.PriceCode in 
				(select distinct pc.PriceCode
				from usersettings.PricesCosts pc
				inner join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
				inner join farm.FormRules f on f.Id = pi.FormRuleId
				where to_seconds(now()) - to_seconds(pi.PriceDate) < f.maxold * 86400)) as T
		inner join farm.core0 c on c.PriceCode = T.PriceCode
		where c.CodeFirmCr = ', ProducerId, ' and c.ProductId in 
			(select Id 
			from catalogs.Products pp
			where pp.CatalogId in (', CatalogId, '))) as TT
	inner join catalogs.Products p on p.Id = TT.ProductId
	group by p.CatalogId, TT.ProducerId, TT.RegionCode, TT.SupplierId) as TTT
left outer join producerinterface.regionnames r on r.RegionCode = TTT.RegionCode
left outer join producerinterface.catalognames cn on cn.CatalogId = TTT.CatalogId
left outer join producerinterface.producernames p on p.ProducerId = TTT.ProducerId
left outer join producerinterface.SupplierNames s on s.SupplierId = TTT.SupplierId
order by cn.CatalogName, p.ProducerName, r.RegionName');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

drop PROCEDURE `ProductResidueReport`;

CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductResidueReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `ProducerId` INT(10) UNSIGNED, IN `DateFrom` datetime)
	LANGUAGE SQL
	NOT DETERMINISTIC
	CONTAINS SQL
	SQL SECURITY DEFINER
	COMMENT ''
BEGIN

  SET @sql = CONCAT('select cn.CatalogName, p.ProducerName, r.RegionName, s.SupplierName, TT.Cost, CAST(TT.Quantity as SIGNED INTEGER) as Quantity from
	(select p.CatalogId, T.ProducerId, T.RegionId, T.SupplierId, Avg(T.Cost) as Cost, SUM(T.Quantity) as Quantity from
		(select ProductId, ProducerId, RegionId, SupplierId, Cost, Quantity
		from reports.AverageCosts
		where Date = \'', DateFrom, '\'
		and RegionId in (', RegionCode, ')
		and ProducerId = ', ProducerId, '
		and ProductId in (select Id 
								from catalogs.Products pp
								where pp.CatalogId in (', CatalogId, '))
		) as T
	inner join catalogs.Products p on p.Id = T.ProductId
	group by p.CatalogId, T.ProducerId, T.RegionId, T.SupplierId) as TT
left outer join producerinterface.regionnames r on r.RegionCode = TT.RegionId
left outer join producerinterface.catalognames cn on cn.CatalogId = TT.CatalogId
left outer join producerinterface.producernames p on p.ProducerId = TT.ProducerId
left outer join producerinterface.SupplierNames s on s.SupplierId = TT.SupplierId
order by cn.CatalogName, p.ProducerName, r.RegionName');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END$$

