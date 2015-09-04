use reports; 
    alter table Reports.MonthlySchedule 
        add index (GeneralReportId), 
        add constraint MonthlySchedule_GeneralReportId 
        foreign key (GeneralReportId) 
        references reports.general_reports (GeneralReportCode);

    alter table Reports.MonthlyScheduleMonths 
        add index (MonthlyScheduleId), 
        add constraint MonthlyScheduleMonths_MonthlyScheduleId 
        foreign key (MonthlyScheduleId) 
        references Reports.MonthlySchedule (Id);

    alter table Reports.MonthlyScheduleDays 
        add index (MonthlyScheduleId), 
        add constraint MonthlyScheduleDays_MonthlyScheduleId 
        foreign key (MonthlyScheduleId) 
        references Reports.MonthlySchedule (Id);

    alter table Reports.WeeklySchedule 
        add index (GeneralReportId), 
        add constraint WeeklySchedule_GeneralReportId 
        foreign key (GeneralReportId) 
        references reports.general_reports (GeneralReportCode);

    alter table Reports.WeeklyScheduleDays 
        add index (WeeklyScheduleId), 
        add constraint WeeklyScheduleDays_WeeklyScheduleId 
        foreign key (WeeklyScheduleId) 
        references Reports.WeeklySchedule (Id);
