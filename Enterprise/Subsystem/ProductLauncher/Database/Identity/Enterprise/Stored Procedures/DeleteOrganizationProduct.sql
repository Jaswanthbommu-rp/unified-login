CREATE PROCEDURE [Enterprise].[DeleteOrganizationProduct] (
	 @PartyId BIGINT
	,@ProductId INT
)
AS
BEGIN
	BEGIN TRY
		UPDATE Enterprise.OrganizationProduct 
		SET ThruDate = GETUTCDATE() 
		OUTPUT INSERTED.OrganizationProductId AS Id, '' AS ErrorMessage
		WHERE PartyId = @PartyId AND ProductId = @ProductId AND ((GETUTCDATE() BETWEEN FromDate AND ThruDate) OR (ThruDate IS NULL));
	END TRY
	BEGIN CATCH
		 DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;