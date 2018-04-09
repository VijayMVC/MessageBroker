CREATE PROCEDURE [AppDbo].[Enforce-RetryLimits]
    @QueueId [dbo].[KeyIdType]
    ,@RetryLimit [dbo].[SizeType]
AS
BEGIN
    SET NOCOUNT ON;

    -- Check to see if we need to do this?
    DECLARE @status int;
    EXEC @status = [AppDbo].[Check-LastProcessedDate] @QueueName = 'Current'
    IF @status = 0
    BEGIN
        -- not now
        RETURN;
    END

    -- Find queue items that are in violation of retry limit
    DECLARE @MessageIds [AppDbo].[MessageId_Table];

    UPDATE  [AppDbo].[ActiveQueue]
    SET     [_deletedDate] = GETUTCDATE()
    OUTPUT  inserted.[MessageId] INTO @MessageIds ([MessageId])
    WHERE   [QueueId] = @QueueId
    AND     [RetryCount] >= @RetryLimit
    AND     [_deletedDate] is NULL;

    -- Copy all to history
    EXEC [AppDbo].[CopyTo-History] @MessageIds = @MessageIds, @ActivityType = 'RetryLimitExceeded', @ErrorMessage = null

END