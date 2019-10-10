--http://jira.realpage.com/browse/GB-5053
DECLARE @RoleId INT;
SELECT @RoleId = r.RoleId
FROM enterprise.role r
     INNER JOIN enterprise.rolevaluetype rr ON rr.rolevaluetypeid = r.rolevaluetypeid
     INNER JOIN enterprise.[right] rt ON r.roleid = rt.roleid
     INNER JOIN enterprise.rightvaluetype rvt ON rvt.rightvaluetypeid = rt.rightvaluetypeid
WHERE productid = 24
      AND rr.value = 'migration analyst';
IF @RoleId IS NOT NULL
BEGIN
	DELETE FROM Enterprise.[Right]
	WHERE ROleId = @RoleId;
	DELETE FROM Enterprise.Role
	WHERE ROleId = @RoleId;
	DELETE FROM Enterprise.RoleValueType
	WHERE Value = 'migration analyst';
END
GO

--http://jira.realpage.com/browse/GB-5124
IF EXISTS(SELECT 1 FROM Enterprise.RoleValueType WHERE Value = 'external analyst')
BEGIN
	UPDATE Enterprise.RoleValueType 
		SET Value = 'Read Only Research'
	WHERE Value = 'external analyst'
END

IF EXISTS(SELECT 1 FROM Enterprise.RoleValueType WHERE Value = 'Implementation Analyst')
BEGIN
	UPDATE Enterprise.RoleValueType 
		SET Value = 'Implementation'
	WHERE Value = 'Implementation Analyst'
END

GO
-- UPDATE IDENTITY SERVER URL FOR AZURE
--- DEV
update ident.IdentityProviderSetting set value = 'https://www-dev.realpage.com/login/identity/connect/authorize' where value = 'https://myldev.corp.realpage.com/identity/connect/authorize'

--- QA
update ident.IdentityProviderSetting set value = 'https://www-qa.realpage.com/login/identity/connect/authorize' where value = 'https://mylqa.realpage.com/identity/connect/authorize'

--- SAT
update ident.IdentityProviderSetting set value = 'https://www-sat.realpage.com/login/identity/connect/authorize' where value = 'https://mylsat.realpage.com/identity/connect/authorize'

--- PREPROD
update ident.IdentityProviderSetting set value = 'https://www-preprod.realpage.com/login/identity/connect/authorize' where value = 'https://mylpreprod.realpage.com/identity/connect/authorize'

--- UAT
update ident.IdentityProviderSetting set value = 'https://www-uat.realpage.com/login/identity/connect/authorize' where value = 'https://myluat.realpage.com/identity/connect/authorize'

--- DEMO
update ident.IdentityProviderSetting set value = 'https://www-demo.realpage.com/login/identity/connect/authorize' where value = 'https://myldemo.realpage.com/identity/connect/authorize'

--- PROD
update ident.IdentityProviderSetting set value = 'https://www.realpage.com/login/identity/connect/authorize' where value = 'https://myl.realpage.com/identity/connect/authorize'
GO
update [Enterprise].[CommunicationEmailTemplate] set Body = '<!DOCTYPE html>
<html dir="ltr" lang="en">
<body>
    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:16px;">
        <tbody>
            <tr>
                <td>
                    <center>
                        <table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
                            <tbody>
                                <tr>
                                    <td align="left">
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:18px 0 0 0;">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:0 10px" align="center">
                                                                        <div style="display:none;color:#fff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">
                                                                            <span>Hi {FIRST NAME}, 
                                                                            Your RealPage Unified Platform account is ready! Your email address is your username. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.</span>
                                                                        </div>
                                                                        <a href="https://www.realpage.com" style="text-decoration:none;">
                                                                            <img src="{IMAGES}/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
                                                                        </a>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>Hi {FIRST NAME},</span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>
                                                                                             Your RealPage Unified Platform account is ready! Your email address is your username. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.
                                                                                        </span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
                                                                                                    <a href="{LINK}" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Set Your Password Now</a>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                               
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">        
                                                                             <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                    <span>
                                                                                                        If you have trouble accessing your profile, please contact your internal help desk or <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> for assistance. For additional information, please read our <a href="{UNIFIED}/RealPage Unified Platform Quick Steps.pdf" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color:#42A5F5;">Quick Start Guide</a>
                                                                                                    </span>
                                                                                                </td>                   
                                                                                            </tr>          
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                              
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">

                                                                                        This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed.  If you’ve received this email in error, please notify <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> by forwarding this email to <a href="mailto:support@realpage.com?subject=support">support@realpage.com</a>.  This message contains confidential information and is intended only for the individual named.
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
                        <tbody>
                            <tr>
                                <td align="center" width="100%">
                                    <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                        <tbody>
                                            <tr>
                                                <td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;font-size:11px;">
                                                    <a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Privacy Policy</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <a href="https://www.realpage.com/" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Contact Us</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2019 RealPage, Inc.</span>
                                                </td>
                                            </tr>                                          
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
    </center>
    </td>
    </tr>
    </tbody>
    </table>
</body>
</html>'
where CommunicationEmailTemplateID = 2
GO
update [Enterprise].[CommunicationEmailTemplate] set Body = '<!DOCTYPE html>
<html dir="ltr" lang="en">
<body>
    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:16px;">
        <tbody>
            <tr>
                <td>
                    <center>
                        <table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
                            <tbody>
                                <tr>
                                    <td align="left">
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:18px 0 0 0;">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:0 10px" align="center">
                                                                        <div style="display:none;color:#fff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">                                                                            
                                                                            <span>Hi {FIRST NAME}, 

                                                                            Your RealPage Unified Platform Administrator account is ready! Your email address is your username. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.</span>
                                                                        </div>
                                                                        <a href="https://www.realpage.com" style="text-decoration:none;">
                                                                            <img src="{IMAGES}/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
                                                                        </a>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>Hello {FIRST NAME},</span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>
                                                                                             Your RealPage Unified Platform Administrator account is ready! Your email address is your username. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.
                                                                                        </span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
                                                                                                    <a href="{LINK}" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Set Your Password Now</a>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                    <span>
                                                                                                        If you have trouble accessing your profile, please contact your internal help desk or <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> for assistance. For additional information, please read our <a href="{UNIFIED}/RealPage Unified Platform Quick Steps.pdf" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color:#42A5F5;">Quick Start Guide</a>
                                                                                                    </span>
                                                                                                </td>                   
                                                                                            </tr>          
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">

                                                                                        This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed.  If you’ve received this email in error, please notify <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> by forwarding this email to <a href="mailto:support@realpage.com?subject=support">support@realpage.com</a>.  This message contains confidential information and is intended only for the individual named.
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
                        <tbody>
                            <tr>
                                <td align="center" width="100%">
                                    <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                        <tbody>
                                            <tr>
                                                <td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;font-size:11px;">
                                                    <a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Privacy Policy</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <a href="https://www.realpage.com/" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Contact Us</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2019 RealPage, Inc.</span>
                                                </td>
                                            </tr>                                           
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
    </center>
    </td>
    </tr>
    </tbody>
    </table>
</body>
</html>'
where CommunicationEmailTemplateID = 1
GO