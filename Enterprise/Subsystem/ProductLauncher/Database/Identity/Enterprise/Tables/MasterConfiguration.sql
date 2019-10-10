CREATE TABLE [Enterprise].[MasterConfiguration](
	[MasterConfigurationId] [bigint] IDENTITY(1,1) NOT NULL,
	[MasterConfigurationTypeId] [int] NULL,
	[AttributeId] [bigint] NULL,
	[FromDate] [datetime] NULL,
	[ThruDate] [datetime] NULL,
	CONSTRAINT [FK_MasterConfiguration_MasterConfigurationType] FOREIGN KEY([MasterConfigurationTypeId]) REFERENCES [Enterprise].[MasterConfigurationType] ([MasterConfigurationTypeId]),
 CONSTRAINT [PK_MasterConfiguration] PRIMARY KEY CLUSTERED 
(
	[MasterConfigurationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
) 
GO
CREATE NONCLUSTERED INDEX IDX_MasterConfiguration_Comp02 ON [Enterprise].[MasterConfiguration]
(
	[MasterConfigurationId] DESC,
	[AttributeId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX IDX_MasterCOnfiguration_AttributeId ON [Enterprise].[MasterConfiguration]
(
	[AttributeId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE INDEX [IX_MasterConfiguration_MasterConfigurationTypeId] ON [Enterprise].[MasterConfiguration] ([MasterConfigurationTypeId]) INCLUDE ([MasterConfigurationId], [AttributeId], [FromDate], [ThruDate])
