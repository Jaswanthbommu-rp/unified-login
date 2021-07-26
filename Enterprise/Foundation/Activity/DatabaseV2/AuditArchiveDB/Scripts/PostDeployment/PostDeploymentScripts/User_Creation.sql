--prod
IF USER_ID('readonly') IS NULL
BEGIN

    CREATE USER [readonly] FOR LOGIN [readonly]

END
