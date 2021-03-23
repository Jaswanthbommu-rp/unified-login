-- =============================================
-- Author:		RohithVundyala
-- Create date: 
-- Description: Deletes the property Instance.
-- =============================================
CREATE PROCEDURE Enterprise.DeletePropertyInstance (
	 @InstanceId UNIQUEIDENTIFIER
)
AS
BEGIN
	IF NOT EXISTS(SELECT 1 
					FROM Enterprise.PropertyInstance P
						INNER JOIN Enterprise.PropertyInstanceMapping PIM ON P.PropertyInstanceId =PIM.PropertyInstanceId
					WHERE PIM.Active = 1 
						AND P.IsDeleted = 0
						AND P.InstanceId = @InstanceId
					)
	BEGIN
		UPDATE Enterprise.PropertyInstance
		SET IsDeleted = 1,
			ThruDate = GETDATE()
		WHERE InstanceId = @InstanceId

		SELECT 
		@InstanceId AS RealPageId,
		'' AS ErrorMessage
	END
	ELSE
	BEGIN
		SELECT 
		@InstanceId AS RealPageId,
		'Property is in use. Cannot delete a property that is in use.' AS ErrorMessage
	END
END