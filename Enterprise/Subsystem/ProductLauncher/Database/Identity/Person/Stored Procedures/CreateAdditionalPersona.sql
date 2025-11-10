Create PROCEDURE [Person].[CreateAdditionalPersona] (
	 @OrganizationRealPageId uniqueidentifier
	,@UserId bigint  
	,@CreatedBy bigint
    ,@PersonaName nvarchar(50)
	,@PersonaId bigint = NULL OUTPUT)
AS
BEGIN
  
  BEGIN TRY
    DECLARE @PersonPartyId bigint;
    DECLARE @OrganizationPartyId bigint;
    DECLARE @MasterConfigurationId bigint;
    DECLARE @MasterConfigurationTypeId bigint;
    DECLARE @MasterSettingId int;
    DECLARE @MasterSettingName nvarchar(512);
	Declare @PersonaEnvironmentTypeId int,
			@PersonaTypeId int, 
			@FromDate datetime,
			@UserLoginPersonaId bigint;

	SET @FromDate = GETUTCDATE()
    DECLARE @PlatformAdminRoleValue NVARCHAR(200);
    DECLARE @NOW DATETIME = GETUTCDATE();
    SELECT @PlatformAdminRoleValue = ps.Value
    FROM Enterprise.GlobalProductConfiguration gpc
    JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
    JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
    JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
    WHERE gpc.ProductId = 3
     AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
     AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
     AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
     AND pst.Name = 'PlatformAdminRole';

	Select @PersonaTypeId = PersonaTypeId From Person.PersonaType Where Name = 'Secondary'
	Select @PersonaEnvironmentTypeId = PersonaEnvironmentTypeID from Person.PersonaEnvironmentType Where Name = 'Production'
    
    -- Get the Party ID for an Organization

    SELECT @OrganizationPartyId = PartyId
    FROM Enterprise.Party
    WHERE RealPageId = @OrganizationRealPageId;

    BEGIN TRANSACTION;

      SELECT @UserLoginPersonaId = ULP.UserLoginPersonaId
      FROM Ident.UserLoginPersona ULP 
		INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
      WHERE ULP.OrganizationPartyId = @OrganizationPartyId
	  AND UL.UserId = @UserId
     
      IF @UserLoginPersonaId IS NOT NULL
      BEGIN
        INSERT INTO Person.Persona (		
			 UserLoginPersonaId	--, UserId
			,PersonaTypeId
			,PersonaEnvironmentTypeId
			,FromDate
			,ThruDate
            ,PersonaName)
          VALUES (@UserLoginPersonaId, @PersonaTypeId, @PersonaEnvironmentTypeId, @FromDate, NULL,@PersonaName);
        SET @PersonaId = SCOPE_IDENTITY();
      END;

	--assign persona role
	IF (@PersonaId IS NOT NULL AND @PersonaId > 0)
	BEGIN
		DECLARE @SettingValue Varchar(50),@RoleId int;

        SELECT	@SettingValue = ISNULL(ps.Value,@PlatformAdminRoleValue)
        FROM	Enterprise.GlobalProductConfiguration gpc
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
        WHERE  gpc.ProductId = 3
		AND pst.Name = 'EmployeeExternelUserDefautRole'
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))

		SELECT @RoleId = RoleId FROM Security.Role WHERE RoleTypeID = 1 AND RoleName = @SettingValue

		INSERT INTO Security.PersonaRole(PersonaId,RoleId,CreatedBy, FromDate,ThruDate)
		SELECT @PersonaId,@RoleId,@CreatedBy,@FromDate,NULL
		
	END    
      SELECT
        @PersonaId AS Id,
        '' AS ErrorMessage;
    COMMIT;
  END TRY
  BEGIN CATCH
    ROLLBACK;
    DECLARE @ErrorLogID int;
    EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
    SELECT
      0 AS Id,
      '' AS RealPageId,
      ErrorMessage
    FROM dbo.ErrorLog
    WHERE ErrorLogID = @ErrorLogID;
  END CATCH;
END;
