CREATE VIEW [AppDbo].[ActiveQueue_View]
AS
    SELECT  x.[MessageId]
            ,x.[QueueId]
            ,x.[ClientMessageId]
            ,x.[Cv]
            ,x.[Payload]
            ,x.[LockedDate]
            ,x.[LockedByAgentId]
            ,x.[RetryCount]
            ,x.[CreatedByAgentId]
            ,x.[_createdDate]
    FROM    [AppDbo].[ActiveQueue] x
    WHERE   [_deletedDate] IS NULL;
