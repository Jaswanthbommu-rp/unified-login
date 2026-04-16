CREATE PROCEDURE [UserAudit].[InsertPrimaryPropertyAudit]
    @JsonVariable NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [UserAudit].[PrimaryPropertyAudit]
        ([RequestId], [ProductId], [ProductName], [PropertyGUID], [PropertyName], [ProductInstanceId])
    SELECT
        RequestId,
        ProductId,
        ProductName,
        PropertyGUID,
        PropertyName,
        ProductInstanceId
    FROM OPENJSON(@JsonVariable, N'$')
    WITH (
        [RequestId]         BIGINT         N'$.RequestId',
        [ProductId]         INT            N'$.ProductId',
        [ProductName]       NVARCHAR(100)  N'$.ProductName',
        [PropertyGUID]      NVARCHAR(100)  N'$.PropertyGUID',
        [PropertyName]      NVARCHAR(256)  N'$.PropertyName',
        [ProductInstanceId] NVARCHAR(256)  N'$.ProductInstanceId'
    );
END