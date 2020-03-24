CREATE   FUNCTION UserManagement.ParseJson (
    @parent nvarchar(max),
	@json nvarchar(max)
)
returns @tempTable table (
	[Id] int IDENTITY(1, 1),
	[parent] nvarchar(max),
    [key] nvarchar(max),
	[value] nvarchar(max),
	[type] int
)
AS
BEGIN
    ; WITH cte (
		[parent],
		[key],
		[value],
		[type]
	)
	AS
	(
        SELECT	@parent AS 'parent',
					IIF(@parent is null, [key], CONCAT(@parent, '.', [key])) AS 'key',
					[value],
					[type]
        FROM	OPENJSON(@json)
    )
    INSERT @tempTable (
		[parent],
		[key],
		[value],
		[type]
	)
    SELECT	x.[parent],
				x.[key],
				x.[value],
				x.[type]
    FROM	cte x
    UNION ALL
    SELECT	x.[parent],
				REPLACE(x.[key], CONCAT(x.[parent], '.'), ''),
				x.[value],
				x.[type]
    FROM	cte y
				CROSS APPLY UserManagement.ParseJson(y.[key], y.[value]) x
    WHERE	ISJSON(y.[value])=1

    RETURN
END