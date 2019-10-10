CREATE TABLE [Auth].[CertificatesType] (
    [CertificatesTypeId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]               NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_CertificatesType] PRIMARY KEY CLUSTERED ([CertificatesTypeId] ASC) WITH (FILLFACTOR = 80)
);

