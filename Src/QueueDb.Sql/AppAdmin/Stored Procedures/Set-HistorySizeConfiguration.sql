CREATE PROCEDURE [AppAdmin].[Set-HistorySizeConfiguration]
    @QueueSize int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LoggingQueueName [dbo].[QueueNameType] = 'History';

    SET @QueueSize = IIF(@QueueSize <= 0, 1, @QueueSize);

    MERGE   [AppDbo].[QueueConfiguration] as target
    USING   (SELECT @LoggingQueueName, @QueueSize) as source ([QueueName], [QueueSize])
    ON      (target.[QueueName] = source.[QueueName])
    WHEN MATCHED THEN
        UPDATE SET [QueueSize] = source.[QueueSize]
    WHEN NOT MATCHED THEN
        INSERT ([QueueName], [QueueSize])
        VALUES (source.[QueueName], source.[QueueSize]);
END