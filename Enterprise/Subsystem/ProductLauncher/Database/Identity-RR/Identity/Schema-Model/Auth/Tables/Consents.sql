CREATE TABLE [Auth].[Consents]
(
[SubjectCode] [nvarchar] (200) NOT NULL,
[ClientCode] [nvarchar] (200) NOT NULL,
[Scopes] [nvarchar] (2000) NOT NULL
)
GO
ALTER TABLE [Auth].[Consents] ADD CONSTRAINT [PK_Consents] PRIMARY KEY CLUSTERED  ([SubjectCode], [ClientCode])
GO
