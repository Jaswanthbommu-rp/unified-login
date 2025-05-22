CREATE TABLE [Security].[ADGroupOrganizationType](
	[ADGroupOrganizationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ADGroupId] [int] NOT NULL,
	[OrganizationTypeId] [int] NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ADGroupOrganizationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroupOrganizationType]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupOrganizationType_ADGroup] FOREIGN KEY([ADGroupId])
REFERENCES [Security].[ADGroup] ([ADGroupId])
GO

ALTER TABLE [Security].[ADGroupOrganizationType] CHECK CONSTRAINT [FK_ADGroupOrganizationType_ADGroup]
GO

ALTER TABLE [Security].[ADGroupOrganizationType]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupOrganizationType_OrganizationType] FOREIGN KEY([OrganizationTypeId])
REFERENCES [Enterprise].[OrganizationType] ([OrganizationTypeId])
GO

ALTER TABLE [Security].[ADGroupOrganizationType] CHECK CONSTRAINT [FK_ADGroupOrganizationType_OrganizationType]
GO

