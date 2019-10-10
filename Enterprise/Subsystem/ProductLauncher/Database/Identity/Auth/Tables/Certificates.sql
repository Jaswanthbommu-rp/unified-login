CREATE TABLE [Auth].[Certificates] (
    [CertificatesId]             INT             IDENTITY (1, 1) NOT NULL,
    [CertificatesTypeId]         INT             NOT NULL,
    [CertificatesLocationTypeID] INT             NOT NULL,
    [Name]                       NVARCHAR (100)  NOT NULL,
    [Description]                NVARCHAR (4000) NULL,
    [SubjectName]                NVARCHAR (1024) NULL,
    [CreatedDate]                DATETIME        CONSTRAINT [DF_Certificates_CreatedDate] DEFAULT (getutcdate()) NOT NULL,
    [ModifiedDate]               DATETIME        CONSTRAINT [DF_Certificates_ModifiedDate] DEFAULT (getutcdate()) NOT NULL,
    [Thumbprint]                 NVARCHAR (100)  NULL,
    [Issuer]                     NVARCHAR (100)  NULL,
    [RawData]                    VARBINARY (MAX) NULL,
    [HasPassword]                BIT             CONSTRAINT [DF_Certificates_HasPassword] DEFAULT ((0)) NOT NULL,
    [Password]                   VARBINARY (MAX) NULL,
    [CertificateExpirationDate]  DATETIME        NULL,
    [CertificateStartDate]       DATETIME        NULL,
    [HasPrivateKey]              BIT             CONSTRAINT [DF_Certificates_HasPrivateKey] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Certificates] PRIMARY KEY CLUSTERED ([CertificatesId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_Certificates_CertificatesLocationType] FOREIGN KEY ([CertificatesLocationTypeID]) REFERENCES [Auth].[CertificatesLocationType] ([CertificatesLocationTypeId]),
    CONSTRAINT [FK_Certificates_CertificatesType] FOREIGN KEY ([CertificatesTypeId]) REFERENCES [Auth].[CertificatesType] ([CertificatesTypeId])
);

