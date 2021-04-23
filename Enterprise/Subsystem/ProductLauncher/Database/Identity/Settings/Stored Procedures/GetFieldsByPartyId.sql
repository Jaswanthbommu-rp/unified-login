Create Procedure [Settings].[GetFieldsByPartyId](
	 @PartyId bigint 	
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

		DECLARE @CustomFields table(
			[FieldId] [bigint] ,
			[OrganizationId] [bigint],
			[Enabled] [bit] ,
			[Name] [nvarchar](100) ,
			[Description] [nvarchar](500) ,
			[FieldTypeId] [tinyint] ,
			[FieldTypeName] [nvarchar](100) ,
			[Required] [bit] ,
			[ReadOnly] [bit] ,
			[DefaultValue] [nvarchar](max) ,
			[SyncField] [nvarchar](150) ,
			[Sequence] [smallint]  ,
			[HelpText] [varchar](max) ,
			[MinCharLength] [int] ,
			[MaxCharLength] [int] ,
			[TotalRecords] int  ,
			RowNumber int  )

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

	INSERT INTO @CustomFields
	SELECT 	 FieldId	
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
		,COUNT(1) OVER () AS TotalRecords
		,CASE @SortValue
								WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY [Sequence] ASC)
								WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY Name ASC)
								WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY [Sequence] ASC)
								WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY Name ASC)
						END AS RowNumber
	From (
		Select sr.[SettingTableRowId] AS 'FieldId',
			   st.[PartyId] AS 'OrganizationId',
			   [TableColumnName],
			   [TableColumnValue]
		from [Settings].[SettingTableColumn] stc
		join [Settings].[SettingTableRow] sr on
			stc.[SettingTableRowId] = sr.[SettingTableRowId]
		join [Settings].[SettingTable] st on
			st.[SettingTableId] = sr.[SettingTableId]
		where st.[PartyId] = @PartyId) As SourceTable
	PIVOT
	(
		MIN([TableColumnValue])
		FOR [TableColumnName] IN (Enabled
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
		,MaxCharLength)) AS PivotOutput

		UPDATE cf SET cf.FieldTypeName = sp.MappingName
		FROM @CustomFields cf
		JOIN Settings.SettingPicklist sp ON
			cf.FieldTypeId = sp.MappingValue
		Where sp.CategoryName = 'CustomFields'

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
	FROM	@CustomFields
	Where ((@FilterEnabled IS NULL) OR ([Enabled] = @FilterEnabled))
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS
	FETCH NEXT(@RowsPerPage) ROWS ONLY
END