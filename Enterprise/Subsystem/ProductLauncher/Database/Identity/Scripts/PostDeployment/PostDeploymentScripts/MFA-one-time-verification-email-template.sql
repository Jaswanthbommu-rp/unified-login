-- Insert MFA AudienceType if it does not exist
IF NOT EXISTS (SELECT 1 FROM Enterprise.CommunicationEventAudienceType WHERE Description = 'MFA Code')
BEGIN
    INSERT INTO Enterprise.CommunicationEventAudienceType (Description)
    VALUES ('MFA Code')
END
GO

-- Insert MFA PurposeType if it does not exist
IF NOT EXISTS (SELECT 1 FROM Enterprise.CommunicationEventPurposeType WHERE Description = 'MFA Verification')
BEGIN
    INSERT INTO Enterprise.CommunicationEventPurposeType (Description)
    VALUES ('MFA Verification')
END
GO

-- Insert MFA Email Template if it does not exist
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.CommunicationEmailTemplate
    WHERE CommunicationEventAudienceTypeId = 
        (SELECT CommunicationEventAudienceTypeId FROM Enterprise.CommunicationEventAudienceType WHERE Description = 'MFA User')
    AND CommunicationEventPurposeTypeId = 
        (SELECT CommunicationEventPurposeTypeId FROM Enterprise.CommunicationEventPurposeType WHERE Description = 'MFA Verification')
)
BEGIN
    INSERT INTO [Enterprise].[CommunicationEmailTemplate]( CommunicationEventAudienceTypeId, CommunicationEventPurposeTypeId, [Subject], [Body] )
    VALUES(
        (SELECT CommunicationEventAudienceTypeId FROM Enterprise.CommunicationEventAudienceType WHERE Description = 'MFA Code'),
        (SELECT CommunicationEventPurposeTypeId FROM Enterprise.CommunicationEventPurposeType WHERE Description = 'MFA Verification'),
        'One-time verification code',
        '<!DOCTYPE html>
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
                                                                                <!-- Greeting -->
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
                                                                                <!-- Message -->
                                                                                <tr>
                                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                        <span>Your one-time authentication code is:</span>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                                <!-- Authentication Code -->
                                                                                <tr>
                                                                                    <td align="center" style="padding:24px 0;">
                                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td style="font-size:32px; font-weight:bold; letter-spacing:8px; color:#212121; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; padding:16px 32px; border:1px solid #e0e0e0; border-radius:4px;">
                                                                                                        {AUTHENTICATION CODE}
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                                <!-- Expiry Notice -->
                                                                                <tr>
                                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                        <span>This code is valid for 5 minutes. Please enter your code into the multi-factor authentication prompt to sign in.</span>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                                <!-- Confidentiality Footer -->
                                                                                <tr>
                                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">
                                                                                                        This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed. If you''ve received this email in error, please notify <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> by forwarding this email to <a href="mailto:support@realpage.com?subject=support">support@realpage.com</a>. This message contains confidential information and is intended only for the individual named.
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
                            </tbody>
                        </table>
                    </center>
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
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2021 RealPage, Inc.</span>
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
</body>
</html>'
    )
END;
GO
