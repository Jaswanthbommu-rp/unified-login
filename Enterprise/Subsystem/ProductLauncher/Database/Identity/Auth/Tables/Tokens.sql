CREATE TABLE [Auth].[Tokens] (
    [TokenKey]                NVARCHAR (128)     NOT NULL,
    [TokenType]               INT                NOT NULL,
    [ClientCode]              NVARCHAR (200)     NOT NULL,
    [SubjectCode]               NVARCHAR (200)     NULL,    
    [JsonCode]                NVARCHAR (MAX)    NULL,
    [AuthCodeChallenge]       NVARCHAR (250)     NULL,
    [AuthCodeChallengeMethod] NVARCHAR (50)      NULL, 
    [Nonce]                   NVARCHAR (200)     NULL,
    [RedirectUri]             NVARCHAR (2000)    NULL,
    [SessionId]               NVARCHAR (200)     NULL,
	[IsOpenId]                BIT                NULL,
    [WasConsentShown]         BIT                NULL,
	[Expiry]                  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_Tokens] PRIMARY KEY CLUSTERED ([TokenKey] ASC, [TokenType] ASC)
);

