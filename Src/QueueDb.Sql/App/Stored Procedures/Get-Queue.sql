CREATE PROCEDURE [App].[Get-Queue]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
            ,(SELECT count(*) FROM [AppDbo].[ActiveQueue_View] c WHERE c.[QueueId] = x.[QueueId]) as 'QueueLength'
            ,(SELECT count(*) FROM [AppDbo].[Schedule_View] s WHERE s.[QueueId] = x.[QueueId]) as 'ScheduleQueueLength'
    FROM    [AppDbo].[QueueManagement_View] x
    WHERE   x.[QueueName] = @QueueName
    AND     x.[Disable] = 0;
END