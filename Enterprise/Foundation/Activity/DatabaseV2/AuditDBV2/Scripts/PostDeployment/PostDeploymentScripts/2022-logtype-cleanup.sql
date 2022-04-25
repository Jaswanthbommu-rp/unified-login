use AuditDBV2
GO
--1117460

IF @@SERVERNAME Not IN ('rcpgbkdbsql005a','rcpgbkdbsql005b')
Begin
	Declare @emailEmail int;
	Declare @emailUser int;
	
	Select @emailEmail = lt.LogTypeId
	from Logging.LogType lt 
	join Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	where ct.Name = 'Email' and lt.Name = 'Email Sent'
	
	Select @emailUser = lt.LogTypeId
	from Logging.LogType lt 
	join Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	where ct.Name = 'User' and lt.Name = 'Email Sent'
	
	Update Logging.Activity
	Set LogTypeId = @emailEmail
	where LogTypeId = @emailUser
	
	declare @companysetupcategory int
	select @companysetupcategory = LogCategoryTypeId from logging.logcategorytype where name = 'companysetup'
	update logging.logtype set logcategorytypeid = @companysetupcategory where name = 'User Update - Internal'
	
	declare @migrationCatId int
	select @migrationCatId = LogCategoryTypeId from logging.logcategorytype where name = 'Migration'
	update logging.logtype set logcategorytypeid = @migrationCatId where name = 'Refresh User Data' 

	-- activity details
	Delete ad
	FROM Logging.ActivityDetail ad 
		JOIN Logging.Activity a on a.ActivityId = ad.ActivityId
		JOIN Logging.LogType lt on lt.LogTypeId = a.LogTypeId
		JOIN Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	WHERE ct.Name = 'User'
		AND lt.Name in ('Clone user', 'Migration Tool', 'Support Tool', 'Email sent')
	
	-- activity
	Delete a
	FROM Logging.Activity a 
		JOIN Logging.LogType lt on lt.LogTypeId = a.LogTypeId
		JOIN Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	WHERE ct.Name = 'User'
		AND lt.Name in ('Clone user', 'Migration Tool', 'Support Tool', 'Email sent')
	
	--logtype
	Delete lt
	from Logging.LogType lt
	Join Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	Where ct.Name = 'User'
	and lt.Name in ('Clone user', 'Migration Tool', 'Support Tool', 'Email sent')
	
	
	
	-- activity details
	Delete ad
	FROM Logging.ActivityDetail ad 
		JOIN Logging.Activity a on a.ActivityId = ad.ActivityId
		JOIN Logging.LogType lt on lt.LogTypeId = a.LogTypeId
		JOIN Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	Where ct.Name = 'Security'
		AND lt.Name in ('Login failure', 'User locked', 'User unlocked')
	
	-- activity
	Delete a
	FROM Logging.Activity a 
		JOIN Logging.LogType lt on lt.LogTypeId = a.LogTypeId
		JOIN Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	Where ct.Name = 'Security'
		AND lt.Name in ('Login failure', 'User locked', 'User unlocked')
	
	--logtype
	Delete lt
	FROM Logging.LogType lt
		Join Logging.LogCategoryType ct on ct.LogCategoryTypeId = lt.LogcategoryTypeId
	Where ct.Name = 'Security'
		AND lt.Name in ('Login failure', 'User locked', 'User unlocked')
	
End
GO