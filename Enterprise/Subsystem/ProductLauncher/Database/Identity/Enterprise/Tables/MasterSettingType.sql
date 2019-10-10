CREATE TABLE [Enterprise].[MasterSettingType](
	[MasterSettingTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ParentMasterSettingTypeId] [int] NULL,
	[Name] [nvarchar](100) NULL,
	[MasterConfigurationTypeId] [int] NULL,
	CONSTRAINT [FK_MasterSettingType_MasterConfigurationType] FOREIGN KEY([MasterConfigurationTypeId]) REFERENCES [Enterprise].[MasterConfigurationType] ([MasterConfigurationTypeId]),
	CONSTRAINT [FK_MasterSettingType_MasterSettingType] FOREIGN KEY([ParentMasterSettingTypeId]) REFERENCES [Enterprise].[MasterSettingType] ([MasterSettingTypeId]),
 CONSTRAINT [PK_MasterSettingType] PRIMARY KEY CLUSTERED 
(
	[MasterSettingTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

