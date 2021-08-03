--prod
IF USER_ID('readonly') IS NULL
BEGIN

    CREATE USER [readonly] FOR LOGIN [readonly]

    GRANT CONNECT TO [readonly]

    EXEC sp_addrolemember N'db_datareader', N'readonly'

END
