CREATE TABLE [dbo].[BuildVersion]
(
[SystemInformationID] [tinyint] NOT NULL IDENTITY(1, 1),
[Database Version] [nvarchar] (25) NOT NULL,
[VersionDate] [datetime] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BuildVersion_ModifiedDate] DEFAULT (getdate())
)
GO
ALTER TABLE [dbo].[BuildVersion] ADD CONSTRAINT [PK_BuildVersion_SystemInformationID] PRIMARY KEY CLUSTERED  ([SystemInformationID])
GO
