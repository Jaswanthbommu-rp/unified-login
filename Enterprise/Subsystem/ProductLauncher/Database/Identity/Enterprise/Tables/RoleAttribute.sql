CREATE TABLE [Enterprise].[RoleAttribute](
	[RoleAttributeId] [int] IDENTITY(1,1) NOT NULL,
	[RoleAttributeTypeId] [int] NULL,
	[RoleValueTypeId] [int] NULL,
	[Value] [nvarchar](200) NULL,
 CONSTRAINT [PK_RoleAttribute] PRIMARY KEY CLUSTERED 
(
	[RoleAttributeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[RoleAttribute]  WITH CHECK ADD  CONSTRAINT [FK_RoleAttribute_RoleAttributeType] FOREIGN KEY([RoleAttributeTypeId])
REFERENCES [Enterprise].[RoleAttributeType] ([RoleAttributeTypeId])
GO

ALTER TABLE [Enterprise].[RoleAttribute] CHECK CONSTRAINT [FK_RoleAttribute_RoleAttributeType]
GO

ALTER TABLE [Enterprise].[RoleAttribute]  WITH CHECK ADD  CONSTRAINT [FK_RoleAttribute_RoleValueType] FOREIGN KEY([RoleValueTypeId])
REFERENCES [Enterprise].[RoleValueType] ([RoleValueTypeId])
GO

ALTER TABLE [Enterprise].[RoleAttribute] CHECK CONSTRAINT [FK_RoleAttribute_RoleValueType]
GO