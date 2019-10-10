IF OBJECT_ID('[Enterprise].[ListProductBatchByRealPageId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductBatchByRealPageId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductBatchByRealPageId](
	 @RealPageId UNIQUEIDENTIFIER
	,@AssignUserPersonaId bigint
	) 

AS

BEGIN

	SET NOCOUNT ON;
		
	SELECT 
		 [ProductBatchId]
		,[PersonPartyId]
		,[CreateUserPersonaId]
		,[AssignUserPersonaId]
		,[ProductId]
		,[StatusTypeId]
		,[RetryCount]
		,[InputJson]
		,[LastRunDate]
		,[CreatedDate]
		,[ModifiedDate]
		,[ErrorDetails]
	FROM [Enterprise].[ProductBatch]
	JOIN Enterprise.Party ON ProductBatch.PersonPartyId = Party.PartyId
	WHERE Party.RealPageId = @RealPageId
	AND AssignUserPersonaId = @AssignUserPersonaId
END;
GO
