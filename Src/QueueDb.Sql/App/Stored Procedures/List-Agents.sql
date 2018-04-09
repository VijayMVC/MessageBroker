CREATE PROCEDURE [App].[List-Agents]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  x.*
    FROM    [AppDbo].[AgentRegistration_View] x;

END