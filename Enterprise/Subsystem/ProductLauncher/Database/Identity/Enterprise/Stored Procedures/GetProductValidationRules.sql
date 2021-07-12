CREATE PROCEDURE Enterprise.GetProductValidationRules
AS
BEGIN
	SELECT 
		ruleType.ProductRuleTypeId
		,ruleType.ProductRuleType
		,ruleValidation.productId
		,p.Name as ProductName
		,ruleValidation.RuleValue
		,ruleValidation.ValidationMessage

FROM Enterprise.ProductRuleType ruleType 
	INNER JOIN Enterprise.ProductValidationRule ruleValidation on ruleType.ProductRuleTypeId = ruleValidation.ProductRuleTypeId
	INNER JOIN Enterprise.Product p on ruleValidation.productid = p.ProductId
END