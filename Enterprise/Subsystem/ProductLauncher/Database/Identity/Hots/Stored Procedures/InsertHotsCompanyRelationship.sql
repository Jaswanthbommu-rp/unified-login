CREATE PROCEDURE [Hots].[InsertHotsCompanyRelationship]
	@BaseLineCompany UNIQUEIDENTIFIER,
	@CloneCompany UNIQUEIDENTIFIER,
	@UserId INT = 1
AS
BEGIN
	DECLARE @BaseLineCompanyPartyId BIGINT, 
			@CloneCompanyPartyId BIGINT

	SELECT @BaseLineCompanyPartyId = PartyId From Enterprise.Party WHERE RealPageId = @BaseLineCompany
	SELECT @CloneCompanyPartyId = PartyId From Enterprise.Party WHERE RealPageId = @CloneCompany

	IF @BaseLineCompanyPartyId IS NULL OR @CloneCompanyPartyId IS NULL
	BEGIN
		SELECT 0 [Id], 'Failed to locate ' + CASE WHEN @BaseLineCompanyPartyId IS NULL THEN 'BaseLineCompany: '+CONVERT(VARCHAR(40),@BaseLineCompany) ELSE 'CloneCompany: '+CONVERT(VARCHAR(40),@CloneCompany) END [ErrorMessage]
		RETURN 0
	END

	IF @BaseLineCompany = @CloneCompany OR @BaseLineCompanyPartyId = @CloneCompanyPartyId
	BEGIN
		SELECT 0 AS Id, 'Company ids cannot be for the same company.' [ErrorMessage]
		RETURN 0
	END

	INSERT INTO Hots.CompanyRelationship ( BaseLineCompanyPartyId, CloneCompanyPartyId, CreateDate, CreatedBy )
	VALUES
		( @BaseLineCompanyPartyId, @CloneCompanyPartyId, GETUTCDATE(), @UserId )
	
	SELECT SCOPE_IDENTITY() [Id], '' [ErrorMessage]
END
