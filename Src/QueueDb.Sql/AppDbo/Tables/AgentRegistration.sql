CREATE TABLE [AppDbo].[AgentRegistration] (
    [AgentId]      [dbo].[KeyIdType]     IDENTITY (1, 1) NOT NULL,
    [AgentName]    [dbo].[AgentNameType] NOT NULL,
    [_createdDate] [dbo].[DateType]      CONSTRAINT [DF_AgentRegistration__createdDate] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_AgentRegistration] PRIMARY KEY CLUSTERED ([AgentId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AgentRegistration]
    ON [AppDbo].[AgentRegistration]([AgentName] ASC);

