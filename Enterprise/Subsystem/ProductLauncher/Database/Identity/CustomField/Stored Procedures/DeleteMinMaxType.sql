
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the MinMaxType table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteMinMaxType (
	 @MinMaxTypeId TINYINT = NULL 
	,@MinMaxTypeName NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[MinMaxType]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@MinMaxTypeName IS NULL  OR  [MinMaxTypeName] = @MinMaxTypeName)

	DELETE 
		[CustomField].[MinMaxType]

	WHERE
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@MinMaxTypeName IS NULL  OR  [MinMaxTypeName] = @MinMaxTypeName)

	RETURN 1 --for success

END

