CREATE TABLE [AppDbo].[QueueConfiguration] (
    [QueueName]         [dbo].[QueueNameType] NOT NULL,
    [QueueSize]         [dbo].[SizeType]      NULL,
    [LastProcessedDate] [dbo].[DateType]      NULL,
    CONSTRAINT [PK_QueueConfiguration] PRIMARY KEY CLUSTERED ([QueueName] ASC)
);







