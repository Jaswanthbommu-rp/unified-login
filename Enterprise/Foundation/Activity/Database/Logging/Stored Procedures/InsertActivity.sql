CREATE PROCEDURE [Logging].[InsertActivity] (
	@LogCategoryType NVARCHAR(100),
	@LogType NVARCHAR(100),
	@CorrelationId uniqueidentifier,
	@Message NVARCHAR(400),
	@FromUserLoginName NVARCHAR(200),
	@FromUserFirstName NVARCHAR(50),
	@FromUserLastName NVARCHAR(50),
	@FromUserLoginId BIGINT,
	@FromUserRealpageId uniqueidentifier = NULL,
	@ToUserLoginName NVARCHAR(200),
	@ToUserLoginId BIGINT,
	@ToUserFirstName NVARCHAR(50),
	@ToUserLastName NVARCHAR(50),
	@ToUserRealpageId uniqueidentifier,
	@BooksMasterOrganizationId INT,
	@OrganizationPartyId INT,
	@BooksProductCode NVARCHAR(100),
	@BooksMasterPropertyId INT = NULL,
	@IsSystemAdminActivity BIT = 0,
	@ServerName NVARCHAR(50),
	@Timestamp DATETIME,
	@AdditionalInformationTPV ADDITIONALINFO READONLY,
	@ActivityId BIGINT OUTPUT,
	@SourceId NVARCHAR(50) = NULL,
	@MappingKey NVARCHAR(200) = NULL,
	@ContextId INT = NULL, --Property = 1,Company = 2,GlobalUpdate = 3,Template = 4
	@InstanceId UNIQUEIDENTIFIER = NULL,
	@IsRealpageEmployee BIT = 0
)
AS
BEGIN
	DECLARE @LogTypeId int;
	DECLARE @MessageId int;
	DECLARE @ProductId int;
	DECLARE @ServerId int;
	DECLARE @Now datetime= GETUTCDATE();
	DECLARE @Key nvarchar(MAX);
	DECLARE @Value nvarchar(MAX);
	DECLARE @LogCategoryTypeId int;
	DECLARE @TPVId int;
	DECLARE @ToUserId bigint;
	DECLARE @FromUserId bigint;
	DECLARE @Increment int;
	IF
	(
		SELECT COUNT(*)
		FROM @AdditionalInformationTPV
	) >= 0
	BEGIN
		SELECT IDENTITY(int, 1, 1) AS TPVId, [key], value, 0 AS PStatus
		INTO #AdditionalInfo
		FROM @AdditionalInformationTPV;
	END;
	BEGIN
		IF @LogCategoryType IS NOT NULL
		BEGIN
			IF EXISTS
			(
				SELECT 1
				FROM Logging.LogCategoryType
				WHERE [Name] = @LogCategoryType
			)
			BEGIN
				SELECT @LogCategoryTypeId = LogCategoryTypeId
				FROM Logging.LogCategoryType
				WHERE [Name] = @LogCategoryType;
			END;
			ELSE
			BEGIN
				SELECT @Increment = MAX(LogcategoryTypeId)
				FROM Logging.LogCategoryType
				INSERT INTO Logging.LogCategoryType( LogcategoryTypeId, [Name] )
				VALUES( @Increment + 1, @LogCategoryType );
				SELECT @LogCategoryTypeId = SCOPE_IDENTITY();
			END;
		END;
		IF @LogType IS NOT NULL
		BEGIN
			IF EXISTS
			(
				SELECT 1
				FROM Logging.LogType AS LT
					 INNER JOIN
					 Logging.LogCategoryType AS LCT
					 ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId
				WHERE LT.[Name] = @LogType AND 
					  LCT.Name = @LogCategoryType
			)
			BEGIN
				SELECT @LogTypeId = LT.LogTypeId
				FROM Logging.LogType AS LT
					 INNER JOIN
					 Logging.LogCategoryType AS LCT
					 ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId
				WHERE LT.[Name] = @LogType AND 
					  LCT.Name = @LogCategoryType;
			END;
			ELSE
			BEGIN
				IF NOT EXISTS
				(
					SELECT 1
					FROM Logging.LogType AS LT
						 INNER JOIN
						 Logging.LogCategoryType AS LCT
						 ON LCT.LogCategoryTypeId = LT.LogCategoryTypeId
					WHERE LT.[Name] = @LogType AND 
						  LCT.Name = @LogCategoryType
				)
				BEGIN
					INSERT INTO Logging.LogType( [Name], LogCategoryTypeId )
					VALUES( @LogType, @LogCategoryTypeId );
					SELECT @LogTypeId = SCOPE_IDENTITY();
				END
			END;
		END;
		IF @BooksProductCode IS NULL
		BEGIN
			SET @ProductId = 0;
		END;
		ELSE
		BEGIN
			IF EXISTS
			(
				SELECT 1
				FROM Logging.Product
				WHERE BooksProductCode = @BooksProductCode
			)
			BEGIN
				SELECT @ProductId = ProductId
				FROM Logging.Product
				WHERE BooksProductCode = @BooksProductCode;
			END;
			ELSE
			BEGIN
				INSERT INTO Logging.Product( BooksProductCode )
				VALUES( @BooksProductCode );
				SELECT @ProductId = SCOPE_IDENTITY();
			END;
		END;
		IF @ServerName IS NULL
		BEGIN
			SET @ServerId = 0;
		END;
		ELSE
		BEGIN
			IF EXISTS
			(
				SELECT ServerId
				FROM Logging.ServerName
				WHERE ServerName = @ServerName
			)
			BEGIN
				SELECT @ServerId = ServerId
				FROM Logging.ServerName
				WHERE ServerName = @ServerName;
			END;
			ELSE
			BEGIN
				INSERT INTO Logging.ServerName( ServerName )
				VALUES( @ServerName );
				SELECT @ServerId = SCOPE_IDENTITY();
			END;
		END;
		IF @ToUserLoginId IS NOT NULL
		BEGIN
			IF EXISTS
			(
				SELECT 1
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @ToUserLoginId
			)
			BEGIN
				SELECT @ToUserId = [UserId]
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @ToUserLoginId;
			END;
			ELSE
			BEGIN
				MERGE Logging.UserLogin AS T
				USING
				(
					SELECT @ToUserLoginId ToUserLoginId,
					@ToUserLoginname ToUserLoginName, @ToUserFirstName ToUserFirstName, @ToUserLastName ToUserLastName, @ToUserRealpageId ToUserRealPageId
				) AS S
				ON S.ToUserLoginId = T.[ProductUserId]
				WHEN MATCHED
					  THEN UPDATE SET T.FirstName = S.ToUserFirstName, T.LastName = S.ToUserLastName, T.RealPageId = S.ToUserRealPageId, T.LoginName = S.ToUserLoginName
				WHEN NOT MATCHED BY TARGET
					  THEN
					  INSERT(LoginName, ProductUserId,
					  FirstName, LastName, RealPageId)
					  VALUES( S.ToUserLoginName, S.ToUserLoginId,
					  S.ToUserFirstName, S.ToUserLastName, S.ToUserRealPageId );
				SELECT @ToUserId = UserId
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @ToUserLoginId;
			END;
		END;
		ELSE
		BEGIN
			SELECT @ToUserId = [UserId]
			FROM Logging.UserLogin
			WHERE UserId = 0
		END;
		IF @FromUserLoginId IS NOT NULL
		BEGIN
			IF EXISTS
			(
				SELECT 1
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @FromUserLoginId AND 
					  [ProductUserId] = @FromUserid
			)
			BEGIN
				SELECT @FromUserId = [UserId]
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @FromUserLoginId;
			END;
			ELSE
			BEGIN
				MERGE Logging.UserLogin AS T
				USING
				(
					SELECT @FromUserLoginId FROMUserLoginId,
					@FromUserLoginname FromUserLoginName, @FromUserFirstName FromUserFirstName, @FromUserLastName FromUserLastName, @FromUserRealpageId FromUserRealPageId
				) AS S
				ON S.FromUserLoginId = T.[ProductUserId]
				WHEN MATCHED
					  THEN UPDATE SET T.FirstName = S.FromUserFirstName, T.LastName = S.FromUserLastName, T.RealPageId = S.FromUserRealPageId, T.LoginName = S.FromUserLoginName
				WHEN NOT MATCHED BY TARGET
					  THEN
					  INSERT(LoginName, ProductUserId,
					  FirstName, LastName, RealPageId)
					  VALUES( S.FromUserLoginName, S.FromUserLoginId,
					  S.FromUserFirstName, S.FromUserLastName, S.FromUserRealPageId );
				SELECT @FromUserId = UserId
				FROM Logging.UserLogin
				WHERE [ProductUserId] = @FromUserLoginId
			END;
		END;
		ELSE
		BEGIN
			SELECT @ToUserId = [UserId]
			FROM Logging.UserLogin
			WHERE UserId = 0
		END;
				    --Process Activity Table
		BEGIN TRY
			INSERT INTO Logging.Activity
				(LogTypeId,
					CorrelationId,
					Message,
					FromUserId,
					ToUserId,
					BooksMasterOrganizationId,
					OrganizationPartyId,
					ProductId,
					ServerId,
					ApplicationTimestamp,
					DatabaseTimestamp,
					BooksMasterPropertyId,
					IsSystemAdminActivity,
					SourceId,
					MappingKey,
					ContextId,
					InstanceId,
					IsRealpageEmpoyee
				)
			VALUES
				(@LogTypeId,
					@CorrelationId,
					@Message,
					@FromUserId,
					CASE
						WHEN @ToUserId IS NULL
						THEN 1
						ELSE @ToUserId
					END,
					ISNULL(@BooksMasterOrganizationId, 0),
					ISNULL(@OrganizationPartyId, 0),
					@ProductId,
					@ServerId,
					@Timestamp,
					@Now,
					@BooksMasterPropertyId,
					@IsSystemAdminActivity,
					@SourceId,
					@MappingKey,
					@ContextId,
					@InstanceId,
					@IsRealpageEmployee
				);
			IF @@ROWCOUNT > 0
			BEGIN
				SET @ActivityId = SCOPE_IDENTITY();
			END;
			SELECT @ActivityId AS Id, '' AS ErrorMessage;
		END TRY
		BEGIN CATCH
			DECLARE @ErrorLogID int;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
			SELECT 0 AS Id, ErrorMessage
			FROM dbo.ErrorLog
			WHERE ErrorLogID = @ErrorLogID;
		END CATCH;
  				 --Process Additional information if there is any
		IF
		(
			SELECT COUNT(*)
			FROM #AdditionalInfo
		) > 0
		BEGIN
			WHILE EXISTS
			(
				SELECT 1
				FROM #AdditionalInfo
				WHERE Pstatus = 0
			)
			BEGIN
				SELECT TOP 1 @tpvid = tpvid, @key = [Key], @Value = Value
				FROM #AdditionalInfo
				WHERE Pstatus = 0;
				INSERT INTO Logging.ActivityDetail( ActivityId, [Key], Value )
				VALUES( @ActivityId, @Key, @value );
				UPDATE #AdditionalInfo
				  SET PStatus = 1
				WHERE TPVId = @tpvid;
			END;
		END;
		ELSE
		BEGIN
			SELECT 0 AS Id, 'No additional information received.' AS ErrorMessage;
		END;
	END;
END;