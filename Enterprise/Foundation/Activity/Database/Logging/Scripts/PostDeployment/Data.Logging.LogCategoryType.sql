
MERGE INTO [logging].[logcategorytype] AS target
USING(VALUES
(1,
 'security',
 N'security activities like login, logout etc.'
),
(2,
 'user',
 N'user activities like create, update etc.'
),
(3,
 'productaccess',
 N'product related activities like product login, create/update user in product etc.'
),
(4,
 'email',
 N'notification related activities.'
)) AS source(logcategorytypeid, name, description)
ON target.logcategorytypeid = source.logcategorytypeid 

--update matched rows 
    WHEN MATCHED
    THEN UPDATE SET
                    name = source.name,
                    description = source.description

--insert new rows 
    WHEN NOT MATCHED BY TARGET
    THEN
      INSERT(logcategorytypeid,
             name,
             description)
      VALUES
(logcategorytypeid,
 name,
 description
)
 
 --delete rows that are in the target but not the source 
    WHEN NOT MATCHED BY SOURCE
    THEN DELETE;
