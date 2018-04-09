CREATE PROCEDURE [App].[Set-Agent]
    @AgentName [dbo].[AgentNameType]
AS
BEGIN
    SET NOCOUNT ON;

    -- Add agent if agent does not already exist
    MERGE   [AppDbo].[AgentRegistration] as target
    USING   (SELECT @AgentName) as source ([AgentName])
    ON      (target.[AgentName] = source.[AgentName])
    WHEN NOT MATCHED THEN
        INSERT ([AgentName]) VALUES (source.[AgentName]);

    -- Return agent information
    SELECT  x.*
    FROM    [AppDbo].[AgentRegistration_View] x
    WHERE   x.[AgentName] = @AgentName;

END