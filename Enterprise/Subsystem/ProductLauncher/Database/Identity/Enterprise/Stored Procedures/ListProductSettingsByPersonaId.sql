CREATE PROCEDURE [Enterprise].[ListProductSettingsByPersonaId] @PersonaId     INT,
                                                              @ProductStatus NVARCHAR(2000) = NULL
AS
     BEGIN
         DECLARE @NOW DATETIME= GETUTCDATE();
         SELECT p.ProductId,
                ps.ProductSettingId,
                pst.Name,
                CASE
				WHEN (pst.Name = 'UsePrimaryProperties')
				THEN p.UsePrimaryProperties
				ELSE ps.value
				END AS Value
         FROM Enterprise.PersonaConfiguration p
              LEFT JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
              LEFT JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
              LEFT JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
              LEFT JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
         WHERE p.PersonaId = @PersonaId
               AND (ps.Value = @ProductStatus
                    OR @ProductStatus IS NULL)
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)
                    OR (@NOW >= p.FromDate
                        AND p.ThruDate IS NULL))
               AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate)
                    OR (@NOW >= pc.FromDate
                        AND pc.ThruDate IS NULL));
     END;
