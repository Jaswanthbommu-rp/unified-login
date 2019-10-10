CREATE TABLE [Enterprise].[MasterSetting](
	[MasterSettingId] [bigint] IDENTITY(1,1) NOT NULL,
	[MasterSettingTypeId] [int] NULL,
	[Value] [nvarchar](4000) NULL,
	[FromDate] [datetime] NULL,
	[ThruDate] [datetime] NULL,
 CONSTRAINT [FK_MasterSetting_MasterSettingType] FOREIGN KEY([MasterSettingTypeId]) REFERENCES [Enterprise].[MasterSettingType] ([MasterSettingTypeId]),
 CONSTRAINT [PK_MasterSetting] PRIMARY KEY CLUSTERED 
(
	[MasterSettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
) 
GO
CREATE NONCLUSTERED INDEX IDX_MasterSetting_Comp01 ON [Enterprise].[MasterSetting]
(
	[MasterSettingTypeId] ASC,
	[MasterSettingId] ASC
)
INCLUDE ( 	[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
