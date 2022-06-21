CREATE PROCEDURE [Enterprise].[SavePersonaProductProperties]
(
	@PersonaId	BIGINT,
	@ProductId	INT,
	@json		varchar(max)
)
AS
BEGIN
	
	DECLARE @Now DATETIME = GETUTCDATE();
	declare @productinstances table ( id int identity, instanceId uniqueidentifier)

	Insert Into @productinstances(instanceId)
	Select ProductProperties.PropertyInstanceId 
	FROM  OPENJSON (@json)
	WITH(
		 PropertyInstanceId varchar(100)
	) AS ProductProperties

	declare @MAX_ID INT
	declare @Current_ID INT = 1

	--First de activate any property instances which is removed 
	UPDATE PIM
    SET
        Thrudate = @Now,
        Active = 0
	FROM Enterprise.PropertyInstanceMapping PIM
	INNER JOIN Enterprise.PropertyInstance P ON
		P.PropertyInstanceId = PIM.PropertyInstanceId
    WHERE PersonaID = @PersonaID
        AND ProductId = @ProductID
        AND Active = 1
		AND P.InstanceId NOT IN (Select distinct InstanceId From @productinstances)

	select @MAX_ID = max(id) from @productinstances

	while @Current_ID <= @MAX_ID
	begin
		declare @PropertyInstanceId bigint

		Select @PropertyInstanceId = [PropertyInstanceId]
		From Enterprise.PropertyInstance
		Where InstanceId = (Select instanceId
		From @productinstances Where id = @Current_ID)

		IF NOT EXISTS
        (
            SELECT 1
            FROM Enterprise.PropertyInstanceMapping
            WHERE PersonaID = @PersonaID
                AND ProductId = @ProductID
                AND PropertyInstanceId = @PropertyInstanceID
                AND Active = 1
        )
		BEGIN TRY
                INSERT INTO Enterprise.PropertyInstanceMapping
                ( PersonaId,
                  PropertyInstanceId,
                  ProductId
                )
                VALUES
                (@PersonaID,
                 @PropertyInstanceID,
                 @ProductID
                );                
        END TRY
        BEGIN CATCH
                DECLARE @ErrorLogID INT;
                EXEC dbo.LogError
                    @ErrorLogID = @ErrorLogID OUTPUT;
                SELECT 0 AS Id,
                    ErrorMessage
                FROM dbo.ErrorLog
                WHERE ErrorLogID = @ErrorLogID;
        END CATCH;
		set @Current_ID = @Current_ID + 1
	end
    SELECT	1 AS Id,'' AS ErrorMessage;
END