CREATE PROCEDURE [App].[Set-Queue]
    @QueueName [dbo].[QueueNameType],
    @CurrentSizeLimit [dbo].[SizeType] = 10000,
    @CurrentRetryLimit [dbo].[SizeType] = 3,
    @LockValidForSec [dbo].[SizeType] = 300
AS
BEGIN
    SET NOCOUNT ON;

    MERGE   [AppDbo].[QueueManagement] as target
    USING   (SELECT @QueueName, @CurrentSizeLimit, @CurrentRetryLimit, @LockValidForSec) as source
            ([QueueName], [CurrentSizeLimit], [CurrentRetryLimit], [LockValidForSec])
    ON      (target.[QueueName] = source.[QueueName])
    WHEN MATCHED THEN
        UPDATE SET [CurrentSizeLimit] = source.[CurrentSizeLimit]
                    ,[CurrentRetryLimit] = source.[CurrentRetryLimit]
                    ,[LockValidForSec] = source.[LockValidForSec]
                    ,[_deletedDate] = null
    WHEN NOT MATCHED THEN
        INSERT (
            [QueueName]
            ,[CurrentSizeLimit]
            ,[CurrentRetryLimit]
            ,[LockValidForSec]
            )
        VALUES
        (
            source.[QueueName]
            ,source.[CurrentSizeLimit]
            ,source.[CurrentRetryLimit]
            ,source.[LockValidForSec]
            );
END
