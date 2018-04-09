CREATE PROCEDURE [App].[Get-History]
    @MessageId [dbo].[IdType]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
    FROM    [AppDbo].[History_View] x
    WHERE   x.[MessageId] = @MessageId;

END