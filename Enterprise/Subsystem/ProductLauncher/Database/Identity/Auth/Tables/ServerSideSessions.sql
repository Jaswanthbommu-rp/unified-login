CREATE TABLE [Auth].[ServerSideSessions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Scheme] [nvarchar](100) NOT NULL,
	[SubjectId] [nvarchar](100) NOT NULL,
	[SessionId] [nvarchar](100) NULL,
	[DisplayName] [nvarchar](100) NULL,
	[Created] [datetime2](7) NOT NULL,
	[Renewed] [datetime2](7) NOT NULL,
	[Expires] [datetime2](7) NULL,
	[Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ServerSideSessions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Index [IX_ServerSideSessions_DisplayName]    Script Date: 6/12/2023 11:06:00 AM ******/
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_DisplayName] ON [Auth].[ServerSideSessions]
(
	[DisplayName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

/****** Object:  Index [IX_ServerSideSessions_Expires]    Script Date: 6/12/2023 11:06:17 AM ******/
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_Expires] ON [Auth].[ServerSideSessions]
(
	[Expires] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


/****** Object:  Index [IX_ServerSideSessions_Key]    Script Date: 6/12/2023 11:06:35 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ServerSideSessions_Key] ON [Auth].[ServerSideSessions]
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_ServerSideSessions_SessionId]    Script Date: 6/12/2023 11:06:47 AM ******/
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_SessionId] ON [Auth].[ServerSideSessions]
(
	[SessionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_ServerSideSessions_SubjectId]    Script Date: 6/12/2023 11:06:58 AM ******/
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_SubjectId] ON [Auth].[ServerSideSessions]
(
	[SubjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
