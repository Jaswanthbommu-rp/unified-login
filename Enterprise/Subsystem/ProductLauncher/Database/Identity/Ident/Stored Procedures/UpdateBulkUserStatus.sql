CREATE PROCEDURE [Ident].[UpdateBulkUserStatus]  
(@RealPageId     PARTYGUID READONLY,  
 @StatusTypeId   INT,  
 @FromDate       DATETIME,  
 @StatusThruDate DATETIME,  
 @PartyId        BIGINT  
)  
AS  
     BEGIN  
         DECLARE @ErrorLogID INT;  
         DECLARE @UserId BIGINT;  
         DECLARE @NOW DATETIME= GETUTCDATE();  
         DECLARE @RealPageGUID UNIQUEIDENTIFIER;  
         DECLARE @Rownum INT;  
         DECLARE @PStatus BIT;  
         DECLARE @CurrentStatusTypeID INT;  
         DECLARE @UserName NVARCHAR(50);  
   DECLARE @NonIDPTypeID INT;  
         DECLARE @NewUserRegistrationExpiryMinutes INT  
        
         IF  
              ( SELECT COUNT(*) FROM @RealPageId ) > 0  
             BEGIN  
                 SELECT IDENTITY( INT, 1, 1) AS RowNum,  
                        RP.RealPageID as 'RealPageId',  
                        ULP.StatusTypeId AS 'PStatus',  
      ULP.UserLoginId AS 'UserId',  
      ULP.OrganizationPartyId AS 'OrganizationPartyId',  
      ULP.PrimaryOrganization as 'PrimaryOrganization',  
      0 as 'TotalCompanies'  
                 INTO #UserList  
                 FROM @RealPageId RP  
      INNER JOIN Enterprise.Party P ON P.RealPageId = RP.RealPageId  
      INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
          
    ;With TotalUserCompanies ( UserId, TotalCompanies )  
     as ( SELECT ULP.UserLoginId, Count(1) FROM #UserList UL  
       INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId  
       GROUP BY ULP.UserLoginId  
      )  
    UPDATE #UserList  
     SET TotalCompanies = TUC.TotalCompanies  
     FROM #UserList UL  
      INNER JOIN TotalUserCompanies TUC  
       ON TUC.UserId = UL.UserId  
  
            END;  
         BEGIN TRY  
  
   Select @NonIDPTypeID = IdentityProviderTypeId    
   FROM [Ident].[IdentityProviderType] Where Name = 'ID3'  
              --Re Activating disable users  
              --case 1 -> user never logged in (pending or expired user status)  
              IF (@StatusTypeId = 1) --Active  
              Begin  
                   --Get new reg activity time and set to pending state  
                   SELECT     @NewUserRegistrationExpiryMinutes = ia.[ActivityTokenExpirationMinutes]  
                   FROM     [Ident].[ActivityConfiguration] ia  
                                  INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = ia.ActivityTypeId  
                   WHERE     ia.PartyId = @PartyId  
                   AND       iat.[ActivityCode] = 'NewUserRegistration'  
  
       UPDATE ULP  
          SET  
               ULP.StatusTypeId = 2, --pending  
               ULP.StatusThruDate = DATEADD(MINUTE, @NewUserRegistrationExpiryMinutes, GETUTCDATE()),
			   ULP.UserDeactivationDate = NULL
      FROM #UserList ULi  
      INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
     WHERE  
      UL.LastLoginDate IS NULL  
      AND UL.IdentityProviderTypeId = @NonIDPTypeID -- NON IDP  
      AND ULI.PStatus = 24 -- current user status is disabled  
  
     UPDATE ULP  
          SET  
				ULP.StatusTypeId = 1, --Active  
				ULP.StatusThruDate = null,  
				ULP.FromDate = @FromDate,  
				ULP.ThruDate = NULL,
				ULP.UserDeactivationDate = NULL
      FROM #UserList ULi  
      INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
     WHERE  
      UL.IdentityProviderTypeId <> @NonIDPTypeID -- 3rd party IDP  
      AND ULI.PStatus = 24 -- current user status is disabled  
  
     UPDATE ULP  
          SET  
				ULP.StatusTypeId = 1, --Active  
				ULP.StatusThruDate = null,  
				ULP.FromDate = @FromDate,  
				ULP.ThruDate = NULL,
				ULP.UserDeactivationDate = NULL
      FROM #UserList ULi  
      INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
     WHERE  
      (UL.LastLoginDate IS NOT NULL AND UL.IdentityProviderTypeId = @NonIDPTypeID)  
      AND ULI.PStatus = 24 -- current user status is disabled  
  
     UPDATE ULP  
      SET  
			   ULP.StatusTypeId = 1, --Active  
			   ULP.StatusThruDate = NULL,
			   ULP.UserDeactivationDate = NULL
     FROM #UserList ULi  
      INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
     WHERE  
      ULI.PStatus = 3 -- locked  
              End  
              ELSE  
              Begin  
     UPDATE ULP  
      SET  
       ULP.StatusTypeId = @StatusTypeId,  
       ULP.StatusThruDate = @StatusThruDate,
	   ULP.UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
     FROM #UserList ULi  
      INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
  
     IF @StatusTypeId = 24  
     BEGIN  
      UPDATE ULP  
       SET  
        ULP.StatusTypeId = @StatusTypeId,  
        ULP.StatusThruDate = @StatusThruDate,
		ULP.UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
      FROM #UserList ULi  
       INNER JOIN Ident.UserLogin UL ON UL.UserId = ULi.UserId  
       INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.OrganizationPartyId = @PartyId  
  
      -- GET THE OTHER COMPANIES THE USER HAS ACCESS TO AND DISABLE THEM  
      SELECT IDENTITY( INT, 1, 1) AS RowNum,  
       RealPageID,  
       PStatus,  
       UserId,  
       ULP.OrganizationPartyId  
      INTO #UserListMultiCompany  
      FROM #UserList RP  
      inner join ident.userloginpersona ULP ON RP.UserId = ulp.UserLoginId  
      WHERE  
       RP.PrimaryOrganization = 1  
       AND  
       RP.TotalCompanies > 1  
  
      UPDATE ULP  
       SET  
        ULP.StatusTypeId = @StatusTypeId,  
        ULP.StatusThruDate = @StatusThruDate,
		ULP.UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
      FROM #UserListMultiCompany ULi  
       INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ULi.UserId AND ULP.OrganizationPartyId != @PartyId  
      WHERE  
       ULP.StatusTypeId != 24  
     END  
              End  
               
             IF @FromDate IS NULL  
                OR @FromDate > @Now  
                AND @StatusTypeId = 3  
                 BEGIN  
                     UPDATE AA  
                       SET  
                           AA.[AttemptCount] = 0  
                     FROM #UserList ULi  
                          INNER JOIN Enterprise.Party P ON P.RealPageId = ULi.RealPageId  
                          INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId  
                          INNER JOIN Ident.[ActivityAttempts] AA ON UL.LoginName = AA.EnterpriseUserName  
                          INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId  
                          INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId  
                     WHERE ATP.ActivityTypeId IN(1, 2, 5)  
                           AND AA.[LastAttemptDateTime] > DATEADD(minute, -60, @Now)  
                           AND AC.PartyId = @PartyId;  
                 END;  
             SELECT '1' AS ErrorMessage;  
         END TRY  
         BEGIN CATCH  
             EXEC dbo.LogError  
                  @ErrorLogID = @ErrorLogID OUTPUT;  
             SELECT '-1' AS ErrorMessage  
             FROM dbo.ErrorLog  
             WHERE ErrorLogID = @ErrorLogID;  
             UPDATE #UserList  
               SET  
                   PStatus = 1  
             WHERE RowNum = @RowNum;  
         END CATCH;  
     END;