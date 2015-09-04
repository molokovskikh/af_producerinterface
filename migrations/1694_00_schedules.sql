use reports;
	CREATE TABLE `monthlyschedule` (
		`Id` INT(11) NOT NULL AUTO_INCREMENT,
		`Hour` INT(11) NULL DEFAULT NULL,
		`Minute` INT(11) NULL DEFAULT NULL,
		`GeneralReportId` BIGINT(20) UNSIGNED NULL DEFAULT NULL,
		PRIMARY KEY (`Id`)
	);
	
    create table MonthlyScheduleMonths (
        MonthlyScheduleId INTEGER not null,
       value SMALLINT
    );

    create table MonthlyScheduleDays (
        MonthlyScheduleId INTEGER not null,
       value INTEGER
    );

    create table Reports.WeeklySchedule (
        Id INTEGER NOT NULL AUTO_INCREMENT,
       Hour INTEGER,
       Minute INTEGER,
      	`GeneralReportId` BIGINT(20) UNSIGNED NULL DEFAULT NULL,
       primary key (Id)
    );

    create table WeeklyScheduleDays (
        WeeklyScheduleId INTEGER not null,
       value INTEGER
    );

