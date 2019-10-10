CREATE PROCEDURE [Enterprise].[ListTimeZone]
AS
BEGIN
    SELECT '(GMT '+ current_utc_offset +') '+Name AS TimeZone,
        Name AS 'TimeZoneOffset'
    FROM sys.time_zone_info
END 