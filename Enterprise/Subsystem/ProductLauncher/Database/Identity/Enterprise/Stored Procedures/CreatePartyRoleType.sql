CREATE PROCEDURE [Enterprise].[CreatePartyRoleType]
    @PartyRoleTypeId INT ,
    @ParentPartyRoleTypeId INT ,
    @Name VARCHAR(50)
AS
    BEGIN
        INSERT  INTO Enterprise.RoleType
                ( PartyRoleTypeId ,
                  ParentPartyRoleTypeId ,
                  Name
                )
        OUTPUT  Inserted.PartyRoleTypeId AS Id ,
                '' AS ErrorMessage
        VALUES  ( @PartyRoleTypeId ,
                  @ParentPartyRoleTypeId ,
                  @Name
                );
    END;
