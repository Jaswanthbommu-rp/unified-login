CREATE TABLE [Settings].[SettingTableRowValue]
(
	[SettingTableRowValueId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserLoginPersonaId] [bigint] NOT NULL,
	[SettingTableRowId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NULL,	
	[ModifiedBy] [bigint] NOT NULL,
	[CreatedDate] [datetime] DEFAULT (getutcdate()) NOT NULL, 
	[UpdatedDate] [datetime]  NULL,
 CONSTRAINT [PK_SettingTableRowValue] PRIMARY KEY CLUSTERED 
(
	[SettingTableRowValueId] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Settings].[SettingTableRowValue]  WITH CHECK ADD  CONSTRAINT [FK_SettingTableRowValue_Row] FOREIGN KEY([SettingTableRowId])
REFERENCES [Settings].[SettingTableRow] ([SettingTableRowId])
GO

ALTER TABLE [Settings].[SettingTableRowValue] CHECK CONSTRAINT [FK_SettingTableRowValue_Row]
GO