CREATE TABLE [Enterprise].[MasterConfigurationSetting](
	[MasterConfigurationSettingId] [bigint] IDENTITY(1,1) NOT NULL,
	[MasterConfigurationId] [bigint] NULL,
	[MasterSettingId] [bigint] NULL,
	[ConfigurationId] [bigint] NULL,
 CONSTRAINT [FK_MasterConfigurationSetting_MasterConfiguration] FOREIGN KEY([MasterConfigurationId]) REFERENCES [Enterprise].[MasterConfiguration] ([MasterConfigurationId]),
 CONSTRAINT [FK_MasterConfigurationSetting_MasterSetting] FOREIGN KEY([MasterSettingId]) REFERENCES [Enterprise].[MasterSetting] ([MasterSettingId]),
 CONSTRAINT [PK_MasterConfigurationSetting] PRIMARY KEY CLUSTERED 
(
	[MasterConfigurationSettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO
CREATE NONCLUSTERED INDEX IDX_MasterConfigurationSetting_Comp01 ON [Enterprise].[MasterConfigurationSetting]
(
	[MasterSettingId] ASC,
	[MasterConfigurationId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX IDX_MasterConfigurationSetting_MasterSettingId ON [Enterprise].[MasterConfigurationSetting]([MasterSettingId]) INCLUDE([MasterConfigurationId]);
GO
CREATE INDEX [IX_MasterConfigurationSetting_Comp02]
ON [Enterprise].[MasterConfigurationSetting] ([MasterConfigurationId])
INCLUDE ([MasterSettingId]);
