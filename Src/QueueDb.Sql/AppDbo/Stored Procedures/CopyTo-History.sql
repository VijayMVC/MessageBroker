CREATE PROCEDURE [AppDbo].[CopyTo-History]
    @MessageIds [AppDbo].[MessageId_Table] READONLY
    ,@ActivityType [dbo].[ActivityTypeType]
    ,@ErrorMessage [dbo].[ErrorMessageType] = null
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @queueSize [dbo].[SizeType];
    EXEC @queueSize = [AppDbo].[GetQueueConfigurationSize] @QueueName = 'History';

    DECLARE @ids TABLE (
        [MessageId] [dbo].[IdType]
        ,[HistoryId] [dbo].[IdType]
        );

    -- Next new history id and make sure we don't go over the history queue size
    INSERT INTO @ids ([MessageId], [HistoryId])
    SELECT  x.[MessageId]
            ,NEXT VALUE FOR [AppDbo].[PushLoggingQueueSequence]
    FROM    (
                SELECT  TOP (@queueSize)
                        x1.[MessageId]
                FROM    @MessageIds x1
                ORDER BY x1.[MessageId] DESC
            ) x;

    -- History is a ring buffer pattern.  ModId = row number (wraps), History ID is always incrementing
    MERGE [AppDbo].[History] as target
    USING (
        SELECT  x.[HistoryId] % @queueSize
                ,x.[HistoryId]
                ,x.[MessageId]
                ,@ActivityType
                ,q.[QueueName]
                ,c.[Cv]
                ,c.[ClientMessageId]
                ,c.[Payload]
                ,a.[AgentName]
                ,@ErrorMessage
                ,c.[RetryCount]
        FROM    @ids x
                inner join [AppDbo].[ActiveQueue] c on x.[MessageId] = c.[MessageId]
                inner join [AppDbo].[QueueManagement] q on c.[QueueId] = q.[QueueId]
                left outer join [AppDbo].[AgentRegistration] a on c.[SettleByAgentId] = a.[AgentId]
        ) as source (
            [ModId]
            ,[HistoryId]
            ,[MessageId]
            ,[ActivityType]
            ,[QueueName]
            ,[Cv]
            ,[ClientMessageId]
            ,[Payload]
            ,[SettleByAgent]
            ,[ErrorMessage]
            ,[RetryCount]
        )
    ON (target.[RowId] = source.[ModId])
    WHEN MATCHED THEN
        UPDATE SET
            [HistoryId] = source.[HistoryId]
            ,[MessageId] = source.[MessageId]
            ,[ActivityType] = source.[ActivityType]
            ,[QueueName] = source.[QueueName]
            ,[Cv] = source.[Cv]
            ,[ClientMessageId] = source.[ClientMessageId]
            ,[Payload] = source.[Payload]
            ,[SettleByAgent] = source.[SettleByAgent]
            ,[ErrorMessage] = source.[ErrorMessage]
            ,[RetryCount] = source.[RetryCount]
    WHEN NOT MATCHED THEN
        INSERT (
            [HistoryId]
            ,[MessageId]
            ,[ActivityType]
            ,[QueueName]
            ,[Cv]
            ,[ClientMessageId]
            ,[Payload]
            ,[SettleByAgent]
            ,[ErrorMessage]
            ,[RetryCount]
        )
        VALUES (
            source.[HistoryId]
            ,source.[MessageId]
            ,source.[ActivityType]
            ,source.[QueueName]
            ,source.[Cv]
            ,source.[ClientMessageId]
            ,source.[Payload]
            ,source.[SettleByAgent]
            ,source.[ErrorMessage]
            ,source.[RetryCount]
            );

END
