CREATE PROCEDURE Enterprise.UpdateOrganizationThirdPartyIDP(@OrganizationPartyId Int,@ThirdPartyIDP NVARCHAR(50))  
AS    
BEGIN   
BEGIN TRY 
DECLARE @IDPId INT;  
DECLARE @CMId INT;  
  
SELECT @IdpId = IdentityProviderTypeId, @CMId = ContactMechanismId FROM ident.identityprovidertype WHERE Description = @ThirdPartyIDP;   
  
-- check to make sure the company doesn't already have 2 idps set up  
IF 2 > (SELECT COUNT(1) FROM enterprise.PartyContactMechanism pcm INNER JOIN ident.IdentityProviderType ipt ON ipt.ContactMechanismId = pcm.ContactMechanismId WHERE pcm.PartyId = @OrganizationPartyId AND pcm.ThruDate is NULL)  
BEGIN        
IF NOT EXISTS (SELECT 1 FROM Enterprise.PartyContactMechanism WHERE PartyId = @OrganizationPartyId AND ContactMechanismId = @CMId AND ThruDate is NULL)       
BEGIN            
INSERT INTO Enterprise.PartyContactMechanism ( partyid, ContactMechanismId, FromDate )             
VALUES (@OrganizationPartyId, @CMid, GETUTCDATE())      
UPDATE Enterprise.Organization SET IdentityProviderTypeId = @IdpId    WHERE PartyId = @OrganizationPartyId 
select @OrganizationPartyId as Id
END          
END   
END TRY    
  BEGIN CATCH        
   DECLARE @ErrorLogID INT;    
   EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
   SELECT  0 AS Id,    
     ErrorMessage    
   FROM    [dbo].ErrorLog    
   WHERE   ErrorLogID = @ErrorLogID;    
  END CATCH    
END;    