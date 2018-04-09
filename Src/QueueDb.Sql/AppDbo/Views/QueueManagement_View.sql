CREATE VIEW [AppDbo].[QueueManagement_View]
AS
    SELECT  x.[QueueId]
            ,x.[QueueName]
            ,x.[CurrentSizeLimit]
            ,x.[CurrentRetryLimit]
            ,x.[LockValidForSec]
            ,x.[Disable]
    FROM    [AppDbo].[QueueManagement] x
    WHERE   x.[_deletedDate] is null;
