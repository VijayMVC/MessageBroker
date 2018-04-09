CREATE VIEW [AppDbo].[History_View]
AS
    SELECT  x.[HistoryId]
            ,x.[MessageId]
            ,x.[ActivityType]
            ,x.[QueueName]
            ,x.[Cv]
            ,x.[ClientMessageId]
            ,x.[Payload]
            ,x.[SettleByAgent]
            ,x.[ErrorMessage]
            ,x.[RetryCount]
            ,x.[_createdDate]
    FROM    [AppDbo].[History] x;