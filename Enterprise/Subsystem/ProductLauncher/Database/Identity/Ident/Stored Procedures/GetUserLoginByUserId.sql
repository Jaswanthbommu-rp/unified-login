CREATE PROCEDURE [Ident].[GetUserLoginByUserId]  
(  
 @UserId     INT              = NULL  
)  
AS  
   BEGIN  
       SELECT DISTINCT  
              ul.UserId,  
              ul.PersonPartyId AS PartyId,  
              ul.[LoginName],  
              p.RealPageId,  
              ULP.StatusTypeId AS StatusId,  
              ul.PasswordModifiedDate,  
              ul.PasswordHash,  
              ul.PasswordSalt,  
              ULP.FromDate,  
              ULP.ThruDate,  
              ULP.StatusThruDate,  
              ul.LastLoginDate 'LastLogin',  
              '' AS [TimeZoneOffset],--MS.Value [TimeZoneOffset],
			  ul.TwoFactorEnabled [TwoFactorEnabled]
       FROM Ident.UserLogin ul  
            INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = Ul.UserId  
            JOIN Enterprise.Party p ON p.PartyId = ul.PersonPartyId  
            --INNER JOIN Enterprise.MasterConfiguration MC ON MC.AttributeId = UL.UserId  
            --INNER JOIN Enterprise.MasterConfigurationSetting MCS ON MC.MasterConfigurationId = MCS.MasterConfigurationId  
            --INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId  
            --INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId  
            --INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId  
       WHERE(UL.UserId = @UserId)  
            --AND MCT.Name = 'UserLogin'  
            --AND MST.Name = 'TimeZone'  
            AND ULP.PrimaryOrganization = 1 -- ONLY JOIN TO THE PRIMARY ORG FOR THIS PROC  ;  
              
     END;