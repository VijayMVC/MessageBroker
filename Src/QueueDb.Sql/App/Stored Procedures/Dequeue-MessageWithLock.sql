CREATE PROCEDURE [App].[Dequeue-MessageWithLock]
    @QueueName [dbo].[QueueNameType]
    ,@AgentId [dbo].[KeyIdType]
AS
BEGIN
    -- Dequeue message with lock, setups up message that is time locked
    -- for the agent, and requires explicit settlement using "Settle-Message" SP.
    SET NOCOUNT ON;

    DECLARE @queueId [dbo].[KeyIdType];
    DECLARE @lockValidForSec [dbo].[SizeType];
    DECLARE @retryLimit [dbo].[SizeType];

    BEGIN TRAN;

    SELECT  @queueId = [QueueId]
            ,@lockValidForSec = [LockValidForSec]
            ,@retryLimit = [CurrentRetryLimit]
    FROM    [AppDbo].[QueueManagement_View]
    WHERE   [QueueName] = @QueueName
    AND     [Disable] = 0

    IF @queueId IS NULL
    BEGIN
        ROLLBACK TRAN;
        RAISERROR ('Cannot find queue %s', 17, 1, @QueueName);
        RETURN
    END

    -- Check for retry limit violations and schedules
    EXEC [AppDbo].[Enforce-RetryLimits] @QueueId = @queueId, @RetryLimit = @retryLimit;
    EXEC [AppDbo].[Process-Schedules] @QueueId = @queueId;

    DECLARE @lockCutoffDate [dbo].[DateType] = [AppDbo].[Calculate-CutoffDate](@lockValidForSec);
    DECLARE @ids TABLE ([MessageId] [dbo].[IdType]);

    -- Get the next queue item and mark it locked
    UPDATE  [AppDbo].[ActiveQueue]
    SET     [RetryCount] = [RetryCount] + 1
            ,[LockedDate] = GETUTCDATE()
            ,[LockedByAgentId] = @AgentId
    OUTPUT  inserted.MessageId INTO @ids ([MessageId])
    FROM    [AppDbo].[ActiveQueue] x
            inner join (
                SELECT  TOP (1) i.[MessageId]
                FROM    [AppDbo].[ActiveQueue_View] i
                WHERE   i.[QueueId] = @queueId
                AND     (i.[LockedDate] IS NULL OR i.[LockedDate] < @lockCutoffDate)
                ORDER BY i.[MessageId] ASC
                ) iCurrent on x.MessageId = iCurrent.MessageId;

    SELECT  x.*
    FROM    [AppDbo].[ActiveQueue_View] x
            inner join @ids i on x.[MessageId] = i.[MessageId];

    COMMIT TRAN;
END