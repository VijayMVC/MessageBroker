CREATE TABLE [AppDbo].[ActiveQueue] (
    [RowId]            [dbo].[IdType]              IDENTITY (1, 1) NOT NULL,
    [MessageId]        [dbo].[IdType]              NOT NULL,
    [QueueId]          [dbo].[KeyIdType]           NOT NULL,
    [ClientMessageId]  [dbo].[ClientMessageIdType] NULL,
    [Cv]               [dbo].[CvType]              NULL,
    [Payload]          [dbo].[PayloadType]         NULL,
    [LockedDate]       [dbo].[DateType]            NULL,
    [LockedByAgentId]  [dbo].[KeyIdType]           NULL,
    [SettleByAgentId]  [dbo].[KeyIdType]           NULL,
    [RetryCount]       [dbo].[SizeType]            CONSTRAINT [DF_Current_ProcessedCount] DEFAULT ((0)) NOT NULL,
    [CreatedByAgentId] [dbo].[KeyIdType]           NOT NULL,
    [_createdDate]     [dbo].[DateType]            CONSTRAINT [DF_Current__createdDate] DEFAULT (getutcdate()) NOT NULL,
    [_deletedDate]     [dbo].[DateType]            NULL,
    CONSTRAINT [PK_Current] PRIMARY KEY CLUSTERED ([RowId] ASC),
    CONSTRAINT [FK_Current_AgentRegistration] FOREIGN KEY ([LockedByAgentId]) REFERENCES [AppDbo].[AgentRegistration] ([AgentId]),
    CONSTRAINT [FK_Current_AgentRegistration1] FOREIGN KEY ([CreatedByAgentId]) REFERENCES [AppDbo].[AgentRegistration] ([AgentId]),
    CONSTRAINT [FK_Current_AgentRegistration2] FOREIGN KEY ([SettleByAgentId]) REFERENCES [AppDbo].[AgentRegistration] ([AgentId]),
    CONSTRAINT [FK_Current_QueueManagement] FOREIGN KEY ([QueueId]) REFERENCES [AppDbo].[QueueManagement] ([QueueId])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Current_MessageId]
    ON [AppDbo].[ActiveQueue]([MessageId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Current_deletedDate]
    ON [AppDbo].[ActiveQueue]([_deletedDate] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Current_LockedDate]
    ON [AppDbo].[ActiveQueue]([QueueId] ASC, [LockedDate] ASC)
    INCLUDE([RetryCount])
    WHERE [_deletedDate] IS NULL;
GO
