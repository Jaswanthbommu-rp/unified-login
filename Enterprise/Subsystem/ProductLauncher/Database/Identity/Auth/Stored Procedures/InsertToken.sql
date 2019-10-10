CREATE PROCEDURE [Auth].[InsertToken]
		@TokenKey		NVARCHAR (128)     ,
		@TokenType		INT     ,
		@ClientCode		NVARCHAR (200) ,
		@SubjectCode	NVARCHAR (200),
		@Expiry			DATETIMEOFFSET (7),
		@JsonCode		NVARCHAR (MAX),
		@AuthCodeChallenge	NVARCHAR (250)    ,
		@AuthCodeChallengeMethod	NVARCHAR (50)     , 
		@IsOpenId		BIT,
		@Nonce			NVARCHAR (200),     
		@RedirectUri	NVARCHAR (2000)   ,
		@SessionId		NVARCHAR (200) ,
		@WasConsentShown BIT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Auth].[Tokens]
           ([TokenKey]
           ,[TokenType]
           ,[ClientCode]
           ,[SubjectCode]
           ,[Expiry]
           ,[JsonCode]
           ,[AuthCodeChallenge]
           ,[AuthCodeChallengeMethod]
           ,[IsOpenId]
           ,[Nonce]
           ,[RedirectUri]
           ,[SessionId]
           ,[WasConsentShown])
     VALUES
           (@TokenKey
           ,@TokenType
           ,@ClientCode
           ,@SubjectCode
           ,@Expiry
           ,@JsonCode
           ,@AuthCodeChallenge
           ,@AuthCodeChallengeMethod
           ,@IsOpenId
           ,@Nonce
           ,@RedirectUri
           ,@SessionId
           ,@WasConsentShown)
END
GO

 