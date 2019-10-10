CREATE TABLE [Auth].[Users]
(
[UserId] [bigint] NOT NULL IDENTITY(1, 1),
[LoginId] [nvarchar] (50) NOT NULL,
[Firstname] [nvarchar] (50) NOT NULL,
[LastName] [nvarchar] (50) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_Auth.Users_IsActive] DEFAULT ((1)),
[PasswordHash] [nvarchar] (1000) NULL,
[PasswordSalt] [nvarchar] (50) NULL,
[IdentityProvider] [nvarchar] (100) NOT NULL,
[Title] [nvarchar] (50) NULL,
[Email] [nvarchar] (50) NULL,
[Phone] [nvarchar] (20) NULL,
[IsLocked] [bit] NOT NULL CONSTRAINT [DF_Users_IsLocked] DEFAULT ((0)),
[IsTainted] [bit] NOT NULL CONSTRAINT [DF_Users_IsTainted] DEFAULT ((0)),
[LastPasswordModifiedDateTime] [datetime] NULL,
[AccountExpiration] [datetime] NULL
)
GO
ALTER TABLE [Auth].[Users] ADD CONSTRAINT [PK_Auth.Users] PRIMARY KEY CLUSTERED  ([UserId])
GO
