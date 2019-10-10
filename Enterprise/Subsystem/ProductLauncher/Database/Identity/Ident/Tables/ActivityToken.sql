
CREATE TABLE [Ident].[ActivityToken](
	[ActivityTokenId] [int] IDENTITY(1,1) NOT NULL,
	[ActivityConfigurationId] [int] NOT NULL,
	[RealPageId] [uniqueidentifier] NOT NULL,
	[ActivityToken] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreateDateTime] [smalldatetime] NOT NULL,
	[ExpireDateTime] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_ActivityToken01] PRIMARY KEY CLUSTERED 
(
	[ActivityTokenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Ident].[ActivityToken] ADD  CONSTRAINT [DF_ActivityToken_IsActive]  DEFAULT ((0)) FOR [IsActive]
GO

ALTER TABLE [Ident].[ActivityToken] ADD  CONSTRAINT [DF_ActivityToken_CreateDateTime]  DEFAULT (getutcdate()) FOR [CreateDateTime]
GO

ALTER TABLE [Ident].[ActivityToken]  WITH CHECK ADD  CONSTRAINT [FK_ActivityToken_ActivityConfiguration] FOREIGN KEY([ActivityConfigurationId])
REFERENCES [Ident].[ActivityConfiguration] ([ActivityConfigurationId])
GO

ALTER TABLE [Ident].[ActivityToken] CHECK CONSTRAINT [FK_ActivityToken_ActivityConfiguration]
GO

ALTER TABLE [Ident].[ActivityToken]  WITH CHECK ADD  CONSTRAINT [FK_ActivityToken_Party] FOREIGN KEY([RealPageId])
REFERENCES [Enterprise].[Party] ([RealPageId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [Ident].[ActivityToken] CHECK CONSTRAINT [FK_ActivityToken_Party]
GO

/****** Object:  Index [IX_ActivityToken_Comp01]    Script Date: 11/28/2018 2:30:41 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivityToken_Comp01] ON [Ident].[ActivityToken]
(
	[ActivityConfigurationId] ASC,
	[RealPageId] ASC,
	[ActivityToken] ASC,
	[IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO








