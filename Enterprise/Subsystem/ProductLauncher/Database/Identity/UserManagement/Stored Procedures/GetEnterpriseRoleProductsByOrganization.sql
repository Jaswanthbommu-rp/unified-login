CREATE PROCEDURE [Security].[GetEnterpriseRoleProductsByOrganization]    
(    
 @RoleTemplateId int,     
 @OrganizationRealPageId UNIQUEIDENTIFIER = NULL,    
    @PartyId BIGINT = NULL    
)    
AS    
BEGIN    
 DECLARE @NOW DATETIME = GETUTCDATE();  
 SELECT DISTINCT RTP.ProductId
	FROM [Security].[RoleTemplate] RT
	JOIN [Security].[RoleTemplateProduct] RTP ON
		RT.RoleTemplateId = RTP.RoleTemplateId
	JOIN Enterprise.Party P ON
		p.PartyId = RT.PartyID
 JOIN Enterprise.OrganizationProduct OP ON OP.ProductId = RTP.ProductId AND p.PartyId = OP.PartyId
 JOIN Enterprise.GlobalProductConfiguration GPC on GPC.ConfigurationId =OP.ConfigurationId AND  GPC.ProductId = RTP.ProductId
 WHERE RT.RoleTemplateId = @RoleTemplateId  
 AND (P.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)   
 AND (P.PartyId = @PartyId OR @PartyId IS NULL)  
 AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
 AND ((@NOW BETWEEN OP.FromDate AND OP.ThruDate) OR (@NOW >= OP.FromDate AND OP.ThruDate IS NULL))  
END;