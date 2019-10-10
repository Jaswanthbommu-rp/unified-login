CREATE TABLE [Auth].[Tokens]
(
[TokenKey] [nvarchar] (128) NOT NULL,
[TokenType] [int] NOT NULL,
[ClientCode] [nvarchar] (200) NOT NULL,
[SubjectCode] [nvarchar] (200) NULL,
[JsonCode] [nvarchar] (3072) NULL,
[AuthCodeChallenge] [nvarchar] (250) NULL,
[AuthCodeChallengeMethod] [nvarchar] (50) NULL,
[Nonce] [nvarchar] (200) NULL,
[RedirectUri] [nvarchar] (2000) NULL,
[SessionId] [nvarchar] (200) NULL,
[IsOpenId] [bit] NULL,
[WasConsentShown] [bit] NULL,
[Expiry] [datetimeoffset] NOT NULL
)
GO
ALTER TABLE [Auth].[Tokens] ADD CONSTRAINT [PK_Tokens] PRIMARY KEY CLUSTERED  ([TokenKey], [TokenType])
GO
