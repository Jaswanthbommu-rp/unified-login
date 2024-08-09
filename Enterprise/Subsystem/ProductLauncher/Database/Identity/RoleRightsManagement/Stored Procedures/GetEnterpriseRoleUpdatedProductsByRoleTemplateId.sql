CREATE PROCEDURE [Security].[GetEnterpriseRoleUpdatedProductsByRoleTemplateId]
(
	@RoleTemplateId int,
	@CreatedDateTime datetime
)
AS
BEGIN
	Declare @RoleProduct Table 	(ProductId int)
	
	Declare @oldRoleData Table (
		RoleTemplateProductId bigint, 
		RoleTemplateProductRoleMappingId bigint)
 
    Declare @oldAdditionalRoleData Table (  
        RoleTemplateProductId bigint,   
        RoleTemplateAdditionalProductRoleMappingId bigint)

	Declare @ProductData table (
		RoleTemplateProductId bigint, 
		ProductId int null)

	
	 DECLARE @5secago datetime = DATEADD(SECOND,-5,@CreatedDateTime)
	
	--Product Role add/unassign
	Insert into @oldRoleData (RoleTemplateProductId, RoleTemplateProductRoleMappingId)
    SELECT  RoleTemplateProductId, RoleTemplateProductRoleMappingId
    FROM security.RoleTemplateProductRoleMapping 
    FOR SYSTEM_TIME AS OF @5secago 

	Insert into @oldAdditionalRoleData (RoleTemplateProductId, RoleTemplateAdditionalProductRoleMappingId)  
    SELECT  RoleTemplateProductId, RoleTemplateAdditionalProductRoleMappingId  
    FROM security.RoleTemplateAdditionalProductRoleMapping   
    FOR SYSTEM_TIME AS OF @5secago 

	--role un assign
	Insert Into @ProductData
    SELECT RoleTemplateProductId,null
    FROM @oldRoleData	
	WHERE RoleTemplateProductRoleMappingId  
	NOT IN (SELECT RoleTemplateProductRoleMappingId FROM [Security].[RoleTemplateProductRoleMapping] )

	Insert Into @ProductData  
    SELECT RoleTemplateProductId,null  
    FROM @oldAdditionalRoleData   
    WHERE RoleTemplateAdditionalProductRoleMappingId    
    NOT IN (SELECT RoleTemplateAdditionalProductRoleMappingId FROM [Security].[RoleTemplateAdditionalProductRoleMapping] )  
  
	--role add
	Insert Into @ProductData
    SELECT RoleTemplateProductId,null
    FROM [Security].[RoleTemplateProductRoleMapping]
	WHERE RoleTemplateProductRoleMappingId  
	NOT IN (SELECT RoleTemplateProductRoleMappingId FROM @oldRoleData)

	Insert Into @ProductData  
    SELECT RoleTemplateProductId,null  
    FROM [Security].[RoleTemplateAdditionalProductRoleMapping]  
    WHERE RoleTemplateAdditionalProductRoleMappingId    
    NOT IN (SELECT RoleTemplateAdditionalProductRoleMappingId FROM @oldRoleData)  

	Update PD Set ProductId = RTP.ProductId
	From @ProductData PD
	JOIN [Security].[RoleTemplateProduct] RTP ON
		RTP.RoleTemplateProductId = PD.RoleTemplateProductId
	Where RTP.RoleTemplateId = @RoleTemplateId

	INSERT INTO @RoleProduct (ProductId)
	Select Distinct ProductId From @ProductData Where ProductId IS NOT NULL

	--final Result set
	Select Distinct ProductId From @RoleProduct Where ProductId IS NOT NULL
END;
