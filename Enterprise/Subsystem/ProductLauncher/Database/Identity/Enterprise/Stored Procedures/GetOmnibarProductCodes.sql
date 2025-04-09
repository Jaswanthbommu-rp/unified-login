CREATE PROCEDURE [Enterprise].[GetOmnibarProductCodes]
AS
BEGIN
	SELECT OPC.Id,
           OPC.OmnibarProductCode,
           EP.BooksProductCode
	FROM Enterprise.OmnibarProductCodes OPC
	JOIN Enterprise.Product EP ON EP.ProductId = OPC.ProductId
	WHERE OPC.IsActive = 1 AND EP.Active = 1
END