CREATE PROCEDURE [App].[List-Schedules]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
    FROM    [AppDbo].[Schedule_View] x
            inner join [AppDbo].[QueueManagement_View] q on x.[QueueId] = q.[QueueId]
    WHERE   q.[QueueName] = @QueueName;

END