CREATE PROCEDURE [UserAudit].[GetUserAccessDataFromAudit]
    @RequestId                  BIGINT,
    @UlDBName                   NVARCHAR(128),
    @OrgPartyId                 BIGINT,
    @ProductsNotSupported       NVARCHAR(MAX),   -- comma-separated INT list
    @ProductsNotEnabled         NVARCHAR(MAX),   -- comma-separated INT list (NULL → treated as -1)
    @PrimaryPropertyValueForPMC NVARCHAR(10)     -- '0' or '1'
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalise the "not enabled" list: default to -1 when empty so the IN() is always valid.
    IF @ProductsNotEnabled IS NULL OR LEN(LTRIM(RTRIM(@ProductsNotEnabled))) = 0
        SET @ProductsNotEnabled = N'-1';

    -- Translate the PMC flag the same way the C# code did.
    DECLARE @PrimaryPropertyValue NVARCHAR(1) = CASE WHEN @PrimaryPropertyValueForPMC = '0' THEN '0' ELSE '1' END;

    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'
        SELECT
            u.[PersonaId],
            u.[UserName]                                                       [UserName],
            u.[FirstName]                                                      [FirstName],
            u.[LastName]                                                       [LastName],
            u.[Status]                                                         [PlatformUserStatus],
            CASE WHEN MONTH(ULP.FromDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,ULP.FromDate)
                 ELSE DATEADD(HOUR,-5,ULP.FromDate) END                        [PlatformCreationDate],
            CASE WHEN MONTH(u.LastLoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,u.LastLoginDate)
                 ELSE DATEADD(HOUR,-5,u.LastLoginDate) END                     [PlatformCompanyLastLoginDate],
            CASE WHEN MONTH(UL.LastLoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,UL.LastLoginDate)
                 ELSE DATEADD(HOUR,-5,UL.LastLoginDate) END                    [PlatformLastLoginDate],
            CASE WHEN MONTH(ULP.UserDeactivationDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,ULP.UserDeactivationDate)
                 ELSE DATEADD(HOUR,-5,ULP.UserDeactivationDate) END            [PlatformDeactivationDate],
            rt.RoleTemplateName                                                [EnterpriseRole],
            CASE
                WHEN @PrimaryPropertyValue = ''0''
                     THEN ''Not Enabled''
                WHEN p.ProductId IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ProductsNotEnabled,  '',''))
                     THEN ''Not Enabled''
                WHEN p.ProductId IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ProductsNotSupported,'',''))
                     THEN ''Not Supported''
                WHEN EPC.UsePrimaryProperties = 1 THEN ''TRUE''
                WHEN EPC.UsePrimaryProperties = 0 THEN ''FALSE''
                ELSE ''Not Enabled''
            END                                                                [UsePrimaryProperties],
            p.[Name]                                                           [ProductName],
            CASE WHEN MONTH(EPC.FromDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,EPC.FromDate)
                 ELSE DATEADD(HOUR,-5,EPC.FromDate) END                        [ProductAccessDate],
            CASE WHEN EST.StatusTypeId IN (24,10,19) THEN ''Disabled'' ELSE EST.[Name] END
                                                                               [ProductUserStatus],
            CASE WHEN MONTH(PLL.LoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,PLL.LoginDate)
                 ELSE DATEADD(HOUR,-5,PLL.LoginDate) END                       [ProductLastLoginDate],
            CASE WHEN p1.PropertyName = ''All'' THEN ''ALL''
                 ELSE COALESCE(CAST(p1.PropertyCount AS VARCHAR(10)),''0'') END [ProductPropertyCount],
            COALESCE(p2.RoleNames,'''')                                        [ProductRolesAssigned],
            CASE WHEN MONTH(EPC.ProductDeactivationDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,EPC.ProductDeactivationDate)
                 ELSE DATEADD(HOUR,-5,EPC.ProductDeactivationDate) END         [ProductDeactivationDate]
        FROM [UserAudit].[User] u
        INNER JOIN [UserAudit].[UserProduct] up
            ON up.AuditUserId = u.audituserid
        INNER JOIN ' + QUOTENAME(@UlDBName) + N'.[Person].[Persona] PPA
            ON PPA.PersonaId = u.PersonaId
        INNER JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[Product] p
            ON up.ProductId = p.ProductId
        LEFT  JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[PersonaConfiguration] EPC
            ON EPC.ProductId = p.ProductId AND EPC.PersonaId = PPA.PersonaId
        LEFT  JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[StatusType] EST
            ON EST.StatusTypeId = EPC.StatusTypeId
        INNER JOIN ' + QUOTENAME(@UlDBName) + N'.[Ident].[UserLoginPersona] ULP
            ON ULP.UserLoginPersonaId = PPA.UserLoginPersonaId
        INNER JOIN ' + QUOTENAME(@UlDBName) + N'.[Ident].[UserLogin] UL
            ON ULP.UserLoginId = UL.UserId
        LEFT  JOIN ' + QUOTENAME(@UlDBName) + N'.[Security].[RoleTemplateUserMapping] rtu
            ON rtu.PersonaId = PPA.PersonaId
        LEFT  JOIN ' + QUOTENAME(@UlDBName) + N'.[Security].[RoleTemplate] rt
            ON rtu.RoleTemplateId = rt.RoleTemplateId
        LEFT  JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[ProductLoginUserActivitySummary] PLL
            ON PLL.ProductId = EPC.ProductId AND PLL.PersonaId = PPA.PersonaId
        LEFT  JOIN (
            SELECT up_i.UserProductId,
                   MAX(CASE WHEN p1_i.PropertyName = ''All properties'' THEN ''All'' ELSE ''Other'' END) AS PropertyName,
                   COUNT(*) AS PropertyCount
            FROM   [UserAudit].[User] u_i
            INNER JOIN [UserAudit].[UserProduct]  up_i ON up_i.AuditUserId  = u_i.audituserid
            INNER JOIN [UserAudit].[UserProperty] p1_i ON p1_i.UserProductId = up_i.UserProductId
            WHERE  u_i.RequestId = @RequestId
            GROUP BY up_i.UserProductId
        ) AS p1 ON up.UserProductId = p1.UserProductId
        LEFT  JOIN (
            SELECT up_r.UserProductId,
                   STRING_AGG(ur_r.RoleName, ''|'') WITHIN GROUP (ORDER BY ur_r.RoleName) AS RoleNames
            FROM   [UserAudit].[User] u_r
            INNER JOIN [UserAudit].[UserProduct] up_r ON up_r.AuditUserId  = u_r.AuditUserId
            INNER JOIN [UserAudit].[UserRole]    ur_r ON ur_r.UserProductId = up_r.UserProductId
            WHERE  u_r.RequestId = @RequestId
            GROUP BY up_r.UserProductId
        ) AS p2 ON up.UserProductId = p2.UserProductId
        WHERE u.RequestId = @RequestId
        ORDER BY [UserName] ASC;';

    EXEC sp_executesql @sql,
        N'@RequestId BIGINT, @PrimaryPropertyValue NVARCHAR(1),
          @ProductsNotEnabled NVARCHAR(MAX), @ProductsNotSupported NVARCHAR(MAX)',
        @RequestId             = @RequestId,
        @PrimaryPropertyValue  = @PrimaryPropertyValue,
        @ProductsNotEnabled    = @ProductsNotEnabled,
        @ProductsNotSupported  = @ProductsNotSupported;
END