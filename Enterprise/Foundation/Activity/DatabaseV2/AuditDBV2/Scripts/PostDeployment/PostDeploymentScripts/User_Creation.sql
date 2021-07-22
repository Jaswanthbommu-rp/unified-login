IF USER_ID('RUALogin') IS NULL
BEGIN
	CREATE USER [RUALogin] FOR LOGIN [RUALogin]   
		WITH DEFAULT_SCHEMA = [Logging];  
    
    GRANT CONNECT TO [RUALogin]

    EXEC sp_addrolemember N'db_owner', N'RUALogin'

END

IF USER_ID('readonly') IS NULL
BEGIN

    CREATE USER [readonly] FOR LOGIN [readonly]

END
