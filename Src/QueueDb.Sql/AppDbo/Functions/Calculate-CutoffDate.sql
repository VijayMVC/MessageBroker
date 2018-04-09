CREATE FUNCTION [AppDbo].[Calculate-CutoffDate]
(
    @lockValidForSec int
)
RETURNS [dbo].[DateType]
AS
BEGIN
    DECLARE @currentDate [dbo].[DateType] = GETUTCDATE();
    DECLARE @cutoffDate [dbo].[DateType] = DATEADD(second, -@lockValidForSec, @currentDate);

    RETURN @cutoffDate;
END
