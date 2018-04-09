CREATE TABLE [AppDbo].[QueueManagement] (
    [QueueId]           [dbo].[KeyIdType]     IDENTITY (1, 1) NOT NULL,
    [QueueName]         [dbo].[QueueNameType] NOT NULL,
    [CurrentSizeLimit]  [dbo].[SizeType]      CONSTRAINT [DF_QueueManagement_CurrentSizeLimit] DEFAULT ((10000)) NOT NULL,
    [CurrentRetryLimit] [dbo].[SizeType]      CONSTRAINT [DF_QueueManagement_CurrentRetryLimit] DEFAULT ((3)) NOT NULL,
    [Disable]           [dbo].[FlagType]      CONSTRAINT [DF_QueueManagement_Disable] DEFAULT ((0)) NOT NULL,
    [LockValidForSec]   [dbo].[SizeType]      CONSTRAINT [DF_QueueManagement_LockValidFor] DEFAULT ((300)) NOT NULL,
    [_deletedDate]       [dbo].[DateType]      NULL,
    CONSTRAINT [PK_QueueManagement] PRIMARY KEY CLUSTERED ([QueueId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_QueueManagement]
    ON [AppDbo].[QueueManagement]([QueueName] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_QueueManagement_deletedDate]
    ON [AppDbo].[QueueManagement]([QueueName] ASC, [_deletedDate] ASC)
    INCLUDE ([CurrentSizeLimit], [CurrentRetryLimit], [Disable], [LockValidForSec]);

GO
