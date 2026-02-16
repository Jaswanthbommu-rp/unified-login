CREATE TABLE [Logging].[ActivityDetail] (
    [ActivityDetailId] BIGINT         IDENTITY (1, 1) NOT NULL,
    [ActivityId]       BIGINT         NOT NULL,
    [Key]              NVARCHAR (MAX) NULL,
    [Value]            NVARCHAR (MAX) NULL,
    [CreatedDate]      DATETIME       CONSTRAINT [DF_ActivityDetail_CreatedDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ActivityDetail_ActivityDetailId] PRIMARY KEY CLUSTERED ([ActivityDetailId] ASC),
    CONSTRAINT [FK_ActivityDetail_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Logging].[Activity] ([ActivityId])
);

GO
CREATE NONCLUSTERED INDEX IX_ActivityDetail_ActivityId
ON Logging.ActivityDetail (ActivityId);