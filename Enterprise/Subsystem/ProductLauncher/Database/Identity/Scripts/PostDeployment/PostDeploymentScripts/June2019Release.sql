IF NOT EXISTS(SELECT 1 FROM ENterprise.CommunicationEventAudienceType WHERE Description = 'External User')
BEGIN
	INSERT INTO Enterprise.CommunicationEventAudienceType ( Description)
		VALUES('External User')
END
GO
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.CommunicationEmailTemplate
	WHERE CommunicationEventAudienceTypeId = 3 AND 
		  CommunicationEventPurposeTypeId = 1
)
BEGIN
	INSERT INTO [Enterprise].[CommunicationEmailTemplate]( CommunicationEventAudienceTypeId, CommunicationEventPurposeTypeId, [Subject], [Body] )
	VALUES( 3, 1, 'Welcome to the New RealPage Platform!', '<!DOCTYPE html>
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
</html>' );
END;
GO


--DECLARE @OrgRowNum INT;
--DECLARE @ActionID INT;
--DECLARE @RightID INT;
--DECLARE @RoleID INT;
--DECLARE @Status INT;
--DECLARE @ActionValueID INT;
--DECLARE @OrgID INT;
--DECLARE @ProductID INT;
--DECLARE @ParentActionId INT;
--DECLARE @UserActionId INT;
--DECLARE @RightCategoryId INT;
--DECLARE @PartyId INT;
--DECLARE @RightName VARCHAR(100);
--DECLARE @RVT INT;
--DECLARE @DefaultRoute NVARCHAR(200);
--DECLARE @RightValueTypeId INT;
--DECLARE @StatusId INT;
--DECLARE @PersonaId INT;
--DECLARE @FromDate DATETIME;
--DECLARE @TRoleId INT;
--DECLARE @TRoleName NVARCHAR(500);
--DECLARE @TRoleDesc NVARCHAR(500);
--DECLARE @TRightId INT;
--DECLARE @TRightName NVARCHAR(500);
--DECLARE @TRightDesc NVARCHAR(500);
--DECLARE @RightCategory INT;
--DECLARE @RoleCategory INT;
--DECLARE @RoleName NVARCHAR(500);
--DECLARE @RoleTypeID INT;
--DECLARE @PerosonaP INT;
--DECLARE @PartyRowNum INT;
--DECLARE @TRightShortName NVARCHAR(100)
--DECLARE @TargetProductId INT;
--DECLARE @VisibilityStatusId INT
--IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
--BEGIN
--	DROP TABLE #RightsUnifiedSettings;
--END;

--IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
--BEGIN
--	DROP TABLE #HoldPartyForUnifiedSettings;
--END;

--CREATE TABLE #RightsUnifiedSettings
--( 
--			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
--);

--INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
--VALUES( 1, 'View Unified Settings', 'View Unified Settings', 'ViewUnifiedSettings' );

--SELECT @ProductId = ProductId
--FROM Enterprise.Product
--WHERE name = 'Unified Platform';

----select * from enterprise.product where name like '%lead%'

--SELECT @TargetProductId = ProductId
--FROM Enterprise.Product
--WHERE Name = 'Unified Platform';

--SET @ActionValueID = 1

--SELECT @RoleCategory = ST.StatusTypeId
--FROM Enterprise.StatusTypeCategoryType AS STCT
--	 JOIN
--	 Enterprise.StatusTypeCategory AS STC
--	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
--	 JOIN
--	 Enterprise.StatusTypeCategoryClassification AS STCC
--	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
--	 JOIN
--	 Enterprise.StatusType AS ST
--	 ON ST.StatusTypeId = STCC.StatusTypeId
--WHERE STC.Name = 'Role Type' AND 
--	  ST.Name = 'System';

--SELECT @RightCategory = ST.StatusTypeId
--FROM Enterprise.StatusTypeCategoryType AS STCT
--	 JOIN
--	 Enterprise.StatusTypeCategory AS STC
--	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
--	 JOIN
--	 Enterprise.StatusTypeCategoryClassification AS STCC
--	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
--	 JOIN
--	 Enterprise.StatusType AS ST
--	 ON ST.StatusTypeId = STCC.StatusTypeId
--WHERE STC.Name = 'Right Type' AND 
--	  ST.Name = 'System';


--SELECT @Status = StatusType.StatusTypeID
--FROM Enterprise.StatusTypeCategoryType
--	 JOIN
--	 Enterprise.StatusTypeCategory
--	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
--	 JOIN
--	 Enterprise.StatusTypeCategoryClassification
--	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
--	 JOIN
--	 Enterprise.StatusType
--	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
--WHERE StatusType.name = 'ALL' AND 
--	  StatusTypeCategoryType.Name = 'Security';

--SET @VisibilityStatusId = @Status;

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.ACTION
--	WHERE ObjectValue = 'View Unified Settings' AND 
--		  ParentActionId IS NULL
--)
--BEGIN
--	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
--	SELECT @ActionID AS N'@ActionID';
--END;

--SELECT @ParentActionId = ActionId
--FROM Enterprise.ACTION
--WHERE ObjectValue = 'Settings' AND 
--	  ObjectType = 'Route' AND 
--	  Description = 'SuperUser';

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.ACTION
--	WHERE ObjectValue = 'View Unified Settings' AND 
--		  ParentActionID = @ParentActionId
--)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
--	SELECT @ActionID AS N'@ActionID';
--END;

--SELECT DISTINCT 
--	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
--INTO #HoldPartyForUnifiedSettings
--FROM Person.Persona AS P
--	 INNER JOIN
--	 Enterprise.Organization AS O
--	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.Name = 'RealPage Employee'

--WHILE EXISTS
--(
--	SELECT 1
--	FROM #HoldPartyForUnifiedSettings
--	WHERE PStatus = 0
--)
--BEGIN
--	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
--	FROM #HoldPartyForUnifiedSettings
--	WHERE PStatus = 0;
--	SELECT @RoleId = RoleId
--	FROM Enterprise.Role AS R
--		 INNER JOIN
--		 Enterprise.RoleValueType AS RR
--		 ON RR.RoleValueTypeId = R.RoleValueTypeId
--	WHERE RR.Value = 'User Administrator' AND 
--		  R.PartyId = @PartyId;
--	DECLARE Rights CURSOR
--	FOR SELECT RightId, Name, Description, ShortName
--		FROM #RightsUnifiedSettings;
--	OPEN Rights;
--	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
--	WHILE @@FETCH_STATUS = 0
--	BEGIN
--		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewUnifiedSetting', @ShortName = 'ViewUnifiedSettings', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
--		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


--		SELECT @ActionID = ActionID
--		FROM Enterprise.ACTION
--		WHERE ObjectValue = 'View Unified Settings'  AND 
--			  ObjectType = 'Right' AND 
--			  ParentActionId IS NULL;
--		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

--		SELECT @RightId = RightId
--		FROM Enterprise.[Right] AS R
--			 INNER JOIN
--			 Enterprise.RightValueType AS RR
--			 ON RR.RightValueTypeId = R.RightValueTypeId
--		WHERE RR.Value = 'Default_ViewUnifiedSetting';
--		SELECT @ActionID = ActionID
--		FROM Enterprise.ACTION
--		WHERE ObjectValue = 'View Unified Settings'   AND 
--			  ObjectType = 'Right' AND 
--			  ParentActionId IS NULL;
--		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



--		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
--	END;
--	CLOSE Rights;
--	DEALLOCATE Rights;
--	UPDATE #HoldPartyForUnifiedSettings
--	  SET PStatus = 1
--	WHERE RowNumber = @PartyRowNum;
--END;

--DECLARE @Dashboard int;

--SELECT @DashBoard = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value = 'Default_ViewUnifiedSetting';

--SELECT @RightValueTypeId = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value IN( 'View Unified Settings' );

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RightDependency
--	WHERE RightValueTypeId = @RightValueTypeId AND 
--		  DependentRightValueTypeId = @DashBoard
--)
--BEGIN
--	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
--	VALUES( @RightValueTypeId, @DashBoard );

--END;
--GO

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
DECLARE @TRightShortName NVARCHAR(100)
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
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
VALUES( 1, 'Manage Unified Platform Security Settings', 'Manage Unified Platform Security Settings', 'ManagePlatFormSecurity' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

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
	WHERE ObjectValue = 'Manage Platform Security' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Platform Security', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Settings' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Platform Security' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Platform Security', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManagePlatFormSecurity', @ShortName = 'ManagePlatFormSecurity', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Platform Security'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManagePlatFormSecurity';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Platform Security'   AND 
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
GO
DECLARE @Dashboard int;
DECLARE @SideMenuRight INT
DECLARE @SideMenuRoute INT
DECLARE @RightValueTypeId INT
DECLARE @UnifiedS INT

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ManagePlatFormSecurity';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Unified Platform Security Settings' );

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

SELECT @SideMenuRight = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRight';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Unified Platform Security Settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRight );

END;

SELECT @SideMenuRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Unified Platform Security Settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRoute );

END;

SELECT @UnifiedS = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Unified Platform Security Settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedS
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedS );

END;
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
DECLARE @TRightShortName NVARCHAR(100)
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
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
VALUES( 1, 'Manage Custom User fields settings', 'Manage Custom User fields settings', 'ManageCustomFields' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

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
	WHERE ObjectValue = 'Manage Custom Fields' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Custom Fields', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Settings' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Custom Fields' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Custom Fields', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageCustomFields', @ShortName = 'ManageCustomFields', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Custom Fields'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManageCustomFields';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Custom User fields settings'   AND 
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
GO
DECLARE @Dashboard int;
DECLARE @SideMenuRight INT
DECLARE @SideMenuRoute INT
DECLARE @RightValueTypeId INT
DECLARE @UnifiedS INT

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ManageCustomFields';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

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


SELECT @SideMenuRight = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRight';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRight );

END;

SELECT @SideMenuRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRoute );

END;

SELECT @UnifiedS = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedS
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedS );

END;


SELECT @SideMenuRight = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRight';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRight );

END;

SELECT @SideMenuRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SideMenuRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SideMenuRoute );

END;

SELECT @UnifiedS = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Custom User fields settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedS
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedS );

END;
GO
DECLARE @StatusId INT;
SELECT @StatusId = StatusTypeId
FROM Enterprise.Statustype
WHERE Name = 'ALL';
IF EXISTS
(
    SELECT 1
    FROM Enterprise.RightValueType
    WHERE Value = 'View Unified Settings'
          AND visibilitystatusid != @StatusId
)
    BEGIN
        UPDATE enterprise.rightvaluetype
          SET
              visibilitystatusid = @StatusId
        WHERE Value = 'View Unified Settings';
    END;
GO

WITH cteUserAction
(
	ActionId,
	RightId,
	Status,
	PersonaId,
	ObjectType,
	ObjectValue,
	minUserActionId
)
AS
(
	SELECT	a1.ActionID,	ua.RightID,	Status, pp.PersonaId, ObjectType, ObjectValue, MIN(UserActionId)
	FROM	Enterprise.ACTION AS A1
			INNER JOIN Enterprise.UserActions AS UA ON A1.ActionID = UA.ActionID  
			INNER JOIN Enterprise.[Right] AS R ON R.RightID = UA.RightID   
			INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId  
			INNER JOIN Enterprise.Role AS RL ON RL.RoleID = R.RoleId  
			INNER JOIN Enterprise.RoleValueType AS RLVT ON RLVT.RoleValueTypeId = RL.RoleValueTypeId  
			INNER JOIN Enterprise.ActionValueType AS AVT ON A1.ActionvalueTypeId = AVT.ActionValueTypeID
			INNER JOIN Enterprise.PersonaPrivilege AS PP  ON PP.RoleId = RL.RoleId
	GROUP BY	a1.ActionID,	ua.RightID,	Status, pp.PersonaId, ObjectType, ObjectValue
	HAVING	COUNT(UserActionId) > 1
)

DELETE	eua
FROM	Enterprise.UserActions eua
		INNER JOIN cteUserAction cte ON (eua.ActionId = cte.ActionId AND eua.UserActionID > cte.minUserActionId AND eua.RightID = cte.RightID)
GO


IF NOT EXISTS(SELECT 1 FROM [Batch].[BatchProcessType] WHERE BatchProcessTypeId = 8 AND BatchProcessConfigurationId = 1 AND	Description = 'User Type changed from External To Admin' AND	Name = 'UserTypeExternalToAdmin')
BEGIN
	INSERT INTO  [Batch].[BatchProcessType]  (BatchProcessTypeId, BatchProcessConfigurationId, Description, Name)
	VALUES (8, 1, 'User Type changed from External To Admin', 'UserTypeExternalToAdmin')
END


IF NOT EXISTS(SELECT 1 FROM [Batch].[BatchProcessType] WHERE BatchProcessTypeId = 9 AND BatchProcessConfigurationId = 1 AND	Description = 'User Type changed from Admin To External' AND	Name = 'UserTypeAdminToExternal')
BEGIN
	INSERT INTO  [Batch].[BatchProcessType]  (BatchProcessTypeId, BatchProcessConfigurationId, Description, Name)
	VALUES (9, 1, 'User Type changed from Admin To External', 'UserTypeAdminToExternal')
END