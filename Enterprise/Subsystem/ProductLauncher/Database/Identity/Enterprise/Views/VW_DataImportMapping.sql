CREATE VIEW [Enterprise].[VW_DataImportMapping]
AS
SELECT PartyId, BlackBook AS 'MasterId', ISNULL(BlueBook, '') AS 'CompanyMasterId'
FROM
(
SELECT DIP.PartyId
	   ,DIA.Name
	   ,DIP.SourceId
 FROM Enterprise.DataImportApplication DIA
	INNER JOIN Enterprise.DataImportMapping DIP
		ON DIP.DataImportApplicationId = DIA.DataImportApplicationId
)	AS T PIVOT(MAX(T.SourceId) FOR T.Name IN ([BlackBook], [BlueBook])) AS P
GO


