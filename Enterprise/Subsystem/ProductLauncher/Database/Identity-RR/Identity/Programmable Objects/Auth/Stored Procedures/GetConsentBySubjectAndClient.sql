IF OBJECT_ID('[Auth].[GetConsentBySubjectAndClient]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetConsentBySubjectAndClient];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetConsentBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode			NVARCHAR(200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Consents WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode

END
GO
