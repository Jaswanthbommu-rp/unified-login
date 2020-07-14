CREATE TABLE [Security].[RoleType](
	[RoleTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ParentRoleTypeId] int NULL,
	[Value] [nvarchar](200) NOT NULL,	
	[Description] [nvarchar](200) NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
CONSTRAINT [PK_SecurityRoleType] PRIMARY KEY CLUSTERED 
(
	[RoleTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[RoleType] ADD  CONSTRAINT [DF_SecurityRoleType_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[RoleType] ADD  CONSTRAINT [DF_SecurityRoleType_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
