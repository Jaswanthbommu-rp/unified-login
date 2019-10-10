CREATE TABLE [dbo].[ErrorLog]
(
[ErrorLogID] [int] NOT NULL IDENTITY(1, 1),
[ErrorTime] [datetime] NOT NULL CONSTRAINT [DF_ErrorLog_ErrorTime] DEFAULT (getdate()),
[UserName] [sys].[sysname] NOT NULL,
[ErrorNumber] [int] NOT NULL,
[ErrorSeverity] [int] NULL,
[ErrorState] [int] NULL,
[ErrorProcedure] [nvarchar] (126) NULL,
[ErrorLine] [int] NULL,
[ErrorMessage] [nvarchar] (4000) NOT NULL
)
GO
ALTER TABLE [dbo].[ErrorLog] ADD CONSTRAINT [PK_ErrorLog_ErrorLogID] PRIMARY KEY CLUSTERED  ([ErrorLogID])
GO
