CREATE TABLE [Enterprise].[UserSyncJobType](
       [UserSyncJobTypeId] [tinyint] NOT NULL,
       [KafkaTopicName] varchar(256) not null,
       [Name] [varchar](50) NOT NULL,
       [Description] [varchar](100) NULL,
CONSTRAINT [PK_UserSyncJob_Type] PRIMARY KEY CLUSTERED 
(
       [UserSyncJobTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO