use producerinterface;

-- --------------------------------------------------------
-- Хост:                         testsql.analit.net
-- Версия сервера:               5.6.16 - Source distribution
-- ОС Сервера:                   Win64
-- HeidiSQL Версия:              9.1.0.4904
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Дамп структуры для таблица producerinterface.Account
CREATE TABLE IF NOT EXISTS `Account` (
  `Id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `Login` varchar(50) DEFAULT NULL,
  `AppointmentId` int(10) DEFAULT NULL,
  `TypeUser` tinyint(4) NOT NULL,
  `Name` varchar(100) DEFAULT NULL,
  `LastName` varchar(30) DEFAULT NULL COMMENT 'Фамилия',
  `FirstName` varchar(30) DEFAULT NULL COMMENT 'Имя',
  `OtherName` varchar(30) DEFAULT NULL COMMENT 'Отчество',
  `Phone` varchar(15) DEFAULT NULL,
  `Password` varchar(50) DEFAULT NULL,
  `Appointment` varchar(50) DEFAULT NULL,
  `CompanyId` int(10) unsigned DEFAULT NULL,
  `PasswordUpdated` datetime DEFAULT NULL,
  `Enabled` tinyint(4) DEFAULT NULL,
  `LastUpdatePermisison` datetime DEFAULT NULL,
  `RegionMask` bigint(20) unsigned DEFAULT NULL,
  `SecureTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `UserLogin` (`Login`),
  KEY `UserId` (`Id`),
  KEY `Account_CompanyId` (`CompanyId`),
  KEY `Account_Appointment` (`AppointmentId`),
  CONSTRAINT `Account_Appointment` FOREIGN KEY (`AppointmentId`) REFERENCES `AccountAppointment` (`Id`) ON DELETE SET NULL ON UPDATE SET NULL,
  CONSTRAINT `Account_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `AccountCompany` (`Id`) ON DELETE SET NULL ON UPDATE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountAppointment
CREATE TABLE IF NOT EXISTS `AccountAppointment` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL DEFAULT '',
  `GlobalEnabled` tinyint(4) DEFAULT NULL,
  `IdAccount` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK1_Appointment_To_AccountCompany_Id` (`IdAccount`),
  CONSTRAINT `FK1_Appointment_To_AccountCompany_Id` FOREIGN KEY (`IdAccount`) REFERENCES `Account` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountCompany
CREATE TABLE IF NOT EXISTS `AccountCompany` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(250) DEFAULT '',
  `ProducerId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`),
  KEY `ProducerId` (`ProducerId`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountEmail
CREATE TABLE IF NOT EXISTS `AccountEmail` (
  `Id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `eMail` varchar(50) NOT NULL DEFAULT '',
  `AccountId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`),
  KEY `AccountId` (`AccountId`),
  CONSTRAINT `Email_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountFeedBack
CREATE TABLE IF NOT EXISTS `AccountFeedBack` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Status` tinyint(4) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `DateAdd` date DEFAULT NULL,
  `AccountId` int(11) unsigned DEFAULT NULL,
  `UrlString` varchar(250) DEFAULT NULL,
  `Type` tinyint(4) DEFAULT NULL,
  `Contacts` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`),
  KEY `FK_FeedBack_To_Account` (`AccountId`),
  CONSTRAINT `FK_FeedBack_To_Account` FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountFeedBackComment
CREATE TABLE IF NOT EXISTS `AccountFeedBackComment` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `IdFeedBack` int(10) unsigned NOT NULL,
  `Comment` varchar(250) DEFAULT NULL,
  `DateAdd` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK1_Comment_To_Feedback` (`IdFeedBack`),
  CONSTRAINT `FK1_Comment_To_Feedback` FOREIGN KEY (`IdFeedBack`) REFERENCES `AccountFeedBack` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountGroup
CREATE TABLE IF NOT EXISTS `AccountGroup` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL DEFAULT '0',
  `Enabled` tinyint(1) NOT NULL,
  `Description` varchar(250) DEFAULT NULL,
  `TypeGroup` tinyint(4) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountGroupToPermission
CREATE TABLE IF NOT EXISTS `AccountGroupToPermission` (
  `IdGroup` int(10) NOT NULL,
  `IdPermission` int(10) NOT NULL,
  PRIMARY KEY (`IdGroup`,`IdPermission`),
  KEY `FK_to_Permission_controlpanel` (`IdPermission`),
  CONSTRAINT `FK_to_Group_controlpanel` FOREIGN KEY (`IdGroup`) REFERENCES `AccountGroup` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_to_Permission_controlpanel` FOREIGN KEY (`IdPermission`) REFERENCES `AccountPermission` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountPermission
CREATE TABLE IF NOT EXISTS `AccountPermission` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `ControllerAction` varchar(50) NOT NULL,
  `ActionAttributes` text,
  `Description` varchar(250) DEFAULT NULL,
  `Enabled` tinyint(1) NOT NULL,
  `TypePermission` tinyint(4) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `ControllerAction` (`ControllerAction`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.AccountUserToGroup
CREATE TABLE IF NOT EXISTS `AccountUserToGroup` (
  `IdUser` int(11) unsigned NOT NULL,
  `IdGroup` int(10) NOT NULL,
  PRIMARY KEY (`IdUser`,`IdGroup`),
  KEY `FK_to_Group_to__controlpanel` (`IdGroup`),
  CONSTRAINT `FK_GroupId_To_AccountGroup` FOREIGN KEY (`IdGroup`) REFERENCES `AccountGroup` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_GroupUserId_To_AccountId` FOREIGN KEY (`IdUser`) REFERENCES `Account` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.CompanyDomainName
CREATE TABLE IF NOT EXISTS `CompanyDomainName` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL DEFAULT '',
  `CompanyId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `CompanyId` (`CompanyId`),
  CONSTRAINT `CompanyId_AccountCompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `AccountCompany` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.DrugDescriptionRemark
CREATE TABLE IF NOT EXISTS `DrugDescriptionRemark` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `EnglishName` varchar(255) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `Interaction` varchar(255) DEFAULT NULL,
  `SideEffect` varchar(255) DEFAULT NULL,
  `IndicationsForUse` varchar(255) DEFAULT NULL,
  `Dosing` varchar(255) DEFAULT NULL,
  `Warnings` varchar(255) DEFAULT NULL,
  `ProductForm` varchar(255) DEFAULT NULL,
  `PharmacologicalAction` varchar(255) DEFAULT NULL,
  `Storage` varchar(255) DEFAULT NULL,
  `Expiration` varchar(255) DEFAULT NULL,
  `Composition` varchar(255) DEFAULT NULL,
  `CreationDate` datetime DEFAULT NULL,
  `Status` int(11) DEFAULT NULL,
  `ModificationDate` datetime DEFAULT NULL,
  `ModificatorId` int(11) unsigned DEFAULT NULL,
  `ProducerUserId` int(11) unsigned DEFAULT NULL,
  `DrugFamilyId` int(11) unsigned DEFAULT NULL,
  `MNNId` int(11) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `ModificatorId` (`ModificatorId`),
  KEY `ProducerUserId` (`ProducerUserId`),
  KEY `DrugFamilyId` (`DrugFamilyId`),
  KEY `MNNId` (`MNNId`),
  CONSTRAINT `DrugDescriptionRemark_DrugFamilyId` FOREIGN KEY (`DrugFamilyId`) REFERENCES `catalogs`.`catalogNames` (`Id`),
  CONSTRAINT `DrugDescriptionRemark_MNNId` FOREIGN KEY (`MNNId`) REFERENCES `catalogs`.`mnn` (`Id`),
  CONSTRAINT `DrugDescriptionRemark_ModificatorId` FOREIGN KEY (`ModificatorId`) REFERENCES `accessright`.`regionaladmins` (`RowID`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.jobextend
CREATE TABLE IF NOT EXISTS `jobextend` (
  `SchedName` varchar(120) NOT NULL,
  `JobName` varchar(200) NOT NULL,
  `JobGroup` varchar(200) NOT NULL,
  `CustomName` varchar(250) NOT NULL,
  `Scheduler` varchar(250) DEFAULT NULL,
  `ReportType` int(10) NOT NULL,
  `ProducerId` bigint(19) NOT NULL,
  `CreatorId` int(11) unsigned NOT NULL,
  `CreationDate` datetime NOT NULL,
  `LastModified` datetime NOT NULL,
  `DisplayStatus` int(10) NOT NULL DEFAULT '0',
  `LastRun` datetime DEFAULT NULL,
  `Enable` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`SchedName`,`JobName`,`JobGroup`),
  KEY `IDX_JobName` (`JobName`),
  CONSTRAINT `jobextend_ibfk_1` FOREIGN KEY (`SchedName`, `JobName`, `JobGroup`) REFERENCES `quartz`.`qrtz_job_details` (`SCHED_NAME`, `JOB_NAME`, `JOB_GROUP`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.LogChangeSet
CREATE TABLE IF NOT EXISTS `LogChangeSet` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Timestamp` datetime NOT NULL,
  `UserId` int(11) unsigned NOT NULL,
  `Ip` varchar(50) DEFAULT NULL,
  `Description` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK1_logChange_To_Account` (`UserId`),
  CONSTRAINT `FK1_logChange_To_Account` FOREIGN KEY (`UserId`) REFERENCES `Account` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.LogForNet
CREATE TABLE IF NOT EXISTS `LogForNet` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Message` text,
  `DateAddLog` datetime NOT NULL,
  `ErrorType` varchar(10) NOT NULL,
  `Host` varchar(50) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.LogObjectChange
CREATE TABLE IF NOT EXISTS `LogObjectChange` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `TypeName` varchar(250) NOT NULL,
  `ObjectReference` varchar(250) NOT NULL,
  `Action` int(10) NOT NULL,
  `LogChangeSetId` int(10) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LogObjectChange_LogChangeSet` (`LogChangeSetId`),
  CONSTRAINT `LogObjectChange_LogChangeSet` FOREIGN KEY (`LogChangeSetId`) REFERENCES `LogChangeSet` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.LogPropertyChange
CREATE TABLE IF NOT EXISTS `LogPropertyChange` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `PropertyName` varchar(250) NOT NULL,
  `ValueOld` text,
  `ValueNew` text,
  `LogObjectChangeId` int(10) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LogPropertyChange_LogObjectChange` (`LogObjectChangeId`),
  CONSTRAINT `LogPropertyChange_LogObjectChange` FOREIGN KEY (`LogObjectChangeId`) REFERENCES `LogObjectChange` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.mailform
CREATE TABLE IF NOT EXISTS `mailform` (
  `Id` int(10) NOT NULL,
  `Subject` varchar(250) NOT NULL,
  `Body` mediumtext,
  `IsBodyHtml` tinyint(1) NOT NULL DEFAULT '0',
  `Description` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.MediaFiles
CREATE TABLE IF NOT EXISTS `MediaFiles` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `ImageName` varchar(50) NOT NULL,
  `ImageFile` longblob NOT NULL,
  `ImageType` varchar(25) NOT NULL,
  `ImageSize` varchar(25) DEFAULT NULL,
  `EntityType` int(10) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.NewsChange
CREATE TABLE IF NOT EXISTS `NewsChange` (
  `IdAccount` int(11) unsigned NOT NULL,
  `IdNews` int(10) unsigned NOT NULL,
  `TypeCnhange` tinyint(4) unsigned NOT NULL,
  `DateChange` datetime NOT NULL,
  `NewsNewTema` varchar(150) DEFAULT NULL,
  `NewsOldTema` varchar(150) DEFAULT NULL,
  `NewsOldDescription` text,
  `NewsNewDescription` text,
  `Id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `IP` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IdAccount_IdNews_TypeCnhange_DateChange` (`IdAccount`,`IdNews`),
  KEY `Id` (`Id`),
  KEY `FK2_NewsChangeToNotification` (`IdNews`),
  CONSTRAINT `FK1_NewsChangeToAccount` FOREIGN KEY (`IdAccount`) REFERENCES `Account` (`Id`),
  CONSTRAINT `FK2_NewsChangeToNotification` FOREIGN KEY (`IdNews`) REFERENCES `NotificationToProducers` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.NotificationToProducers
CREATE TABLE IF NOT EXISTS `NotificationToProducers` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(150) DEFAULT '',
  `Description` text,
  `DatePublication` date DEFAULT NULL,
  `Enabled` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для процедура producerinterface.PharmacyRatingReport
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PharmacyRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
ph.PharmacyName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, PharmacyId,
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
	group by CatalogId, PharmacyId
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.PharmacyNames ph on ph.PharmacyId = T.PharmacyId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END//
DELIMITER ;


-- Дамп структуры для процедура producerinterface.ProductConcurentRatingReport
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductConcurentRatingReport`(IN `CatalogNamesId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Min(Cost) as MinCost,
	Avg(Cost) as AvgCost,
	Max(Cost) as MaxCost,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (select Id from catalogs.Catalog cc
							where cc.NameId in
							(', CatalogNamesId, ')
							)
	and RegionCode in (', RegionCode, ')
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END//
DELIMITER ;


-- Дамп структуры для процедура producerinterface.ProductPriceDynamicsReport
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductPriceDynamicsReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select cn.CatalogName, r.RegionName, TT.Date, TT.AvgCost from
	(select p.CatalogId, T.RegionId, T.Date, Avg(T.AvgCost) as AvgCost from
		(select ProductId, RegionId, Date, Avg(Cost) as AvgCost
		from reports.AverageCosts
		where Date >= \'', DateFrom, '\'
		and Date < \'', DateTo, '\'
		and RegionId in (', RegionCode, ')
		and ProductId in (select Id 
								from catalogs.Products pp
								where pp.CatalogId in (', CatalogId, '))
		group by ProductId, RegionId, Date
		order by ProductId, RegionId, Date) as T
	inner join catalogs.Products p on p.Id = T.ProductId
	group by p.CatalogId, T.RegionId, T.Date) as TT
left outer join producerinterface.regionnames r on r.RegionCode = TT.RegionId
left outer join producerinterface.catalognames cn on cn.CatalogId = TT.CatalogId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END//
DELIMITER ;


-- Дамп структуры для процедура producerinterface.ProductRatingReport
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `ProductRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `SupplierId` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
if(c.IsPharmacie = 1, p.ProducerName, \'Нелекарственный ассортимент\') as ProducerName,
r.RegionName, 
s.SupplierName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, RegionCode, SupplierId, ProducerId,
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

END//
DELIMITER ;


-- Дамп структуры для таблица producerinterface.profilenews
CREATE TABLE IF NOT EXISTS `profilenews` (
  `Id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `Topic` varchar(255) NOT NULL,
  `Text` text NOT NULL,
  `ProducerId` int(10) unsigned DEFAULT NULL,
  `EditedDate` datetime NOT NULL,
  `CreatedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_profile_news_producers` (`ProducerId`),
  KEY `ProducerId` (`ProducerId`),
  CONSTRAINT `FK_profile_news_producers` FOREIGN KEY (`ProducerId`) REFERENCES `producers` (`Id`) ON DELETE NO ACTION ON UPDATE CASCADE,
  CONSTRAINT `ProfileNews_ProducerId` FOREIGN KEY (`ProducerId`) REFERENCES `catalogs`.`Producers` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.promotions
CREATE TABLE IF NOT EXISTS `promotions` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `UpdateTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Enabled` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `AdminId` int(10) unsigned DEFAULT NULL,
  `ProducerId` int(10) unsigned NOT NULL,
  `ProducerUserId` int(10) unsigned NOT NULL,
  `Annotation` mediumtext NOT NULL,
  `PromoFile` varchar(255) DEFAULT NULL,
  `PromoFileId` int(10) DEFAULT NULL,
  `AgencyDisabled` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `Name` varchar(255) NOT NULL COMMENT '???????????? ?????',
  `RegionMask` bigint(20) unsigned NOT NULL COMMENT '??????? ?????? ?????',
  `Begin` datetime DEFAULT NULL COMMENT '???? ?????? ?????',
  `End` datetime DEFAULT NULL COMMENT '???? ????????? ?????',
  `Status` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `ProducerAdminId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `AdminId` (`AdminId`),
  KEY `FK_promotions_produceruser` (`ProducerUserId`),
  KEY `promotions_ProducerId` (`ProducerId`),
  KEY `AdminId_2` (`AdminId`),
  KEY `ProducerUserId` (`ProducerUserId`),
  KEY `PromoFileId` (`PromoFileId`),
  KEY `ProducerAdminId` (`ProducerAdminId`),
  CONSTRAINT `fk_promotions_to_fileid` FOREIGN KEY (`PromoFileId`) REFERENCES `MediaFiles` (`Id`),
  CONSTRAINT `FK_promotions_AccountId` FOREIGN KEY (`ProducerUserId`) REFERENCES `Account` (`Id`),
  CONSTRAINT `FK_promotions_AdminId_To_ProducerId` FOREIGN KEY (`ProducerAdminId`) REFERENCES `Account` (`Id`),
  CONSTRAINT `FK_supplierpromotions_accessright.regionaladmins` FOREIGN KEY (`AdminId`) REFERENCES `accessright`.`regionaladmins` (`RowID`) ON DELETE CASCADE,
  CONSTRAINT `promotions_AdminId` FOREIGN KEY (`AdminId`) REFERENCES `accessright`.`regionaladmins` (`RowID`),
  CONSTRAINT `promotions_ProducerId` FOREIGN KEY (`ProducerId`) REFERENCES `catalogs`.`producers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251 ROW_FORMAT=COMPACT;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.promotionsimage
CREATE TABLE IF NOT EXISTS `promotionsimage` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `ImageName` varchar(50) NOT NULL,
  `ImageFile` longblob NOT NULL,
  `ImageType` varchar(25) NOT NULL,
  `ImageSize` varchar(25) DEFAULT NULL,
  `NewsOrPromotions` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для процедура producerinterface.PromotionsInRegionMask
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `PromotionsInRegionMask`(IN `RGM` bigint(20) UNSIGNED)
BEGIN

 IF(RGM <= 0) THEN
 	select ps.Id, ps.RegionMask
	from promotions ps;
 ELSE  
   select ps.Id Id, ps.RegionMask
 	from promotions ps
 	where RGM & ps.RegionMask; 
 END IF;
 
END//
DELIMITER ;


-- Дамп структуры для таблица producerinterface.promotionToDrug
CREATE TABLE IF NOT EXISTS `promotionToDrug` (
  `DrugId` int(10) unsigned NOT NULL,
  `PromotionId` int(10) unsigned NOT NULL,
  PRIMARY KEY (`PromotionId`,`DrugId`),
  KEY `PromotionId` (`PromotionId`),
  KEY `DrugId` (`DrugId`),
  KEY `PromotionId_2` (`PromotionId`),
  KEY `DrugId_2` (`DrugId`),
  CONSTRAINT `FK_promotioncatalogs_supplierpromotions` FOREIGN KEY (`PromotionId`) REFERENCES `promotions` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251 ROW_FORMAT=COMPACT;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.ReportRunLog
CREATE TABLE IF NOT EXISTS `ReportRunLog` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `JobName` varchar(200) NOT NULL,
  `AccountId` int(11) unsigned DEFAULT NULL,
  `Ip` varchar(50) DEFAULT NULL,
  `RunStartTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RunNow` tinyint(1) NOT NULL DEFAULT '1',
  `MailTo` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IDX_JobName` (`JobName`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.reporttemplate
CREATE TABLE IF NOT EXISTS `reporttemplate` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` varchar(255) DEFAULT NULL,
  `Type` int(11) DEFAULT NULL,
  `GeneralReportId` int(11) DEFAULT NULL,
  `UserId` int(11) DEFAULT NULL,
  `Modificator` int(11) DEFAULT NULL,
  `CreationDate` datetime DEFAULT NULL,
  `ModificationDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для таблица producerinterface.reportxml
CREATE TABLE IF NOT EXISTS `reportxml` (
  `SchedName` varchar(120) NOT NULL,
  `JobName` varchar(200) NOT NULL,
  `JobGroup` varchar(200) NOT NULL,
  `Xml` mediumtext NOT NULL,
  PRIMARY KEY (`SchedName`,`JobName`,`JobGroup`),
  KEY `JobName` (`JobName`),
  CONSTRAINT `reportxml_ibfk_1` FOREIGN KEY (`SchedName`, `JobName`, `JobGroup`) REFERENCES `quartz`.`qrtz_job_details` (`SCHED_NAME`, `JOB_NAME`, `JOB_GROUP`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.


-- Дамп структуры для процедура producerinterface.SupplierRatingReport
DELIMITER //
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE `SupplierRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
s.SupplierName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, SupplierId, 
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
	group by CatalogId, SupplierId
	order by Summ desc) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END//
DELIMITER ;


-- Дамп структуры для таблица producerinterface.user_logs
CREATE TABLE IF NOT EXISTS `user_logs` (
  `Id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `ProducerUserId` int(11) unsigned zerofill DEFAULT NULL,
  `ModelId` int(11) unsigned zerofill DEFAULT NULL,
  `ModelClass` varchar(50) DEFAULT NULL,
  `Date` datetime NOT NULL,
  `Type` tinyint(4) NOT NULL,
  `Message` text NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_user_logs_users` (`ProducerUserId`),
  KEY `ProducerUserId` (`ProducerUserId`),
  CONSTRAINT `FK_user_logs_users` FOREIGN KEY (`ProducerUserId`) REFERENCES `produceruser` (`Id`) ON UPDATE CASCADE,
  CONSTRAINT `user_logs_ProducerUserId` FOREIGN KEY (`ProducerUserId`) REFERENCES `ProducerUser` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

-- Экспортируемые данные не выделены.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;


create or replace DEFINER=`RootDBMS`@`127.0.0.1` view assortment as
select c.Id as CatalogId, concat(cn.Name, ' ', cf.Form) as CatalogName, c.Pharmacie as IsPharmacie, a.ProducerId
from catalogs.catalog c
inner join catalogs.assortment a on a.CatalogId = c.Id
left join catalogs.catalognames cn on cn.id = c.NameId
left join catalogs.catalogforms cf on cf.Id = c.FormId 
where c.Hidden = 0;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view catalognames as
select c.Id as CatalogId, concat(cn.Name, ' ', cf.Form) as CatalogName, c.Pharmacie as IsPharmacie
from catalogs.catalog c
left join catalogs.catalognames cn on cn.id = c.NameId
left join catalogs.catalogforms cf on cf.Id = c.FormId 
where c.Hidden = 0;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view catalognameswithuptime as
select cn.Id, cn.Name, cn.DescriptionId, cn.MnnId, COALESCE(d.UpdateTime, cn.UpdateTime) as UpdateTime 
from catalogs.catalognames cn
left join catalogs.Descriptions d on d.Id = cn.DescriptionId;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view drugdescription as
select Id as DescriptionId, Name as DescriptionName, EnglishName, Description, 
Interaction, SideEffect, IndicationsForUse, Dosing, `Warnings`, ProductForm, PharmacologicalAction, `Storage`, 
Expiration, Composition
from catalogs.Descriptions;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view drugfamily as
select distinct cn.Id as FamilyId, cn.Name as FamilyName, a.ProducerId, cn.DescriptionId, cn.MnnId, cn.UpdateTime 
from catalogs.assortment a
inner join catalogs.Catalog c on c.Id = a.CatalogId
inner join catalogs.catalognames cn on cn.Id = c.NameId;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view drugfamilynames as
select Id as FamilyId, Name as FamilyName
from catalogs.catalognames;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view drugformproducer as
select c.Id as CatalogId, a.ProducerId, c.NameId as DrugFamilyId, c.Name as CatalogName, c.VitallyImportant, c.MandatoryList, c.Monobrend, c.Narcotic, c.Toxic, c.Combined
from catalogs.assortment a
inner join catalogs.Catalog c on c.Id = a.CatalogId
where c.Hidden = 0;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view drugmnn as
select Id as MnnId, Mnn, RussianMnn
from catalogs.mnn;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view mailformwithfooter as
select m.Id, m.Subject, m.Body, mm.Body as Footer, m.IsBodyHtml, m.Description, mm.Subject as Header
from mailform m
inner join mailform mm on mm.Id = 8;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view pharmacynames as
select cl.Id as PharmacyId, concat(cl.Name, ' - ', rg.Region) as PharmacyName
from Customers.Clients cl
left join farm.regions rg on rg.RegionCode = cl.RegionCode;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view producernames as
select p.Id as ProducerId, p.Name as ProducerName
from catalogs.Producers p;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view propertychangeview as
select p.Id as ChangePropertyId, p.LogObjectChangeId as ChangeObjectId,
p.PropertyName, p.ValueOld, p.ValueNew, o.TypeName
from LogPropertyChange p
inner join LogObjectChange o on o.Id = p.LogObjectChangeId;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view ratingreportorderitems as
select `p`.`CatalogId` AS `CatalogId`,
`oh`.`RegionCode` AS `RegionCode`,
`pd`.`FirmCode` AS `SupplierId`,
`ol`.`CodeFirmCr` AS `ProducerId`,
`oh`.`ClientCode` AS `PharmacyId`,
`ol`.`Cost`,
`ol`.`Quantity`,
`oh`.`RowID` AS `OrderId`,
`oh`.`AddressId`,
`pd`.`IsLocal` AS `IsLocal`,
`oh`.`WriteTime` AS `WriteTime` 
from (((`ordersold`.`ordershead` `oh` 
join `ordersold`.`orderslist` `ol` on((`ol`.`OrderID` = `oh`.`RowID`))) 
join `catalogs`.`products` `p` on((`p`.`Id` = `ol`.`ProductId`))) 
join `usersettings`.`pricesdata` `pd` on((`pd`.`PriceCode` = `oh`.`PriceCode`)));

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view regionnames as
select r.RegionCode, r.Region as RegionName
from farm.regions r 
where r.DrugsSearchRegion = 0 
and r.RegionCode not in (524288);

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view reportrunlogwithuser as
select rl.Id, rl.JobName, rl.Ip, rl.RunStartTime, rl.RunNow, a.Name as UserName, p.ProducerName, rl.MailTo
from ReportRunLog rl
left outer join Account a on a.Id = rl.AccountId 
left outer join AccountCompany ac on ac.Id = a.CompanyId
left outer join ProducerNames p on p.ProducerId = ac.ProducerId;

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

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view usernames as
select a.Id as UserId, a.Name as UserName, a.Login as Email, p.Id as ProducerId, p.Name as ProducerName 
from producerinterface.Account a
inner join producerinterface.AccountCompany ac on a.CompanyId = ac.Id
inner join catalogs.producers p on p.Id = ac.ProducerId;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view jobextendwithproducer as
select j.`SchedName`, j.`JobName`, j.`JobGroup`, j.`CustomName`, j.`Scheduler`, j.`ReportType`,
j.`ProducerId`, j.CreatorId, a.Name as `Creator`, j.`CreationDate`, j.`LastModified`, j.`DisplayStatus`, j.`LastRun`,
j.`Enable`, p.ProducerName
from jobextend j
LEFT JOIN producernames p ON p.ProducerId = j.ProducerId 
LEFT JOIN Account a ON a.Id = j.CreatorId;

create or replace DEFINER=`RootDBMS`@`127.0.0.1` view logchangeview as
select s.Id as ChangeSetId, s.Timestamp, s.Ip, s.Description,
au.Id as UserId, au.Name as UserName, au.Login as Email, 
p.ProducerId, p.ProducerName,
o.Id as ChangeObjectId, o.`Action`, o.TypeName
from LogChangeSet s
inner join Account au on au.Id = s.UserId
inner join AccountCompany ac on au.CompanyId = ac.Id
inner join producernames p on p.ProducerId = ac.ProducerId
inner join LogObjectChange o on o.LogChangeSetId = s.Id;

