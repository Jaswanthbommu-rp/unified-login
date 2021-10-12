CREATE TABLE [Security].[ADGroupProductRole]
(
	[ADGroupProductRoleId] INT IDENTITY(1,1) PRIMARY KEY,
	[ADGroupID] INT NOT NULL,
	[ProductId] INT NOT NULL,
	[RoleName] NVARCHAR(400) NOT NULL,
	[IsAdminRole] TINYINT DEFAULT(0) NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL
)
GO

ALTER TABLE [Security].[ADGroupProductRole] ADD  CONSTRAINT [DF_ADGroupProductRole_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[ADGroupProductRole]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupProductRole_ADGroup] FOREIGN KEY([ADGroupID])
REFERENCES [Security].[ADGroup] ([ADGroupID])

GO

ALTER TABLE [Security].[ADGroupProductRole]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupProductRole_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])

