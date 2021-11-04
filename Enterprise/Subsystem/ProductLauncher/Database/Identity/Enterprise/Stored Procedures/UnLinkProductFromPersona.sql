CREATE PROCEDURE Enterprise.UnLinkProductFromPersona
(@PersonaId BIGINT,
 @ProductId INT
)
AS
     BEGIN
         DECLARE @ErrorLogID INT;
		 DECLARE @ConfigurationId BIGINT;
         SELECT @ConfigurationId = ConfigurationId
         FROM Enterprise.PersonaConfiguration
         WHERE PersonaId = @PersonaId
               AND ProductId = @ProductId;
         
		 BEGIN TRY
             IF EXISTS
				(
					SELECT 1
					FROM ident.SamlUserAttribute
					WHERE productid = @ProductId
						  AND personaid = @PersonaId
				)
                 BEGIN
                     DELETE FROM ident.SamlUserAttribute
                     WHERE productid = @ProductId
                           AND personaid = @PersonaId;
                 END;
             IF EXISTS
				(
					SELECT 1
					FROM enterprise.PersonaConfiguration
					WHERE productid = @ProductId
						  AND personaid = @PersonaiD
				)
                 BEGIN
                     DELETE FROM enterprise.PersonaConfiguration
                     WHERE productid = @ProductId
                           AND personaid = @PersonaiD;
                 END;
             IF EXISTS
				(
					SELECT 1
					FROM Enterprise.EmployeeProductMapping
					WHERE productid = @ProductId
						  AND personaid = @PersonaiD
				)
                 BEGIN
                     DELETE FROM Enterprise.EmployeeProductMapping
                     WHERE productid = @ProductId
                           AND personaid = @PersonaiD;
                 END;
             IF EXISTS
				(
					SELECT 1
					FROM ENterprise.ProductConfiguration
					WHERE COnfigurationId = @ConfigurationId
				)
                 BEGIN
                     DELETE FROM ENterprise.ProductConfiguration
                     WHERE COnfigurationId = @ConfigurationId;
                 END;
				 SELECT @PersonaId  AS Id, '' AS ErrorMessage
         END TRY
         BEGIN CATCH
             EXEC dbo.LogError
                  @ErrorLogID = @ErrorLogID OUTPUT;
             SELECT @PersonaId AS Id,
                    ErrorMessage
             FROM dbo.ErrorLog
             WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
     END;
