CREATE PROCEDURE [UserAudit].[GetUserPropertiesFromAudit]
    @RequestId BIGINT,
    @UlDBName  NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'
        SELECT
            p.[Name] [ProductName],
            CASE WHEN up.[Status] = 3 THEN ''Error''
                 WHEN up.[Status] = 2 THEN ''No properties assigned''
                 WHEN up2.propertyname IS NULL THEN ''No property found''
                 ELSE up2.propertyname
            END [PropertyName],
            UP.USERNAME AS [ProductUserLogin],
            u1.username  [LoginName],
            u1.firstname [FirstName],
            u1.lastname  [LastName],
            CASE WHEN u1.UserType = ''User''           THEN ''Regular User''
                 WHEN u1.UserType = ''SuperUser''       THEN ''RealPage System Administrator''
                 WHEN u1.UserType = ''User (No Email)'' THEN ''Regular User (No Email)''
                 ELSE u1.UserType
            END [UserType],
            u1.[Status] [Status],
            CASE WHEN MONTH(u1.LastLoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR, -6, u1.LastLoginDate)
                 ELSE DATEADD(HOUR, -5, u1.LastLoginDate)
            END AS [LastLoginDate]
        FROM [useraudit].[user] u1
        INNER JOIN [UserAudit].[Request] r       ON r.RequestId    = u1.RequestId
        INNER JOIN [useraudit].[userproduct] up  ON u1.audituserid = up.AuditUserId
        INNER JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[Product] p
            ON p.ProductId = up.ProductId
        LEFT JOIN [useraudit].[userproperty] up2
            ON up.UserProductId = up2.userproductid
        LEFT JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[PersonaConfiguration] PC
            ON PC.ProductId  = UP.ProductId
            AND PC.PersonaId = u1.PersonaId
        WHERE u1.RequestId = @RequestId
          AND NOT (p.ProductId = 3 AND u1.[Status] = ''Disabled'')
          AND ((PC.StatusTypeId <> 10 AND PC.StatusTypeId <> 7 AND PC.StatusTypeId <> 19)
               OR PC.StatusTypeId IS NULL)
        ORDER BY u1.username, p.[Name];';

    EXEC sp_executesql @sql, N'@RequestId BIGINT', @RequestId = @RequestId;
END