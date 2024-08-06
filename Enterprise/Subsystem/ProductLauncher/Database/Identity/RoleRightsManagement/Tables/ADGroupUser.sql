CREATE TABLE [Security].[ADGroupUser](
	[ADGroupUserId] [int] IDENTITY(1,1) NOT NULL,
	[ADGroupId] [int] NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL
 CONSTRAINT [PK_ADGroupUser] PRIMARY KEY CLUSTERED 
(
	[ADGroupUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroupUser] ADD  CONSTRAINT [DF_ADGroupUser_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[ADGroupUser]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupUser_ADGroup] FOREIGN KEY([ADGroupId])
REFERENCES [Security].[ADGroup] ([ADGroupId])
GO

ALTER TABLE [Security].[ADGroupUser]  CHECK CONSTRAINT [FK_ADGroupUser_ADGroup]
GO

ALTER TABLE [Security].[ADGroupUser] WITH CHECK ADD  CONSTRAINT [FK_ADGroupUser_Persona] FOREIGN KEY([PersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO

ALTER TABLE [Security].[ADGroupUser] CHECK CONSTRAINT [FK_ADGroupUser_Persona]
GO

CREATE NONCLUSTERED INDEX [IDX_ADGroupUser_PersonaId] ON [Security].[ADGroupUser] ([PersonaId])
GO

