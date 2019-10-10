IF OBJECT_ID('[Auth].[DeleteConsentBySubjectAndClient]') IS NOT NULL
	DROP PROCEDURE [Auth].[DeleteConsentBySubjectAndClient];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[DeleteConsentBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode			NVARCHAR(200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	delete FROM Auth.Consents WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode

END
GO
