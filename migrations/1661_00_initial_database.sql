-- --------------------------------------------------------
-- Хост:                         127.0.0.1
-- Версия сервера:               5.6.20 - MySQL Community Server (GPL)
-- ОС Сервера:                   Win64
-- HeidiSQL Версия:              9.1.0.4904
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Дамп структуры базы данных producerinterface
CREATE DATABASE IF NOT EXISTS `producerinterface` /*!40100 DEFAULT CHARACTER SET cp1251 */;
USE `producerinterface`;


-- Дамп структуры для таблица producerinterface.producers
CREATE TABLE IF NOT EXISTS `producers` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` text,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=cp1251;

-- Дамп данных таблицы producerinterface.producers: ~2 rows (приблизительно)
/*!40000 ALTER TABLE `producers` DISABLE KEYS */;
INSERT INTO `producers` (`Id`, `Name`) VALUES
	(1, 'asdasd'),
	(2, 'Тестовый производитель');
/*!40000 ALTER TABLE `producers` ENABLE KEYS */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
