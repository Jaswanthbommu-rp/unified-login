
Create PROCEDURE [Enterprise].[CreateOrganizationProductConfiguration](
	 @PartyId int
	,@ProductId int
	,@FromDate datetime = NULL
	,@ThruDate datetime = NULL
)

AS

BEGIN

	SET NOCOUNT ON;
	DECLARE @NOW datetime = GETUTCDATE();
	DECLARE @ProductSettingId int = NULL
	DECLARE @ConfigurationId int = NULL

	IF @Fromdate IS NULL
		SET @FromDate = @NOW;

	
	SELECT @ConfigurationId = p.ConfigurationId 
	FROM Enterprise.ProductConfiguration P
	Join Enterprise.Organizationproduct op on
		op.ConfigurationId = p.ConfigurationId
	Where op.ProductID = @ProductId
	And op.PartyId = @PartyId
	And op.ThruDate IS NULL
	And op.ConfigurationId NOT IN (select ConfigurationId from Enterprise.GlobalProductConfiguration Where ProductId = @ProductId and ThruDate IS NULL)

	
	 BEGIN TRY
				IF @ConfigurationId IS NULL
				BEGIN
					INSERT INTO Configuration (CreateDate) 
					VALUES (@NOW);

					SELECT @ConfigurationId = SCOPE_IDENTITY();
				END
				ELSE
				BEGIN
					SELECT @ConfigurationId as Id, '' AS ErrorMessage
				END
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

        END CATCH;
END;