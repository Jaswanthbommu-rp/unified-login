CREATE PROCEDURE [Enterprise].[GetUserSyncJobTask]
(@UserSyncJobTaskId BIGINT)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NOW DATETIME = GETUTCDATE();
    SELECT DISTINCT TOP (1)
           sjt.UserSyncJobId,
           x.PersonaId,
           x.LoginName,
           x.ProductId,
           sjt.[Source]
    FROM Enterprise.UserSyncJobTask sjt
        CROSS APPLY
    (
        SELECT TOP 1000000000000
               sua.PersonaId,
               sua.Value AS LoginName,
               p.ProductId,
               ISNULL(p.UDMSourceCode, p.BooksProductCode) AS Source
        FROM Enterprise.UserSyncJob sj
            INNER JOIN Ident.SamlUserAttribute sua
                ON sj.PersonaId = sua.PersonaId
            INNER JOIN Enterprise.Product p
                ON p.ProductId = sua.ProductId
            INNER JOIN Ident.SamlAttribute sa
                ON sua.SamlAttributeId = sa.SamlAttributeId
        WHERE sjt.UserSyncJobId = sj.UserSyncJobId
              AND sa.Name = 'UserId'
              AND
              (
                  (@NOW
              BETWEEN sua.FromDate AND sua.ThruDate
                  )
                  OR
                  (
                      @NOW >= sua.FromDate
                      AND sua.ThruDate IS NULL
                  )
              )
              AND sjt.Source = ISNULL(p.UDMSourceCode, p.BooksProductCode)
    ) AS x
    WHERE UserSyncJobTaskId = @UserSyncJobTaskId;
END;
GO
