CREATE PROCEDURE [Security].[UpdateAdGroupAssingment]
	@productId int,
	@adgroups [Security].[ADGroupAssignmentType] READONLY
AS
BEGIN
	BEGIN TRAN
		DELETE FROM SECURITY.ADGroupProduct WHERE ProductId = @productId

		IF (EXISTS(SELECT TOP 1 1 FROM @adgroups) )
		BEGIN
		
			SELECT AdGroupId, ProductId, AssignmentOrder, CreatedBy, GETUTCDATE() as CreatedDate
			INTO #ADGroupProduct        
			FROM @adgroups; 
		
			INSERT INTO SECURITY.ADGroupProduct(ADGroupId, ProductId, AssignmentOrder, CreatedBy, CreatedDate)
			SELECT AdGroupId, ProductId, AssignmentOrder, CreatedBy, CreatedDate
			FROM #ADGroupProduct

			DROP TABLE #ADGroupProduct
		END
	COMMIT
END
