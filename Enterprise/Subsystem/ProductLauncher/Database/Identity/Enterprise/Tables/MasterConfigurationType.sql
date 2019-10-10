CREATE TABLE [Enterprise].[MasterConfigurationType](
	[MasterConfigurationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_MasterConfigurationType] PRIMARY KEY CLUSTERED 
(
	[MasterConfigurationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
) 