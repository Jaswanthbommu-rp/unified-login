GO
IF NOT EXISTS(SELECT 1 FROM [Enterprise].[RoleAttributeType] WHERE Name = 'AccessAllProperties')
BEGIN
	INSERT INTO [Enterprise].[RoleAttributeType] (Name)
		VALUES ('AccessAllProperties')
END
GO
DECLARE @RAType NVARCHAR(50)
DECLARE @RVTId INT 
SELECT @RAType = RoleAttributeTypeId FROM [Enterprise].[RoleAttributeType] WHERE Name = 'AccessAllProperties'
SELECT @RVTId = RoleValueTypeId FROM Enterprise.RoleValueType WHERE Value = 'Manage Amenity No Pricing'
IF NOT EXISTS( SELECT 1 FROM [Enterprise].[RoleAttribute] WHERE ROleAttributeTypeId = @RAType AND ROleValueTypeId = @RVTId)
BEGIN
	INSERT INTO [Enterprise].[RoleAttribute] (RoleAttributeTypeId, RoleValueTypeId, Value)
	VALUES (@RAType, @RVTId, 1)
END

SELECT @RVTId = RoleValueTypeId FROM Enterprise.RoleValueType WHERE Value = 'Manage Amenity With Pricing'
IF NOT EXISTS( SELECT 1 FROM [Enterprise].[RoleAttribute] WHERE ROleAttributeTypeId = @RAType AND ROleValueTypeId = @RVTId)
BEGIN
	INSERT INTO [Enterprise].[RoleAttribute] (RoleAttributeTypeId, RoleValueTypeId, Value)
	VALUES (@RAType, @RVTId, 1)
END
GO


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'AppPartner'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'AppPartner', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Commercial'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Commercial', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Developer'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Developer', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Fee Manager'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Fee Manager', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Investor'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Investor', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Multifamily'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Multifamily', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Other'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Other', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Owner'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Owner', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Property'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Property', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Single Family'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Single Family', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Supplier'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Supplier', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Test Data'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Test Data', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Vacation Rental'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Vacation Rental', GETUTCDATE() );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.OrganizationType
	WHERE Name = 'Vendor'
)
BEGIN
	INSERT INTO Enterprise.OrganizationTYpe( Name, CreateDate )
	VALUES( 'Vendor', GETUTCDATE() );
END;

GO
DECLARE @OrgType INT
SELECT @OrgType = OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name = 'Multifamily'
IF NOT EXISTS(SELECT 1 FROM Enterprise.Organization WHERE ORganizationTypeId > 0)
BEGIN
UPDATE Enterprise.Organization	
	SET ORganizationTypeId = @OrgType
WHERE Name != 'RealPage Employee'
END
GO
DECLARE @OrgType INT
SELECT @OrgType = OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name = 'Other'
UPDATE Enterprise.Organization	
	SET ORganizationTypeId = @OrgType
WHERE Name = 'RealPage Employee'
GO
UPDATE Enterprise.OrganizationType SET Thrudate = GETUTCDATE()
WHERE Name IN 
(
'AppPartner'
,'Commercial'
,'Developer'
,'Fee Manager'
,'Investor'
,'Owner'
,'Property'
,'Single Family'
,'Supplier'
,'Test Data'
,'Vacation Rental'
)
GO



GO
Update [Enterprise].[CommunicationEmailTemplate] SET Body = '<!DOCTYPE html>
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

                                                                            Your RealPage Unified Platform Administrator account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.</span>
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
                                                                                             Your RealPage Unified Platform Administrator account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.
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
Where CommunicationEmailTemplateID = 1
GO
Update [Enterprise].[CommunicationEmailTemplate] SET Body = '<!DOCTYPE html>
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
                                                                            Your RealPage Unified Platform account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.</span>
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
                                                                                             Your RealPage Unified Platform account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have {EXPIRYDAYS} days to log in before the link below expires.
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
Where CommunicationEmailTemplateID = 2
GO
GO
UPDATE PS
	SET Value = 1
  FROM [Enterprise].[ProductSettingType] PST
  INNER JOIN Enterprise.ProductSetting PS
	ON PS.ProductSettingTypeId = PST.ProductSettingTypeId
INNER JOIN Enterprise.Product P
	ON P.ProductId = PS.ProductId
  WHERE PST.Name = 'productNotAvailableForRegularUserNoEmail'
  AND P.Name = 'ILM Lead Management'
GO
---USER STATUS CHNAGES
IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTYpe WHERE Name = 'Troubleshoot')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusType ON
	INSERT INTO Enterprise.StatusType (StatusTYpeId, Name)
		VALUES (0, 'Troubleshoot')
	SET IDENTITY_INSERT Enterprise.StatusType OFF
END
GO
IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTYpe WHERE Name = 'Expired')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusType ON
	INSERT INTO Enterprise.StatusType (StatusTYpeId, Name)
		VALUES (23, 'Expired')
	SET IDENTITY_INSERT Enterprise.StatusType OFF
END
GO
IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTYpe WHERE Name = 'Disabled')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusType ON
	INSERT INTO Enterprise.StatusType (StatusTYpeId, Name)
		VALUES (24, 'Disabled')
	SET IDENTITY_INSERT Enterprise.StatusType OFF
END
GO
GO
IF EXISTS (SELECT 1 FROM Enterprise.RightValueType WHERE Value = 'Access to GetWork')
BEGIN
	UPDATE Enterprise.RightValueType 
	SET Value = 'Access to Vendor Marketplace' 
	WHERE Value = 'Access to GetWork'
END
GO

IF EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'GetWork')
BEGIN
	UPDATE Enterprise.Product
		SET Name = 'Vendor Marketplace'
	WHERE Name = 'GetWork'
END

GO
IF EXISTS(SELECT 1 FROM Enterprise.ProductType WHERE Name = 'GetWork')
BEGIN
	UPDATE Enterprise.ProductTYpe
		SET Name = 'Vendor Marketplace', Description = 'Vendor Marketplace'
	WHERE Name = 'GetWork'
END


GO

 

GO
DECLARE @ProductTypeId INT= 505, 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Integration Marketplace';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;
select newid()
--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'Integration Marketplace'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 505,
             @ParentProductTypeId = 500,
             @Name = 'Integration Marketplace',
             @Description = 'Integration Marketplace',
             @ProductTypeGUID = '9FBECC10-2CA1-4D62-8435-CD3CAE02101B';
    END;

SET @ProductId = 39;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = '28FABBF8-BC06-4441-9F6D-6F91932BFE13',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 505;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'IMP'
        WHERE ProductId = @ProductId;
    END;



SET @ProductId = 39;
INSERT INTO @ProductConfiguration
(SettingName,
SettingDescription,
SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','integrationmarketplace')
,('ProductUrl','','/product/IntegrationMarketplace')
,('TitleId','','Integration Marketplace')
,('TitleUniqueId','','D7A45BAE-4F86-4BFB-80C1-A6B9809B5E6F')
,('IsNewTab','','1')
,('MetatagUniqueId','','IntegrationMarketplace')
,('IsResource','','1')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('LockOnProductAccess', '', '0')
,('ApiEndPoint','','https://qa-rpxapi.realpage.com/intmarketplaceuiapi')
,('ProductStatus', '', '8')


SET @ProductID = 39
SET @LoginURI = 'https://qa-rpx.realpage.com'
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;

GO



DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Manage Integration Marketplace Product Access', 
'Manage Integration Marketplace Product Access',
'AccessIntegrationMarketplace' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Integration Marketplace';

SET @ActionValueID = 1

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Integration Marketplace' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Integration Marketplace', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Dashboard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Integration Marketplace' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Integration Marketplace', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.PartyId = 350;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_IntegrationMarketplace', @ShortName = 'AccessIntegrationMarketplace', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Integration Marketplace'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_IntegrationMarketplace';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Integration Marketplace'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_IntegrationMarketplace';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Integration Marketplace Product Access' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;

GO
 IF NOT EXISTS( SELECT 1  FROM [Ident].[SamlAttribute] WHERE Name = 'RoleCode')
 BEGIN
	EXECUTE [Ident].[CreateSamlAttribute] @AttributeName = 'RoleCode', @SamlAttributeTypeId = 1
 END
 GO

IF EXISTS (SELECT 1 FROM Enterprise.RightValueType WHERE VALUE = 'Access to Vendor MarketPlace')
BEGIN
	UPDATE Enterprise.RightValueType
		SET Value = 'Access to Vendor Marketplace'
	WHERE Value = 'Access to Vendor MarketPlace'
END

GO
    ----STATUS MERGE


GO
WITH CTEFDate
     AS (
     SELECT USerId,
            [1],
            [2],
            [3]
     FROM
		(
			SELECT ul.UserId,
				   us.StatusTypeId,
				   us.FromDate
			FROM Ident.UserLogin AS ul
				 INNER JOIN Person.Persona PE ON Ul.UserId = PE.Userid
				 INNER JOIN Enterprise.Party AS p ON p.PartyId = pe.PersonPartyId
				 LEFT JOIN Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId
		) AS T PIVOT(MAX(FromDate) FOR StatusTypeId IN([1],
                                               [2],
                                               [3])) AS P1),
     CTETDate
     AS (
     SELECT USerId,
            [1],
            [2],
            [3]
     FROM
		(
			SELECT ul.UserId,
				   us.StatusTypeId,
				   us.ThruDate
			FROM Ident.UserLogin AS ul
				 INNER JOIN Person.Persona PE ON Ul.UserId = PE.Userid
				 INNER JOIN Enterprise.Party AS p ON p.PartyId = pe.PersonPartyId
				 LEFT JOIN Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId
		) AS T PIVOT(MAX(THruDate) FOR StatusTypeId IN([1],
                                               [2],
                                               [3])) AS P1),
     OneSiteUSers
     AS (
     SELECT PersonaId,
            ProductId
     FROM Enterprise.PersonaConfiguration
     WHERE ProductId = 1)
     SELECT UL.UserId,
            P.PersonaId,
            PA.Name,
            PE.FIrstName,
            PE.LastNAme,
            UL.LoginName,
            UL.LastLoginDate,
            UL.FromDate AS ULActiveDate,
            UL.ThruDate AS ULThrudate,
            A.[1] AS ActiveFdate,
            A.[2] AS PendingFDate,
            A.[3] LockedFDate,
            B.[1] AS ActiveTdate,
            B.[2] AS PendingTDate,
            B.[3] LockedTDate,
            CASE
                WHEN PC.ProductId IS NULL
                THEN 0
                ELSE 1
            END 'IsOneSitAssigned',
            CAST('NoStatus' AS NVARCHAR(50)) AS PStatus
     INTO #Temp
     FROM CTEFDate A
          INNER JOIN CTETDate B ON B.UserId = A.UserId
          INNER JOIN Ident.UserLogin UL ON A.UserId = UL.UserId
          RIGHT JOIN Person.Persona P ON P.UserId = UL.UserId
          INNER JOIN Enterprise.Organization PA ON PA.PartyId = P.OrganizationPartyId
          INNER JOIN Person.Person PE ON PE.PartyId = P.PersonPartyID
          LEFT JOIN OneSiteUSers PC ON PC.PersonaId = P.PersonaId
     ORDER BY UserId;

UPDATE #Temp
  SET
      PStatus = CASE
                    WHEN U.IdentityProviderTypeId != 4
                    THEN 'Active'
                END
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId;

UPDATE #Temp
  SET
      PStatus = 'Active'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NULL
      AND ActiveTdate IS NULL
      AND PendingTDate IS NULL
      AND LockedTDate IS NULL
      AND PStatus IS NULL;

UPDATE #Temp
  SET
      PStatus = 'Active'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NOT NULL
      AND ActiveTdate IS NULL
      AND PendingTDate IS NULL
      AND LockedTDate IS NULL
      AND PStatus IS NULL;

UPDATE #Temp
  SET
      PStatus = 'Active'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NOT NULL
      AND ActiveTdate IS NULL
      AND PendingTDate IS NULL
      AND LockedTDate < GETUTCDATE()
      AND PStatus IS NULL;
UPDATE #Temp
  SET
      PStatus = 'Active'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NULL
      AND (LockedFDate IS NULL
           OR LockedFDate < GETUTCDATE())
      AND ActiveTdate IS NULL
      AND PendingTDate IS NULL
      AND (LockedTDate IS NULL
           OR LockedFDate < GETUTCDATE() - 1)
      AND PStatus IS NULL;

--UPDATE #Temp
--  SET
--      PStatus = 'Pending'
--FROM #Temp A
--     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
--WHERE ActiveFDate IS NOT NULL
--      AND (PendingFDate IS NOT NULL
--           OR PendingFDate < GETUTCDATE())
--      AND (ActiveTdate IS NULL
--           OR ActiveTDate > GETUTCDATE())
--      AND PendingTDate > DATEADD(HH, 72, PendingFDate);

UPDATE #Temp
  SET
      PStatus = 'Expired'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NULL
      AND ActiveTdate IS NULL
      AND PendingTDate IS NOT NULL
      AND LockedTDate IS NULL
      AND PendingFDate < PendingTDate
      AND PStatus IS NULL;

UPDATE #Temp
  SET
      PStatus = 'Expired' 
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE A.LastLoginDate IS NULL
      AND ActiveFDate IS NOT NULL
      AND ULThruDate IS NULL
      AND PendingFDate < GETUTCDATE()
      AND LockedFDate IS NULL
      AND ActiveTdate IS NULL
      AND (PEndingTDate IS NOT NULL
           OR PendingTDate < GETUTCDATE())
      AND LockedTDate IS NULL
      AND PSTatus != 'Expired';


UPDATE #Temp
  SET
      PStatus = 'Expired'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NOT NULL
      AND ActiveTdate IS NULL
      AND PendingTDate IS NOT NULL
      AND LockedTDate IS NULL
      AND PendingFDate < PendingTDate

UPDATE #Temp
  SET
      PStatus = 'Expired'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND ULThruDate IS NULL
      AND PendingFDate < GETUTCDATE()
      AND LockedFDate IS NULL
      AND ActiveTdate IS NULL
      AND (PEndingTDate IS NOT NULL
           AND PendingTDate < GETUTCDATE())
      AND LockedTDate IS NULL
      AND PSTatus != 'Expired';
UPDATE #Temp
  SET
      PStatus = 'DISABLED'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE 
(ActiveFDate < ActiveTDate
 OR ActiveTDate < ActiveFDAte);
UPDATE #Temp
  SET
      PStatus = 'DISABLED'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE(ULActiveDate IS NOT NULL
      AND ULThruDate < GETUTCDATE());
UPDATE #Temp
  SET
      PStatus = 'Locked'
FROM #Temp A
     INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
WHERE ActiveFDate IS NOT NULL
      AND PendingFDate IS NOT NULL
      AND LockedFDate IS NOT NULL
      AND LockedTDate > GETUTCDATE();


UPDATE UL
	SET UL.CreateDate =  P.FromDate
 FROM
	Ident.UserLogin UL
		INNER JOIN Person.Persona P
		ON P.UserId = UL.UserId

UPDATE UL
	SET UL.StatusId = S.StatusTypeId
FROM #Temp T
INNER JOIN Ident.UserLogin UL
	ON UL.UserId = T.UserId
INNER JOIN Enterprise.StatusType S
	ON S.Name = T.PStatus


