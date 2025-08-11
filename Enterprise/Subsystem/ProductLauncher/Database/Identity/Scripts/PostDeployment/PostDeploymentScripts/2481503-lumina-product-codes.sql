
--User Story 2481503: PME-442148- Lumina - Insert the new omnibar product code into the " enterprise.OmnibarProductCodes " database table across all environments

-- ONESITE Affordable
IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'AFF')
BEGIN
    INSERT INTO [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive])
    VALUES (1, N'AFF', N'ONESITE Affordable', 1)
END

-- ONESITE Classic
IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'CLOS')
BEGIN
    INSERT INTO [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive])
    VALUES (1, N'CLOS', N'ONESITE Classic', 1)
END

-- Vendor Credentialing (Vendors)
IF NOT EXISTS (SELECT 1 FROM [Enterprise].[OmnibarProductCodes] WHERE [OmnibarProductCode] = N'VCVENDOR')
BEGIN
    INSERT INTO [Enterprise].[OmnibarProductCodes] ([ProductId], [OmnibarProductCode], [Description], [IsActive])
    VALUES (16, N'VCVENDOR', N'Vendor Credentialing', 1)
END