CREATE PROCEDURE Enterprise.ListRootActions
AS
BEGIN
	SELECT ActionID, ProductId, ObjectValue, ObjectType, AV.Value
	FROM Enterprise.Action A
		INNER JOIN Enterprise.ActionValueType AV
			ON A.ActionvalueTypeId = AV.ActionValueTypeID
	WHERE A.ParentActionId is NULL
END

