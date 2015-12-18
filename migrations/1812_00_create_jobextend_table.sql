use quartz;

CREATE TABLE `jobextend` (
	`SchedName` VARCHAR(120) NOT NULL,
	`JobName` VARCHAR(200) NOT NULL,
	`JobGroup` VARCHAR(200) NOT NULL,
	`CustomName` VARCHAR(250) NOT NULL,
	`Scheduler` VARCHAR(250) NULL,
	`ReportType` INT(10) NOT NULL,
	`ProducerId` BIGINT(19) NOT NULL,
	`Creator` VARCHAR(250) NOT NULL,
	`CreationDate` DATETIME NOT NULL,
	`LastModified` DATETIME NOT NULL,
	`Enable` BOOLEAN NOT NULL DEFAULT true,
	PRIMARY KEY (`SchedName`, `JobName`, `JobGroup`),

	FOREIGN KEY (SchedName, JobName, JobGroup)
      REFERENCES qrtz_job_details(SCHED_NAME, JOB_NAME, JOB_GROUP)
      ON DELETE CASCADE
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB;

