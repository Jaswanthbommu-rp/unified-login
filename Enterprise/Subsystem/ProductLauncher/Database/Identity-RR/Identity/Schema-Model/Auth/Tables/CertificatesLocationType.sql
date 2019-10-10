CREATE TABLE [Auth].[CertificatesLocationType]
(
[CertificatesLocationTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Auth].[CertificatesLocationType] ADD CONSTRAINT [PK_CertificatesLocationType] PRIMARY KEY CLUSTERED  ([CertificatesLocationTypeId])
GO
