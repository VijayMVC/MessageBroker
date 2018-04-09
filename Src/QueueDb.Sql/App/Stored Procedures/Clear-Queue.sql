CREATE PROCEDURE [App].[Clear-Queue]
    @QueueName [dbo].[QueueNameType]
    ,@CopyToHistory [dbo].[FlagType] = 0
AS
BEGIN
    -- Clear all messages in a queue, and optional copy to history
    SET NOCOUNT ON;

    DECLARE @deletedDate [dbo].[DateType] = GETUTCDATE();
    DECLARE @messageIds [AppDbo].[MessageId_Table];

    BEGIN TRAN;

    -- Mark all current and dead-letter queue items as deleted
    UPDATE  [AppDbo].[ActiveQueue]
    SET     [_deletedDate] = @deletedDate
    OUTPUT  inserted.[MessageId] INTO @messageIds ([MessageId])
    FROM    [AppDbo].[ActiveQueue] x
            inner join [AppDbo].[QueueManagement] c on x.[QueueId] = c.[QueueId]
                and c.[QueueName] = @QueueName;

    IF @CopyToHistory = 1
    BEGIN
        EXEC [AppDbo].[CopyTo-History] @MessageIds = @messageIds, @ActivityType = 'Deleted', @ErrorMessage = null
    END

    COMMIT TRAN;
END