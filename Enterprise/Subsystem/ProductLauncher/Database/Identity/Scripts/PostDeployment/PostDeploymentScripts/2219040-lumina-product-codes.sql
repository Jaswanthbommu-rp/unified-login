IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'ul')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (3, N'ul', N'Unified Login', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'mt')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (27, N'mt', N'Migration Tool', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'umves')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (18, N'umves', N'Utility Management', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'umrbms')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (18, N'umrbms', N'Utility Management', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'umrboc')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (18, N'umrboc', N'Utility Management', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'fac')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (75, N'fac', N'Facilities', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'aocore')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (4, N'aocore', N'Asset Optimization', 1)
END
--Knock products
IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'KNCKLMA')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (91, N'KNCKLMA', N'Knock CRM', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'KNCKANA')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (91, N'KNCKANA', N'Knock Analytics', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'KNCKADM')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (91, N'KNCKADM', N'Knock Admin', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'KNCKCLAW')
BEGIN
    INSERT [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive]) 
    VALUES (91, N'KNCKCLAW', N'Knock CLAW', 1)
END