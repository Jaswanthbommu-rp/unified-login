CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeName]
(
				 @IdentityProviderTypeName varchar(50)
)
AS
BEGIN
	DECLARE @ProviderGroupTable TABLE ( ProviderFilter VARCHAR(20) )

	INSERT INTO @ProviderGroupTable (ProviderFilter ) VALUES ( @IdentityProviderTypeName )

	IF @IdentityProviderTypeName IN ( 'SAML', 'OIDC' )
	BEGIN
		DELETE FROM @ProviderGroupTable
		INSERT INTO @ProviderGroupTable ( ProviderFilter ) VALUES ( @IdentityProviderTypeName+'%' )
	END

	IF @IdentityProviderTypeName IN ( 'ALL' )
	BEGIN
		DELETE FROM @ProviderGroupTable
		INSERT INTO @ProviderGroupTable ( ProviderFilter ) VALUES ( '%' )
	END

	DECLARE @filter VARCHAR(20)
	SELECT @filter = ProviderFilter FROM @ProviderGroupTable

	DECLARE @cols       NVARCHAR(MAX)
	DECLARE @selectCols NVARCHAR(MAX)

	SELECT
		@cols       = STRING_AGG(QUOTENAME(Name), N', ')
		                  WITHIN GROUP (ORDER BY Name),
		@selectCols = STRING_AGG(
		                  CASE Name
		                      WHEN 'ValidateIssuer'      THEN N'CONVERT(bit, pvt.[ValidateIssuer]) AS [ValidateIssuer]'
		                      WHEN 'ForceAuthentication' THEN N'CONVERT(bit, ISNULL(pvt.[ForceAuthentication], ''1'')) AS [ForceAuthentication]'
		                      ELSE N'pvt.' + QUOTENAME(Name)
		                  END, N', ')
		                  WITHIN GROUP (ORDER BY Name)
	FROM (
		SELECT DISTINCT ipst.Name
		FROM   Ident.IdentityProviderSettingType ipst
		       INNER JOIN Ident.IdentityProviderType ipt
		           ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
		WHERE  ipt.Name LIKE @filter
	) x

	DECLARE @sql NVARCHAR(MAX) = N'
		SELECT ' + @selectCols + N',
		       pvt.Description,
		       @IdentityProviderTypeName AS ProviderName
		FROM (
			SELECT ipt.ContactMechanismId, ipt.IdentityProviderTypeId, ipt.Name AS IdentityTypeName,
			       ipt.Description, ipst.Name AS SettingTypeName, ips.Value
			FROM   Ident.IdentityProviderType ipt
			       INNER JOIN [Ident].[IdentityProviderSettingType] ipst
			           ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			       INNER JOIN [Ident].[IdentityProviderSetting] ips
			           ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
			WHERE  ipt.Name LIKE @filter
		) p
		PIVOT(MAX(Value) FOR SettingTypeName IN (' + @cols + N')) AS pvt'

	EXEC sp_executesql @sql,
		N'@IdentityProviderTypeName varchar(50), @filter VARCHAR(20)',
		@IdentityProviderTypeName = @IdentityProviderTypeName,
		@filter                   = @filter
END;
