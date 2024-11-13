
IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientScopes' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientScopes
END

GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientClaims' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientClaims
END

GO
IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientSecrets' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientSecrets
END

GO
IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientUserClaim' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientUserClaim
END

GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ScopeClaims' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ScopeClaims
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ScopeSecrets' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ScopeSecrets
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Tokens' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Tokens
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Certificates' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Certificates
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'CertificatesType' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.CertificatesType
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'CertificatesLocationType' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.CertificatesLocationType
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Consents' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Consents
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Scopes' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Scopes
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientRedirectUris' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientRedirectUris
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientPostLogoutRedirectUris' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientPostLogoutRedirectUris
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientIdentityProviderRestrictions' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientIdentityProviderRestrictions
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientCustomGrantTypes' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientCustomGrantTypes
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'ClientCorsOrigins' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.ClientCorsOrigins
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Claim' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Claim
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'DeveloperClients' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.DeveloperClients
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.objects so INNER JOIN sys.objects po ON po.object_id = so.parent_object_id WHERE so.name = 'FK_Clients_ClientID' AND po.name = 'Product' AND so.schema_id = SCHEMA_ID('auth'))
BEGIN
	ALTER TABLE [Auth].[Product] DROP CONSTRAINT [FK_Clients_ClientID]
END

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'Clients' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.Clients
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'DeviceCodes' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.DeviceCodes
END
GO

IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = 'PersistedGrants' AND schema_id = SCHEMA_ID('auth'))
BEGIN
	DROP TABLE Auth.PersistedGrants
END
GO

------------ STORED PROCS ------------

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClaimDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClaimDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClaimInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClaimInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClaimSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClaimSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClaimUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClaimUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientClaimsDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientClaimsDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientClaimsInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientClaimsInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientClaimsSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientClaimsSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientClaimsUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientClaimsUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientPostLogoutRedirectUrisDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientPostLogoutRedirectUrisDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientPostLogoutRedirectUrisInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientPostLogoutRedirectUrisInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientPostLogoutRedirectUrisSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientPostLogoutRedirectUrisSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientPostLogoutRedirectUrisUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientPostLogoutRedirectUrisUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientRedirectUrisDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientRedirectUrisDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientRedirectUrisInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientRedirectUrisInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientRedirectUrisSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientRedirectUrisSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientRedirectUrisUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientRedirectUrisUpdate
END
GO


IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientScopesDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientScopesDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientScopesInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientScopesInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientScopesSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientScopesSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientScopesUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientScopesUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientSecretsDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientSecretsDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientSecretsInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientSecretsInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientSecretsSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientSecretsSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientSecretsUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientSecretsUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientUserClaimDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientUserClaimDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientUserClaimInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientUserClaimInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientUserClaimSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientUserClaimSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientUserClaimUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientUserClaimUpdate
END
GO


IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeClaimsDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeClaimsDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeClaimsInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeClaimsInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeClaimsSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeClaimsSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeClaimsUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeClaimsUpdate
END
GO


IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeSecretsDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeSecretsDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeSecretsInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeSecretsInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeSecretsSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeSecretsSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopeSecretsUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopeSecretsUpdate
END
GO


IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopesDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopesDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopesInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopesInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopesSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopesSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ScopesUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ScopesUpdate
END
GO


IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientsDelete' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientsDelete
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientsInsert' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientsInsert
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientsUpdate' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientsUpdate
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'ClientsSelect' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.ClientsSelect
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'UnlockUsers' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.UnlockUsers
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchScopeSecret' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchScopeSecret
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'InsertToken' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.InsertToken
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'InsertConsent' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.InsertConsent
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'DeleteConsentBySubjectAndClient' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.DeleteConsentBySubjectAndClient
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'DeleteTokenByKey' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.DeleteTokenByKey
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetUsers' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetUsers
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClient' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClient
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClientClaim' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClientClaim
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClientPostLogoutRedirectUri' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClientPostLogoutRedirectUri
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClientRedirectUri' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClientRedirectUri
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClientScope' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClientScope
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchClientSecret' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchClientSecret
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchScope' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchScope
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'SearchScopeClaim' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.SearchScopeClaim
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetToken' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetToken
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetUserClaimTypesRequiredForClient' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetUserClaimTypesRequiredForClient
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllScopes' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllScopes
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllPortfolioProductUserClaims' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllPortfolioProductUserClaims
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'DeleteTokenBySubjectAndClient' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.DeleteTokenBySubjectAndClient
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetProducts' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetProducts
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClientByClientCode' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClientByClientCode
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClientDetails' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClientDetails
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllScopeClaims' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllScopeClaims
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllScopeSecrets' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllScopeSecrets
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllSecurityQuestions' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllSecurityQuestions
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClientSecretsByClientId' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClientSecretsByClientId
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetConsentBySubjectAndClient' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetConsentBySubjectAndClient
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClientScopesByClientId' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClientScopesByClientId
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetConsentsBySubject' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetConsentsBySubject
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAuthenticateUser' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAuthenticateUser
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClientRedirectUrisByClientId' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClientRedirectUrisByClientId
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetEnterpriseUserLockActivities' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetEnterpriseUserLockActivities
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllPortfolioProductUser' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllPortfolioProductUser
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetEnterpriseUserStatus' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetEnterpriseUserStatus
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetTokensBySubject' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetTokensBySubject
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'CreatePasswordPolicy' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.CreatePasswordPolicy
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetClaimsWithProducts' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetClaimsWithProducts
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'GetAllOrganizationClientUserClaims' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.GetAllOrganizationClientUserClaims
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'UpdateConsent' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.UpdateConsent
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'UpdatePasswordPolicy' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.UpdatePasswordPolicy
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'UpdateActivityTokenFlag' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.UpdateActivityTokenFlag
END
GO

IF EXISTS ( SELECT TOP 1 1 FROM sys.objects WHERE name = 'UpdateTokenExpiry' AND schema_id = SCHEMA_ID('auth') AND type = 'p' )
BEGIN
	DROP PROCEDURE Auth.UpdateTokenExpiry
END
GO
