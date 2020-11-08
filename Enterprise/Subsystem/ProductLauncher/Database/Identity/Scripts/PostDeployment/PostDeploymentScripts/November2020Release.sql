
/*This script is to update Renovation Manager Description*/
GO
DECLARE @ProductSettingTypeId int,
		@ProductId int= 55;
		
IF EXISTS (SELECT 1 FROM Enterprise.Product  WHERE ProductId = @ProductId)
BEGIN
   SELECT @ProductSettingTypeId=ProductSettingTypeId 
   FROM Enterprise.ProductSettingType WHERE [Name] = 'LearnMore'

	UPDATE Enterprise.Product
	SET [Description]='Renovation Manager is a tool that assists users in reducing vacancy loss, ensuring the asset is executing the plan as expected and allows for comprehensive analysis of capital performance with executed rent, expense and ROI comparisons against budget, prior lease and un-renovated market rent.'
    WHERE ProductId=@ProductId

	--Removing LearnMore for Renovation Manger
	IF EXISTS (select 1 from Enterprise.ProductSetting where ProductId=@ProductId and ProductSettingTypeId=@ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		UPDATE Enterprise.ProductSetting 
		SET ThruDate='2020-10-27 20:23:29.250' 
		WHERE ProductId=@ProductId and ProductSettingTypeId=@ProductSettingTypeId
	END


END
GO

