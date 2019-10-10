CREATE PROCEDURE [Enterprise].[GetOrganizationByBlueBookId_Ver01](@BlueBookId INT)
AS
     BEGIN
         DECLARE @RealPageId UNIQUEIDENTIFIER;
         SELECT @RealPageId = RealPageId
         FROM Enterprise.DataImportMapping D
              INNER JOIN ENterprise.DataImportApplication DI ON DI.DataImportApplicationId = D.DataImportApplicationId
              INNER JOIN Enterprise.Party P ON P.PartyId = D.PartyId
         WHERE SourceId = @BlueBookId
               AND DI.Name = 'BlueBook';
         IF LEN(@RealPageId) > 0
             BEGIN
                 EXEC Enterprise.GetOrganization_Ver01
                      @RealPageId;
             END;
     END;