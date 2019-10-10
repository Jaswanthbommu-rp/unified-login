CREATE PROCEDURE [Enterprise].[GetOrganization_Ver03] (
	@RealPageId uniqueidentifier = NULL,
	@PartyId bigint = NULL,
	@BlueBookId bigint = NULL,
	@BlackBookId bigint = NULL
)
AS
BEGIN
	SELECT	O.PartyId,
				O.Name,
				P.RealPageId,
				COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,
				COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,
				o.OrganizationTypeId
	FROM	[Enterprise].Organization AS o
				INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId
				LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)
	WHERE	((@RealPageId IS NULL) OR (RealPageId = @RealPageId))
	AND		((@PartyId IS NULL) OR (o.PartyId = @PartyId))
	AND		((@BlueBookId IS NULL) OR (@BlueBookId = D.CompanyMasterId))
	AND		((@BlackBookId IS NULL)	OR (@BlackBookId = D.MasterId));
END;