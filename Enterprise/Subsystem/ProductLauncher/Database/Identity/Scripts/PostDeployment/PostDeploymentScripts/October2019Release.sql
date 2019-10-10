GO
IF NOT EXISTS(SELECT 1 FROM ENterprise.CommunicationEventAudienceType WHERE Description = 'Multi Company User')
BEGIN
	SET IDENTITY_INSERT Enterprise.CommunicationEventAudienceType ON
	INSERT INTO Enterprise.CommunicationEventAudienceType (CommunicationEventAudienceTypeId, Description)
		VALUES(4, 'Multi Company User')
	SET IDENTITY_INSERT Enterprise.CommunicationEventAudienceType OFF
END
GO

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.CommunicationEmailTemplate CET
	INNER JOin Enterprise.CommunicationEventAudienceType CAT ON CET.CommunicationEventAudienceTypeId = CAT.CommunicationEventAudienceTypeId
	WHERE 
		cat.Description = 'Multi Company User'
		AND 
		  CommunicationEventPurposeTypeId = 1
)
BEGIN
	INSERT INTO [Enterprise].[CommunicationEmailTemplate]( CommunicationEventAudienceTypeId, CommunicationEventPurposeTypeId, [Subject], [Body] )
	SELECT CommunicationEventAudienceTypeId, 1, 'RealPage Platform Access Change Notification', '<!DOCTYPE html>
<html dir="ltr" lang="en">
<body>
	<table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:16px;">
		<tbody>
			<tr>
				<td>
					<center>
						<table border="0" cellspacing="0" cellpadding="0" width="600"
							style="margin:0 auto; max-width:535px; width:inherit;">
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
																	<td style="padding:18px 0 0 0;"></td>
																</tr>
																<tr>
																	<td style="padding:0 10px" align="center">
																		<div style="display:none;color:#fff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">
																			<span>You have been granted additional access to {COMPANY NAME}.  To navigate to this new company, open your profile by clicking your name in the header to see the option to toggle between companies.</span>
																		</div>
																		<a href="https://www.realpage.com"
																			style="text-decoration:none;">
																			<img src="{IMAGES}/RealPage-Logo.png"
																				alt="RealPage" width="270" height="80"
																				style="margin: 0; border: 0; padding: 0; display: block;" />
																		</a>
																	</td>
																</tr>
															</tbody>
														</table>
													</td>
												</tr>
											</tbody>
										</table>
										<table border="0" cellspacing="0" cellpadding="0" width="100%">
											<tbody>
												<tr>
													<td width="100%"
														style="padding:24px 24px 32px 24px; border-style:none;">
														<table border="0" cellspacing="0" cellpadding="0" width="100%">
															<tbody>
																<tr>
																	<td width="100%" style="padding:18px 0 0 0">
																		<table border="0" cellspacing="0"
																			cellpadding="0" width="100%">
																			<tbody>
																				<tr>
																					<td
																						style="padding:0 10px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
																						<span>Hi {FIRST NAME},</span>
																					</td>
																				</tr>
																			</tbody>
																		</table>
																	</td>
																</tr>
																<tr>
																	<td width="100%" style="padding:18px 0 0 0">
																		<table border="0" cellspacing="0"
																			cellpadding="0" width="100%">
																			<tbody>
																				<tr>
																					<td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
																						<span>You have been granted additional access to {COMPANY NAME}.  To navigate to this new company, open your profile by clicking your name in the header to see the option to toggle between companies.</span>
																					</td>
																				</tr>
																			</tbody>
																		</table>
																	</td>
																</tr>
																<tr>
																	<td align="center" style="padding:18px 0 0 0">
																		<table border="0" cellpadding="0"
																			cellspacing="0" align="center">
																			<tbody>
																				<tr>
																					<td>
																						<table width="100%" border="0"
																							cellspacing="0"
																							cellpadding="0">
																							<tr>
																								<td
																									style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
																									<span>If you have trouble accessing your profile, please contact <a href="https://www.realpage.com/support/"
																											style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage
																											Support</a> for assistance.
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
																		<table border="0" cellspacing="0"
																			cellpadding="0" width="100%">
																			<tbody>
																				<tr>
																					<td
																						style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">
																						This email and any files
																						transmitted with it are
																						confidential and intended solely
																						for the use of the individual or
																						entity to whom they are
																						addressed. If you’ve received
																						this email in error, please
																						notify <a
																							href="https://www.realpage.com/support/"
																							style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage
																							Support</a> by forwarding
																						this email to <a
																							href="mailto:support@realpage.com?subject=support">support@realpage.com</a>.
																						This message contains
																						confidential information and is
																						intended only for the individual
																						named. </td>
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
												<td align="center" width="100%"
													style="border-top:1px solid #757575; padding:16px 0;font-size:11px;">
													<a href="https://www.realpage.com/privacy-policy"
														style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
														<span>Privacy Policy</span>
													</a>
													<span
														style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
													<a href="https://www.realpage.com/"
														style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
														<span>Contact Us</span>
													</a>
													<span
														style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
													<span
														style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy;
														2019 RealPage, Inc.</span>
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
FROM
	Enterprise.CommunicationEventAudienceType WHERE Description = 'Multi Company User'

END;
GO
IF OBJECT_ID('tempdb..#procresult') IS NOT NULL
BEGIN
	DROP TABLE #procresult;
END;
create table #procresult ( id int, realpageid uniqueidentifier, errormessage varchar(250) )

begin try

	begin tran

	declare @noreplayemail table ( seq int identity(1,1), partyid int )

	;with existingcompanieswithemails as (
		 select o.partyid
			FROM    
				Enterprise.Organization O
				JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyId = O.PartyId
				join Enterprise.ContactMechanismUsage cmu  on pcm.PartyContactMechanismId = cmu.PartyContactMechanismID  
				join enterprise.ContactMechanismUsageType cmut on cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID
				JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = pcm.ContactMechanismID
			where
				cmut.Name = 'Email'
				AND
				ea.ElectronicAddressString = 'no-reply@realpage.com'
	)
	insert into @noreplayemail (partyid)
	select o.partyid from enterprise.Organization O
		left join existingcompanieswithemails ex on o.PartyId = ex.PartyId
	where
		ex.PartyId is null

	declare @currentrow INT = 1, 
		@maxrows INT, 
		@currentpartyid INT, 
		@CurrentContactMechanismId INT = 0,
		@CurrentPartyContactId INT = 0,
		@CurrentRealPageID uniqueidentifier,
		@currentutcdate DateTime,
		@emailContactMechanismUsageTypeId INT


	SELECT    @emailContactMechanismUsageTypeId = cecmut.ContactMechanismUsageTypeID
    FROM        Enterprise.ContactMechanismUsageType pecmut
                    INNER JOIN Enterprise.ContactMechanismUsageType cecmut ON (pecmut.ContactMechanismUsageTypeID = cecmut.ParentContactMechanismUsageTypeID)
    WHERE    pecmut.Name = 'Email Notification'
    AND            cecmut.Name = 'Email'

	if @emailContactMechanismUsageTypeId is null 
	begin
		RAISERROR (N'Missing ContactMechanismUsageTypeId cont', 16,1)		
	end

	select @maxrows = max(seq) from @noreplayemail

	while @currentrow <= @maxrows
	begin
		select @currentpartyid = partyid from @noreplayemail where seq = @currentrow
		truncate table #procresult

		select @CurrentRealPageID = RealPageId, @CurrentContactMechanismId = 0, @CurrentPartyContactId = 0 from enterprise.party where partyid = @currentpartyid
		set @currentutcdate = GETUTCDATE()

		--print @CurrentRealPageID
		exec Person.CreateContactMechanism @ContactMechanismId = @CurrentContactMechanismId OUT
		
		--print @CurrentContactMechanismId
		insert into #procresult ( id, realpageid, errormessage )
		exec Person.LinkContactMechanismToParty @RealPageId = @CurrentRealPageId, @ContactMechanismId = @CurrentContactMechanismId, @FromDate = @currentutcdate, @ThruDate = '9999-12-31 23:59:59.997'
		
		select @CurrentPartyContactId = id from #procresult
		
		exec Person.LinkUsageTypeToPartyContactMechanism @PartyContactMechanismId = @CurrentPartyContactId, @ContactMechanismUsageTypeId = @emailContactMechanismUsageTypeId
		
		INSERT INTO enterprise.ElectronicAddress ( ContactMechanismID, ElectronicAddressString, ElectronicAddressType )
			VALUES ( @CurrentContactMechanismId, 'no-reply@realpage.com', 'Email' )

		set @currentrow = @currentrow + 1
		
	end
		PRINT 'Insert of no-reply email for compaines complete'
		commit;
end try
begin catch
	IF(@@TRANCOUNT > 0)
		ROLLBACK TRAN;
	THROW;
end catch
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
VALUES( 1, 'Access to Settings Admin', 'Access to Settings Admin', 'AccessSettingsAdmin' );

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
	WHERE ObjectValue = 'Access Settings Admin' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access Settings Admin', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access Settings Admin' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access Settings Admin', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings

FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p

	 ON P.partyid  = O.PartyId
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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessSettingsAdmin', @ShortName = 'AccessSettingsAdmin', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AccessSettingsAdmin';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin'  AND 
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
DECLARE @SideMenu int
DECLARE @RightValueTypeId INT


SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AccessSettingsAdmin';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Settings Admin' );

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRight';

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

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
VALUES( 1, 'Access to Settings Admin for OneSite', 'Access to Settings Admin for OneSite', 'AccessSettingsAdminOneSite' );

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
	WHERE ObjectValue = 'Access Settings Admin OneSite' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access Settings Admin OneSite', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access Settings Admin OneSite' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access Settings Admin OneSite', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings

FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p

	 ON P.partyid  = O.PartyId
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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessSettingsAdminOneSite', @ShortName = 'AccessSettingsAdminOneSite', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin OneSite'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AccessSettingsAdminOneSite';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin OneSite'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings Admin OneSite'  AND 
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
DECLARE @SideMenu int
DECLARE @RightValueTypeId INT


SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AccessSettingsAdminOneSite';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Settings Admin for OneSite' );

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRight';

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

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

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


