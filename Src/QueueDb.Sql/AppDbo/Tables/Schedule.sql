CREATE TABLE [AppDbo].[Schedule] (
    [RowId]            [dbo].[IdType]              IDENTITY (1, 1) NOT NULL,
    [ScheduleId]       [dbo].[IdType]              NOT NULL,
    [QueueId]          [dbo].[KeyIdType]           NOT NULL,
    [ClientMessageId]  [dbo].[ClientMessageIdType] NULL,
    [Cv]               [dbo].[CvType]              NULL,
    [Payload]          [dbo].[PayloadType]         NULL,
    [ScheduleDate]     [dbo].[DateType]            NULL,
    [CreatedByAgentId] [dbo].[KeyIdType]           NOT NULL,
    [_createdDate]     [dbo].[DateType]            CONSTRAINT [DF__ScheduleQueue_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [_deletedDate]     [dbo].[DateType]            NULL,
    CONSTRAINT [PK_Schedule_1] PRIMARY KEY CLUSTERED ([RowId] ASC),
    CONSTRAINT [FK_Schedule_AgentRegistration] FOREIGN KEY ([CreatedByAgentId]) REFERENCES [AppDbo].[AgentRegistration] ([AgentId]),
    CONSTRAINT [FK_ScheduleQueue_QueueManagement] FOREIGN KEY ([QueueId]) REFERENCES [AppDbo].[QueueManagement] ([QueueId])
);

GO
CREATE NONCLUSTERED INDEX [IX_ScheduleQueue_QueueId]
    ON [AppDbo].[Schedule]([QueueId] ASC, [_deletedDate] ASC, [ScheduleDate] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Schedule_ScheduleId]
    ON [AppDbo].[Schedule]([ScheduleId] ASC);

