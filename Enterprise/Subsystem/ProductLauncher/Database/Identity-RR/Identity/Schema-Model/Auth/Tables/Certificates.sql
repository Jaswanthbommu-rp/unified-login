CREATE TABLE [Auth].[Certificates]
(
[CertificatesId] [int] NOT NULL IDENTITY(1, 1),
[CertificatesTypeId] [int] NOT NULL,
[CertificatesLocationTypeID] [int] NOT NULL,
[Name] [nvarchar] (100) NOT NULL,
[Description] [nvarchar] (4000) NULL,
[SubjectName] [nvarchar] (1024) NULL,
[CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_Certificates_CreatedDate] DEFAULT (getutcdate()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Certificates_ModifiedDate] DEFAULT (getutcdate()),
[Thumbprint] [nvarchar] (100) NULL,
[Issuer] [nvarchar] (100) NULL,
[RawData] [varbinary] (max) NULL,
[HasPassword] [bit] NOT NULL CONSTRAINT [DF_Certificates_HasPassword] DEFAULT ((0)),
[Password] [varbinary] (max) NULL,
[CertificateExpirationDate] [datetime] NULL,
[CertificateStartDate] [datetime] NULL,
[HasPrivateKey] [bit] NOT NULL CONSTRAINT [DF_Certificates_HasPrivateKey] DEFAULT ((0))
)
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [PK_Certificates] PRIMARY KEY CLUSTERED  ([CertificatesId])
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [FK_Certificates_CertificatesLocationType] FOREIGN KEY ([CertificatesLocationTypeID]) REFERENCES [Auth].[CertificatesLocationType] ([CertificatesLocationTypeId])
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [FK_Certificates_CertificatesType] FOREIGN KEY ([CertificatesTypeId]) REFERENCES [Auth].[CertificatesType] ([CertificatesTypeId])
GO
