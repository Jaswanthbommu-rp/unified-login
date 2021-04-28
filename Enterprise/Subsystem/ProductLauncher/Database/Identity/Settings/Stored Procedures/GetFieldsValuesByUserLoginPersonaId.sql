CREATE PROCEDURE [Settings].[GetFieldsValuesByUserLoginPersonaId](
	@OrganizationPartyId bigint,
	@UserLoginPersonaId bigint,
	@Enabled bit = NULL
) 
 AS
 BEGIN
	DECLARE @CustomFields table(
			[FieldId] [bigint] ,
			[OrganizationId] [bigint],
			[Enabled] [bit] ,
			[Name] [nvarchar](100) ,
			[Description] [nvarchar](500) ,
			[FieldTypeId] [tinyint] ,
			[FieldTypeName] [nvarchar](100) ,
			[Required] [bit] ,
			[ReadOnly] [bit] ,
			[DefaultValue] [nvarchar](max) ,
			[SyncField] [nvarchar](150) ,
			[Sequence] [smallint]  ,
			[HelpText] [varchar](max) ,
			[MinCharLength] [int] ,
			[MaxCharLength] [int] ,
			UserLoginPersonaId bigint )

	INSERT INTO @CustomFields
	SELECT 	 FieldId	
		,OrganizationId		
		,Enabled	
		,Name
		,Description
		,FieldTypeId
		,FieldTypeName
		,Required
		,ReadOnly
		,DefaultValue
		,SyncField
		,Sequence
		,HelpText
		,MinCharLength
		,MaxCharLength
		,0
	From (
		Select sr.[SettingTableRowId] AS 'FieldId',
			   st.[PartyId] AS 'OrganizationId',
			   [TableColumnName],
			   [TableColumnValue]
		from [Settings].[SettingTableColumn] stc
		join [Settings].[SettingTableRow] sr on
			stc.[SettingTableRowId] = sr.[SettingTableRowId]
		join [Settings].[SettingTable] st on
			st.[SettingTableId] = sr.[SettingTableId]
		where st.[PartyId] = @OrganizationPartyId) As SourceTable
	PIVOT
	(
		MIN([TableColumnValue])
		FOR [TableColumnName] IN (Enabled
		,Name
		,Description
		,FieldTypeId
		,FieldTypeName
		,Required
		,ReadOnly
		,DefaultValue
		,SyncField
		,Sequence
		,HelpText
		,MinCharLength
		,MaxCharLength)) AS PivotOutput

		UPDATE cf SET cf.FieldTypeName = sp.MappingName
		FROM @CustomFields cf
		JOIN Settings.SettingPicklist sp ON
			cf.FieldTypeId = sp.MappingValue
		Where sp.CategoryName = 'CustomFields'

		SELECT	FieldId
					,OrganizationId
					,Enabled
					,Name
					,Description
					,FieldTypeId
					,FieldTypeName
					,Required
					,ReadOnly
					,DefaultValue
					,SyncField
					,Sequence
					,HelpText
					,MinCharLength
					,MaxCharLength
					,@UserLoginPersonaId AS 'UserLoginPersonaId'
					,srv.Value
					,srv.SettingTableRowValueId AS 'FieldValueId'
		FROM	@CustomFields cf
		left outer Join Settings.SettingTableRowValue srv on
			cf.FieldId = srv.SettingTableRowId 
			And srv.UserLoginPersonaId = @UserLoginPersonaId
		Where ((@Enabled IS NULL) OR (cf.Enabled = @Enabled))
		ORDER BY cf.[Sequence] ASC
 END