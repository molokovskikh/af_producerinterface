	use producerInterface;

DELETE FROM producerInterface.jobextend
  WHERE JobName = '32bfe241-7947-4613-b555-47ce1d6627d4' 
  OR JobName = '3c979945-79b0-4d68-95e8-cfb687b59100';
  
 	ALTER TABLE producerInterface.jobextend 
	CHANGE COLUMN `ProducerId` `ProducerId` INT(10) UNSIGNED NULL AFTER `ReportType`;
	
	ALTER TABLE producerInterface.jobextend 
	ADD CONSTRAINT `FK_jobextend_catalogs.producers` FOREIGN KEY (`ProducerId`) REFERENCES `catalogs`.`producers` (`Id`);