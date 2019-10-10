DECLARE @ProductTypeGUID UNIQUEIDENTIFIER;
SELECT @ProductTypeGUID = NEWID();
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ProductType
    WHERE Name = 'OmniChannel'
)
    BEGIN
        EXEC Enterprise.CreateProductType
             @ProductTypeId = 306,
             @ParentProductTypeId = 300,
             @Name = 'OmniChannel',
             @Description = 'OmniChannel',
             @ProductTypeGUID = @ProductTypeGUID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'OmniChannel'
)
    BEGIN
        UPDATE Enterprise.Product
          SET
              ProductTypeId = 306
        WHERE Name = 'OmniChannel';
    END;

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='52'

