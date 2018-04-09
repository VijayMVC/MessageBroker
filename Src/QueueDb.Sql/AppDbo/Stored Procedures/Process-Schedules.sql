CREATE PROCEDURE [AppDbo].[Process-Schedules]
    @QueueId [dbo].[KeyIdType]
AS
BEGIN
    SET NOCOUNT ON;

    -- Check to see if we need to do this?
    DECLARE @status int;
    EXEC @status = [AppDbo].[Check-LastProcessedDate] @QueueName = 'Schedule'
    IF @status = 0
    BEGIN
        -- not now
        RETURN;
    END

    -- Find schedules
    DECLARE @scheduleIds TABLE ([ScheduleId] [dbo].[IdType], [MessageId] [dbo].[IdType] NULL);

    UPDATE  [AppDbo].[Schedule]
    SET     [_deletedDate] = GETUTCDATE()
    OUTPUT  inserted.[ScheduleId] INTO @scheduleIds ([ScheduleId])
    WHERE   [QueueId] = @QueueId
    AND     [_deletedDate] IS NULL
    AND     [ScheduleDate] <= GETUTCDATE();

    UPDATE  @scheduleIds
    SET     [MessageId] = NEXT VALUE FOR [AppDbo].[MessageIdSequence];

    INSERT INTO [AppDbo].[ActiveQueue] (
        [MessageId]
        ,[QueueId]
        ,[ClientMessageId]
        ,[Cv]
        ,[Payload]
        ,[CreatedByAgentId]
    )
    SELECT  x.[MessageId]
            ,s.[QueueId]
            ,s.[ClientMessageId]
            ,s.[Cv]
            ,s.[Payload]
            ,s.[CreatedByAgentId]
    FROM    @scheduleIds x
            inner join [AppDbo].[Schedule] s on x.[ScheduleId] = s.[ScheduleId];
END