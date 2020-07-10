CREATE TYPE [dbo].[ProductConfigurationType] AS TABLE(
	[SettingName] [nvarchar](50) NULL,
	[SettingDescription] [nvarchar](100) NULL,
	[SettingValue] [nvarchar](1000) NULL,
	[SettingSensitiveData] [tinyint] NULL
)
GO