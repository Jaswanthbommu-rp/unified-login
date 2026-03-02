
CREATE PROCEDURE [Ident].[GetUserLoginOnly]  
(  
 @EnterpriseUserName VARCHAR(255) = NULL,  
 @UserId    INT          = NULL,  
 @RealPageId   UNIQUEIDENTIFIER = NULL  
)  
AS  
BEGIN  
 IF @RealPageId IS NOT NULL  
 BEGIN  
  SELECT   
   ul.UserId,  
   ul.PersonPartyId [PartyId],  
   P1.RealPageId,  
   ul.[LoginName],  
   ul.PasswordModifiedDate,  
   ul.PasswordHash,  
   ul.PasswordSalt,  
   ul.LastLoginDate [LastLogin], 
   case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],  
   ul.TwoFactorEnabled [TwoFactorEnabled],
   ulp.UserDeactivationDate [UserDeactivationDate],
   ulp.PrimaryOrganization ,
   pr.RoleTypeIdFrom as UserRoleTypeId,
   ulp.IsRPEmployee
  FROM Ident.UserLogin ul  
   INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId  
   INNER JOIN Ident.UserLoginPersona ulp on ul.UserId = ulp.UserLoginId
   INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId  
   INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = p1.PartyId AND pr.PartyIdTo = ulp.OrganizationPartyId AND pr.RoleTypeIdTo = 205 AND pr.ThruDate IS NULL
  WHERE  
   (   
    P1.RealPageId = @RealPageId  
   )  
 END  
 ELSE IF @UserId IS NOT NULL  
 BEGIN  
  SELECT   
   ul.UserId,  
   ul.PersonPartyId,  
   P1.RealPageId,  
   ul.[LoginName],  
   ul.PasswordModifiedDate,  
   ul.PasswordHash,  
   ul.PasswordSalt,  
   ul.LastLoginDate [LastLogin],  
   case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],  
   ul.TwoFactorEnabled [TwoFactorEnabled]  ,
   ulp.UserDeactivationDate [UserDeactivationDate],
   ulp.PrimaryOrganization ,
   pr.RoleTypeIdFrom as UserRoleTypeId,
   ulp.IsRPEmployee
  FROM Ident.UserLogin ul  
   INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId  
   INNER JOIN Ident.UserLoginPersona ulp on ul.UserId = ulp.UserLoginId
   INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId  
   INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = p1.PartyId AND pr.PartyIdTo = ulp.OrganizationPartyId AND pr.RoleTypeIdTo = 205 AND pr.ThruDate IS NULL

  WHERE  
   ul.UserId = @UserId  
 END  
  
 ELSE IF @EnterpriseUserName is not null  
 BEGIN  
  SELECT   
   ul.UserId,  
   ul.PersonPartyId,  
   P1.RealPageId,  
   ul.[LoginName],  
   ul.PasswordModifiedDate,  
   ul.PasswordHash,  
   ul.PasswordSalt,  
   ul.LastLoginDate [LastLogin],  
   case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],  
   ul.TwoFactorEnabled [TwoFactorEnabled],
    ulp.UserDeactivationDate [UserDeactivationDate],
   ulp.PrimaryOrganization ,
   pr.RoleTypeIdFrom as UserRoleTypeId,
   ulp.IsRPEmployee
  FROM Ident.UserLogin ul  
   INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId  
   INNER JOIN Ident.UserLoginPersona ulp on ul.UserId = ulp.UserLoginId
   INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId  
   INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = p1.PartyId AND pr.PartyIdTo = ulp.OrganizationPartyId AND pr.RoleTypeIdTo = 205 AND pr.ThruDate IS NULL

  WHERE  
   ul.LoginName = @EnterpriseUserName  
 END  
END;


