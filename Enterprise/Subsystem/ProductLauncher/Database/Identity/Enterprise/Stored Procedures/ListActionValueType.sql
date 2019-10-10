CREATE PROCEDURE Enterprise.ListActionValueType
AS 
BEGIN
	SELECT 
		ActionValueTypeID
		, Value 'ActionValuename'
		, Description
	FROM Enterprise.ActionValueType
END
GO