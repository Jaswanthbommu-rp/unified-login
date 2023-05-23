CREATE TABLE [Auth].[ClientUserClaim] (
    [ClientUserClaimId] BIGINT IDENTITY (1, 1) NOT NULL,
    [ClientId]          INT NOT NULL,
    [ClaimId]           BIGINT NOT NULL,
    CONSTRAINT [PK_ClientUserClaim] PRIMARY KEY CLUSTERED ([ClientUserClaimId] ASC),
    CONSTRAINT [FK_ClientUserClaim_Claim] FOREIGN KEY ([ClaimId]) REFERENCES [Auth].[Claim] ([ClaimId]),
    CONSTRAINT [FK_ClientUserClaim_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id])
);

