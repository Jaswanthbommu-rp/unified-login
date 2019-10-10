IF OBJECT_ID('[Enterprise].[ListRelationshipType]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListRelationshipType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
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
GO
