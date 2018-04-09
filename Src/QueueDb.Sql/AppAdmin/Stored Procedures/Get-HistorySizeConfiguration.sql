CREATE PROCEDURE [AppAdmin].[Get-HistorySizeConfiguration]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LoggingQueueName [dbo].[QueueNameType] = 'History';

    SELECT  x.*
    FROM    [AppDbo].[QueueConfiguration_View] x
    WHERE   x.[QueueName] = @LoggingQueueName;
END
