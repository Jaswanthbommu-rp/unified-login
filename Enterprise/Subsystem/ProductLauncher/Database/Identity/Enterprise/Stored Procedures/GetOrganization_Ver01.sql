CREATE PROCEDURE [Enterprise].[GetOrganization_Ver01]
(@RealPageId  UNIQUEIDENTIFIER = NULL,
 @BlueBookId  INT              = NULL,
 @BlackBookId INT              = NULL
)
AS
     BEGIN
         SELECT O.PartyId,
                O.Name,
                P.RealPageId,
                --CreateDate,
                COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,
                COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId
         FROM [Enterprise].Organization AS o
              INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId
              LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)
         WHERE((@RealPageId IS NULL)
               OR (RealPageId = @RealPageId))
              AND ((@BlueBookId IS NULL)
                   OR (@BlueBookId = D.MasterId))
              AND ((@BlackBookId IS NULL)
                   OR (@BlackBookId = D.CompanyMasterId));
     END;

