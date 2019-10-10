CREATE PROCEDURE [Enterprise].[GetBookIdByOrganization] @RealPageId UNIQUEIDENTIFIER
AS
     BEGIN
         SELECT MasterId AS BlackBookId,
                CompanyMasterId  AS BlueBookId
         FROM Enterprise.VW_DataImportMapping DIM
              JOIN Enterprise.Party P ON P.PartyId = DIM.PartyId
         WHERE RealPageId = @RealPageId;
     END;