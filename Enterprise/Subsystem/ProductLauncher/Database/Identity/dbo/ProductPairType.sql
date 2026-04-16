CREATE TYPE [dbo].[ProductPairType] AS TABLE
(
    SourceProductId INT NOT NULL,
    TargetProductId INT NOT NULL
);