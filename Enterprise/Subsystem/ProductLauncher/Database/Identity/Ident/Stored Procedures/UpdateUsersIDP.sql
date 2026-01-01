CREATE PROCEDURE [Ident].[UpdateUsersIDP]    
(    
  @OrganizationPartyId BIGINT    
 ,@UserIds [Enterprise].[BigIntListType] READONLY    
 ,@IsEnabled BIT    
)    
AS    
BEGIN    
 BEGIN TRY        
  BEGIN TRAN;      
   DECLARE @IDPValue INT;    
   
   -- Status Type IDs (verified from StatusType table)
   DECLARE @ActiveStatusId INT = 1;       -- Active
   DECLARE @PendingStatusId INT = 2;      -- Pending
   DECLARE @LockedStatusId INT = 3;       -- Locked
   DECLARE @ExpiredStatusId INT = 23;     -- Expired
   DECLARE @DisabledStausID INT = 24;     -- Disabled
    
   IF(@IsEnabled = 1)    
   BEGIN    
    --Find Company IDP in Enterprise.Organization table     
    SELECT @IDPValue = O.IdentityProviderTypeId    
    FROM Enterprise.Organization O     
    WHERE O.PartyId = @OrganizationPartyId    
   END    
   ELSE    
   BEGIN    
    DECLARE @OldIDPValue INT;    
    SELECT @OldIDPValue = O.IdentityProviderTypeId    
    FROM Enterprise.Organization O     
    WHERE O.PartyId = @OrganizationPartyId    
    
    SELECT @IDPValue = ipt.IdentityProviderTypeId FROM enterprise.PartyContactMechanism pcm     
    INNER JOIN ident.IdentityProviderType ipt ON ipt.ContactMechanismId = pcm.ContactMechanismId     
    WHERE pcm.PartyId = @OrganizationPartyId AND ipt.IdentityProviderTypeId <> @OldIDPValue    
   END    
    
   -- Update IDP for users
   UPDATE UL SET UL.IdentityProviderTypeId = @IDPValue    
   FROM IDENT.USERLOGIN UL    
   JOIN IDENT.USERLOGINPERSONA ULP ON UL.UserId = ULP.UserLoginId     
   JOIN PERSON.PERSONA P ON P.UserLoginPersonaId = ULP.UserLoginPersonaId    
   JOIN Enterprise.PartyRelationship  PR ON PR.PartyIdFrom = UL.PersonPartyId AND PR.PartyIdTo = ULP.OrganizationPartyId    
   JOIN @UserIds UIDS ON UIDS.Id = UL.UserId    
   WHERE ULP.OrganizationPartyId = @OrganizationPartyId     
   AND ULP.PrimaryOrganization = 1     
   AND UL.IdentityProviderTypeId <> @IDPValue    
   AND PR.RoleTypeIdFrom <> 405     
   AND PR.RoleTypeIdTo = 205     
   AND PR.ThruDate IS NULL    

   -- UPDATE USER STATUS: Pending/Expired -> Active
   -- When IDP toggle is changed (enabled or disabled),
   -- users with Pending or Expired status should become Active
   UPDATE ULP 
   SET ULP.StatusTypeId = @ActiveStatusId,
       ULP.StatusThruDate = NULL
   FROM IDENT.USERLOGINPERSONA ULP
   JOIN IDENT.USERLOGIN UL ON UL.UserId = ULP.UserLoginId
   JOIN @UserIds UIDS ON UIDS.Id = UL.UserId
   WHERE ULP.OrganizationPartyId = @OrganizationPartyId
   AND ULP.PrimaryOrganization = 1
   AND ULP.StatusTypeId IN (@PendingStatusId, @ExpiredStatusId,@LockedStatusId,@DisabledStausID)
    
   SELECT 1 AS BIT;      
  COMMIT;         
 END TRY        
 BEGIN CATCH        
  DECLARE @ErrorLogID INT;        
  EXEC dbo.LogError        
  @ErrorLogID = @ErrorLogID OUTPUT;        
  ROLLBACK;        
 END CATCH;     
    
END
GO