 -- ============================================= 
-- Author: Monte Jennings 
-- Create date:
-- Description: Searches the Field table for the record with the indicated criteria. 
-- ============================================= 
CREATE PROCEDURE [CustomField].[GetFieldsByMasterId] (
	@SourceId bigint 
	,@DataImportApplicationId int = 1
	,@FilterBy nvarchar(max)
	,@SortBy nvarchar(max)
	,@RowsPerPage int = 0
	,@PageNumber int = 1
) 
AS 
BEGIN
	DECLARE @SortOrder nvarchar(128) = N'InitialSort',
		@SortDirection nvarchar(4) = N'ASC',
		@SortValue int,
		@FilterEnabled bit = NULL

	SELECT	@RowsPerPage = CASE
		WHEN @RowsPerPage <= 0 THEN 2147483647
		ELSE @RowsPerPage
	END;

	IF (ISJSON(@SortBy) = 0)
	BEGIN
		SET @SortBy = NULL
	END

	IF (ISJSON(@FilterBy) = 0)
	BEGIN
		SET @FilterBy = NULL
	END

	SELECT	@FilterEnabled = CONVERT(bit, FilterValue)
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					FilterValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0
	AND			ColumnName = 'Enabled'

	SELECT	@SortValue =
		CASE SortDirection
			WHEN N'ASC' THEN
				CASE ColumnName
					WHEN N'InitialSort' THEN 100
					WHEN N'Sequence' THEN 100
					WHEN N'Name' THEN 101
					ELSE 100
				END
			WHEN N'DESC' THEN
				CASE @sortOrder
					WHEN N'InitialSort' THEN -100
					WHEN N'Sequence' THEN -100
					WHEN N'Name' THEN -101
					ELSE -100
				END
		END
	FROM	OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SortDirection nvarchar(max) '$.SortDirection'
				)
	WHERE	ISJSON(@SortBy) > 0;

	WITH cteCustomFileds
	(
		FieldId,
		OrganizationId,
		Enabled,
		Name,
		Description,
		FieldTypeId,
		FieldTypeName,
		Required,
		ReadOnly,
		DefaultValue,
		SyncField,
		Sequence,
		HelpText,
		MinCharLength,
		MaxCharLength,
		TotalRecords,
		RowNumber
	)
	AS
	(
		SELECT	cff.[FieldId]
						,cff.[OrganizationId]
						,cff.[Enabled]
						,cff.[Name]
						,cff.[Description]
						,cff.[FieldTypeId]
						,cfft.[Name]
						,cff.[Required]
						,cff.[ReadOnly]
						,cff.[DefaultValue]
						,cff.[SyncField]
						,cff.[Sequence]
						,cff.[HelpText]
						,cff.[MinCharLength]
						,cff.[MaxCharLength]
						,COUNT(1) OVER () AS TotalRecords
						,CASE @SortValue
							WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY cff.[Sequence] ASC)
							WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY cff.Name ASC)
							WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY cff.[Sequence] ASC)
							WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY cff.Name ASC)
					END AS RowNumber
		FROM	[CustomField].[Field] cff
					INNER JOIN [CustomField].FieldType cfft ON cff.FieldTypeId = cfft.FieldTypeId
					INNER JOIN Enterprise.DataImportMapping edim ON cff.[OrganizationId] = edim.PartyId
		WHERE	edim.SourceId = @SourceId 
		AND			edim.DataImportApplicationId = @DataImportApplicationId
		AND			((@FilterEnabled IS NULL) OR (cff.[Enabled] = @FilterEnabled))
	)


	SELECT	FieldId
					,OrganizationId
					,Enabled
					,Name
					,Description
					,FieldTypeId
					,FieldTypeName
					,Required
					,ReadOnly
					,DefaultValue
					,SyncField
					,Sequence
					,HelpText
					,MinCharLength
					,MaxCharLength
					,TotalRecords
	FROM	cteCustomFileds
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS
	FETCH NEXT(@RowsPerPage) ROWS ONLY
END 