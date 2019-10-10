CREATE PROCEDURE [CustomField].[GetFieldsValuesByUserLoginPersonaId] (
	@OrganizationPartyId bigint,
	@UserLoginPersonaId bigint,
	@Enabled bit = NULL
) 
 AS
 BEGIN
	SELECT	cff.[FieldId]
				,cff.[OrganizationId]
				,cff.[Enabled]
				,cff.[Name]
				,cff.[Description]
				,cff.[FieldTypeId]
				,cfft.Name AS 'FieldTypeName'
				,cff.[Required]
				,cff.[ReadOnly]
				,cff.[DefaultValue]
				,cff.[SyncField]
				,cff.[Sequence]
				,cff.[HelpText]
				,cff.MinCharLength
				,cff.MaxCharLength
				,cffv.FieldValueId
				,ISNULL(cffv.UserLoginPersonaId, @UserLoginPersonaId) AS 'UserLoginPersonaId'
				,cffv.Value
	FROM	[CustomField].[Field] cff
				INNER JOIN [CustomField].[FieldType] cfft ON (cff.FieldTypeId = cfft.FieldTypeId)
				LEFT OUTER JOIN [CustomField].[FieldValue] cffv ON (cff.FieldId = cffv.FieldId AND cffv.UserLoginPersonaId = @UserLoginPersonaId)
	WHERE	cff.OrganizationId = @OrganizationPartyId
	AND		((@Enabled IS NULL) OR (cff.Enabled = @Enabled))
	ORDER BY cff.[Sequence] ASC
END