IF OBJECT_ID('[Enterprise].[UpdatePartyRoleByRealPageId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[UpdatePartyRoleByRealPageId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[UpdatePartyRoleByRealPageId] (
	@PartyRoleId int,
	@RoleTypeID int
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Enterprise.PartyRole
		SET		RoleTypeId = @RoleTypeID
		OUTPUT	inserted.PartyRoleId AS Id,
				'' AS ErrorMessage
		WHERE	PartyRoleId = @PartyRoleId
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
