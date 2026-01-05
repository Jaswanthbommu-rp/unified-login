
CREATE PROCEDURE [Logging].[ListActivity] (
	@SearchCriteriaTPV SEARCHCRITERIA READONLY,
	@SortOrderColumnName NVARCHAR(100),
	@SortOrder NVARCHAR(100),
	@RowsPerPage INT = 0,
	@PageNumber INT = 1,
	@IncludeRPEmployeeActivity BIT = 0,
	@TotalRows INT OUTPUT
)
AS
BEGIN 
SET NOCOUNT ON;

	DECLARE @SelectColumns NVARCHAR(MAX)
	DECLARE @SelectCount NVARCHAR(MAX)
	DECLARE @From NVARCHAR(MAX)
	DECLARE @OrderByOffset NVARCHAR(MAX) = ''
	DECLARE @SearchCriteria NVARCHAR(4000) = ''
	DECLARE @OffsetMinutes VARCHAR(5)
	DECLARE @InternationalDateFormat VARCHAR(100) 
	DECLARE @InternationalTimeFormat VARCHAR(100) = 'hh:mm:s tt'  

		 
	SELECT	@RowsPerPage =
		CASE
			WHEN @RowsPerPage <= 0 THEN 2147483647
			ELSE @RowsPerPage
		END

	--OffsetMinutes passed from the UI and is used to adjust the ApplicationTimeStamp to local timezone
	--Name: OffsetMinutes
	--Value: In Minutes
	SELECT		@OffsetMinutes = [Value]
	FROM		@SearchCriteriaTPV
	WHERE	[Name] = 'OffsetMinutes'

	SET @OffsetMinutes = ISNULL(@OffsetMinutes, '0');

	SELECT  @InternationalTimeFormat = Value    
	FROM  @SearchCriteriaTPV    
	WHERE Name = 'InternationalTimeFormat'  

	SET @InternationalTimeFormat = CASE @InternationalTimeFormat  
										WHEN '12Hours' THEN 'hh:mm tt' 
										WHEN '24Hours' THEN 'HH:mm'           
										ELSE 'hh:mm:s tt'  
									END

	SELECT  @InternationalDateFormat = Value  
	FROM  @SearchCriteriaTPV  
	WHERE Name = 'InternationalDateFormat'

	SET @InternationalDateFormat = CASE @InternationalDateFormat  
										WHEN 'mm/dd/yyyy' THEN CONVERT(varchar,'MM/dd/yyyy ' + @InternationalTimeFormat)  
										WHEN 'dd/mm/yyyy' THEN CONVERT(varchar,'dd/MM/yyyy ' + @InternationalTimeFormat) 
										WHEN 'yyyy/mm/dd' THEN CONVERT(varchar,'yyyy/MM/dd ' + @InternationalTimeFormat)  
										ELSE 'MM/dd/yyyy hh:mm:s tt'  
									END 


	SET @SelectColumns = '
	SELECT		A.ActivityId,
					LT.Name AS ''LogActivityTypeName'',
					LCT.Name AS ''LogCategoryName'',
					A.Message AS ''Message'',
					ful.LoginName AS ''FromUserLoginName'',
					FUL.FirstName AS ''FromUserFirstName'',
					FUL.LastName AS ''FromUserLastName'',
					FUL.RealPageId AS ''FromUserRealPageId'',
					A.ApplicationTimeStamp,
					FORMAT(DATEADD(minute, ' + @OffsetMinutes + ', A.ApplicationTimeStamp), '''+ @InternationalDateFormat +''') AS ''ApplicationTimestampOffset'',
					ContextId,
					ContextReferenceId
				'
	SET @From = '
			FROM	
				Logging.Activity A WITH(NOLOCK)
				INNER JOIN Logging.LogType LT WITH(NOLOCK) ON A.LogTypeId = LT.LogTypeId
				INNER JOIN Logging.LogCategoryType LCT WITH(NOLOCK) ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId
				INNER JOIN Logging.UserLogin FUL WITH(NOLOCK) ON FUL.UserId = A.CreatedBy --FromUserId 
				'

	SET @SelectCount = '
    SELECT @TotalRows = COUNT (ActivityId)
    '
	IF EXISTS (SELECT 1 FROM @SearchCriteriaTPV)
	BEGIN

		SELECT 
			@SearchCriteria =
				COALESCE(@SearchCriteria +' AND '+
				CASE
					WHEN [Name] = 'Message' THEN ' ( '+'CHARINDEX(''' + CONVERT(NVARCHAR(200), REPLACE([VALUE],'''','''''')) + ''', A.Message, 1) > 0 ' +' ) ' 
					WHEN [Name] = 'StartDate' THEN 'A.ApplicationTimeStamp  > ' + '''' + [Value] + '''' 
					WHEN [Name] =  'EndDate' THEN 'A.ApplicationTimeStamp <= ' + '''' + [Value] + '''' 
					WHEN [Name] = 'FromRealPageId' THEN 'FUL.RealPageId = ' + '''' + [Value] + '''' 
					WHEN [Name] = 'RealPageId' THEN '(FUL.RealPageId = ' + '''' + [Value] + '''' + ' OR ' + 'A.ContextReferenceId = ' + '''' + [Value] + ''')'
					WHEN [Name] = 'LogCategoryTypeId' THEN 'LCT.LogCategoryTypeId IN (' +  [Value] + ')'
					WHEN [Name] = 'ToRealPageId' THEN 'A.ContextReferenceId = ' + '''' + [Value] + '''' --In future we can remove it if  from UI passing ContextReferenceId instead of ToRealPageId
					ELSE 'A.' + [Name] + ' = ' + '''' + [Value] + '''' 
				END ,'')
		FROM 
			@SearchCriteriaTPV
		WHERE
			[Name] NOT IN ('OffsetMinutes', 'SaveFormat','InternationalDateFormat','InternationalTimeFormat');

		SET @SearchCriteria = CHAR(9) + 'WHERE' + CHAR(9) + SUBSTRING(@SearchCriteria,5,LEN(@SearchCriteria)) +
							  'AND A.IsRealPageEmployee IN (0,' + CAST(@IncludeRPEmployeeActivity AS NVARCHAR(1)) + ')'

	END

	IF (ISNULL(@SortOrderColumnName,'') = '')
	BEGIN
		SET @SortOrderColumnName = 'ApplicationTimeStamp'
	END;

	IF (ISNULL(@SortOrder,'') = '')
	BEGIN
		SET @SortOrder = 'DESC'
	END;

	SET @OrderByOffset = CHAR(13) + CHAR(9) + 'ORDER BY '+
		CASE
			WHEN @SortOrderColumnName = 'Logtypename' THEN 'LT.Name'
			ELSE 'A.'+@SortOrderColumnName
		END + 
		CHAR(13) + CHAR(9) + @SortOrder + 
		CHAR(13) + CHAR(9) + 'OFFSET (' + CONVERT(varchar(10), @PageNumber) + ' - 1) * ' + CONVERT(varchar(10), @RowsPerPage) + ' ROWS' + 
		CHAR(13) + CHAR(9) + 'FETCH NEXT ' + CONVERT(varchar(10), @RowsPerPage) + ' ROWS ONLY'

	SET @SelectCount = @SelectCount + CHAR(13) + @From + ISNULL(@SearchCriteria, '')

	SET @SelectColumns = @SelectColumns + CHAR(13) + @From + ISNULL(@SearchCriteria, '') + @OrderByOffset

	--PRINT @SelectColumns;
	--PRINT @SelectCount;

	EXECUTE (@SelectColumns)	
	--EXECUTE (@SelectCount)		
	EXEC SP_EXECUTESQL @SelectCount, N'@TotalRows INT output', @TotalRows out

END;
