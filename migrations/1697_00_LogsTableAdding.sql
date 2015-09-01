use producerinterface;

ALTER TABLE `users`	CHANGE COLUMN `Id` `Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT FIRST;
ALTER TABLE `users` DROP COLUMN `Login`;
ALTER TABLE `users`	CHANGE COLUMN `Producer` `ProducerId` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `Appointment`;

CREATE TABLE `profile_news` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Topic` VARCHAR(255) NOT NULL,
	`Text` TEXT NOT NULL,
	`ProducerId` INT(10) UNSIGNED NULL DEFAULT NULL,
	`EditedDate` DATETIME NOT NULL,
	`CreatedDate` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`Id`),
	INDEX `FK_profile_news_producers` (`ProducerId`),
	CONSTRAINT `FK_profile_news_producers` FOREIGN KEY (`ProducerId`) REFERENCES `producers` (`Id`) ON UPDATE CASCADE ON DELETE NO ACTION
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;



CREATE TABLE `user_permissions` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;

CREATE TABLE `user_roles` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;

CREATE TABLE `user_role` (
	`UserId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`RoleId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`PermissionId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	PRIMARY KEY (`Id`),
	INDEX `role` (`RoleId`),
	INDEX `user` (`UserId`),
	INDEX `permission` (`PermissionId`),
	CONSTRAINT `FK_user_role_user_permissions` FOREIGN KEY (`PermissionId`) REFERENCES `user_permissions` (`Id`),
	CONSTRAINT `FK_user_role_user_roleslist` FOREIGN KEY (`RoleId`) REFERENCES `user_roles` (`Id`),
	CONSTRAINT `FK_user_role_users` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;

CREATE TABLE `user_permission_role` (
	`PermissionId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`RoleId` INT(11) UNSIGNED NOT NULL,
	INDEX `role` (`RoleId`),
	INDEX `permission` (`PermissionId`),
	CONSTRAINT `FK_user_permission_role_user_permissions` FOREIGN KEY (`PermissionId`) REFERENCES `user_permissions` (`Id`),
	CONSTRAINT `FK_user_permission_role_user_roleslist` FOREIGN KEY (`RoleId`) REFERENCES `user_roles` (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
CREATE TABLE `user_logs` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`UserId` INT(11) UNSIGNED ZEROFILL NULL DEFAULT NULL,
	`ModelId` INT(11) UNSIGNED ZEROFILL NULL DEFAULT NULL,
	`ModelClass` VARCHAR(50) NULL DEFAULT NULL,
	`Date` DATETIME NOT NULL,
	`Type` TINYINT(4) NOT NULL,
	`Message` TEXT NOT NULL,
	PRIMARY KEY (`Id`),
	INDEX `FK_user_logs_users` (`UserId`),
	CONSTRAINT `FK_user_logs_users` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON UPDATE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;


