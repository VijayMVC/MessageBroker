CREATE PROCEDURE [App].[Delete-Queue]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeletedDate [dbo].[DateType] = GETUTCDATE();

    -- Mark all current and dead-letter queue items as deleted
    UPDATE  [AppDbo].[ActiveQueue]
    SET     [_deletedDate] = @DeletedDate
    FROM    [AppDbo].[ActiveQueue] x
            inner join [AppDbo].[QueueManagement] c on x.[QueueId] = c.[QueueId]
    WHERE   c.[QueueName] = @QueueName;

    -- Mark queue as deleted
    UPDATE  [AppDbo].[QueueManagement]
    SET     [_deletedDate] = GETUTCDATE()
    WHERE   [QueueName] = @QueueName;

END