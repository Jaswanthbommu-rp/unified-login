IF OBJECT_ID('[Auth].[InsertConsent]') IS NOT NULL
	DROP PROCEDURE [Auth].[InsertConsent];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[InsertConsent]
	@SubjectCode  NVARCHAR (200)  ,
    @ClientCode NVARCHAR (200) ,
    @Scopes   NVARCHAR (2000) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Auth].[Consents] ([SubjectCode],[ClientCode],[Scopes]) VALUES  (@SubjectCode, @ClientCode, @Scopes)

END
GO
