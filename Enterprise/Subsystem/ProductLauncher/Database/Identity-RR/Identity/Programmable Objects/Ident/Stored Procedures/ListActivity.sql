IF OBJECT_ID('[Ident].[ListActivity]') IS NOT NULL
	DROP PROCEDURE [Ident].[ListActivity];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
 
CREATE PROCEDURE [Ident].[ListActivity]
AS
BEGIN
	 
	SET NOCOUNT ON;

    SELECT   [ActivityId] ,[ActivityCode] ,[Description]
      ,[MaxActivityAttemptCount] ,[ActivityTokenExpirationMinutes]
	FROM [Ident].[Activity]
END
GO
