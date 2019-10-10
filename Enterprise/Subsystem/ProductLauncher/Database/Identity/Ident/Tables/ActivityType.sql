CREATE TABLE [Ident].[ActivityType] (
    [ActivityTypeId]                     INT           NOT NULL,
    [ActivityCode]                   VARCHAR (50)  NOT NULL,
    [Description]                    VARCHAR (100) NULL
    CONSTRAINT [PK_ActivityType01] PRIMARY KEY CLUSTERED ([ActivityTypeId] ASC)
);

