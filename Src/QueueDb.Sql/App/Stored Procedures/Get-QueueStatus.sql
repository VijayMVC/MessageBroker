CREATE PROCEDURE [App].[Get-QueueStatus]
AS
BEGIN
    SELECT  x.[QueueName]
            ,'QueueCount' = (SELECT count(*) FROM [AppDbo].[ActiveQueue] i1 WHERE i1.[QueueId] = x.[QueueId] AND i1.[_deletedDate] is null)
            ,x.[CurrentSizeLimit]
            ,'QueueSize' = (SELECT count(*) FROM [AppDbo].[ActiveQueue] i2 WHERE i2.[QueueId] = x.[QueueId])
    FROM    [AppDbo].[QueueManagement_View] x

END