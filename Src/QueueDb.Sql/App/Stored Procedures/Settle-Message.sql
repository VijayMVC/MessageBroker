CREATE PROCEDURE [App].[Settle-Message]
    @MessageId [dbo].[IdType]
    ,@AgentId [dbo].[KeyIdType]
    ,@SettleType [dbo].[ActivityTypeType]
    ,@ErrorMessage [dbo].[ErrorMessageType] = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Settle types are for messages that have been locked by an agent
    -- 'Processed' - message has been processed, move to history
    -- 'Abandon' - message has been abandon by the agent, release lock
    -- 'Rejected' - message cannot be processed, move to history

    DECLARE @messageIds [AppDbo].[MessageId_Table];

    BEGIN TRAN;

    UPDATE  [AppDbo].[ActiveQueue]
    SET     [SettleByAgentId] = @AgentId
            ,[_deletedDate] = GETUTCDATE()
    OUTPUT  inserted.[MessageId] INTO @messageIds ([MessageId])
    WHERE   [MessageId] = @MessageId
    AND     [LockedByAgentId] = @AgentId
    AND     [_deletedDate] IS NULL;

    IF (SELECT COUNT(*) FROM @messageIds) = 0
    BEGIN
        -- Nothing to do
        COMMIT TRAN;
        RETURN;
    END

    IF @SettleType = 'Processed'
    BEGIN
        EXEC [AppDbo].[CopyTo-History] @MessageIds = @messageIds, @ActivityType = 'Processed', @ErrorMessage = @ErrorMessage;
        COMMIT TRAN;
        RETURN;
    END

    IF @SettleType = 'Abandon'
    BEGIN
        UPDATE  [AppDbo].[ActiveQueue]
        SET     [LockedDate] = NULL
                ,[LockedByAgentId] = NULL
                ,[_deletedDate] = NULL
        WHERE   [MessageId] = @MessageId

        COMMIT TRAN;
        RETURN;
    END

    IF @SettleType = 'Rejected'
    BEGIN
        EXEC [AppDbo].[CopyTo-History] @MessageIds = @messageIds, @ActivityType = 'Rejected', @ErrorMessage = @ErrorMessage;
        COMMIT TRAN;
        RETURN;
    END

    -- Should never get here
    RAISERROR ('Unknown settle type %s', 17, 1, @SettleType);
    ROLLBACK TRAN;
END