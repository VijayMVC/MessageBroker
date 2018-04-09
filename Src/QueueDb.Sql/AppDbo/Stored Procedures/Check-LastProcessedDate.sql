CREATE PROCEDURE [AppDbo].[Check-LastProcessedDate]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    -- Check to see if we need to do this?
    DECLARE @lastProcessedDate [dbo].[DateType] = (SELECT [LastProcessedDate] FROM [AppDbo].[QueueConfiguration] WHERE [QueueName] = @QueueName)
    IF @lastProcessedDate IS NOT NULL AND GETUTCDATE() <= DATEADD(second, 5, @lastProcessedDate)
    BEGIN
        -- Return false
        RETURN 0;
    END

    -- Rest or write last processed date
    MERGE   [AppDbo].[QueueConfiguration] as target
    USING   (SELECT @QueueName) as source ([QueueName])
    ON      (target.[QueueName] = source.[QueueName])
    WHEN MATCHED THEN
        UPDATE SET
            [LastProcessedDate] = GETUTCDATE()
    WHEN NOT MATCHED THEN
        INSERT (
            [QueueName]
            ,[LastProcessedDate]
        ) VALUES (
            source.[QueueName]
            ,GETUTCDATE()
            );

    RETURN 1;
END
