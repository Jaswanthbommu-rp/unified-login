CREATE PROCEDURE Enterprise.OrganizationIDPList(@OrganizationPartyId Int)  

AS  

         BEGIN  
		 SELECT DISTINCT    
                    ipt.Description AS 'IDPName',    
                    ipt.ContactMechanismId AS 'ContactMechanismId'    
             FROM Enterprise.PartyContactMechanism PCM    
                  INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PCM.ContactMechanismId and pcm.ThruDate is NULL  
             WHERE PCM.PartyId = @OrganizationPartyId or ipt.Description in ('IdentityServer','Azure AD','OIDC Provider For Google')

         END;  