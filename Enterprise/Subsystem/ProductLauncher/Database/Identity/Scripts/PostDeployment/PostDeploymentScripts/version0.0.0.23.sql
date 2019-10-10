
/*Populate Status tables with custom right statuses*/

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusTypeCategoryType
    WHERE Name = 'Security'
)
    BEGIN
        INSERT INTO Enterprise.StatusTypeCategoryType
        (ParentStatusTypeCategoryTypeId,
         Name
        )
        VALUES
        (NULL,
         'Security'
        );
END;
SELECT @userrightsid = StatusTypeCategoryTypeid
FROM Enterprise.StatusTypeCategoryType
WHERE name = 'Security';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusTypeCategory
    WHERE name = 'Right Type'
)
    BEGIN
        INSERT INTO Enterprise.StatusTypeCategory
        (ParentStatusTypeCategoryId,
         StatusTypeCategoryTypeId,
         Name
        )
        VALUES
        (NULL, -- ParentStatusTypeCategoryId - int
         @userrightsid, -- StatusTypeCategoryTypeId - int
         'Right Type' -- Name - varchar(50)
        );
END;
SELECT @statustypecategoryid = StatusTypeCategoryId
FROM Enterprise.StatusTypeCategory
WHERE name = 'Right Type';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'Default'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('Default');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Security'
              AND Enterprise.StatusType.Name = 'Default';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'Custom'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('Custom');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Security'
              AND Enterprise.StatusType.Name = 'Custom';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;


--IF EXISTS(SELECT 1 FROM Enterprise.RightValueType WHERE StatusTypeId IS NULL)
--BEGIN
--    UPDATE Enterprise.RightValueType
--	   SET StatusTypeId = ( SELECT  Enterprise.StatusType.StatusTypeId
--    FROM Enterprise.StatusType
--         INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
--         INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
--         INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
--    WHERE Enterprise.StatusTypeCategoryType.name = 'Security'
--          AND Enterprise.StatusTypeCategory.Name = 'Right Type'
--          AND Enterprise.StatusType.Name = 'Default')
--END;
EXEC sys.sp_updateextendedproperty
     @name = N'Build',
     @value = '24';