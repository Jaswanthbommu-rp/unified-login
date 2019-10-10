CREATE TABLE [Auth].[CertificatesLocationType] (
    [CertificatesLocationTypeId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]                       NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_CertificatesLocationType] PRIMARY KEY CLUSTERED ([CertificatesLocationTypeId] ASC) WITH (FILLFACTOR = 80)
);

