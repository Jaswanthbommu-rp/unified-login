CREATE TABLE [Enterprise].[OrganizationAddress]
(
    [CompanyPartyId] BIGINT NOT NULL,
    [Address] NVARCHAR(255) NULL,
    [City] NVARCHAR(100) NULL,
    [State] NVARCHAR(50) NULL,
    [PostalCode] NVARCHAR(20) NULL,
    [County] NVARCHAR(100) NULL,
    [Country] NVARCHAR(100) NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME NULL,
    CONSTRAINT [FK_OrganizationAddress_Organization] 
        FOREIGN KEY ([CompanyPartyId]) 
        REFERENCES [Enterprise].[Organization]([PartyId]),
    CONSTRAINT [UQ_OrganizationAddress_CompanyPartyId] 
        UNIQUE ([CompanyPartyId]) -- ensures one address per company
);

GO
-- Covering index for SP lookups by CompanyPartyId
CREATE NONCLUSTERED INDEX IX_OrganizationAddress_CompanyPartyId
ON Enterprise.OrganizationAddress (CompanyPartyId)
INCLUDE (Address, City, State, PostalCode, County, Country, CreatedDate, ModifiedDate);

-- Covering index for PostalCode lookups
CREATE NONCLUSTERED INDEX IX_OrganizationAddress_PostalCode
ON Enterprise.OrganizationAddress (PostalCode)
INCLUDE (CompanyPartyId, City, State, Country);

-- Covering index for City lookups
CREATE NONCLUSTERED INDEX IX_OrganizationAddress_City
ON Enterprise.OrganizationAddress (City)
INCLUDE (CompanyPartyId, PostalCode, State, Country);

GO