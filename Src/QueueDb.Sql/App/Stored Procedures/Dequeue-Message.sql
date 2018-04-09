CREATE PROCEDURE [App].[Dequeue-Message]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    -- Dequeue message will try to get the next queue item and copy it to
    -- history.  This SP implements implicit settle operation.
    SET NOCOUNT ON;

    DECLARE @queueId [dbo].[KeyIdType];
    DECLARE @lockValidForSec [dbo].[SizeType];
    DECLARE @retryLimit [dbo].[SizeType];

    SELECT  @queueId = [QueueId]
            ,@lockValidForSec = [LockValidForSec]
            ,@retryLimit = [CurrentRetryLimit]
    FROM    [AppDbo].[QueueManagement_View]
    WHERE   [QueueName] = @QueueName

    IF @lockValidForSec IS NULL
    BEGIN
        RAISERROR ('Cannot find queue %s', 17, 1, @QueueName);
        RETURN
    END

    -- Check for retry limit violations and schedules
    EXEC [AppDbo].[Enforce-RetryLimits] @QueueId = @queueId, @RetryLimit = @retryLimit;
    EXEC [AppDbo].[Process-Schedules] @QueueId = @queueId;

    DECLARE @lockCutoffDate [dbo].[DateType] = [AppDbo].[Calculate-CutoffDate](@lockValidForSec);
    DECLARE @messageIds [AppDbo].[MessageId_Table];

    BEGIN TRAN;

    -- Get the next queue item and mark it processed, will check for messages that
    -- are available or the lock has expired
    UPDATE  [AppDbo].[ActiveQueue]
    SET     [SettleByAgentId] = [CreatedByAgentId]
            ,[RetryCount] = [RetryCount] + 1
            ,[_deletedDate] = GETUTCDATE()
    OUTPUT  inserted.[MessageId] INTO @messageIds ([MessageId])
    FROM    [AppDbo].[ActiveQueue] x
            inner join (
                SELECT  TOP (1) i.[MessageId]
                FROM    [AppDbo].[ActiveQueue_View] i
                WHERE   i.[QueueId] = @queueId
                AND     (i.[LockedDate] IS NULL OR i.[LockedDate] > @lockCutoffDate)
                ORDER BY i.[MessageId] ASC
                ) iCurrent on x.MessageId = iCurrent.MessageId;

    -- Anything to return?
    DECLARE @MessageId [dbo].[IdType] = (SELECT TOP 1 [MessageId] FROM @messageIds)
    IF @MessageId IS NULL
    BEGIN
        COMMIT TRAN;
        RETURN;
    END

    -- Add to history
    EXEC [AppDbo].[CopyTo-History] @MessageIds = @messageIds, @ActivityType = 'Processed', @ErrorMessage = null

    -- Return queue message
    SELECT  x.*
    FROM    [AppDbo].[ActiveQueue] x
            inner join @messageIds i on x.[MessageId] = i.[MessageId];

    COMMIT TRAN;
END