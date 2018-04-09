CREATE PROCEDURE [App].[Disable-Queue]
    @QueueName [dbo].[QueueNameType],
    @Disable [dbo].[FlagType]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE  [AppDbo].[QueueManagement]
    SET     [Disable] = @Disable
    WHERE   [QueueName] = @QueueName;

END