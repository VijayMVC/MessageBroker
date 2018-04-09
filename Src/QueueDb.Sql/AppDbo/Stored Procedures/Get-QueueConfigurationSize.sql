CREATE PROCEDURE [AppDbo].[GetQueueConfigurationSize]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    DECLARE @queueSize [dbo].[SizeType] = (SELECT [QueueSize] FROM [AppDbo].[QueueConfiguration] WHERE [QueueName] = @QueueName)
    IF @queueSize IS NOT NULL
    BEGIN
        RETURN @queueSize;
    END

    MERGE   [AppDbo].[QueueConfiguration] as target
    USING   (SELECT @QueueName) as source ([QueueName])
    ON      (target.[QueueName] = source.[QueueName])
    WHEN MATCHED THEN
        UPDATE SET
            [LastProcessedDate] = GETUTCDATE()
    WHEN NOT MATCHED THEN
        INSERT (
            [QueueName]
            ,[QueueSize]
        ) VALUES (
            source.[QueueName]
            ,10000
            );

    RETURN 1000;
END
