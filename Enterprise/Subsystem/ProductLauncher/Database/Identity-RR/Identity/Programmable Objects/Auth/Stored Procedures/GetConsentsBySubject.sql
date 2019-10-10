IF OBJECT_ID('[Auth].[GetConsentsBySubject]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetConsentsBySubject];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetConsentsBySubject]
	@SubjectCode		NVARCHAR (200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Consents WHERE SubjectCode = @SubjectCode 

END
GO
