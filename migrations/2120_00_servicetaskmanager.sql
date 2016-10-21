	use producerInterface;
CREATE TABLE `servicetaskmanager` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`JobName` VARCHAR(200) NOT NULL,
	`ServiceName` VARCHAR(200) NOT NULL,
	`ServiceType` VARCHAR(400) NOT NULL,
	`JsonData` TEXT NULL,
	`CreationDate` DATETIME NOT NULL,
	`LastModified` DATETIME NOT NULL,
	`LastRun` DATETIME NULL DEFAULT NULL,
	`AccountId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`Enabled` TINYINT(1) NOT NULL,
	PRIMARY KEY (`Id`),
	INDEX `FK_servicetaskmanager_account` (`AccountId`),
	CONSTRAINT `FK_servicetaskmanager_account` FOREIGN KEY (`AccountId`) REFERENCES `account` (`Id`)
);