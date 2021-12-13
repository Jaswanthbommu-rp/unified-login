CREATE PROCEDURE [Person].[CreatePersona] (
	 @PersonRealPageId uniqueidentifier
	,@UserLoginPersonaId bigint
	,@OrganizationRealPageId uniqueidentifier
	,@PersonaTypeId int
	,@UserId bigint
	,@PersonaEnvironmentTypeId int
	,@FromDate datetime
	,@ThruDate datetime = NULL
    ,@PersonaName nvarchar(50) = 'Primary'
	,@PersonaId bigint = NULL OUTPUT)
AS
BEGIN
  IF @FromDate IS NULL
  BEGIN
    SELECT
      @FromDate = GETUTCDATE()
  END;
  BEGIN TRY
    DECLARE @PersonPartyId bigint;
    DECLARE @OrganizationPartyId bigint;
    DECLARE @MasterConfigurationId bigint;
    DECLARE @MasterConfigurationTypeId bigint;
    DECLARE @MasterSettingId int;
    DECLARE @MasterSettingName nvarchar(512);
    -- Get the Party ID for a Person

    SELECT
      @PersonPartyId = PartyId
    FROM Enterprise.Party
    WHERE RealPageId = @PersonRealPageId;

    -- Get the Party ID for an Organization

    SELECT
      @OrganizationPartyId = PartyId
    FROM Enterprise.Party
    WHERE RealPageId = @OrganizationRealPageId;
    BEGIN TRANSACTION;

      -- Check if it exists

      SELECT
        @PersonaId = PersonaId
      FROM Person.Persona PE
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
		INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
      WHERE UL.PersonPartyId = @PersonPartyId
      AND ULP.OrganizationPartyId = @OrganizationPartyId
      AND PE.PersonaTypeId = @PersonaTypeId;
      IF @PersonaId IS NULL
      BEGIN
        INSERT INTO Person.Persona (		
			 UserLoginPersonaId	--, UserId
			,PersonaTypeId
			,PersonaEnvironmentTypeId
			,FromDate
			,ThruDate
            ,PersonaName)
          VALUES (@UserLoginPersonaId, @PersonaTypeId, @PersonaEnvironmentTypeId, @FromDate, @ThruDate,@PersonaName);
        SET @PersonaId = SCOPE_IDENTITY();
      END;

	--insert the new persona in the ActivePersona table if one doesn't already exist
	IF NOT EXISTS ( SELECT TOP 1 1 FROM Person.ActivePersona where PartyId = @PersonPartyId )
	BEGIN
		INSERT INTO Person.ActivePersona (   
			 PartyId 
			,PersonaId)
		Values(@PersonPartyId,@PersonaId)
	END

    /*
    Add default settings 
    1. Add the row to MasterConfiguration Table
    2. Configure default values (ThemeColor, TimeZone)
    */

      SELECT
        @MasterConfigurationTypeId = MasterConfigurationTypeId
      FROM Enterprise.MasterConfigurationType
      WHERE Name = 'UserLogin';

      --Setup Theme Color
      SELECT
        @MasterSettingId = MasterSettingId
      FROM Enterprise.MasterSetting AS MS
      INNER JOIN Enterprise.MasterSettingType AS MST
        ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
      WHERE MST.MasterConfigurationTypeId = @MasterConfigurationTypeId
      AND MST.Name = 'ThemeColor'
      AND MS.Value = 'Light';
      IF NOT EXISTS (SELECT
          1
        FROM Enterprise.MasterConfiguration
        WHERE MasterConfigurationTypeId = @MasterConfigurationTypeId
        AND AttributeId = @UserId)
      BEGIN
        INSERT INTO Enterprise.MasterConfiguration (MasterConfigurationTypeId, AttributeId)
          VALUES (@MasterConfigurationTypeId, @UserId);
        SELECT
          @MasterConfigurationId = SCOPE_IDENTITY();
      END;
      IF NOT EXISTS (SELECT
          1
        FROM Enterprise.MasterConfigurationSetting
        WHERE MasterConfigurationId = @MasterConfigurationId
        AND MasterSettingId = @MasterSettingId)
      BEGIN
        INSERT INTO Enterprise.MasterConfigurationSetting (MasterConfigurationId, MasterSettingId)
          VALUES (@MasterConfigurationId, @MasterSettingId);
      END;

      --Setup default timezone (inherited from organization)

      SELECT
        @MasterSettingName = MS.Value
      FROM Enterprise.MasterConfigurationSetting AS MCS
      INNER JOIN Enterprise.MasterConfiguration AS MC
        ON MC.MasterConfigurationId = MCS.MasterConfigurationId
        INNER JOIN Enterprise.MasterSetting AS MS
          ON MCS.MasterSettingId = MS.MasterSettingId
        INNER JOIN Enterprise.MasterSettingType AS MST
          ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
        INNER JOIN Enterprise.MasterConfigurationType AS MCT
          ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
      WHERE MCT.Name = 'Organization'
      AND MC.AttributeId = @OrganizationPartyId
      AND MST.Name = 'TimeZone';
      SELECT
        @MasterSettingId = MasterSettingId
      FROM Enterprise.MasterSetting AS MS
      INNER JOIN Enterprise.MasterSettingType AS MST
        ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
      WHERE MST.MasterConfigurationTypeId = @MasterConfigurationTypeId
      AND MST.Name = 'TimeZone'
      AND MS.Value = @MasterSettingName;
      IF NOT EXISTS (SELECT
          1
        FROM Enterprise.MasterConfigurationSetting
        WHERE MasterConfigurationId = @MasterConfigurationId
        AND MasterSettingId = @MasterSettingId)
      BEGIN
        INSERT INTO Enterprise.MasterConfigurationSetting (MasterConfigurationId, MasterSettingId)
          VALUES (@MasterConfigurationId, @MasterSettingId);
      END;
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
