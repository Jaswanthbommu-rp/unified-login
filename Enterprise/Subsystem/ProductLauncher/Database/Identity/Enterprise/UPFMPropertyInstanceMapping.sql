CREATE TYPE [Enterprise].[UPFMPropertyInstanceMapping] AS TABLE
(
    PropertyInstanceID BIGINT NOT NULL PRIMARY KEY,  -- Added PRIMARY KEY for performance
    IsDeleted BIT NOT NULL DEFAULT 0
);