CREATE TABLE [Enterprise].[NavigationMenuSettingAccess](
	[NavigationMenuSettingAccessId] [int] IDENTITY(1,1) NOT NULL,
	[NavigationMenuId] [int] NOT NULL,
	[SettingCategoryTypeId] [smallint] NOT NULL,
	[MappingName] nvarchar(200) NOT NULL
	
 CONSTRAINT [PK_NavigationMenuSettingAccess] PRIMARY KEY CLUSTERED 
(
	[NavigationMenuSettingAccessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[NavigationMenuSettingAccess]  WITH NOCHECK ADD  CONSTRAINT [FK_NavigationMenuSettingAccess_NavigationMenu] FOREIGN KEY([NavigationMenuId])
REFERENCES [Enterprise].[NavigationMenu] ([Id])
GO

ALTER TABLE [Enterprise].[NavigationMenuSettingAccess] CHECK CONSTRAINT [FK_NavigationMenuSettingAccess_NavigationMenu]
GO

ALTER TABLE [Enterprise].[NavigationMenuSettingAccess]   WITH NOCHECK ADD  CONSTRAINT [FK_NavigationMenuSettingAccess_SettingsCategoryType] FOREIGN KEY([SettingCategoryTypeId])
REFERENCES Settings.SettingCategoryType ([SettingCategoryTypeId])
GO

ALTER TABLE [Enterprise].[NavigationMenuSettingAccess] CHECK CONSTRAINT [FK_NavigationMenuSettingAccess_SettingsCategoryType]
GO