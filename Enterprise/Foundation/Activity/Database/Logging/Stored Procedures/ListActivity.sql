
CREATE PROCEDURE [Logging].[ListActivity] (
	@SearchCriteriaTPV SEARCHCRITERIA READONLY,
	@SortOrderColumnName nvarchar(100),
	@SortOrder nvarchar(100),
	@RowsPerPage int = 0,
	@PageNumber int = 1,
	@TotalRows int = NULL OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @LogTypeId int;
	DECLARE @LogcategoryTypeId int;
	DECLARE @OrganizationID int;
	DECLARE @PropertyId int;
	DECLARE @IsSystemAdminActivity bit;
	DECLARE @ProductId int;
	DECLARE @FromUserId bigint;
	DECLARE @RealPageId uniqueidentifier;
	DECLARE @ToUserId bigint;
	DECLARE @TimeStamp datetime;
	DECLARE @Cmd01 nvarchar(MAX);
	DECLARE @Cmd02 nvarchar(MAX);
	DECLARE @Count int;
	DECLARE @SearchCriteria nvarchar(4000);
	DECLARE @DefaultSortCriteria nvarchar(1000);
	DECLARE @DefaultSortOrder varchar(4);
	DECLARE @SCTPVRowId int;
	DECLARE @SCName nvarchar(200);
	DECLARE @SCValue nvarchar(200);
	DECLARE @SOName nvarchar(100);
	DECLARE @SOTPVRowId int;
	DECLARE @OffsetMinutes varchar(5);
		 
	DECLARE @TotalRows_t TABLE (RowCnt int)

	SELECT	@RowsPerPage =
		CASE
			WHEN @RowsPerPage <= 0 THEN 2147483647
			ELSE @RowsPerPage
		END

	SET @DefaultSortCriteria = 'ApplicationTimeStamp';
	SET @DefaultSortOrder = 'DESC';

	--OffsetMinutes passed from the UI and is used to adjust the ApplicationTimeStamp to local timezone
	--Name: OffsetMinutes
	--Value: In Minutes
	SELECT		@OffsetMinutes = Value
	FROM		@SearchCriteriaTPV
	WHERE	Name = 'OffsetMinutes'

	SET @OffsetMinutes = ISNULL(@OffsetMinutes, '0');

	DECLARE @InternationalTimeFormat VARCHAR(100) = 'hh:mm:s tt'  
	SELECT  @InternationalTimeFormat = Value    
	FROM  @SearchCriteriaTPV    
	WHERE Name = 'InternationalTimeFormat'  

	SET @InternationalTimeFormat = CASE @InternationalTimeFormat  
										WHEN '12Hours' THEN 'hh:mm tt' 
										WHEN '24Hours' THEN 'HH:mm'           
										ELSE 'hh:mm:s tt'  
									END

	DECLARE @InternationalDateFormat VARCHAR(100) = 'mm/dd/yyyy'
	SELECT  @InternationalDateFormat = Value  
	FROM  @SearchCriteriaTPV  
	WHERE Name = 'InternationalDateFormat'

	SET @InternationalDateFormat = CASE @InternationalDateFormat  
										WHEN 'mm/dd/yyyy' THEN CONVERT(varchar,'MM/dd/yyyy ' + @InternationalTimeFormat)  
										WHEN 'dd/mm/yyyy' THEN CONVERT(varchar,'dd/MM/yyyy ' + @InternationalTimeFormat) 
										WHEN 'yyyy/mm/dd' THEN CONVERT(varchar,'yyyy/MM/dd ' + @InternationalTimeFormat)  
										ELSE 'MM/dd/yyyy hh:mm:s tt'  
									END 

	SET @Cmd01 = '
	SELECT		A.ActivityId,
					LT.Name AS ''LogActivityTypeName'',
					LCT.Name AS ''LogCategoryName'',
					A.Message AS ''Message'',
					ful.LoginName AS ''FromUserLoginName'',
					FUL.FirstName AS ''FromUserFirstName'',
					FUL.LastName AS ''FromUserLastName'',
					FUL.RealPageId AS ''FromUserRealPageId'',
					tul.LoginName AS ''ToUserLoginName'',
					TUL.FirstName AS ''ToUserFirstName'',
					TUL.LastName AS ''ToUserLastName'',
					TUL.RealPageId AS ''ToUserRealPageId'',
					A.BooksMasterPropertyId AS ''PropertyId'',
					A.IsSystemAdminActivity AS ''IsSystemAdminActivity'',
					A.ApplicationTimeStamp,
					FORMAT(DATEADD(minute, ' + @OffsetMinutes + ', A.ApplicationTimeStamp), '''+ @InternationalDateFormat +''') AS ''ApplicationTimestampOffset'',
					A.SourceId,
					A.MappingKey,
					A.ContextId,
					A.InstanceId
	FROM		Logging.Activity A
					INNER JOIN Logging.LogType LT ON A.LogTypeId = LT.LogTypeId
					INNER JOIN Logging.LogCategoryType LCT ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId
					INNER JOIN Logging.UserLogin FUL ON FUL.UserId = A.FromUserId 
					INNER JOIN Logging.userLogin TUL ON TUL.UserId = A.ToUserId ';
--' ORDER BY LT.LogTypeId, LCT.LogCategoryTypeId, A.ApplicationTimeStamp '

	SET @Cmd02 = '
	SELECT		COUNT (ActivityId)
	FROM		Logging.Activity A
					INNER JOIN Logging.LogType LT ON A.LogTypeId = LT.LogTypeId
					INNER JOIN Logging.LogCategoryType LCT ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId ';
	IF	(SELECT COUNT(*) FROM @SearchCriteriaTPV	) > 0
	BEGIN
		SELECT		IDENTITY( INT, 1, 1) AS RowNum,
						Name,
						Value,
						0 AS PStatus
		INTO		#HoldSearchCriteria
		FROM		@SearchCriteriaTPV
		WHERE	Name NOT IN ('OffsetMinutes', 'SaveFormat', 'InternationalDateFormat', 'InternationalTimeFormat');

		IF EXISTS	(SELECT TOP 1 1 FROM #HoldSearchCriteria WHERE name = 'RealPageId')
		BEGIN
			SELECT		@RealpageId = Value
			FROM		#HoldSearchCriteria
			WHERE	Name = 'RealPageId';
			
			SELECT @FromUserId = UserId, @ToUserId = UserId FROM Logging.Userlogin WHERE RealPageId = @RealPageId

			DELETE
			FROM		#HoldSearchCriteria
			WHERE	Name = 'RealPageId';

			INSERT INTO #HoldSearchCriteria (
				Name,
				Value,
				PStatus
			)
			VALUES
			(
				'FromRealPageId',
				@RealPageId,
				0
			),
			(
				'ToRealPageId',
				@RealPageId,
				0
			);

			SET @Cmd02 = @Cmd02 + CHAR(13) + '					INNER JOIN Logging.UserLogin FUL ON FUL.UserId = A.FromUserId 
					INNER JOIN Logging.userLogin TUL ON TUL.UserId = A.ToUserId ';
		END;

		SET @SearchCriteria = CHAR(9) + 'WHERE' + CHAR(9);
	END;

	WHILE EXISTS ( SELECT 1 FROM #HoldSearchCriteria WHERE PStatus = 0)
	BEGIN
		SELECT		TOP 1
						@SCTPVRowId = RowNum,
						@SCName = Name,
						@SCValue = Value
		FROM		#HoldSearchCriteria
		WHERE	PStatus = 0
		ORDER BY RowNum;

		SET @SearchCriteria = @SearchCriteria +
			CASE
				WHEN @ScName = 'Message' THEN 'CHARINDEX(''' + CONVERT(nvarchar(200), @SCValue) + ''', '
				ELSE ''
			END +
			CASE
				WHEN @ScName = 'FromRealpageId' THEN '(A.'
				WHEN @ScName = 'ToRealpageId' THEN 'A.'
				WHEN @ScName = 'LogCategoryTypeId' THEN 'LCT.'
				ELSE 'A.'
			END +
			CASE
				WHEN @SCName IN ('StartDate', 'EndDate') THEN 'ApplicationTimeStamp'
				WHEN @SCName = 'FromRealPageId' THEN 'FromUserId'
				WHEN @SCName = 'ToRealPageId' THEN 'ToUserId'
				WHEN @SCName = 'PropertyId' THEN 'BooksMasterPropertyId'
				ELSE @SCName
			END +
			CASE
				WHEN @SCName = 'Message' THEN ', '
				WHEN @SCName = ('StartDate') THEN ' > '
				WHEN @SCName = 'EndDate' THEN ' <= '
				WHEN @SCName IN ('FromRealPageId', 'ToRealPageId') THEN ' = '
				WHEN @SCName IN ('LogCategoryTypeId') THEN  ' IN '
				ELSE ' = '
			END +
			CASE
				WHEN @SCName IN ('StartDate','EndDate','SourceId','MappingKey','InstanceId') THEN ''''
				WHEN @SCName IN ('LogCategoryTypeId') THEN ' ('
				ELSE ''
			END +
			CASE
				WHEN @SCname IN('EndDate') THEN @SCValue--+' 23:59:59'
				WHEN @SCName = 'Message' THEN '1)'
				WHEN @SCName IN ('FromRealPageId', 'ToRealPageId') THEN CONVERT(VARCHAR,@FromUserId)
				ELSE CONVERT(nvarchar(200), @SCValue)
			END +
			CASE
				--WHEN @ScName IN ('FromRealPageId') THEN ''''
				WHEN @ScName IN ('ToRealPageId') THEN ')'
				WHEN @SCName = 'Message' THEN ' > 0'
				WHEN @SCName IN ('StartDate', 'EndDate','SourceId','MappingKey','InstanceId')
				THEN ''''
				WHEN @SCName IN ('LogCategoryTypeId') THEN ')'
				ELSE ''
			END;

		UPDATE	#HoldSearchCriteria
		SET			PStatus = 1
		WHERE	ROwNum = @SCTPVRowId;

		IF (SELECT COUNT(*) FROM #HoldSearchCriteria WHERE PStatus = 0) > 0
		BEGIN
			SET @SearchCriteria = @SearchCriteria +
				CASE
					WHEN @ScName = 'FromRealPageId' THEN ' OR '
					ELSE CHAR(13) + CHAR(9) + 'AND' + CHAR(9) + CHAR(9) + CHAR(9)
				END;
		END;
	END;

	/*
	-- Diffusing this part for time being. 
	-- Current scenario is for just one order by clause            
	IF (SELECT COUNT(*) FROM @SortOrderTPV) > 0
	BEGIN
		SELECT		IDENTITY(INT, 1, 1) AS RowNum,
						Name,
						0 AS PStatus
		INTO		#HoldSortOrder
		FROM		@SortOrderTPV;

		SET @SortOrder = ' ORDER BY ';
	END;
	
	WHILE EXISTS (SELECT 1 FROM #HoldSortOrder WHERE PStatus = 0)
	BEGIN
		SELECT TOP 1
						@SOTPVRowId = RowNum,
						@SOName = Name
		FROM		#HoldSortOrder
		WHERE	PStatus = 0;

		SET @SortOrder = @SortOrder + ' A.' + @SOName + ' ';

		UPDATE	#HoldSortOrder
		SET			PStatus = 1
		WHERE	ROwNum = @SOTPVRowId;
		IF	(SELECT COUNT(*) FROM #HoldSortOrder WHERE PStatus = 0) > 0
		BEGIN
			SET @SortOrder = @SortOrder + ', ';
		END;
	END;
	*/

	IF @SortOrderColumnName = '' OR @SortOrderColumnName IS NULL
	BEGIN
		SET @SortOrderColumnName = @DefaultSortCriteria;
	END;

	IF @SortOrder = '' OR @SortOrder IS NULL
	BEGIN
		SET @SortOrder = @DefaultSortOrder;
	END;

	SET @Cmd02 = @Cmd02 + CHAR(13) + ISNULL(@SearchCriteria, '');

	SET @Cmd01 = @Cmd01 + CHAR(13) + ISNULL(@SearchCriteria, '') + CHAR(13) + CHAR(9) + 'ORDER BY '+
		CASE
			WHEN @SortOrderColumnName = 'Logtypename' THEN 'LT.'
			ELSE 'A.'
		END +
		CASE
			WHEN @SortOrderColumnName = 'LogtypeName' THEN 'Name'
			ELSE @SortOrderColumnName
		END + ' ' + @SortOrder + CHAR(13) + CHAR(9) + 'OFFSET (' + CONVERT(varchar(10), @PageNumber) + ' - 1) * ' + CONVERT(varchar(10), @RowsPerPage) + ' ROWS' + CHAR(13) + CHAR(9) + 'FETCH NEXT ' + CONVERT(varchar(10), @RowsPerPage) + ' ROWS ONLY';

	-- SET @Cmd01 = @Cmd01+ISNULL(@SearchCriteria, '') + ' ORDER BY LT.Name ' + @SortOrder + ' , A.ApplicationTimeStamp '+' OFFSET ('+CONVERT(varchar(10), @PageNumber) + ' - 1 ) * ' + CONVERT(varchar(10), @RowsPerPage) + '  ROWS  FETCH NEXT ' + CONVERT(varchar(10), @RowsPerPage) + ' ROWS ONLY';
	EXECUTE (@Cmd01);

	INSERT INTO @TotalRows_t 
	EXECUTE (@Cmd02)
	SELECT @TotalRows = ROwCnt FROM @TotalRows_t
	--PRINT @Cmd01;
	--PRINT @Cmd02;
END;