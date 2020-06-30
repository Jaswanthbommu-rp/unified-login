CREATE PROCEDURE [Enterprise].[CreateOrganizationDomain]
(
	@DomainName VARCHAR(20)
	,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL
)
AS
BEGIN
	IF @FromDate IS NULL
		SET @FromDate = GETUTCDATE();

	IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.OrganizationDomain WHERE Name = @DomainName )
	BEGIN
		INSERT INTO Enterprise.OrganizationDomain ( Name, FromDate, ThruDate )
			VALUES
			( 
			  @DomainName,
			  @FromDate,
			  @ThruDate
			)
	END

	SELECT 
		OrganizationDomainId as Id
		,'' AS ErrorMessage
	FROM Enterprise.OrganizationDomain 
	WHERE
		Name = @DomainName
END

