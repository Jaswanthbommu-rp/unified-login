SET IDENTITY_INSERT [Enterprise].[Comparator] ON 

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 1)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (1, N'Equal To', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 2)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (2, N'Not Equal To', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 3)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (3, N'Greater Than', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 4)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (4, N'Less Than', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 5)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (5, N'Greater Than or Equal To', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 6)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (6, N'Less Than or Equal To', CAST(N'2019-03-18T16:11:27.613' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[Comparator] WHERE ComparatorId = 13)
INSERT [Enterprise].[Comparator] ([ComparatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (13, N'Contains', CAST(N'2019-04-03T14:17:13.517' AS DateTime), N'00000000-0000-0000-0000-000000000000')

SET IDENTITY_INSERT [Enterprise].[Comparator] OFF

SET IDENTITY_INSERT [Enterprise].[SameSiteValue] ON 

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 1)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (1, N'CPU iPhone OS 12', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 2)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (2, N'iPad; CPU OS 12', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 3)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (3, N'Macintosh; Intel Mac OS X 10_14', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 4)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (4, N'Version/', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 5)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (5, N'Safari', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 6)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (6, N'Chrome/5', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 7)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (7, N'Chrome/6', 13, 2198, CAST(N'2020-02-07T14:16:01.913' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 8)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (8, N'Chrome/8', 13, 2198, CAST(N'2020-02-10T00:00:00.000' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[SameSiteValue] WHERE [SameSiteValueId] = 9)
INSERT [Enterprise].[SameSiteValue] ([SameSiteValueId], [SameSiteName], [ComparatorId], [CreatedBy], [CreatedDate]) VALUES (9, N'iPad; CPU OS 12', 13, 2198, CAST(N'2020-02-10T00:00:00.000' AS DateTime))

SET IDENTITY_INSERT [Enterprise].[SameSiteValue] OFF

SET IDENTITY_INSERT [Enterprise].[LogicalOperator] ON 

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[LogicalOperator] WHERE [LogicalOperatorId] = 1)
INSERT [Enterprise].[LogicalOperator] ([LogicalOperatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (1, N'And', CAST(N'2019-03-18T16:12:33.610' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[LogicalOperator] WHERE [LogicalOperatorId] = 2)
INSERT [Enterprise].[LogicalOperator] ([LogicalOperatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (2, N'Or', CAST(N'2019-03-18T16:12:33.610' AS DateTime), N'00000000-0000-0000-0000-000000000000')

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[LogicalOperator] WHERE [LogicalOperatorId] = 3)
INSERT [Enterprise].[LogicalOperator] ([LogicalOperatorId], [Name], [CreatedDate], [CreatedBy]) VALUES (3, N'Not', CAST(N'2019-03-18T16:12:33.610' AS DateTime), N'00000000-0000-0000-0000-000000000000')

SET IDENTITY_INSERT [Enterprise].[LogicalOperator] OFF

SET IDENTITY_INSERT [Enterprise].[LogicalGroup] ON 

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[LogicalGroup] WHERE [LogicalGroupId] = 2)
INSERT [Enterprise].[LogicalGroup] ([LogicalGroupId], [LogicalGrouper], [SameSiteIdLeft], [SameSiteIdRight], [LogicalOperatorId], [Sequence], [CreatedBy], [CreatedDate]) VALUES (2, 1, 3, 4, 1, 1, 2198, CAST(N'2020-02-07T14:13:27.657' AS DateTime))

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[LogicalGroup] WHERE [LogicalGroupId] = 3)
INSERT [Enterprise].[LogicalGroup] ([LogicalGroupId], [LogicalGrouper], [SameSiteIdLeft], [SameSiteIdRight], [LogicalOperatorId], [Sequence], [CreatedBy], [CreatedDate]) VALUES (3, 1, 4, 5, 1, 2, 2198, CAST(N'2020-02-07T14:13:27.657' AS DateTime))

SET IDENTITY_INSERT [Enterprise].[LogicalGroup] OFF

GO

DECLARE	@Now datetime = GETUTCDATE(),
	@ProductId int,
	@ProductConfigurationId int

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Unified Platform'

if not exists ( select top 1 1 from enterprise.ProductSettingType where name = 'IsSuppressSameSiteEnabled' )
begin
	insert into enterprise.ProductSettingType ( name, Description) values ( 'IsSuppressSameSiteEnabled', 'Used to suppress samesite cookie attribute' )
end

if not exists ( select top 1 1 from enterprise.productsetting where productid = 3 and ProductSettingTypeId = ( select ProductSettingTypeId from enterprise.ProductSettingType where name = 'IsSuppressSameSiteEnabled'))
begin
	insert into enterprise.productsetting ( productid, ProductSettingTypeId, value, FromDate ) 
		select @ProductId, ProductSettingTypeId, '1', GETUTCDATE() from enterprise.ProductSettingType where name = 'IsSuppressSameSiteEnabled'
end

SELECT DISTINCT TOP 1 @ProductConfigurationId = epc.ConfigurationId
FROM	Enterprise.GlobalProductConfiguration egpc  
			INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
			INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
			INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
WHERE		egpc.ProductId = @ProductId
AND			((@NOW BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@NOW >= egpc.FromDate AND egpc.ThruDate IS NULL))  
AND			((@NOW BETWEEN epc.FromDate AND epc.ThruDate) OR (@NOW >= epc.FromDate AND epc.ThruDate IS NULL))  
AND			((@NOW BETWEEN eps.FromDate AND eps.ThruDate) OR (@NOW >= eps.FromDate AND eps.ThruDate IS NULL))  
ORDER BY epc.ConfigurationId DESC

IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration egpc  
				INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
	WHERE	egpc.ProductId = @ProductId  
	AND			((@Now BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@Now >= egpc.FromDate AND egpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN epc.FromDate AND epc.ThruDate) OR (@Now >= epc.FromDate AND epc.ThruDate IS NULL))  
	AND			((@Now BETWEEN eps.FromDate AND eps.ThruDate) OR (@Now >= eps.FromDate AND eps.ThruDate IS NULL))  
	AND			epst.Name = 'IsSuppressSameSiteEnabled'
)
BEGIN
	IF (@ProductConfigurationId IS NOT NULL)
	BEGIN
		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		SELECT		@ProductConfigurationId,
						PS.ProductSettingId, 
						@Now,
						NULL
		FROM		Enterprise.ProductSetting PS INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId
			where ProductId = @ProductId AND PST.Name = 'IsSuppressSameSiteEnabled'
	END
END