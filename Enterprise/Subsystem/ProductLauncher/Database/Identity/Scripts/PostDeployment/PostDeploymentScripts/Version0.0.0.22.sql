
/*Populate Status tables with role type statuses*/


IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTypeCategoryType WHERE Name = 'Security')
BEGIN
	INSERT INTO Enterprise.StatusTypeCategoryType (ParentStatusTypeCategoryTypeId, Name)
	VALUES
	(   NULL, 'Security' )
END

SELECT @userrightsid = StatusTypeCategoryTypeid FROM Enterprise.StatusTypeCategoryType WHERE name = 'Security'

IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTypeCategory WHERE name = 'Role Type')
BEGIN
	INSERT INTO Enterprise.StatusTypeCategory
	(
		ParentStatusTypeCategoryId,
		StatusTypeCategoryTypeId,
		Name
	)
	VALUES
	(   null, -- ParentStatusTypeCategoryId - int
		@userrightsid, -- StatusTypeCategoryTypeId - int
		'Role Type' -- Name - varchar(50)
	)
END

SELECT @statustypecategoryid = StatusTypeCategoryId FROM Enterprise.StatusTypeCategory WHERE name = 'Role Type'

IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusType WHERE name = 'Default')
BEGIN
	INSERT INTO Enterprise.StatusType(name)
	VALUES
	( 'Default' )
	SELECT @ident = @@IDENTITY
	INSERT INTO Enterprise.StatusTypeCategoryClassification
	(
		StatusTypeId,
		StatusTypeCategoryId,
		FromDate,
		ThruDate
	)
	VALUES
	(   @ident,         -- StatusTypeId - int
		@statustypecategoryid,         -- StatusTypeCategoryId - int
		GETDATE(), -- FromDate - datetime
		null  -- ThruDate - datetime
	)
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusType WHERE name = 'Custom')
BEGIN
	INSERT INTO Enterprise.StatusType(name)
	VALUES
	( 'Custom' )
	SELECT @ident = @@IDENTITY
	INSERT INTO Enterprise.StatusTypeCategoryClassification
	(
		StatusTypeId,
		StatusTypeCategoryId,
		FromDate,
		ThruDate
	)
	VALUES
	(   @ident,         -- StatusTypeId - int
		@statustypecategoryid,         -- StatusTypeCategoryId - int
		GETDATE(), -- FromDate - datetime
		null  -- ThruDate - datetime
	)
END



SELECT @StatusTypeId = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
	 WHERE ST.Name = 'Default'

--UPDATE Enterprise.RoleValueType
--SET
--    StatusTypeId = @StatusTypeId



EXEC sys.sp_updateextendedproperty @name=N'Build', @value='23'