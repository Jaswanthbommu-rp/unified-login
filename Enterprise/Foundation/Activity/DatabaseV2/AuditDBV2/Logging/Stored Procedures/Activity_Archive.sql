

CREATE PROC Logging.Activity_Archive
(
	@NoOfMonths INT = 12  
)
AS
BEGIN
SET NOCOUNT ON

	BEGIN TRY

		BEGIN TRANSACTION
			
			--[LogCategoryType]
			MERGE AuditArchiveDB.[Logging].[LogCategoryType] AS TARGET
			USING [Logging].[LogCategoryType] AS SOURCE 
				ON TARGET.LogCategoryTypeId = SOURCE.LogCategoryTypeId
			WHEN MATCHED 
			THEN 
				UPDATE 
				SET TARGET.[Name] = SOURCE.[Name], 
					TARGET.[Description] = SOURCE.[Description] 
			WHEN NOT MATCHED BY TARGET 
			THEN 
				INSERT (LogCategoryTypeId, [Name], [Description]) 
				VALUES (SOURCE.LogCategoryTypeId, SOURCE.[Name], SOURCE.[Description]);
			
			--[LogType]
			MERGE AuditArchiveDB.[Logging].[LogType] AS TARGET
			USING [Logging].[LogType] AS SOURCE 
				ON TARGET.LogTypeId = SOURCE.LogTypeId
			WHEN MATCHED 
			THEN 
				UPDATE 
				SET TARGET.LogcategoryTypeId = SOURCE.LogcategoryTypeId,
					TARGET.[Name] = SOURCE.[Name], 
					TARGET.[Description] = SOURCE.[Description] 
			WHEN NOT MATCHED BY TARGET 
			THEN 
				INSERT (LogTypeId,LogCategoryTypeId, [Name], [Description]) 
				VALUES (SOURCE.LogTypeId,SOURCE.LogCategoryTypeId, SOURCE.[Name], SOURCE.[Description]);

		
			DROP TABLE IF EXISTS #Temp_ActivityIds_New_Old

			CREATE TABLE #Temp_ActivityIds_New_Old
			(
				ActivityId_New INT,			
				ActivityId_Old INT
			)

			DROP TABLE IF EXISTS #Temp_Activities

			--@NoOfMonths -> Last 12 months -> Total Activity data
			SELECT 
				 A.ActivityId
				,A.OrganizationPartyId
				,A.LogTypeId
				,A.[Message]
				,A.ContextId
				,A.ContextReferenceId
				,A.ApplicationTimeStamp
				,A.CreatedBy
				,A.CreatedDate	
			INTO #Temp_Activities
			FROM
				[Logging].[Activity] AS A
			WHERE
				CAST(A.CreatedDate AS DATE) < CAST(DATEADD(MONTH, -@NoOfMonths, GETUTCDATE()) AS DATE)
			ORDER BY 
				A.CreatedDate ASC

			DROP TABLE IF EXISTS #Temp_ActivityDetails

			SELECT 
				 AD.ActivityDetailId
				,AD.ActivityId 
				,AD.[Key]
				,AD.[Value]
				,AD.CreatedDate
			INTO #Temp_ActivityDetails
			FROM
				[Logging].[ActivityDetail] AD
			INNER JOIN
				#Temp_Activities A ON AD.ActivityId = A.ActivityId
			ORDER BY 
				AD.CreatedDate ASC
			
			DROP TABLE IF EXISTS #Temp_DistinctUsers

			SELECT DISTINCT
				UL.UserId
				,UL.LoginName
				,UL.FirstName
				,UL.LastName
				,UL.RealPageId
			INTO #Temp_DistinctUsers
			FROM
				#Temp_Activities AS A
			INNER JOIN
				[Logging].[UserLogin] UL ON A.CreatedBy = UL.UserId 

			DROP TABLE IF EXISTS #Temp_UserIds_New_Old

			CREATE TABLE #Temp_UserIds_New_Old
			(
				UserId_New INT,			
				UserId_Old INT
			)

			--Users migration			
			MERGE AuditArchiveDB.[Logging].[UserLogin] AS TARGET
			USING #Temp_DistinctUsers AS SOURCE 
				ON TARGET.RealPageId = SOURCE.RealPageId
			WHEN MATCHED 
			THEN 
				UPDATE 
				SET TARGET.LoginName = SOURCE.LoginName,
					TARGET.FirstName = SOURCE.FirstName, 
					TARGET.LastName = SOURCE.LastName, 
					TARGET.RealPageId = SOURCE.RealPageId
			WHEN NOT MATCHED BY TARGET 
			THEN 
				INSERT (LoginName,FirstName,LastName,RealPageId) 
				VALUES (SOURCE.LoginName,SOURCE.FirstName, SOURCE.LastName, SOURCE.RealPageId)
			OUTPUT INSERTED.UserId, SOURCE.UserId INTO #Temp_UserIds_New_Old(UserId_New,UserId_Old);

			DROP TABLE IF EXISTS #Temp_Activities_Final

			SELECT 
				 A.ActivityId
				,A.OrganizationPartyId
				,A.LogTypeId
				,A.[Message]
				,A.ContextId
				,A.ContextReferenceId
				,A.ApplicationTimeStamp
				,U.UserId_New AS CreatedBy --
				,A.CreatedDate	
			INTO #Temp_Activities_Final
			FROM
				#Temp_Activities A
			LEFT JOIN
				#Temp_UserIds_New_Old U ON A.CreatedBy = U.UserId_Old
			
			--ActivityDetail Deleting in current database last year data 	
			DELETE AD
			FROM
				[Logging].[ActivityDetail] AS AD
			INNER JOIN
				#Temp_ActivityDetails T ON AD.ActivityDetailId = T.ActivityDetailId			

			--Activity Deleting in current database last year data 
			DELETE A
			FROM
				[Logging].[Activity] AS A
			INNER JOIN
				#Temp_Activities T ON A.ActivityId = T.ActivityId			
				
			--Activity last year data inserting into Archive database
			MERGE INTO AuditArchiveDB.Logging.Activity AS TARGET
			USING #Temp_Activities_Final SOURCE ON 1=0   -- always false for only insert
			WHEN NOT MATCHED BY TARGET          
			THEN INSERT (
							OrganizationPartyId
							,LogTypeId
							,[Message]
							,ContextId
							,ContextReferenceId
							,ApplicationTimeStamp
							,CreatedBy
							,CreatedDate
						)
					 VALUES (
							ISNULL(SOURCE.OrganizationPartyId,0)
							,SOURCE.LogTypeId
							,SOURCE.[Message]
							,SOURCE.ContextId
							,SOURCE.ContextReferenceId
							,SOURCE.ApplicationTimeStamp
							,SOURCE.CreatedBy
							,SOURCE.CreatedDate
						)
			OUTPUT INSERTED.ActivityId, SOURCE.ActivityId INTO #Temp_ActivityIds_New_Old(ActivityId_New,ActivityId_Old);					

			--ActivityDetail last year data  inserting into Archive database
			INSERT INTO AuditArchiveDB.Logging.ActivityDetail 
			(
				ActivityId
				,[Key]
				,[Value]
				,CreatedDate
			)
			SELECT 
				ActivityId_New 
				,[Key]
				,[Value]
				,CreatedDate
			FROM
				#Temp_ActivityDetails T
			INNER JOIN
				#Temp_ActivityIds_New_Old TA On T.ActivityId = TA.ActivityId_Old
			

			DROP TABLE IF EXISTS #Temp_Activities
			DROP TABLE IF EXISTS #Temp_ActivityDetails
			DROP TABLE IF EXISTS #Temp_ActivityIds_New_Old
			DROP TABLE IF EXISTS #Temp_DistinctUsers
			DROP TABLE IF EXISTS #Temp_Activities_Final
			
		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
	
		IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION

		SELECT ERROR_NUMBER() AS Id,ERROR_MESSAGE() AS ErrorMessage

	END CATCH

END