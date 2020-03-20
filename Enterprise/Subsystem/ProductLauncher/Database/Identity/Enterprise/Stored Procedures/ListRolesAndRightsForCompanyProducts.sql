CREATE PROCEDURE [Enterprise].[ListRolesAndRightsForCompanyProducts](
     @Partyid INT
    ,@ProductId INT
    ,@TargetProductId NVARCHAR(MAX)
    ,@RoleId INT = NULL
    )
AS
BEGIN
    WITH ProductTable
    AS (
        SELECT value AS TargetProductId
        FROM STRING_SPLIT(@TargetProductId, ',')
        WHERE RTRIM(value) <> ''
        )
    SELECT DISTINCT 
         r.RoleID [RoleId]
        ,RVT.Value [Role]
        ,RVT.ShortName [RoleNickName]
        ,ST.Name [RoleType]
        ,r.DefaultRole AS DefaultRole
        ,r2.RightID [RightId]
        ,rvtt.Value [Right]
        ,rvtt.ShortName [RightNickName]
        ,rvtt.RightValueTypeId      
    FROM Enterprise.ROLE r
    INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = r.RoleValueTypeId
    INNER JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = RVT.StatusTypeId
    INNER JOIN Enterprise.[Right] r2 ON r.RoleID = r2.RoleID
    INNER JOIN Enterprise.RightValueType rvtt ON rvtt.RightValueTypeId = r2.RightValueTypeId
    INNER JOIN Enterprise.UserActions UA ON UA.RightID = r2.RightID
    INNER JOIN Enterprise.Action A ON A.ActionID = UA.ActionID
    INNER JOIN Enterprise.StatusType STT ON UA.STATUS = STT.StatusTypeId
    INNER JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeId = ST.StatusTypeId
    INNER JOIN Enterprise.StatusTypeCategory STC ON STC.StatusTypeCategoryId = STCC.StatusTypeCategoryId
    INNER JOIN Enterprise.StatusTypeCategoryType STCT ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
    INNER JOIN ProductTable PT ON rvtt.ProductId = PT.TargetProductId
    WHERE r.PartyID = @Partyid
        AND rvtt.ProductId = @ProductId
        AND (r.RoleID = @RoleId OR @RoleId IS NULL)
        AND STT.Name <> 'HIDDEN'
        AND r2.RoleID <> - 1;
END;
