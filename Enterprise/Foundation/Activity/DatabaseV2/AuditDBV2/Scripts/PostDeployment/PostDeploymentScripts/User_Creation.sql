IF USER_ID('identityserver') IS NULL
BEGIN
	CREATE USER [identityserver] FOR LOGIN [identityserver]   
		WITH DEFAULT_SCHEMA = [Logging];  
    
    GRANT CONNECT TO [identityserver]

    EXEC sp_addrolemember N'db_owner', N'identityserver'

END

IF USER_ID('readonly') IS NULL
BEGIN

    CREATE USER [readonly] FOR LOGIN [readonly]

    GRANT CONNECT TO [readonly]

    EXEC sp_addrolemember N'db_datareader', N'readonly'

END
