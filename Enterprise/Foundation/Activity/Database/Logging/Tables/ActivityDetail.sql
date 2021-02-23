CREATE TABLE [Logging].[ActivityDetail] (
    [ActivityDetailId] BIGINT          IDENTITY (1, 1) NOT NULL,
    [ActivityId]       BIGINT          NOT NULL,
    [Key]              NVARCHAR (MAX)   NULL,
    [Value]            NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ActivityDetail] PRIMARY KEY CLUSTERED ([ActivityDetailId] ASC),
    CONSTRAINT [FK_ActivityDetail_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Logging].[Activity] ([ActivityId])
);



