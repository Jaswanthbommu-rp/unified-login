--basic Information changes related to product names and role types
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductType WHERE ProductTypeId = 503)
BEGIN
    INSERT INTO Enterprise.ProductType(ProductTypeID, ParentProductTypeId, Name, Description, ProductTypeGUID)
    VALUES (503, 500, 'Landing', 'Landing', NEWID())
END

UPDATE Enterprise.Product 
    SET ProductTypeID = 503
    WHERE Name ='Unified Login'


IF EXISTS
(
	SELECT 1
	FROM Enterprise.Product
	WHERE Name = 'Landing'
)
BEGIN
	UPDATE enterprise.product
	  SET name = 'Unified Login'
	WHERE Name = 'Landing';
END;

IF EXISTS
(
	SELECT 1
	FROM enterprise.rolevaluetype
	WHERE Value = 'Super User'
)
BEGIN
	UPDATE Enterprise.RoleValueType
	  SET Value = 'User Administrator'
	WHERE value = ( 'Super User' );
END;

IF EXISTS
(
	SELECT 1
	FROM enterprise.rolevaluetype
	WHERE Value = 'User Role'
)
BEGIN
	UPDATE Enterprise.RoleValueType
	  SET Value = 'Basic End User'
	WHERE value = 'User Role';
END;


IF EXISTS
(
	SELECT 1
	FROM Enterprise.ProductType
	WHERE Name = 'Platform Services' AND 
		  Description = 'Platform Services'
)
BEGIN
	UPDATE enterprise.producttype
	  SET name = 'Administration', description = 'Administration'
	WHERE name = 'Platform Services' AND 
		  description = 'Platform Services';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.StatusType
	WHERE Name = 'ForceResetPassword'
)
BEGIN
	INSERT INTO enterprise.statustype( name )
	VALUES( 'ForceResetPassword' );
END;

EXEC sys.sp_updateextendedproperty
     @name = N'Build',
     @value = '25';