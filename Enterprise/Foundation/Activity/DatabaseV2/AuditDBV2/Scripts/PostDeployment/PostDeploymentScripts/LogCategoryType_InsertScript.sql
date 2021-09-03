

--[Logging].[LogCategoryType] data

GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'security') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '1','security','security activities like login, logout etc.' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'user') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '2','user','user activities like create, update etc.' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'productaccess') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '3','productaccess','product related activities like product login, create/update user in product etc.' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'email') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '4','email','notification related activities.' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Migration') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '5','Migration','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Settings') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '6','Settings','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'CIMPL') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '7','CIMPL','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Company or Property') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '8','Company or Property','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Global Update') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '9','Global Update','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Templates') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '10','Templates','' )
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Admin Console') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '11','Admin Console','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'CompanySetup') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '12','CompanySetup','Company activities, such as create and product enablement' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Internal Settings') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '13','Internal Settings','' ) 
END 
GO

IF NOT EXISTS (SELECT 1 FROM [Logging].[LogCategoryType] WHERE [Name] = 'Internal Templates') 
BEGIN 
INSERT [Logging].[LogCategoryType] (LogCategoryTypeId, [Name], [Description]) VALUES ( '14','Internal Templates','' ) 
END 
GO


