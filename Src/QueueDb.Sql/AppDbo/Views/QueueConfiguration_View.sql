CREATE VIEW [AppDbo].[QueueConfiguration_View]
AS
    SElECT  x.[QueueName]
            ,x.[QueueSize]
            ,x.[LastProcessedDate]
    FROM    [AppDbo].[QueueConfiguration] x;