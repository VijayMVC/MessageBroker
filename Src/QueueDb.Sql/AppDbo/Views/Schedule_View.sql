CREATE VIEW [AppDbo].[Schedule_View]
AS
    SELECT  x.[ScheduleId]
            ,x.[QueueId]
            ,x.[ClientMessageId]
            ,x.[Cv]
            ,x.[Payload]
            ,x.[ScheduleDate]
            ,x.[CreatedByAgentId]
            ,x.[_createdDate]
    FROM    [AppDbo].[Schedule] x
    WHERE   [_deletedDate] IS NULL;
