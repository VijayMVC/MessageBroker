CREATE PROCEDURE [App].[List-ActiveMessages]
    @QueueName [dbo].[QueueNameType]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
    FROM    [AppDbo].[ActiveQueue_View] x
            inner join [AppDbo].[QueueManagement] m on x.[QueueId] = m.[QueueId]
                and m.[Disable] = 0;

END