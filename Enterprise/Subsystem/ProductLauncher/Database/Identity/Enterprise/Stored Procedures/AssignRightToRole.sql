CREATE PROCEDURE Enterprise.AssignRightToRole
(
    @OrganizationId INT,
    @RightValueTypeId INT,
    @RoleId INT,
    @AssignRightToRole BIT = 0
)
AS
BEGIN


    DECLARE @RightName NVARCHAR(200);
    DECLARE @RightCategoryId INT;
    DECLARE @RightShortName NVARCHAR(50);
    DECLARE @RightDesc NVARCHAR(200);
    DECLARE @Rightid INT;
    DECLARE @ProductId INT;

    SELECT @RightName = Value,
           @RightCategoryId = StatusTypeId,
           @RightShortName = ShortName,
           @RightDesc = Description,
           @ProductId = ProductId
    FROM Enterprise.RightValueType
    WHERE RightValueTypeId = @RightValueTypeId;
    IF EXISTS
    (
        SELECT 1
        FROM Enterprise.[Right]
        WHERE RightValueTypeId = @RightValueTypeId
              AND PartyId = @OrganizationId
              AND RoleID <> -1
    )
       AND @AssignRightToRole = 0
    BEGIN
        SELECT @RoleId AS RoleId,
               'Right is already assigned to the role.' AS ErrorMessage;
        RETURN;
    END;
    IF EXISTS
    (
        SELECT 1
        FROM Enterprise.[Right]
        WHERE RightValueTypeId = @RightValueTypeId
              AND PartyId = @OrganizationId
              AND RoleID = -1
    )
       AND @AssignRightToRole = 1
    BEGIN
        UPDATE Enterprise.[Right]
        SET RoleID = @RoleId
        WHERE PartyId = @OrganizationId
              AND RightValueTypeId = @RightValueTypeId;
        SELECT @RoleId AS RoleId,
               '' AS ErrorMessage;
        RETURN;
    END;
    IF NOT EXISTS
    (
        SELECT 1
        FROM Enterprise.[Right]
        WHERE RightValueTypeId = @RightValueTypeId
              AND PartyId = @OrganizationId
			  AND RoleID = @RoleId
    )
       AND @AssignRightToRole = 1
    BEGIN

        EXECUTE Enterprise.CreateRight @RoleID = @RoleId,
                                       @RightName = @RightName,
                                       @RightCategoryId = @RightCategoryId,
                                       @PartyId = @OrganizationId,
                                       @ProductId = @ProductId,
                                       @ShortName = @RightShortName,
                                       @Description = @RightDesc,
                                       @RightID = @Rightid OUTPUT;
        SELECT @RoleId AS RoleId,
               '' AS ErrorMessage;


    END;
END;