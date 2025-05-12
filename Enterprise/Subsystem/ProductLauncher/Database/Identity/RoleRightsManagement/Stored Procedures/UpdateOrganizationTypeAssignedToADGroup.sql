CREATE PROCEDURE [Security].[UpdateOrganizationTypeAssignedToADGroup](    
	@AdGroupId INT, @OrganizationTypeIdsList NVARCHAR(MAX), @CreatedBy INT
)    
AS    
BEGIN    
	DECLARE @CreatedDate datetime = GETUTCDATE()
	DECLARE @OrganizationTypes TABLE (OrganizationTypeId INT PRIMARY KEY)      
	IF (LEN(@OrganizationTypeIdsList) > 0)      
	BEGIN      
		INSERT INTO @OrganizationTypes(OrganizationTypeId)
		SELECT CONVERT(int, value)      
		FROM STRING_SPLIT(@OrganizationTypeIdsList, ',');      
	END    
	DELETE    
	FROM [Security].AdGroupOrganizationType    
	WHERE ADGroupId = @AdGroupId    
       
	INSERT INTO [Security].AdGroupOrganizationType(ADGroupId, OrganizationTypeId, CreatedBy, CreatedDate)    
	SELECT @AdGroupId, OrganizationTypeId, @CreatedBy, @CreatedDate    
	FROM @OrganizationTypes    
END
GO

