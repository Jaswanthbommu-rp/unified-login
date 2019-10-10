CREATE TABLE [Ident].[ActivityType] (
    [ActivityTypeId] TINYINT       IDENTITY (1, 1) NOT NULL,
    [ActiityName]    NVARCHAR (50) NULL,
    CONSTRAINT [PK_ActivityType] PRIMARY KEY CLUSTERED ([ActivityTypeId] ASC)
);

