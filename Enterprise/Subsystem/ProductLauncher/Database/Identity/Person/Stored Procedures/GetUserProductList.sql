CREATE PROCEDURE [Person].[GetUserProductList]
    @OrganizationPartyId  BIGINT,
    @PersonaId            BIGINT,
    @ProductIds           NVARCHAR(MAX)          = NULL,  -- comma-separated, nullable
    @ProductSubstitutions [dbo].[ProductPairType] READONLY  -- (SourceProductId INT, TargetProductId INT)
AS
BEGIN
    SET NOCOUNT ON;

    -- Step 1: Build the company product list from active organization products.
    DECLARE @companyproductlist TABLE (productid INT);

    INSERT INTO @companyproductlist (productid)
    SELECT DISTINCT op.productid
    FROM   enterprise.OrganizationProduct op
    INNER JOIN Enterprise.GlobalProductConfiguration gpc
        ON gpc.ProductId      = op.ProductId
        AND op.ConfigurationId = gpc.ConfigurationId
    WHERE  op.PartyId   = @OrganizationPartyId
      AND  op.ThruDate  IS NULL
      AND  gpc.ThruDate IS NULL;

    -- Step 2: Replace product 4 with AO-source products.
    IF EXISTS (SELECT 1 FROM @companyproductlist WHERE productid = 4)
    BEGIN
        INSERT INTO @companyproductlist (productid)
            SELECT productid FROM enterprise.Product WHERE UDMSourceCode = 'AO';
        DELETE FROM @companyproductlist WHERE productid = 4;
    END

    -- Step 3: Apply product substitution pairs from the TVP.
    DECLARE @SourceId INT, @TargetId INT;
    DECLARE sub_cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT SourceProductId, TargetProductId FROM @ProductSubstitutions;
    OPEN sub_cur;
    FETCH NEXT FROM sub_cur INTO @SourceId, @TargetId;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF     EXISTS    (SELECT 1 FROM @companyproductlist WHERE productid = @SourceId)
           AND NOT EXISTS(SELECT 1 FROM @companyproductlist WHERE productid = @TargetId)
            INSERT INTO @companyproductlist (productid) VALUES (@TargetId);

        FETCH NEXT FROM sub_cur INTO @SourceId, @TargetId;
    END
    CLOSE sub_cur;
    DEALLOCATE sub_cur;

    -- Step 4: Parse optional product filter.
    CREATE TABLE #FilteredProducts (Val INT);
    IF @ProductIds IS NOT NULL AND LEN(LTRIM(RTRIM(@ProductIds))) > 0
        INSERT INTO #FilteredProducts
            SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ProductIds, ',')
            WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @HasProduct3 BIT = CASE WHEN EXISTS(SELECT 1 FROM #FilteredProducts WHERE Val = 3) THEN 1 ELSE 0 END;

    -- Step 5 & 6: Main result + product 3 unconditional row.
    SELECT
        pc.personaid                          [PersonaId],
        pc.productid                          [ProductId],
        ISNULL(sam.[Value], 'Not Used')       [ProductUserLogin]
    FROM enterprise.PersonaConfiguration pc
    INNER JOIN enterprise.ProductConfiguration pc2
        ON pc2.ConfigurationId = pc.ConfigurationId
    INNER JOIN enterprise.ProductSetting ps
        ON ps.ProductSettingId = pc2.ProductSettingId
    INNER JOIN enterprise.ProductSettingType pst
        ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
    LEFT  JOIN Ident.SamlUserAttribute sam
        ON sam.ProductId      = pc.ProductId
        AND sam.PersonaId     = pc.PersonaId
        AND sam.ThruDate      IS NULL
        AND sam.SamlAttributeId = 1
    INNER JOIN Person.Persona per
        ON per.PersonaId = pc.PersonaId
    INNER JOIN Ident.UserLoginPersona ulp
        ON ulp.UserLoginPersonaId = per.UserLoginPersonaId
    INNER JOIN @companyproductlist cp
        ON cp.productid = pc.ProductId
    WHERE pc.PersonaId = @PersonaId
      AND pst.[Name]   = 'ProductStatus'
      AND pc.ThruDate  IS NULL
      AND pc2.ThruDate IS NULL
      AND ((SELECT COUNT(*) FROM #FilteredProducts) = 0
           OR pc.ProductId IN (SELECT Val FROM #FilteredProducts))

    UNION ALL
    -- Product 3 is always included when explicitly requested.
    SELECT @PersonaId, 3, 'Not Used'
    WHERE  @HasProduct3 = 1;

    DROP TABLE IF EXISTS #FilteredProducts;
END