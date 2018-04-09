CREATE TABLE [AppDbo].[History] (
    [RowId]           [dbo].[IdType]              IDENTITY (0, 1) NOT NULL,
    [HistoryId]       [dbo].[IdType]              NOT NULL,
    [MessageId]       [dbo].[IdType]              NOT NULL,
    [ActivityType]    [dbo].[ActivityTypeType]    NOT NULL,
    [QueueName]       [dbo].[QueueNameType]       NULL,
    [Cv]              [dbo].[CvType]              NULL,
    [ClientMessageId] [dbo].[ClientMessageIdType] NULL,
    [Payload]         [dbo].[PayloadType]         NULL,
    [SettleByAgent]   [dbo].[AgentNameType]       NULL,
    [ErrorMessage]    [dbo].[ErrorMessageType]    NULL,
    [RetryCount]      [dbo].[SizeType]            NULL,
    [_createdDate]    [dbo].[DateType]            CONSTRAINT [DF_History__createdDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_History] PRIMARY KEY CLUSTERED ([RowId] ASC),
    CONSTRAINT [CK_History] CHECK ([ActivityType]='Processed' OR [ActivityType]='Rejected' OR [ActivityType]='RetryLimitExceeded' OR [ActivityType]='Deleted')
);

GO
CREATE NONCLUSTERED INDEX [IX_History_MessageId]
    ON [AppDbo].[History]([MessageId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_History_HistoryId]
    ON [AppDbo].[History]([HistoryId] ASC);

