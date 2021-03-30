Create PROCEDURE [CustomField].[GetFieldsByPartyId] (
	@PartyId bigint 
) 
AS 
BEGIN
	
		SELECT	cff.[FieldId]
				,cff.[OrganizationId]
				,cff.[Enabled]
				,cff.[Name]
				,cff.[Description]
				,cff.[FieldTypeId]
				,cfft.[Name]
				,cff.[Required]
				,cff.[ReadOnly]
				,cff.[DefaultValue]
				,cff.[SyncField]
				,cff.[Sequence]
				,cff.[HelpText]
				,cff.[MinCharLength]
				,cff.[MaxCharLength]
		FROM	[CustomField].[Field] cff
				INNER JOIN [CustomField].FieldType cfft ON cff.FieldTypeId = cfft.FieldTypeId				
		WHERE	cff.OrganizationId = @PartyId
	ORDER BY cff.[Sequence]
END