CREATE TABLE [Auth].[CertificatesType]
(
[CertificatesTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Auth].[CertificatesType] ADD CONSTRAINT [PK_CertificatesType] PRIMARY KEY CLUSTERED  ([CertificatesTypeId])
GO
