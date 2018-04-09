CREATE PROCEDURE [AppAdmin].[Reset-Database]
AS
BEGIN
    SET NOCOUNT ON;

    TRUNCATE TABLE [AppDbo].[History];
    TRUNCATE TABLE [AppDbo].[QueueConfiguration];
    DELETE [AppDbo].[Schedule];
    DELETE [AppDbo].[ActiveQueue];
    DELETE [AppDbo].[AgentRegistration];
    DELETE [AppDbo].[QueueManagement];

    ALTER SEQUENCE [AppDbo].[PushLoggingQueueSequence] RESTART WITH 0;
    ALTER SEQUENCE [AppDbo].[MessageIdSequence] RESTART WITH 1;
    ALTER SEQUENCE [AppDbo].[ScheduleIdSequence] RESTART WITH 1;

    DBCC CHECKIDENT ([AppDbo.ActiveQueue], RESEED, 0);
    DBCC CHECKIDENT ([AppDbo.History], RESEED, 0);
    DBCC CHECKIDENT ([AppDbo.AgentRegistration], RESEED, 0);
    DBCC CHECKIDENT ([AppDbo.QueueManagement], RESEED, 0);
    DBCC CHECKIDENT ([AppDbo.Schedule], RESEED, 0);
END