--EXEC Enterprise.DisableUsersForProduct 1,350
CREATE PROCEDURE Enterprise.DisableUsersForProduct
(
	@PartyId bigint,
	@ProductId int	
)
AS
BEGIN   
   DECLARE @EditorPartyId bigint, @EditorPersonaId bigint, 
	@CorrelationId UNIQUEIDENTIFIER= NEWID(),
	@NOW DATETIME= GETUTCDATE(),
	@DisableUserProductSettingType varchar(1) = '0'
	
	--Query to get Admin PersonaId and PartyId
		SELECT   
		@EditorPartyId= UL.PersonPartyId,
		@EditorPersonaId = P.PersonaId
		FROM 
			Enterprise.OrganizationAdminUser OAU
			INNER JOIN Ident.UserLoginPersona ULP ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
			INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
		WHERE OAU.OrganizationPartyId = @PartyId 

	--Query to get Product settings
		SELECT 		   
		@DisableUserProductSettingType = ISNULL(ps.Value,'0')  
		FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
		WHERE  gpc.ProductId = @ProductId  AND GPC.ProductId = @ProductId AND PST.Name = 'DisableUsersOnProductCancel'
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		
		--INSERT RECORD INTO BATCH.BatchProcessor TABLE
		IF(@DisableUserProductSettingType = '1')
		BEGIN			
			INSERT INTO Batch.BatchProcessor
			(
				CorrelationId,
				EditorUserPartyId,
				EditorUserPersonaId,
				SubjectUserPersonaId,
				BatchProcessTypeId,
				ProductId,
				StatusTypeId,
				RetryCount,
				InputJSON,
				CreatedDateTime				
			)
			SELECT
				@CorrelationId,
				@EditorPartyId,
				@EditorPersonaId,
				PC.PersonaId,
				1,
				PC.ProductId,
				5,
				0,
				'{"IsAssigned":false,"CompanyId":0,"CanReceiveMonthlyReport":false}',
				GETDATE()			
			FROM 
				Enterprise.PersonaConfiguration pc
				INNER JOIN Person.Persona PA ON PC.PersonaId = PA.PersonaId
				INNER JOIN Ident.UserLoginPersona ULP ON PA.UserLoginPersonaId = ULP.UserLoginPersonaId					
			WHERE 
				pc.ProductId = @ProductId AND pc.StatusTypeId = 8 AND PC.PersonaId <> @EditorPersonaId AND ULP.OrganizationPartyId = @PartyId	
		END
END	