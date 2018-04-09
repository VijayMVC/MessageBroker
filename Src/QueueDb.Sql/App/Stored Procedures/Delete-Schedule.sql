CREATE PROCEDURE [App].[Delete-Schedule]
    @ScheduleId [dbo].[IdType]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE  [AppDbo].[Schedule]
    SET     [_deletedDate] = GETUTCDATE()
    WHERE   [ScheduleId] = @ScheduleId;

END