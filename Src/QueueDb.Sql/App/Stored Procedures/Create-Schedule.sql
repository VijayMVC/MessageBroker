CREATE PROCEDURE [App].[Create-Schedule]
    @QueueName [dbo].[QueueNameType]
    ,@AgentId [dbo].[KeyIdType]
    ,@ClientMessageId [dbo].[ClientMessageIdType] = NULL
    ,@Cv [dbo].[CvType] = null
    ,@Payload [dbo].[PayloadType]  = NULL
    ,@ScheduleDate [dbo].[DateType]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @queueId [dbo].[KeyIdType];
    DECLARE @currentQueueSize [dbo].[SizeType];

    IF @ScheduleDate < GETUTCDATE()
    BEGIN
        RAISERROR ('Message schedule is in the past or current.', 17, 1);
        RETURN;
    END        

    SELECT  @queueId = [QueueId]
            ,@currentQueueSize = [CurrentSizeLimit]
    FROM    [AppDbo].[QueueManagement]
    WHERE   [QueueName] = @QueueName 
    AND     [_deletedDate] IS NULL
    AND     [Disable] = 0

    IF @queueId IS NULL
    BEGIN
        RAISERROR ('Message queue name %s is not active or does not exist.', 17, 1, @QueueName);
        RETURN;
    END        

    BEGIN TRAN;

    -- Get next schedule number
    DECLARE @scheduleId [dbo].[IdType] = NEXT VALUE FOR [AppDbo].[ScheduleIdSequence];

    -- Search for deleted records first to overwrite
    DECLARE @RowId [dbo].[IdType] = (SELECT TOP(1) [RowId] FROM [AppDbo].[Schedule] WHERE [_deletedDate] IS NOT NULL);
    IF @RowId is NOT NULL
    BEGIN
        UPDATE  [AppDbo].[Schedule]
        SET     [ScheduleId] = @scheduleId
                ,[QueueId] = @queueId
                ,[ClientMessageId] = @ClientMessageId
                ,[Cv] = @Cv
                ,[Payload] = @Payload
                ,[ScheduleDate] = @ScheduleDate
                ,[CreatedByAgentId] = @AgentId
                ,[_createdDate] = GETUTCDATE()
                ,[_deletedDate] = NULL
        WHERE   [RowId] = @RowId;

        COMMIT TRAN;
        SELECT @scheduleId as 'Id';
        RETURN;
    END

    -- Check queue size limit
    DECLARE @CurrentSize int = (SELECT count(*) FROM [AppDbo].[Schedule_View] WHERE [QueueId] = @queueId);
    IF @CurrentSize >= @currentQueueSize
    BEGIN
        RAISERROR ('Message schedule queue is full', 17, 1);
        ROLLBACK TRAN;
        RETURN;
    END

    -- Create new entry for queue item
    INSERT INTO [AppDbo].[Schedule] (
        [ScheduleId]
        ,[QueueId]
        ,[ClientMessageId]
        ,[Cv]
        ,[Payload]
        ,[ScheduleDate]
        ,[CreatedByAgentId]
    )
    VALUES (
        @scheduleId
        ,@queueId
        ,@ClientMessageId
        ,@Cv
        ,@Payload
        ,@ScheduleDate
        ,@AgentId
    );

    COMMIT TRAN;
    SELECT @scheduleId as 'Id';

END