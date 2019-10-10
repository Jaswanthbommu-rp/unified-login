CREATE PROCEDURE [Enterprise].[ListContactMechanismUsageType] (
	@ContactMechanismUsageTypeName nvarchar(100) = NULL
)
AS
BEGIN
	SET @ContactMechanismUsageTypeName = NULLIF(@ContactMechanismUsageTypeName, '')

	SELECT	cmut.ContactMechanismUsageTypeId,
			cmut.ParentContactMechanismUsageTypeId,
			cmut.Name
	FROM	Enterprise.ContactMechanismUsageType cmut
			LEFT OUTER JOIN Enterprise.ContactMechanismUsageType pcmut ON (cmut.ParentContactMechanismUsageTypeID = pcmut.ContactMechanismUsageTypeID)
	WHERE	(@ContactMechanismUsageTypeName IS NULL OR pcmut.Name = @ContactMechanismUsageTypeName)
END
