CREATE TYPE [UserAudit].[UserProductInfo] AS TABLE
(
    [PersonaId]   BIGINT         NOT NULL,
    [ProductId]   INT            NOT NULL,
    [UserName]    NVARCHAR(512)  NOT NULL
);