CREATE TABLE [Enterprise].[UserSuperVisor](
	[SuperVisorId] [INT] IDENTITY(1,1) NOT NULL,
	[UserId] [BIGINT] NULL,
	[SuperVisorUserId] [BIGINT] NULL,
	CONSTRAINT [FK_UserSuperVisor_UserLogin_SuperVisorUser] FOREIGN KEY([SuperVisorUserId]) REFERENCES [Ident].[UserLogin] ([UserId]),
	CONSTRAINT [FK_UserSuperVisor_UserLogin_UserId] FOREIGN KEY([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]),
PRIMARY KEY CLUSTERED 
(
	[SuperVisorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO