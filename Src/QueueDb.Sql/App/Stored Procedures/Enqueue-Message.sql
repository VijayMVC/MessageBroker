CREATE PROCEDURE [App].[Enqueue-Message]
    @QueueName [dbo].[QueueNameType]
    ,@AgentId [dbo].[KeyIdType]
    ,@ClientMessageId [dbo].[ClientMessageIdType] = null
    ,@Cv [dbo].[CvType] = null
    ,@Payload [dbo].[PayloadType]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @queueId [dbo].[KeyIdType];
    DECLARE @currentQueueSize [dbo].[SizeType];
    
    BEGIN TRAN;

    SELECT  @queueId = [QueueId]
            ,@currentQueueSize = [CurrentSizeLimit]
    FROM    [AppDbo].[QueueManagement_View]
    WHERE   [QueueName] = @QueueName 
    AND     [Disable] = 0

    IF @queueId IS NULL
    BEGIN
        ROLLBACK TRAN;
        RAISERROR ('Message queue name %s is not active or does not exist.', 17, 1, @QueueName);
        RETURN;
    END        

    -- Check queue size limit
    DECLARE @CurrentSize int = (SELECT count(*) FROM [AppDbo].[ActiveQueue_View] WHERE [QueueId] = @queueId);
    IF @CurrentSize >= @currentQueueSize
    BEGIN
        RAISERROR ('Message queue is full', 17, 1);
        ROLLBACK TRAN;
        RETURN;
    END

    -- Get next message number
    DECLARE @messageId [dbo].[IdType] = NEXT VALUE FOR [AppDbo].[MessageIdSequence];

    -- Search for deleted records first to overwrite
    DECLARE @RowIds TABLE ([RowId] [dbo].[IdType]);

    UPDATE  TOP (1) [AppDbo].[ActiveQueue]
    SET     [MessageId] = @messageId
            ,[QueueId] = @queueId
            ,[ClientMessageId] = @ClientMessageId
            ,[Cv] = @Cv
            ,[Payload] = @Payload
            ,[LockedDate] = null
            ,[LockedByAgentId] = null
            ,[RetryCount] = 0
            ,[CreatedByAgentId] = @AgentId
            ,[_createdDate] = GETUTCDATE()
            ,[_deletedDate] = null
    OUTPUT  INSERTED.[RowId] into @RowIds ([RowId])
    WHERE   [_deletedDate] IS NOT NULL;

    IF EXISTS (SELECT * FROM @RowIds)
    BEGIN
        SELECT @messageId as 'Id';
        COMMIT TRAN;
        RETURN;
    END

    -- Create new entry for queue item
    INSERT INTO [AppDbo].[ActiveQueue] (
        [MessageId]
        ,[QueueId]
        ,[ClientMessageId]
        ,[Cv]
        ,[Payload]
        ,[CreatedByAgentId]
    )
    VALUES (
        @messageId
        ,@queueId
        ,@ClientMessageId
        ,@Cv
        ,@Payload
        ,@AgentId
    );

    SELECT @messageId as 'Id';
    COMMIT TRAN;
END