use accessright;

CREATE TABLE `admin_roles` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
CREATE TABLE `admin_permissions` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
CREATE TABLE `admin_permission_role` (
	`PermissionId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`RoleId` INT(11) UNSIGNED NOT NULL,
	INDEX `role` (`RoleId`),
	INDEX `permission` (`PermissionId`),
	CONSTRAINT `FK_admin_permission_role_admin_permissions` FOREIGN KEY (`PermissionId`) REFERENCES `admin_permissions` (`Id`) ON UPDATE CASCADE,
	CONSTRAINT `FK_admin_permission_role_admin_roles` FOREIGN KEY (`RoleId`) REFERENCES `admin_roles` (`Id`) ON UPDATE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
CREATE TABLE `admin_role` (
	`AdminId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`RoleId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`PermissionId` INT(11) UNSIGNED NULL DEFAULT NULL,
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	PRIMARY KEY (`Id`),
	INDEX `FK_admin_role_regionaladmins` (`AdminId`),
	INDEX `FK_admin_role_admin_permissions` (`PermissionId`),
	INDEX `FK_admin_role_admin_roles` (`RoleId`),
	CONSTRAINT `FK_admin_role_admin_permissions` FOREIGN KEY (`PermissionId`) REFERENCES `admin_permissions` (`Id`),
	CONSTRAINT `FK_admin_role_admin_roles` FOREIGN KEY (`RoleId`) REFERENCES `admin_roles` (`Id`),
	CONSTRAINT `FK_admin_role_regionaladmins` FOREIGN KEY (`AdminId`) REFERENCES `regionaladmins` (`RowID`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
CREATE TABLE `admin_logs` (
	`Id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`AdminId` INT(11) UNSIGNED ZEROFILL NULL DEFAULT NULL,
	`ModelId` INT(11) UNSIGNED ZEROFILL NULL DEFAULT NULL,
	`ModelClass` VARCHAR(50) NULL DEFAULT NULL,
	`Date` DATETIME NOT NULL,
	`Type` TINYINT(4) NOT NULL,
	`Message` TEXT NOT NULL,
	PRIMARY KEY (`Id`),
	INDEX `FK_admin_logs_regionaladmins` (`AdminId`),
	CONSTRAINT `FK_admin_logs_regionaladmins` FOREIGN KEY (`AdminId`) REFERENCES `regionaladmins` (`RowID`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT=27
;