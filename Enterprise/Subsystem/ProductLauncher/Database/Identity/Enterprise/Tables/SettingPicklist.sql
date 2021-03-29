CREATE TABLE [Enterprise].[SettingPicklist](
	[PicklistID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](50) NOT NULL,
	[MappingName] [nvarchar](50) NOT NULL,
	[MappingValue] [int] NULL,
	[Description] [nvarchar](100) NULL,
	[ModifiedBy] [int] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[ThruDate] [datetime] NULL
) ON [PRIMARY]
GO