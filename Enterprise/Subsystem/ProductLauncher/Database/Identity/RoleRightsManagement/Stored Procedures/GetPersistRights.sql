CREATE PROCEDURE [Security].[GetPersistRights]
	
AS
BEGIN
	SELECT RightId, RightName, [Value], PersistRight from [Security].[Right] WHERE PersistRight = 1
END
