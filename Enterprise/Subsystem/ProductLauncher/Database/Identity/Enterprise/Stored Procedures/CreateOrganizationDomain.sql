CREATE PROCEDURE [Enterprise].[CreateOrganizationDomain]
(
	@DomainName VARCHAR(20)
	,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON;

	IF @DomainName IS NULL OR LTRIM(RTRIM(@DomainName)) = ''
	BEGIN
		SELECT CAST(0 AS INT) AS Id, 'DomainName is required' AS ErrorMessage;
		RETURN;
	END

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
