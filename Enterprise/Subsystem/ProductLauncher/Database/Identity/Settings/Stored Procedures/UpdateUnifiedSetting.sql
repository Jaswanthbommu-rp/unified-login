 CREATE PROCEDURE [Settings].[UpdateUnifiedSetting] (    
 @PartyId bigint,    
 @Category Varchar(50),    
 @JsonUnifiedSettings nvarchar(max),    
 @CreatedBy bigint)    
AS    
BEGIN    
 BEGIN TRY    
  DECLARE @settings TABLE (    
   Id int identity,    
   MappingName varchar(200),    
   MappingValue nvarchar(max),    
   Editable bit,    
   Hidden bit    
  )    
    
  Insert Into @settings(MappingName,MappingValue,Editable,Hidden)    
  SELECT Name,Value,Editable,Hidden    
  FROM OPENJSON (@JsonUnifiedSettings)    
  WITH ( Name varchar(max) '$.Name',    
    Value varchar(max) '$.Value',    
    Editable bit '$.Editable',    
    Hidden bit '$.Hidden')    
  WHERE ISJSON(@JsonUnifiedSettings) > 0    
    
  declare @MAX_ID INT    
  declare @Current_ID INT = 1    
  declare @mappingName varchar(200)    
  Declare @mappingValue varchar(100)    
  Declare @editable bit,@hidden bit    
  Declare @categoryId int    
  Declare @RightId bigint    
    
  Select @categoryId = SettingCategoryTypeId     
  From [Settings].[SettingCategoryType]     
  Where [Name] = @Category    
    
  IF (@categoryId IS NULL)    
  BEGIN    
   INSERT INTO [Settings].[SettingCategoryType] ([Name])    
   SELECT @Category    
    
   SET @categoryId = SCOPE_IDENTITY();    
  END    
    
  select @MAX_ID = max(Id) from @settings    
    
  WHILE @Current_ID <= @MAX_ID    
  begin    
   select @mappingName = MappingName,    
       @mappingValue = MappingValue,    
       @editable = Editable,@hidden = Hidden    
    from @settings where Id = @Current_ID    
       
       
   IF ((NULLIF(LTRIM(RTRIM(@mappingName)), '') IS NOT NULL) AND    
    (NULLIF(LTRIM(RTRIM(@mappingValue)), '') IS NOT NULL))    
   BEGIN    
    IF @mappingName IN ('Login', 'ForcedLock', 'NewUserRegistration')    
    BEGIN    
        
     UPDATE iac SET iac.ActivityTokenExpirationMinutes =    
       CASE    
        WHEN @mappingValue > 0 THEN @mappingValue    
        ELSE iac.ActivityTokenExpirationMinutes    
       END    
     FROM [Ident].[ActivityConfiguration] iac    
     INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId     
     WHERE iat.ActivityCode = 'ForcedLock'    
     AND @mappingName = 'ForcedLock'    
     AND PartyId = @PartyId    
    
     UPDATE iac SET iac.MaxActivityAttemptCount =    
       CASE    
        WHEN @mappingValue > 0 THEN @mappingValue    
        ELSE iac.MaxActivityAttemptCount    
       END    
     FROM [Ident].[ActivityConfiguration] iac    
     INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId     
     WHERE iat.ActivityCode = 'Login'    
     AND @mappingName = 'Login'    
     AND PartyId = @PartyId    
    
     UPDATE iac SET iac.ActivityTokenExpirationMinutes =    
       CASE    
        WHEN @mappingValue > 0 AND @mappingValue % 1 = 0 THEN @mappingValue * 1440    
        WHEN @mappingValue IS NOT NULL THEN @mappingValue    
        ELSE iac.ActivityTokenExpirationMinutes    
       END    
     FROM [Ident].[ActivityConfiguration] iac    
     INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId     
     WHERE iat.ActivityCode = 'NewUserRegistration'    
     AND @mappingName = 'NewUserRegistration'    
     AND PartyId = @PartyId    
    END    
    ELSE    
    BEGIN    
     IF NOT EXISTS (SELECT 1 FROM [Settings].[OrganizationSettings]    
        WHERE MappingName = @mappingName    
        And PartyId = @PartyId    
        And SettingCategoryTypeId = @categoryId)    
    
     BEGIN    
      INSERT INTO [Settings].[OrganizationSettings](PartyId,SettingCategoryTypeId,MappingName,    
       MappingValue,Editable,Hidden,CreatedBy,CreatedDate)    
      SELECT @PartyId,@categoryId,@mappingName,@mappingValue,@editable,@hidden,@CreatedBy,GETUTCDATE()    
     END    
     ELSE    
     BEGIN    
      UPDATE [Settings].[OrganizationSettings] SET MappingValue = @mappingValue,    
        UpdatedDate = GETUTCDATE()    
      WHERE MappingName = @mappingName    
      And PartyId = @PartyId    
      And SettingCategoryTypeId = @categoryId    
     END    
     SELECT @RightId = RightId from [Security].[Right] where RightName = @mappingName;    
     SELECT @mappingValue = MappingValue FROM [Settings].[OrganizationSettings]    
        WHERE MappingName = @mappingName    
        And PartyId = @PartyId    
        And SettingCategoryTypeId = @categoryId    
       
     IF @RightId IS NOT NULL
     BEGIN
     IF  @mappingValue IS NOT NULL AND @mappingValue = '1' AND NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]    
        WHERE RightId = @RightId     
        And OrgPartyId = @PartyId)    
     BEGIN     
      INSERT INTO [Security].[OrganizationOverRideRight](RightId, OrgPartyId, VisibilityStatusId, CreatedBy, CreatedDate)    
      SELECT @RightId, @PartyId, 9, @CreatedBy, GETUTCDATE()    
     END    
     ELSE    
     BEGIN    
      DELETE FROM [Security].[OrganizationOverRideRight]    
      WHERE RightId = @RightId     
      AND OrgPartyId = @PartyId    
     END         
    END 
    END
   END    
   set @Current_ID = @Current_ID + 1    
  end    
  SELECT COUNT(Id) AS Id,    
     '' AS ErrorMessage    
  FROM @settings    
    
 END TRY    
 BEGIN CATCH    
        DECLARE @ErrorLogID int;    
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
        SELECT 0 AS Id,    
     ErrorMessage    
        FROM dbo.ErrorLog    
        WHERE ErrorLogID = @ErrorLogID;    
 END CATCH    
END    