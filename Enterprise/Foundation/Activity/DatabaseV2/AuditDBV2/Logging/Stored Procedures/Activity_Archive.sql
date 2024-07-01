
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
				,A.IsRealPageEmployee
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
			LEFT JOIN
				AuditArchiveDB.[Logging].[UserLogin] AUL ON A.CreatedBy = AUL.UserId
			WHERE
				AUL.UserId IS NULL

			--Users migration	
			INSERT INTO AuditArchiveDB.[Logging].[UserLogin]
			(
				UserId,LoginName,FirstName,LastName,RealPageId
			)
			SELECT 
				UserId,LoginName,FirstName,LastName,RealPageId
			FROM
				#Temp_DistinctUsers

				
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
			INSERT INTO [AuditArchiveDB].[Logging].Activity 
			(
				ActivityId
				,OrganizationPartyId
				,LogTypeId
				,[Message]
				,ContextId
				,ContextReferenceId
				,ApplicationTimeStamp
				,CreatedBy
				,CreatedDate
				,IsRealPageEmployee
			) 
			SELECT 
				ActivityId
				,ISNULL(OrganizationPartyId,0)
				,LogTypeId
				,[Message]
				,ContextId
				,ContextReferenceId
				,ApplicationTimeStamp
				,CreatedBy
				,CreatedDate
				,IsRealPageEmployee
			FROM 
				#Temp_Activities
	
			INSERT INTO [AuditArchiveDB].[Logging].[ActivityDetail] 
			(
				ActivityDetailId
				,ActivityId
				,[Key]
				,[Value]
				,CreatedDate
			)
			SELECT 
				ActivityDetailId
				,ActivityId
				,[Key]
				,[Value]
				,CreatedDate
			FROM
				#Temp_ActivityDetails
			

			DROP TABLE IF EXISTS #Temp_Activities
			DROP TABLE IF EXISTS #Temp_ActivityDetails
			DROP TABLE IF EXISTS #Temp_DistinctUsers
			
		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
	
		IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION

		SELECT ERROR_NUMBER() AS Id,ERROR_MESSAGE() AS ErrorMessage

	END CATCH

END