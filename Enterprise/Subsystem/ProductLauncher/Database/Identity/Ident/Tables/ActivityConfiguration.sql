CREATE TABLE [Ident].[ActivityConfiguration](
	[ActivityConfigurationId] [int] IDENTITY(1,1) NOT NULL,
	[PartyId] [bigint] NULL,
	[ActivityTypeId] [int] NULL,
	[MaxActivityAttemptCount] [tinyint] NULL,
	[ActivityTokenExpirationMinutes] [int] NULL,
 CONSTRAINT [PK_ActivityConfiguration] PRIMARY KEY CLUSTERED 
(
	[ActivityConfigurationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Ident].[ActivityConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_ActivityConfiguration_ActivityType] FOREIGN KEY([ActivityTypeId])
REFERENCES [Ident].[ActivityType] ([ActivityTypeId])
GO

ALTER TABLE [Ident].[ActivityConfiguration] CHECK CONSTRAINT [FK_ActivityConfiguration_ActivityType]
GO


CREATE INDEX [IX_ActivityConfiguration_PartyId_ActivityTypeId] ON [Ident].[ActivityConfiguration] ([PartyId], [ActivityTypeId]) INCLUDE ([ActivityConfigurationId], [ActivityTokenExpirationMinutes])