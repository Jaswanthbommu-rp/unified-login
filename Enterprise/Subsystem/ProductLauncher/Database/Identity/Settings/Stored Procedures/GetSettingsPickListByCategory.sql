CREATE PROCEDURE [Settings].[GetSettingsPickListByCategory] @categoryName varchar(50)
AS
BEGIN
SELECT *
	FROM [Settings].[SettingPicklist]
	WHERE CategoryName = @categoryName
	AND ThruDate is NULL
END
