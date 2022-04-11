CREATE PROCEDURE [Enterprise].[DeleteExternalUserRelationship]  
(  
 @UserLoginPersonaId BIGINT  
)  
AS  
BEGIN  
	Delete from Enterprise.ExternalUserRelationship
	where UserLoginPersonaId = @UserLoginPersonaId
END