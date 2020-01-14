CREATE TABLE [Auth].[Claim] (
    [ClaimId]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [ClaimName]         NVARCHAR (255) NOT NULL,
    [SAMLAttributeName] NVARCHAR (50)  NULL,
    [ProductId]         INT         NOT NULL,
    CONSTRAINT [PK_Claim] PRIMARY KEY CLUSTERED ([ClaimId] ASC),
    CONSTRAINT [FK_Claim_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
);

