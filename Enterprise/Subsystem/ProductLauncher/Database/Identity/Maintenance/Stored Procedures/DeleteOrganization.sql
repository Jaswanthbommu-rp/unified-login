CREATE PROCEDURE [Maintenance].[DeleteOrganization]
    @OrganizationPartyId BIGINT,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@OrganizationRemovalQueueId INT = 0,
	@LogExecutionTime INT = 0,
	@IPB_ReturnResultSet BIT = 1
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;

			DECLARE @RetryCount TINYINT = 0,
				@TranCount INT = 0,
				@IsOrganizationRemovalEnabled INT = 0

			SET @TranCount = @@TRANCOUNT

			IF @OrganizationRemovalQueueId <> 0
			BEGIN
				SELECT	@IsOrganizationRemovalEnabled = ISNULL(ps.Value,0)
					FROM	Enterprise.GlobalProductConfiguration gpc
							JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
							JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
							JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
					WHERE  gpc.ProductId = 3
					AND (gpc.ThruDate IS NULL)
					AND ( pc.ThruDate IS NULL)
					AND ( ps.ThruDate IS NULL)
					And PST.Name = 'IsOrganizationRemovalEnabled'

				IF @IsOrganizationRemovalEnabled <> 1
				BEGIN
					INSERT INTO Maintenance.OrganizationRemovalQueueError
					(
					    OrganizationRemovalQueueId,
					    ErrorMessage
					)
					VALUES
					(   @OrganizationRemovalQueueId,
					    'OrganizationRemoval not enabled for environment'
				    )
					SELECT @OrganizationRemovalQueueId, OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Complete'
					UPDATE Maintenance.OrganizationRemovalQueue 
					SET OrganizationRemovalRetryCount = @RetryCount, 
						OrganizationRemovalQueueStatusId = (SELECT TOP (1) OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Complete' ORDER BY OrganizationRemovalQueueStatusId ) 
					WHERE 
						OrganizationRemovalQueueId = @OrganizationRemovalQueueId
					IF @IPB_ReturnResultSet = 1
					BEGIN
						RETURN @IsOrganizationRemovalEnabled
					END
				END
				
				INSERT INTO Maintenance.OrganizationRemovalQueueHistory
				(
				    OrganizationRemovalQueueId,
				    OrganizationRemovalQueueStatusId
				)
				SELECT @OrganizationRemovalQueueId, OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Pending Database Removal'
				SELECT @RetryCount = OrganizationRemovalRetryCount FROM Maintenance.OrganizationRemovalQueue WHERE @OrganizationRemovalQueueId = @OrganizationRemovalQueueId
			END

			IF @TranCount = 0
				BEGIN TRANSACTION;
			ELSE
				SAVE TRANSACTION DeleteOrganization_Transation

			DECLARE	@Organization TABLE (
				PartyID bigint NOT NULL,
				Name nvarchar(150)
)
			DECLARE @Configuration TABLE (
				ConfigurationId int 
			)

			DECLARE @Right TABLE (
				RightID int,
				RoleID int,
				RightValueTypeId int,
				PartyId bigint
			)

			DECLARE @Role TABLE (
				RoleID int,
				RoleValueTypeId int,
				PartyId bigint
			)

			DECLARE @UserLoginPersona TABLE (
				UserLoginId bigint,
				OrganizationPartyId bigint
			)

			DECLARE @Person TABLE (
				PartyID bigint
			)

			INSERT INTO @Organization (
				PartyId,
				Name
			)
			SELECT	eo.PartyID,
							eo.Name
			FROM		Enterprise.Organization eo
						INNER JOIN Enterprise.Party P
							ON eo.PartyId = P.PartyId
							WHERE eo.PartyId = @OrganizationPartyId AND P.RealPageId = @OrganizationRealPageId

			DELETE	edim
			FROM	Enterprise.DataImportMapping edim
						INNER JOIN @Organization o ON (o.PartyId = edim.PartyId)

			DELETE	epp
			FROM	Enterprise.PersonaPrivilege epp
						INNER JOIN Enterprise.[Group] eg ON (eg.GroupId = epp.GroupId)
						INNER JOIN @Organization o ON (o.PartyId = eg.OrganizationPartyId)

			DELETE epp
			FROM	Enterprise.PersonaPrivilege epp
						INNER JOIN Enterprise.Role R ON (r.RoleID = epp.roleid)
						INNER JOIN @Organization o ON (o.PartyId = r.PartyID)
			DELETE	eg
			FROM	Enterprise.[Group] eg
						INNER JOIN @Organization o ON (o.PartyId = eg.OrganizationPartyId)

			DELETE	eop
			FROM	Enterprise.OrganizationProduct eop
						INNER JOIN @Organization o ON (o.PartyId = eop.PartyId)

			DELETE	epo
			FROM	Enterprise.PersonaOrganization epo
						INNER JOIN @Organization o ON (o.PartyId = epo.OrganizationId)

			DELETE	oau
			FROM	Enterprise.OrganizationAdminUser oau
						INNER JOIN @Organization o ON (o.PartyId = oau.OrganizationPartyId)

			DELETE	ecee
			FROM	Enterprise.CommunicationEventEmail ecee
						INNER JOIN Enterprise.CommunicationEvent ece ON (ece.CommunicationEventId = ecee.CommunicationEventId)
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdFrom)
						INNER JOIN @Organization o ON (o.PartyId = epcm.PartyId)

			DELETE	ecce
			FROM	Enterprise.CESCommunicationEvent ecce
						INNER JOIN Enterprise.CommunicationEvent ece ON (ece.CommunicationEventId = ecce.CommunicationEventId)
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdFrom)
						INNER JOIN @Organization o ON (o.PartyId = epcm.PartyId)

			DELETE	ece
			FROM	Enterprise.CommunicationEvent ece
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdFrom)
						INNER JOIN @Organization o ON (o.PartyId = epcm.PartyId)

			DELETE	epcm
			FROM	Enterprise.PartyContactMechanism epcm
						INNER JOIN @Organization o ON (o.PartyId = epcm.PartyId)

			DELETE	epr
			FROM	Enterprise.PartyRelationship epr
						INNER JOIN @Organization o ON (o.PartyId = epr.PartyIdTo)

			DELETE	epr
			FROM	Enterprise.ProductRelationship epr
						INNER JOIN @Organization o ON (o.PartyId = epr.PartyIdFrom)

			INSERT INTO @Right (
				RightID,
				RoleID,
				RightValueTypeId,
				PartyId
			)
			SELECT	eri.RightID,
							eri.RoleID,
							eri.RightValueTypeId,
							eri.PartyId
			FROM		Enterprise.[Right] eri
							INNER JOIN @Organization o ON (o.PartyId = eri.PartyId)

			DELETE	eri
			FROM		Enterprise.[Right] eri
							INNER JOIN @Organization o ON (o.PartyId = eri.PartyId)

			DELETE	erd
			FROM		Enterprise.RightDependency erd
							INNER JOIN @Right r ON (erd.RightValueTypeId = r.RightValueTypeId)

			DELETE	erivt
			FROM		Enterprise.RightValueType erivt
							INNER JOIN @Right ri ON (ri.RightValueTypeId = erivt.RightValueTypeId)
							INNER JOIN Enterprise.[Right] eri ON (eri.RightValueTypeId = erivt.RightValueTypeId AND eri.PartyId = ri.PartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': RightValueType' end

			DELETE PR 
					FROM Security.PersonaRole PR 
					INNER JOIN Security.[Role] R ON PR.RoleId = r.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': PersonaRole' end

			DELETE	epp
			FROM	Enterprise.PersonaPrivilege epp
						INNER JOIN @Right ri ON (epp.RoleID = ri.RoleID)

			INSERT INTO @Role (
				RoleID,
				RoleValueTypeId,
				PartyId
			)
			SELECT	ero.RoleID,
							ero.RoleValueTypeId,
							ero.PartyID
			FROM		Enterprise.Role ero
							INNER JOIN @Right ri ON (ero.PartyID = ri.PartyId AND ero.RoleID = ri.RoleID)
							INNER JOIN Enterprise.[Right] eri ON (eri.RoleID = ri.RoleID AND eri.PartyId = ri.PartyId)

			DELETE AGR
					FROM Security.ADGroupRole AGR 
					INNER JOIN Security.[Role] R ON R.RoleId = AGR.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

			DELETE RR
					FROM Security.RoleRight RR 
					INNER JOIN Security.[Role] R ON R.RoleId = RR.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

			DELETE ODR
					FROM Security.OrganizationDefaultRole ODR
					INNER JOIN @Organization o ON (o.PartyId = ODR.OrgPartyId)
					INNER JOIN Security.[Role] R ON R.RoleId = ODR.RoleId
			
			DELETE ROT
					FROM security.RoleOrganizationType ROT
					INNER JOIN Security.[Role] R ON R.RoleId = ROT.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyId)
					
			DELETE OOR
					FROM Security.OrganizationOverRideRole OOR
					INNER JOIN @Organization o ON (o.PartyId = OOR.OrgPartyId)

			DELETE R 
					FROM Security.[Role] R 
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

			DELETE	ero
			FROM		Enterprise.Role ero
							INNER JOIN @Right ri ON (ero.PartyID = ri.PartyId AND ero.RoleID = ri.RoleID)
							INNER JOIN Enterprise.[Right] eri ON (eri.RoleID = ri.RoleID AND eri.PartyId = ri.PartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Enterprise.Role' end

			DELETE	erovt
			FROM		Enterprise.RoleValueType erovt
							INNER JOIN @Role ero ON (ero.RoleValueTypeId = erovt.RoleValueTypeId)

			DELETE	iaa
			FROM	Ident.ActivityAttempts iaa
						INNER JOIN Ident.ActivityConfiguration iac ON (iac.ActivityConfigurationId = iaa.ActivityConfigurationId)
						INNER JOIN @Organization o ON (o.PartyId = iac.PartyId)

			DELETE	iat
			FROM	Ident.ActivityToken iat
						INNER JOIN Ident.ActivityConfiguration iac ON (iac.ActivityConfigurationId = iat.ActivityConfigurationId)
						INNER JOIN @Organization o ON (o.PartyId = iac.PartyId)

			DELETE	iph
			FROM	Ident.PasswordHistory iph
						INNER JOIN Ident.ActivityConfiguration iac ON (iac.ActivityConfigurationId = iph.ActivityConfigurationId)
						INNER JOIN @Organization o ON (o.PartyId = iac.PartyId)

			DELETE	iac
			FROM	Ident.ActivityConfiguration iac
						INNER JOIN @Organization o ON (o.PartyId = iac.PartyId)

			DELETE	ipp
			FROM	Ident.PasswordPolicy ipp
						INNER JOIN @Organization o ON (o.PartyId = ipp.PartyId)
			
			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': PasswordPolicy' end

			DELETE	epr
			FROM	Enterprise.PartyRelationship epr
						INNER JOIN Person.Person pp ON (pp.PartyId = epr.PartyIdFrom)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId and epr.PartyIdTo = o.PartyID)	
						
			DELETE	epr
			FROM	Enterprise.PartyRole epr
						INNER JOIN Person.Person pp ON (pp.PartyId = epr.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)	

			DELETE eea
			FROM	Enterprise.ElectronicAddress eea
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.ContactMechanismId = eea.ContactMechanismID)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
			
			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': ElectronicAddress' end

			DELETE esa
			FROM	Enterprise.StreetAddress esa
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.ContactMechanismId = esa.ContactMechanismID)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	ecee
			FROM	Enterprise.CommunicationEventEmail ecee
						INNER JOIN Enterprise.CommunicationEvent ece ON (ece.CommunicationEventID = ecee.CommunicationEventId)
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdTo)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	ecce
			FROM	Enterprise.CESCommunicationEvent ecce
						INNER JOIN Enterprise.CommunicationEvent ece ON (ece.CommunicationEventID = ecce.CommunicationEventId)
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdTo)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	ece
			FROM	Enterprise.CommunicationEvent ece
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.PartyContactMechanismId = ece.PartyContactMechanismIdTo)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)	

			DELETE ecm
			FROM	Enterprise.ContactMechanism ecm
						INNER JOIN Enterprise.PartyContactMechanism epcm ON (epcm.ContactMechanismId = ecm.ContactMechanismID)
						INNER JOIN Person.Person pp ON (pp.PartyId = epcm.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
			
			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': ContactMechanism' end
			
			DELETE pap
			FROM	Person.ActivePersona pap
						INNER JOIN Person.Person pp ON (pp.PartyId = pap.PartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)	

			DELETE	epb
			FROM	Enterprise.ProductBatch epb
						INNER JOIN Person.Person pp ON (pp.PartyId = epb.PersonPartyId)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
			
			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': ProductBatch' end

			INSERT INTO @Person (
				PartyID
			)
			SELECT	pp.PartyId
			FROM	Person.Person pp
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId) AND iulp.PrimaryOrganization = 1
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE pp
			FROM	Person.Person pp
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId) AND iulp.PrimaryOrganization = 1
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Person' end
			
			DELETE ue
			FROM	Enterprise.UserEmployeeId ue
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = ue.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
			
			DELETE strv
			FROM	Settings.SettingTableRowValue strv
						INNER JOIN Settings.SettingTableRow str1 ON str1.SettingTableRowId = strv.SettingTableRowId
						INNER JOIN Settings.SettingTable st ON st.SettingTableId = str1.SettingTableId
						INNER JOIN @Organization o ON (o.PartyId = st.PartyId)

			DELETE str1
			FROM	Settings.SettingTableRow str1
						INNER JOIN Settings.SettingTable st ON st.SettingTableId = str1.SettingTableId
						INNER JOIN @Organization o ON (o.PartyId = st.PartyId)

			DELETE str2
			FROM	Settings.SettingTable str2
					INNER JOIN @Organization o ON (o.PartyId = str2.PartyId)

			DELETE rtpm
			FROM Security.RoleTemplateAdditionalProductRoleMapping rtpm
					INNER JOIN Security.RoleTemplateProduct rtp ON rtpm.RoleTemplateProductId = rtp.RoleTemplateProductId
					INNER JOIN Security.RoleTemplate rt ON rt.RoleTemplateId = rtp.RoleTemplateId
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)

			DELETE rtpm
			FROM Security.RoleTemplateProductRoleMapping rtpm
					INNER JOIN Security.RoleTemplateProduct rtp ON rtpm.RoleTemplateProductId = rtp.RoleTemplateProductId
					INNER JOIN Security.RoleTemplate rt ON rt.RoleTemplateId = rtp.RoleTemplateId
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)

			DELETE rtpat
			FROM	Security.RoleTemplateProductAdditionalTab rtpat
					INNER JOIN Security.RoleTemplate rt ON rt.RoleTemplateId = rtpat.RoleTemplateId
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)

			DELETE rtp
			FROM	Security.RoleTemplateProduct rtp
					INNER JOIN Security.RoleTemplate rt ON rt.RoleTemplateId = rtp.RoleTemplateId
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)

			DELETE rtum
			FROM	Security.RoleTemplateUserMapping rtum
					INNER JOIN Security.RoleTemplate rt ON rt.RoleTemplateId = rtum.RoleTemplateId
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)

			DELETE rt
			FROM	Security.RoleTemplate rt
					INNER JOIN @Organization o ON (o.PartyId = rt.PartyID)
			
			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': RoleTemplate' end
			
			DELETE oor
			FROM	Security.OrganizationOverRideRight oor
					INNER JOIN @Organization o ON (o.PartyId = oor.OrgPartyId)

			DELETE crr
			FROM	Hots.CompanyRelationship crr
					INNER JOIN @Organization o ON (o.PartyId = crr.CloneCompanyPartyId)
			
			DELETE	pim
			FROM	Enterprise.PropertyInstanceMapping pim
						INNER JOIN Person.Persona pp ON (pp.PersonaId = pim.PersonaID)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = pp.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': PropertyInstanceMapping' end

			DECLARE @PropertyInstanceToDelete TABLE ( propertyinstanceid BIGINT NOT NULL )
			INSERT INTO @PropertyInstanceToDelete ( propertyinstanceid )
				SELECT PRR.ClonePropertyInstanceId FROM Hots.PropertyRelationship PRR 
					INNER JOIN @Organization o ON (o.PartyId = PRR.CloneCompanyPartyId) 
			
			DELETE	usppp
			FROM	Enterprise.UserSyncProductPrimaryPropertiesStaging usppp
						INNER JOIN Person.Persona pp ON pp.PersonaId = usppp.PersonaID
						INNER JOIN Ident.UserLoginPersona iulp ON iulp.UserLoginPersonaId = pp.UserLoginPersonaId
						INNER JOIN @Organization o ON o.PartyId = iulp.OrganizationPartyId

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': UserSyncProductPrimaryPropertiesStaging' end

			DELETE pim
			FROM	Enterprise.PropertyInstance pim
					INNER JOIN @PropertyInstanceToDelete PID ON PID.propertyinstanceid = pim.PropertyInstanceId

			DELETE prr
			FROM	Hots.PropertyRelationship prr
					INNER JOIN @Organization o ON (o.PartyId = prr.CloneCompanyPartyId)
			
			DELETE	ut
			FROM		Ident.UserTokens ut
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = ut.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
						
			DELETE	SMS
			FROM		Ident.SMSAuthenticationCode SMS
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = SMS.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			--if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Ident.UserLoginPersona' end

			DELETE	iulp
			FROM		Ident.UserLoginPersona iulp
							INNER JOIN Ident.UserLogin iul ON (iul.UserId = iulp.UserLoginId)
							INNER JOIN @Person p ON (p.PartyID = iul.PersonPartyId)
						where
							iulp.PrimaryOrganization = 1
			DELETE	iul
			FROM		Ident.UserLogin iul
							INNER JOIN @Person p ON (p.PartyID = iul.PersonPartyId)
							left outer join ident.UserLoginPersona ULP on iul.UserId = ulp.UserLoginId
						where
							ulp.UserLoginId is null
							and ulp.PrimaryOrganization = 1
			DELETE	ep
			FROM		Enterprise.Party ep
							INNER JOIN Ident.UserLogin iul ON iul.PersonPartyId = ep.PartyId
							INNER JOIN ident.UserLoginPersona ULP on iul.UserId = ulp.UserLoginId and ulp.PrimaryOrganization = 1
							INNER JOIN @Person p ON (p.PartyID = ep.PartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Enterprise.Party Person' end

			DELETE	epiul
			FROM	Enterprise.PersonaIdentityUserLogin epiul
						INNER JOIN Person.Persona pp ON (pp.PersonaId = epiul.PersonaID)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = pp.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			INSERT INTO @UserLoginPersona (
				UserLoginId,
				OrganizationPartyId
			)
			SELECT	iulp.UserLoginId,
						iulp.OrganizationPartyId
			FROM	Ident.UserLoginPersona iulp
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId) AND iulp.PrimaryOrganization = 1
			
			IF @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Before UserLoginPersona' END
			
			DELETE	iulp
			FROM	Ident.UserLoginPersona iulp
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)
		
			IF @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': UserLoginPersona' END
            
			DELETE	iul
			FROM	Ident.UserLogin iul
						INNER JOIN @UserLoginPersona ulp ON (ulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = ulp.OrganizationPartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = ulp.UserLoginId AND iulp.OrganizationPartyId = ulp.OrganizationPartyId AND iulp.PrimaryOrganization = 1)

			INSERT INTO @Configuration (
				ConfigurationId
			)
			SELECT	epc.ConfigurationId
			FROM	Enterprise.PersonaConfiguration epc
						INNER JOIN Person.Persona pp ON (pp.PersonaId = epc.PersonaId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = pp.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	ec
			FROM	Enterprise.Configuration ec
						INNER JOIN @Configuration c ON (c.ConfigurationId = ec.ConfigurationId)

			DELETE	epc
			FROM	Enterprise.PersonaConfiguration epc
						INNER JOIN Person.Persona pp ON (pp.PersonaId = epc.PersonaId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = pp.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	pp
			FROM	Person.Persona pp
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = pp.UserLoginPersonaId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Person.Persona' end

			DELETE	eri
			FROM	Enterprise.[Right] eri
						INNER JOIN Enterprise.Role ero ON (ero.RoleID = eri.RoleID)
						INNER JOIN Enterprise.Party ep ON (ep.PartyId = ero.PartyID)
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			DELETE	ero
			FROM	Enterprise.Role ero
						INNER JOIN Enterprise.Party ep ON (ep.PartyId = ero.PartyID)
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			if @LogExecutionTime = 1 begin print convert(varchar(max),dateadd(hh,-5,getutcdate()),121) + ': Enterprise.Role' end

			DELETE CR
			FROM Hots.CompanyRelationship CR
						INNER JOIN @Organization O ON CR.BaseLineCompanyPartyId = O.PartyID OR CR.CloneCompanyPartyId = O.PartyID
						
			DELETE	ep
			FROM	Enterprise.Party ep
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			IF @TranCount = 0
				COMMIT;

			IF @OrganizationRemovalQueueId <> 0
			BEGIN
				UPDATE Maintenance.OrganizationRemovalQueue SET OrganizationRemovalQueueStatusId = (SELECT TOP (1) OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Database Removed' ORDER BY OrganizationRemovalQueueStatusId ) WHERE OrganizationRemovalQueueId = @OrganizationRemovalQueueId
				INSERT INTO Maintenance.OrganizationRemovalQueueHistory
				(
				    OrganizationRemovalQueueId,
				    OrganizationRemovalQueueStatusId				    
				)
				SELECT @OrganizationRemovalQueueId, OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Database Removed'
			END
			IF @IPB_ReturnResultSet = 1
			BEGIN
				SELECT @OrganizationPartyId AS ID
			END
		END TRY
		BEGIN CATCH
			DECLARE @error int, @message varchar(4000), @xstate int;
			SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
						
			IF @xstate = 1 and @TranCount = 0
				ROLLBACK;
			IF @xstate = 1 and @TranCount > 0
				ROLLBACK TRANSACTION DeleteOrganization_Transation

			SET @RetryCount += 1

			INSERT INTO Maintenance.OrganizationRemovalQueueHistory
			(
				OrganizationRemovalQueueId,
				OrganizationRemovalQueueStatusId				    
			)
			SELECT @OrganizationRemovalQueueId, OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Database Removal Failed'

			UPDATE Maintenance.OrganizationRemovalQueue 
				SET OrganizationRemovalRetryCount = @RetryCount, 
					OrganizationRemovalQueueStatusId = (SELECT TOP (1) OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'Database Removal Failed' ORDER BY OrganizationRemovalQueueStatusId ) 
				WHERE 
					OrganizationRemovalQueueId = @OrganizationRemovalQueueId
				
			INSERT INTO Maintenance.OrganizationRemovalQueueError
			(
				OrganizationRemovalQueueId,
				ErrorMessage
			)
			VALUES ( @OrganizationRemovalQueueId, @message )
			RAISERROR ('Maintenance.DeleteOrganization: %d: %s', 16, 1, @error, @message) ;
			IF @IPB_ReturnResultSet = 1
			BEGIN
				SELECT  0 AS Id
			END
		END CATCH
    END;
GO