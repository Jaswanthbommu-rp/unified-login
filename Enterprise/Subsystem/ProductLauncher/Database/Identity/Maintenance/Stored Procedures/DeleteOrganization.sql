CREATE OR alter PROCEDURE [Maintenance].[DeleteOrganization]
    @OrganizationPartyId BIGINT,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@OrganizationRemovalQueueId INT = 0
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;

			DECLARE @RetryCount TINYINT = 0,
				@TranCount INT = 0

			SET @TranCount = @@TRANCOUNT

			IF @OrganizationRemovalQueueId <> 0
			BEGIN
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

			DELETE	eg
			FROM	Enterprise.[Group] eg
						INNER JOIN @Organization o ON (o.PartyId = eg.OrganizationPartyId)

			DELETE	eop
			FROM	Enterprise.OrganizationProduct eop
						INNER JOIN @Organization o ON (o.PartyId = eop.PartyId)

			DELETE	epo
			FROM	Enterprise.PersonaOrganization epo
						INNER JOIN @Organization o ON (o.PartyId = epo.OrganizationId)

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

			DELETE PR 
					FROM Security.PersonaRole PR 
					INNER JOIN Security.[Role] R ON PR.RoleId = r.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

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

			DELETE RR
					FROM Security.RoleRight RR 
					INNER JOIN Security.[Role] R ON R.RoleId = RR.RoleId
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)

			DELETE ODR
					FROM Security.OrganizationDefaultRole ODR
					INNER JOIN @Organization o ON (o.PartyId = ODR.OrgPartyId)
					INNER JOIN Security.[Role] R ON R.RoleId = ODR.RoleId

			DELETE R 
					FROM Security.[Role] R 
					INNER JOIN @Organization o ON (o.PartyId = R.OrgPartyID)


			DELETE	ero
			FROM		Enterprise.Role ero
							INNER JOIN @Right ri ON (ero.PartyID = ri.PartyId AND ero.RoleID = ri.RoleID)
							INNER JOIN Enterprise.[Right] eri ON (eri.RoleID = ri.RoleID AND eri.PartyId = ri.PartyId)

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

			DELETE	epr
			FROM	Enterprise.PartyRelationship epr
						INNER JOIN Person.Person pp ON (pp.PartyId = epr.PartyIdFrom)
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)	
						
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

			INSERT INTO @Person (
				PartyID
			)
			SELECT	pp.PartyId
			FROM	Person.Person pp
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE pp
			FROM	Person.Person pp
						INNER JOIN Ident.UserLogin iul ON (iul.PersonPartyId = pp.PartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			--DELETE ue
			--FROM	Enterprise.UserEmployeeId ue
			--			INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginPersonaId = ue.UserLoginPersonaId)
			--			INNER JOIN Ident.UserLogin iul ON (iul.UserId = iulp.UserLoginId)
			--			INNER JOIN @Person p ON (p.PartyID = iul.PersonPartyId)
			--
			DELETE	iulp
			FROM		Ident.UserLoginPersona iulp
							INNER JOIN Ident.UserLogin iul ON (iul.UserId = iulp.UserLoginId)
							INNER JOIN @Person p ON (p.PartyID = iul.PersonPartyId)

			DELETE	iul
			FROM		Ident.UserLogin iul
							INNER JOIN @Person p ON (p.PartyID = iul.PersonPartyId)

			DELETE	ep
			FROM		Enterprise.Party ep
							INNER JOIN @Person p ON (p.PartyID = ep.PartyId)

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
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	iulp
			FROM	Ident.UserLoginPersona iulp
						INNER JOIN @Organization o ON (o.PartyId = iulp.OrganizationPartyId)

			DELETE	iul
			FROM	Ident.UserLogin iul
						INNER JOIN @UserLoginPersona ulp ON (ulp.UserLoginId = iul.UserId)
						INNER JOIN @Organization o ON (o.PartyId = ulp.OrganizationPartyId)
						INNER JOIN Ident.UserLoginPersona iulp ON (iulp.UserLoginId = ulp.UserLoginId AND iulp.OrganizationPartyId = ulp.OrganizationPartyId)

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

			DELETE	eri
			FROM	Enterprise.[Right] eri
						INNER JOIN Enterprise.Role ero ON (ero.RoleID = eri.RoleID)
						INNER JOIN Enterprise.Party ep ON (ep.PartyId = ero.PartyID)
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			DELETE	ero
			FROM	Enterprise.Role ero
						INNER JOIN Enterprise.Party ep ON (ep.PartyId = ero.PartyID)
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			DELETE	ep
			FROM	Enterprise.Party ep
						INNER JOIN @Organization o ON (o.PartyId = ep.PartyId)

			IF @@TRANCOUNT = 0
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
			SELECT @OrganizationPartyId AS ID
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

			SELECT  0 AS Id
					
		END CATCH
    END;
