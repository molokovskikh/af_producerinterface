use producerinterface;
CREATE TABLE `users` (
	`Id` INT(11) NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(50) NOT NULL DEFAULT '0',
	`Login` VARCHAR(50) NOT NULL DEFAULT '0',
	`Password` VARCHAR(50) NOT NULL DEFAULT '0',
	`Email` VARCHAR(50) NOT NULL DEFAULT '0',
	`Appointment` VARCHAR(50) NOT NULL DEFAULT '0',
	`Producer` INT(10) UNSIGNED NULL DEFAULT NULL,
	`PasswordUpdated` DATETIME NULL DEFAULT NULL,
	`PasswordToUpdate` TINYINT(4) NOT NULL DEFAULT '0',
	`Enabled` TINYINT(4) NOT NULL DEFAULT '0',
	PRIMARY KEY (`Id`),
	INDEX `producerUser_key` (`Producer`),
	CONSTRAINT `producerUser_key` FOREIGN KEY (`Producer`) REFERENCES `producers` (`Id`) ON UPDATE CASCADE ON DELETE NO ACTION
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT=40
;
