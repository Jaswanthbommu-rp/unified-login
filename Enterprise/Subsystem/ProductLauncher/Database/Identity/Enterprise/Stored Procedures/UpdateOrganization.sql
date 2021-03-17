CREATE PROCEDURE [Enterprise].[UpdateOrganization]
    @OrganizationId UNIQUEIDENTIFIER,
	@OrganizationName NVARCHAR(150),
	@OrganizationTypeId INT,
	@OrganizationDomainId INT,
	@OrganizationStatus TinyInt = 1
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;

			UPDATE o
			SET Name = @OrganizationName
				,OrganizationTypeId = CASE WHEN @OrganizationTypeId IS NULL THEN O.OrganizationTypeId
					ELSE @OrganizationTypeId
				END
				,OrganizationDomainId = CASE WHEN @OrganizationDomainId IS NULL THEN O.OrganizationDomainId
					ELSE @OrganizationDomainId
				END
				,IsActive = @OrganizationStatus
			FROM [Enterprise].Organization o
			JOIN [Enterprise].Party p ON p.PartyId = o.PartyId
			WHERE p.RealPageId = @OrganizationId

			IF EXISTS ( SELECT Top 1 1
				FROM [IDENT].PasswordPolicy pp 
				INNER JOIN [Enterprise].Party p ON pp.PartyId = P.PartyId
				INNER JOIN [Enterprise].Organization o ON p.PartyId = o.PartyId 
				INNER JOIN [Enterprise].OrganizationType ot ON o.OrganizationTypeId = ot.OrganizationTypeId
				WHERE
					p.RealPageId = @OrganizationId
					AND ot.Name = 'Vendor'
					AND PP.EnablePasswordExpiration = 1
			)
			BEGIN
				UPDATE [IDENT].PasswordPolicy 
					SET EnablePasswordExpiration = 0, PasswordExpirationPeriodInDays = 0 
				FROM [IDENT].PasswordPolicy pp 
					INNER JOIN [Enterprise].Party p ON pP.PartyId = P.PartyId
				WHERE 
					p.RealPageId = @OrganizationId
					AND PP.EnablePasswordExpiration = 1
			END
			SET NOCOUNT OFF;

			COMMIT;
			SELECT @OrganizationId AS RealPageId
		END TRY
		BEGIN CATCH
			ROLLBACK;

			DECLARE @ErrorLogID INT;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    [dbo].ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
    END;