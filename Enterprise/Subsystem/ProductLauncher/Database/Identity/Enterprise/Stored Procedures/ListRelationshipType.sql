CREATE PROCEDURE [Enterprise].[ListRelationshipType]
	@RelationshipTypeName varchar(50) = NULL
AS
BEGIN
	SELECT	RelationshipTypeId,
			RoleTypeIdValidFrom,
			RoleTypeIdValidTo,
			Name,
			Description
	FROM	Enterprise.[RelationshipType]
	WHERE	(@RelationshipTypeName IS NULL OR Name = @RelationshipTypeName)
END