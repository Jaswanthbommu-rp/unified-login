CREATE PROCEDURE [Enterprise].[CreatePartyRoleByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER,
	@RoleTypeID int
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION; 
		UPDATE pr
		   SET RoleTypeId = @RoleTypeId
		   OUTPUT inserted.RoleTypeId AS Id,  
		   '' AS ErrorMessage  
		   FROM Enterprise.Party pa 
		   INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
		   INNER JOIN Enterprise.PartyRole pr ON (p.PartyId = pr.PartyId)
		   WHERE	pa.RealPageId = @RealPageId

		IF @@ROWCOUNT = 0
		BEGIN
		
			INSERT  INTO Enterprise.PartyRole (
				PartyId,
				RoleTypeId
			)
			OUTPUT	Inserted.PartyRoleId AS Id,
					@RealPageId AS RealPageId,
					'' AS ErrorMessage
			SELECT	p.PartyId,
					@RoleTypeID
			FROM	Enterprise.Party pa
					INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
			WHERE	pa.RealPageId = @RealPageId

			
		END
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;