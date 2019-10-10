
BEGIN TRY
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempPersona')
	BEGIN
		BEGIN TRANSACTION
			IF EXISTS (SELECT 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = 'Person' and CONSTRAINT_NAME = 'FK_Persona_UserLoginPersona')
			BEGIN
				ALTER TABLE Person.Persona DROP CONSTRAINT FK_Persona_UserLoginPersona
				ALTER TABLE Ident.UserLoginPersona DROP CONSTRAINT FK_UserLoginPersona_Organization
				ALTER TABLE CustomField.FieldValue DROP CONSTRAINT FK_FieldValue_UserLoginPersona

				ALTER TABLE Ident.UserLoginPersona SET (LOCK_ESCALATION = TABLE)
				ALTER TABLE Person.Persona SET (LOCK_ESCALATION = TABLE)
			END

			IF EXISTS (SELECT 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = 'Ident' and CONSTRAINT_NAME = 'FK_UserLoginPersona_Organization')
			BEGIN
				ALTER TABLE Ident.UserLoginPersona DROP CONSTRAINT FK_UserLoginPersona_Organization

				ALTER TABLE Ident.UserLoginPersona SET (LOCK_ESCALATION = TABLE)
				ALTER TABLE Enterprise.Organization SET (LOCK_ESCALATION = TABLE)
			END

			IF EXISTS (SELECT 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = 'CustomField' and CONSTRAINT_NAME = 'FK_FieldValue_UserLoginPersona')
			BEGIN
				ALTER TABLE CustomField.FieldValue DROP CONSTRAINT FK_FieldValue_UserLoginPersona

				ALTER TABLE Ident.UserLoginPersona SET (LOCK_ESCALATION = TABLE)
				ALTER TABLE CustomField.FieldValue SET (LOCK_ESCALATION = TABLE)
			END

			SET IDENTITY_INSERT [Ident].[UserLoginPersona] ON

			INSERT INTO [Ident].[UserLoginPersona]
				(UserLoginPersonaId
				,[UserLoginId]
				,[StatusTypeId]
				,[OrganizationPartyId]
				,[PrimaryOrganization]
				,[FromDate]
				,[ThruDate]
				,[StatusThruDate])
				SELECT
					UL.UserLoginPersonaId
				,UL.[UserLoginId]
				,UL.[StatusTypeId]
				,UL.[OrganizationPartyId]
				,UL.[PrimaryOrganization]
				,UL.[FromDate]
				,UL.[ThruDate]
				,UL.[StatusThruDate]
			FROM
				TempUserLoginPersona UL

			SET IDENTITY_INSERT  [Ident].[UserLoginPersona] OFF

			UPDATE Person.Persona SET UserLoginPersonaId = TP.UserLoginPersonaId
			FROM Person.Persona P
			INNER JOIN TempPersona TP ON P.PersonaId = TP.PersonaId
			

			UPDATE Ident.UserLogin SET PersonPartyID = TP.PersonPartyId
			FROM Ident.UserLogin UL
			INNER JOIN TempUserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
			INNER JOIN TempPersona TP ON ULP.UserLoginPersonaId = TP.UserLoginPersonaId

			UPDATE CustomField.FieldValue SET UserLoginPersonaID = ULP.UserLoginPersonaID
			FROM CustomField.FieldValue FV
			INNER JOIN TempFieldValue TFV ON TFV.FieldValueId = FV.FieldValueId
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = TFV.UserLoginId

		
			ALTER TABLE Ident.UserLoginPersona SET (LOCK_ESCALATION = TABLE)
			ALTER TABLE Person.Persona ADD CONSTRAINT
				FK_Persona_UserLoginPersona FOREIGN KEY	(UserLoginPersonaId) 
				REFERENCES Ident.UserLoginPersona (UserLoginPersonaId) ON DELETE CASCADE ON UPDATE CASCADE 

			ALTER TABLE Enterprise.Organization SET (LOCK_ESCALATION = TABLE)
			ALTER TABLE  Ident.UserLoginPersona ADD CONSTRAINT
				FK_UserLoginPersona_Organization FOREIGN KEY	(OrganizationPartyId) 
				REFERENCES Enterprise.Organization ([PartyId]) ON DELETE NO ACTION ON UPDATE NO ACTION 
			
			ALTER TABLE Ident.UserLoginPersona SET (LOCK_ESCALATION = TABLE)
			ALTER TABLE  CustomField.FieldValue ADD CONSTRAINT
				FK_FieldValue_UserLoginPersona FOREIGN KEY	(UserLoginPersonaID) 
				REFERENCES Ident.UserLoginPersona (UserLoginPersonaID) ON DELETE NO ACTION ON UPDATE NO ACTION; 

			DROP TABLE TempUserLoginPersona
			DROP TABLE TempPersona
			DROP TABLE TempFieldValue;
			
		COMMIT
	END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION
END CATCH;