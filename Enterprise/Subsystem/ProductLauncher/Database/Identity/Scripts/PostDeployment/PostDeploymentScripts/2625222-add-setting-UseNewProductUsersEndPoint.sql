--User Story 2625222: Define a new product setting that controls whether we GET user audit data from /productusers or existing process

DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'UseNewProductUsersEndPoint')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('UseNewProductUsersEndPoint', 'Products leveraging productusers endpoint to streamline user audit reporting.', 0);
END
GO