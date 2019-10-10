IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'identityserver')
CREATE LOGIN [identityserver] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [IdentityServer] FOR LOGIN [identityserver] WITH DEFAULT_SCHEMA=[Auth]
GO
REVOKE CONNECT TO [IdentityServer]
