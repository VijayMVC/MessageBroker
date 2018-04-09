CREATE PROCEDURE [App].[List-Queues]
    @Disable [dbo].[FlagType] = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
    FROM    [AppDbo].[QueueManagement_View] x
    WHERE   x.[Disable] = @Disable
END