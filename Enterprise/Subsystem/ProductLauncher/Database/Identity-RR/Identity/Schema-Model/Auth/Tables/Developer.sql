CREATE TABLE [Auth].[Developer]
(
[DeveloperId] [bigint] NOT NULL IDENTITY(1, 1),
[LoginId] [nvarchar] (128) NOT NULL,
[Name] [nvarchar] (200) NOT NULL,
[LogoUri] [nvarchar] (2000) NULL,
[WebsiteUri] [nvarchar] (2000) NULL,
[Phone] [nvarchar] (30) NULL,
[ValidationToken] [nvarchar] (256) NOT NULL,
[PasswordHashed] [nvarchar] (250) NOT NULL,
[PasswordSalt] [nvarchar] (16) NULL,
[VersionId] [timestamp] NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF__Developer__IsAct__6166761E] DEFAULT ((1)),
[IsAccountValidated] [bit] NOT NULL CONSTRAINT [DF__Developer__IsAcc__625A9A57] DEFAULT ((0)),
[ValidationTokenExpiry] [datetime] NULL,
[DateCreated] [datetime] NOT NULL CONSTRAINT [DF__Developer__DateC__634EBE90] DEFAULT (getutcdate()),
[DateModified] [datetime] NULL CONSTRAINT [DF__Developer__DateM__6442E2C9] DEFAULT (getutcdate()),
[LastLoginDate] [datetime] NULL CONSTRAINT [DF__Developer__LastL__65370702] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Auth].[Developer] ADD CONSTRAINT [PK_dbo.Developer] PRIMARY KEY CLUSTERED  ([DeveloperId])
GO
