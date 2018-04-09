CREATE VIEW [AppDbo].[AgentRegistration_View]
AS
    SELECT  x.[AgentId]
            ,x.[AgentName]
            ,x.[_createdDate]
    FROM    [AppDbo].[AgentRegistration] x;