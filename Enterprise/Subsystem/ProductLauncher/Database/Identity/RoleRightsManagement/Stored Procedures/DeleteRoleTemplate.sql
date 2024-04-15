CREATE PROCEDURE [Security].[DeleteRoleTemplate] (
 @RoleTemplateId BIGINT = NULL
)  
AS  
BEGIN    
	BEGIN TRY
	BEGIN TRANSACTION;
		DECLARE @RoleTemplateUserMappingId BIGINT

		SELECT	@RoleTemplateUserMappingId = ISNULL(RoleTemplateUserMappingId,'') 
		FROM Security.RoleTemplateUserMapping
		WHERE RoleTemplateId = @RoleTemplateId

		IF (@RoleTemplateUserMappingId IS NULL OR @RoleTemplateUserMappingId = 0)
		BEGIN
	
	   -- DELETE [Security].[RoleTemplateProductAdditionalTab]  
       DELETE FROM [Security].[RoleTemplateProductAdditionalTab] WHERE RoleTemplateId = @RoleTemplateId

		--DELETE Security.RoleTemplateAdditionalProductRoleMapping
		DELETE additionalRole
		FROM Security.RoleTemplateAdditionalProductRoleMapping additionalRole
					INNER JOIN Security.RoleTemplateProduct roleTemplateProduct On roleTemplateProduct.RoleTemplateProductId = additionalRole.RoleTemplateProductId
					INNER JOIN Security.RoleTemplate roleTemplate on roleTemplateProduct.RoleTemplateId = roleTemplate.RoleTemplateId
		WHERE roleTemplateProduct.RoleTemplateId = @RoleTemplateId

		--DELETE Security.RoleTemplateProductRoleMapping
		DELETE productRoleMapping
		FROM Security.RoleTemplateProductRoleMapping productRoleMapping
					INNER JOIN Security.RoleTemplateProduct roleTemplateProduct	on roleTemplateProduct.RoleTemplateProductId = productRoleMapping.RoleTemplateProductId
					INNER JOIN Security.RoleTemplate roleTemplate on roleTemplateProduct.RoleTemplateId = roleTemplate.RoleTemplateId
		WHERE roleTemplateProduct.RoleTemplateId = @RoleTemplateId

		--DELETE FROM Security.RoleTemplateProduct
		DELETE product
		FROM Security.RoleTemplateProduct product
				INNER JOIN Security.RoleTemplate roleTemplate on product.RoleTemplateId = roleTemplate.RoleTemplateId
		WHERE roleTemplate.RoleTemplateId = @RoleTemplateId		

		--DELETE FROM Security.RoleTemplate
		DELETE FROM Security.RoleTemplate 
		WHERE RoleTemplateId = @RoleTemplateId
		
			SELECT	@RoleTemplateId AS Id ,
				'' AS ErrorMessage  
		END
		ELSE
		BEGIN
			SELECT	0 AS Id ,
					'Unable to delete Enterprise role as it is assigned to users' AS ErrorMessage
		END
	COMMIT;
	END TRY   
	BEGIN CATCH
		ROLLBACK;
		DECLARE @ErrorLogID INT;  
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
		SELECT  0 AS Id,  
		ErrorMessage  
		FROM    dbo.ErrorLog  
		WHERE   ErrorLogID = @ErrorLogID;  
	END CATCH  
END;