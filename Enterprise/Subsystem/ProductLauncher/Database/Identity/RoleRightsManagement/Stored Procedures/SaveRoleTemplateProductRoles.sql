CREATE PROCEDURE [Security].[SaveRoleTemplateProductRoles]
(
	@RoleTemplateId bigint,
	@json varchar(max)
)
AS
BEGIN
	BEGIN TRY  
	BEGIN TRANSACTION;
	DECLARE @RoleTemplateProductId bigint,
			@ErrorLogID int;
	--START - INSERT DATA FROM JSON TO TEMP TABLES
	--Insert Products from JSON to TempTable
	Declare @RoleTemplateProduct TABLE(RoleTemplateId bigint, ProductId bigint, ProcessedFlag int Default(0))
	Insert Into @RoleTemplateProduct(RoleTemplateId,ProductId)
	SELECT @RoleTemplateId, Products.productId 
	FROM  OPENJSON ( @json  )  
		WITH (
			Products  nvarchar(max) as Json
			)
			As RoleTemplates
			cross apply openjson(RoleTemplates.Products)
			WITH(
			ProductId BIGINT
			) as Products

	--Insert Roles From Json to TempTable
	Declare @RoleTemplateProductRoles TABLE(RoleTemplateProductId bigint, ProductId bigint,RoleTemplateProductRoleMappingID bigint, RoleId Varchar(100), RoleName varchar(510))
	Insert Into @RoleTemplateProductRoles(RoleTemplateProductId,ProductId,RoleTemplateProductRoleMappingID,RoleId,RoleName)
	SELECT Products.RoleTemplateProductId, Products.ProductId,Roles.RoleTemplateProductRoleMappingID,Roles.RoleId,Roles.RoleName 
	FROM  
		OPENJSON (@json)  
		WITH (
			Products nvarchar(max) as json
			)
			As RoleTemplates
			cross apply openjson(RoleTemplates.Products)
			WITH(
			ProductId BIGINT,
			RoleTemplateProductId bigint,
			Roles nvarchar(max) as json
				
			) as Products
			cross apply openJson(Products.Roles)
			WITH(
			RoleTemplateProductRoleMappingID bigint,
			RoleId varchar(150),
			RoleName varchar(200)			
			)  as Roles
			
	--Insert Additional Roles From Json to TempTable
	Declare @RoleTemplateAdditionalProductRoleMapping TABLE(RoleTemplateProductId bigint,ProductId bigint,RoleTemplateProductRoleMappingID bigint, AttributeName Varchar(255), AttributeValue varchar(255))
	Insert Into @RoleTemplateAdditionalProductRoleMapping(RoleTemplateProductId,ProductId,RoleTemplateProductRoleMappingID,AttributeName,AttributeValue)
	SELECT distinct
		Products.RoleTemplateProductId,
		Products.ProductId, 
		AdditionalAttributes.RoleTemplateProductRoleMappingID,
		AdditionalAttributes.AttributeName, 
		AdditionalAttributes.AttributeValue 
	FROM  
		OPENJSON ( @json  )  
		WITH (
			Products nvarchar(max) as json
			)
			As RoleTemplates
			cross apply openjson(RoleTemplates.Products)
			WITH(
			ProductId BIGINT,
			RoleTemplateProductId bigint,
			Roles nvarchar(max) as json,
			AdditionalAttributes nvarchar(max) as Json
			) as Products
			cross apply openJson(Products.AdditionalAttributes)
			WITH(
			RoleTemplateProductRoleMappingID bigint,
			AttributeName varchar(150),
			AttributeValue varchar(150)
			) as AdditionalAttributes
			
	--Insert deleted Roles From Json to TempTable
	Declare @RoleTemplateRemoveProductRoles TABLE(RoleTemplateProductId bigint,ProductId bigint,RoleTemplateProductRoleMappingID bigint)
	Insert Into @RoleTemplateRemoveProductRoles(RoleTemplateProductId, ProductId,RoleTemplateProductRoleMappingID)
	SELECT Products.RoleTemplateProductId,Products.ProductId,RemoveRoles.RoleTemplateProductRoleMappingID
	FROM  
		OPENJSON (@json)  
		WITH (
			Products nvarchar(max) as json
			)
			As RoleTemplates
			cross apply openjson(RoleTemplates.Products)
			WITH(
			ProductId BIGINT,
			RoleTemplateProductId bigint,
			RemovedRoleList nvarchar(max) as json
				
			) as Products
			cross apply openJson(Products.RemovedRoleList)
			WITH(
			RoleTemplateProductRoleMappingID bigint
			)  as RemoveRoles

	--Insert Deleted Additional Roles From Json to TempTable
	Declare @RoleTemplateRemoveAdditionalProductRoleMapping TABLE(RoleTemplateProductId bigint,ProductId bigint,RoleTemplateAdditionalProductRoleMappingId bigint)
	Insert Into @RoleTemplateRemoveAdditionalProductRoleMapping(RoleTemplateProductId,ProductId,RoleTemplateAdditionalProductRoleMappingId)
	SELECT distinct
		Products.RoleTemplateProductId,
		Products.ProductId, 
		RemoveAdditionalAttributes.RoleTemplateAdditionalProductRoleMappingId
	FROM  
		OPENJSON ( @json  )  
		WITH (
			Products nvarchar(max) as json
			)
			As RoleTemplates
			cross apply openjson(RoleTemplates.Products)
			WITH(
			ProductId BIGINT,
			RoleTemplateProductId bigint,
			RemovedAdditionalAttributes nvarchar(max) as Json
			) as Products
			cross apply openJson(Products.RemovedAdditionalAttributes)
			WITH(
			RoleTemplateAdditionalProductRoleMappingId bigint
			) as RemoveAdditionalAttributes

	--Insert deleted Products from JSON to TempTable
	Declare @RemoveRoleTemplateProduct TABLE(RoleTemplateId bigint, RoleTemplateProductId bigint)
	Insert Into @RemoveRoleTemplateProduct(RoleTemplateId,RoleTemplateProductId)
	SELECT @RoleTemplateId, RemovedProducts.RoleTemplateProductId 
	FROM  OPENJSON ( @json  )  
		WITH (
			RemovedRoleTemplateProducts  nvarchar(max) as Json
			)
			As RemovedRoleTemplateProduct
			cross apply openjson(RemovedRoleTemplateProduct.RemovedRoleTemplateProducts)
			WITH(
			RoleTemplateProductId BIGINT
			) as RemovedProducts
	
	/*
	select * from @RoleTemplateProduct
	select * from @RoleTemplateProductRoles
	select * from @RoleTemplateAdditionalProductRoleMapping
	select * from @RoleTemplateRemoveProductRoles
	select * from @RoleTemplateRemoveAdditionalProductRoleMapping
	select * from @RemoveRoleTemplateProduct
	*/
	--END - INSERT DATA FROM JSON TO TEMP TABLES

	--INSERT PRODUCTS
	IF(@RoleTemplateId IS NOT NULL AND @RoleTemplateId > 0)
	BEGIN	
		--Loop through products and insert roles
		WHILE(1 = 1)
		BEGIN
			DECLARE @ProductId INT;
			SET @ProductID = NULL;
			SELECT TOP 1 
				@ProductId = ProductId 
			FROM 
				@RoleTemplateProduct
			WHERE ProcessedFlag = 0;

			IF (@ProductID IS NULL) 
			BEGIN
				BREAK;
			END;
			--INSERT RoleTemplateProduct
			IF((@RoleTemplateId IS NOT NULL OR @RoleTemplateId <> 0) AND (@ProductId IS NOT NULL OR @ProductId <> 0) )
			BEGIN
				SET @RoleTemplateProductId = NULL
				SELECT @RoleTemplateProductId = ISNULL(RoleTemplateProductId,'')
							FROM Security.RoleTemplateProduct 
							WHERE RoleTemplateId = @RoleTemplateId AND ProductId = @ProductId				
				IF (@RoleTemplateProductId IS NULL)
				BEGIN
					INSERT INTO Security.RoleTemplateProduct(RoleTemplateId,ProductId)
					Values(@RoleTemplateId,@ProductId)
					SELECT @RoleTemplateProductId = SCOPE_IDENTITY();
				END
			END						
			IF(@RoleTemplateProductId IS NOT NULL OR @RoleTemplateProductId <> 0)
			BEGIN
			--INSERT ROLES FOR PRODUCT			
			INSERT INTO Security.RoleTemplateProductRoleMapping  
				(
				RoleTemplateProductId,
				ProductRoleId,
				ProductRoleName
				)
			select 
				@RoleTemplateProductId
				,tempRoles.RoleId
				,tempRoles.RoleName
			FROM @RoleTemplateProductRoles tempRoles 
				LEFT OUTER JOIN Security.RoleTemplateProductRoleMapping roles 
					ON roles.ProductRoleId = tempRoles.RoleId and Roles.RoleTemplateProductId = @RoleTemplateProductId
				WHERE tempRoles.ProductId = @ProductId and roles.RoleTemplateProductRoleMappingId IS NULL
			  
			--INSERT ADDITIONAL ROLES
			INSERT INTO Security.RoleTemplateAdditionalProductRoleMapping
			(
			RoleTemplateProductId,
			AttributeName,
			AttributeValue
			)
			select 
				@RoleTemplateProductId,
				tempAdMap.AttributeName,
				tempAdMap.AttributeValue
			FROM @RoleTemplateAdditionalProductRoleMapping tempAdMap 
			LEFT OUTER JOIN Security.RoleTemplateAdditionalProductRoleMapping rtam on tempAdMap.AttributeName = rtam.AttributeName
					and rtam.RoleTemplateProductId = @RoleTemplateProductId
			WHERE tempAdMap.ProductId = @ProductId and rtam.RoleTemplateAdditionalProductRoleMappingId IS NULL
			END

			UPDATE @RoleTemplateProduct
				SET    ProcessedFlag = 1
				WHERE  ProductId = @ProductId;
		END	

		--Remove Roles
		DELETE RTM 		
		FROM Security.RoleTemplateProductRoleMapping RTM
				INNER JOIN @RoleTemplateRemoveProductRoles TempRoleDelete 
					on RTM.RoleTemplateProductRoleMappingId = TempRoleDelete.RoleTemplateProductRoleMappingID

		--Remove Additional Roles
		DELETE RTM 
		FROM Security.RoleTemplateAdditionalProductRoleMapping RTM
				INNER JOIN @RoleTemplateRemoveAdditionalProductRoleMapping TempRoleDelete 
					on RTM.RoleTemplateAdditionalProductRoleMappingId = TempRoleDelete.RoleTemplateAdditionalProductRoleMappingId

		--To Remove Role Template Product( To Remove product, Roles and Additional Roles from child tables should be deleted first)
		--Remove Roles
		DELETE RTM
		FROM Security.RoleTemplateProductRoleMapping RTM
				INNER JOIN @RemoveRoleTemplateProduct TempRoleDelete 
					on RTM.RoleTemplateProductId = TempRoleDelete.RoleTemplateProductId

		--Remove Additional Roles
		DELETE RTM		
		FROM Security.RoleTemplateAdditionalProductRoleMapping RTM
				INNER JOIN @RemoveRoleTemplateProduct TempRoleDelete 
					on RTM.RoleTemplateProductId = TempRoleDelete.RoleTemplateProductId

		--Remove Role Template Product
		DELETE RTP		
		FROM Security.RoleTemplateProduct RTP
				INNER JOIN @RemoveRoleTemplateProduct TempRoleDelete 
					on RTP.RoleTemplateProductId = TempRoleDelete.RoleTemplateProductId
		
		SELECT	@RoleTemplateId AS Id ,
				'' AS ErrorMessage
	END	
	COMMIT;  
	END TRY    
	BEGIN CATCH  
		ROLLBACK;  
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
	END CATCH  
END