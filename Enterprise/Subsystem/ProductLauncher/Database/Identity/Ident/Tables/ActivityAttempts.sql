CREATE TABLE [Ident].[ActivityAttempts](
	[ActivityAttemptsId] [int] IDENTITY(1,1) NOT NULL,
	[ActivityConfigurationId] [int] NOT NULL,
	[EnterpriseUserName] [nvarchar](200) NOT NULL,
	[AuthenticationServiceId] [nvarchar](50) NULL,
	[AttemptCount] [tinyint] NOT NULL,
	[IpAddress] [nvarchar](50) NULL,
	[BrowserType] [nvarchar](20) NULL,
	[BrowserName] [nvarchar](20) NULL,
	[Version] [nvarchar](10) NULL,
	[Platform] [nvarchar](20) NULL,
	[IsMobile] [bit] NOT NULL,
	[DeviceType] [nvarchar](20) NULL,
	[LastAttemptDateTime] [smalldatetime] NOT NULL,
	[Timezone] [nvarchar](100) NULL,
 CONSTRAINT [PK_ActivityAttempts01] PRIMARY KEY CLUSTERED 
(
	[ActivityAttemptsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Ident].[ActivityAttempts] ADD  CONSTRAINT [DF_ActivityAttempts_AttemptCount]  DEFAULT ((0)) FOR [AttemptCount]
GO

ALTER TABLE [Ident].[ActivityAttempts] ADD  CONSTRAINT [DF_ActivityAttempts_IsMobile]  DEFAULT ((0)) FOR [IsMobile]
GO

ALTER TABLE [Ident].[ActivityAttempts] ADD  CONSTRAINT [DF_ActivityAttempts_LastAttemptDateTime]  DEFAULT (getutcdate()) FOR [LastAttemptDateTime]
GO

ALTER TABLE [Ident].[ActivityAttempts]  WITH CHECK ADD  CONSTRAINT [FK_ActivityAttempts_ActivityConfiguration] FOREIGN KEY([ActivityConfigurationId])
REFERENCES [Ident].[ActivityConfiguration] ([ActivityConfigurationId])
GO

ALTER TABLE [Ident].[ActivityAttempts] CHECK CONSTRAINT [FK_ActivityAttempts_ActivityConfiguration]
GO

/****** Object:  Index [IX_ActivityAttempts_ActivityId_COmp01]    Script Date: 11/28/2018 2:28:31 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivityAttempts_ActivityId_COmp01] ON [Ident].[ActivityAttempts]
(
	[ActivityConfigurationId] ASC,
	[EnterpriseUserName] ASC,
	[LastAttemptDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX [IX_ActivityAttempts_EnterpriseUserName_LastAttemptDateTime] ON [Ident].[ActivityAttempts] ([EnterpriseUserName],[LastAttemptDateTime]) INCLUDE ([ActivityAttemptsId], [ActivityConfigurationId], [AttemptCount])
GO
CREATE NONCLUSTERED INDEX IX_ActiviyAttempt_Comp01
ON [Ident].[ActivityAttempts] ([EnterpriseUserName],[LastAttemptDateTime])
INCLUDE ([ActivityAttemptsId],[ActivityConfigurationId],[AuthenticationServiceId],[AttemptCount],[IpAddress],[BrowserType],[BrowserName],[Version],[Platform],[IsMobile],[DeviceType],[Timezone])
GO
CREATE NONCLUSTERED INDEX [IX_ActivityAttempts_LastAttemptDateTime]
ON [Ident].[ActivityAttempts] ([LastAttemptDateTime]) WITH (ONLINE = ON)
GO
