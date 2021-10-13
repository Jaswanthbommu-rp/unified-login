--START UserStory 944608
IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'RealPage IA' AND ActiveDirectoryId = 'b894d20b-fc29-4aca-927f-485b585a4a36')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'RealPage IA', 'b894d20b-fc29-4aca-927f-485b585a4a36', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'Realpage Support Login' AND ActiveDirectoryId = 'a8cc84a7-37ad-4189-b24b-3f1f724e7c9c')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'Realpage Support Login', 'a8cc84a7-37ad-4189-b24b-3f1f724e7c9c', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Asset_Optimization_Product_Access' AND ActiveDirectoryId = 'c403db5e-39d6-4018-adf5-431c531b10ad')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Asset_Optimization_Product_Access', 'c403db5e-39d6-4018-adf5-431c531b10ad', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Lead2Lease_Product_Access' AND ActiveDirectoryId = '8a387488-da7e-48c0-aa85-9f531c6675b9')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Lead2Lease_Product_Access', '8a387488-da7e-48c0-aa85-9f531c6675b9', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Financial_Suite_Product_Access' AND ActiveDirectoryId = '7ebdc6e2-7950-40a2-bf9b-1017c392dd1f')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Financial_Suite_Product_Access', '7ebdc6e2-7950-40a2-bf9b-1017c392dd1f', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Marketing_Center_Product_Access' AND ActiveDirectoryId = '798b48e9-833e-4bc2-82ee-70e93a645d0b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Marketing_Center_Product_Access', '798b48e9-833e-4bc2-82ee-70e93a645d0b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Prospect_Contact_Center_Product_Access' AND ActiveDirectoryId = '1e9c8b87-3712-44f0-a16e-0474b8800155')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Prospect_Contact_Center_Product_Access', '1e9c8b87-3712-44f0-a16e-0474b8800155', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Spend_Management_Product_Access' AND ActiveDirectoryId = '92868a2c-543e-4bf3-9db3-23667943e3fc')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Spend_Management_Product_Access', '92868a2c-543e-4bf3-9db3-23667943e3fc', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Renters_Insurance_Product_Access' AND ActiveDirectoryId = '0df0dcda-6a56-481f-b2ca-22827b2cb4ad')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Renters_Insurance_Product_Access', '0df0dcda-6a56-481f-b2ca-22827b2cb4ad', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Vendor_Credentialing_Product_Access' AND ActiveDirectoryId = '1969f8a8-69bf-4404-96cf-096cbec5419a')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Vendor_Credentialing_Product_Access', '1969f8a8-69bf-4404-96cf-096cbec5419a', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Resident_Portals_Product_Access' AND ActiveDirectoryId = 'b031ff8a-c53e-4cb4-9676-f56a9ba0e983')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Resident_Portals_Product_Access', 'b031ff8a-c53e-4cb4-9676-f56a9ba0e983', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Utility_Management_Product_Access' AND ActiveDirectoryId = 'de375f2d-afe8-467d-a84f-3268a0c5f14b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Utility_Management_Product_Access', 'de375f2d-afe8-467d-a84f-3268a0c5f14b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Document_Director_Product_Access' AND ActiveDirectoryId = '53489af2-278e-46d8-bff3-7807eb588a0d')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Document_Director_Product_Access', '53489af2-278e-46d8-bff3-7807eb588a0d', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_On-Site_Product_Access' AND ActiveDirectoryId = '5161d0e3-e882-4e4f-a340-e5d4058c82c5')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_On-Site_Product_Access', '5161d0e3-e882-4e4f-a340-e5d4058c82c5', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Unified_Amenities_Product_Access' AND ActiveDirectoryId = '78625744-bff6-47ce-a638-20405414768b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Unified_Amenities_Product_Access', '78625744-bff6-47ce-a638-20405414768b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Vendor_Marketplace_Product_Access' AND ActiveDirectoryId = '18f04377-eb3d-48a5-9e87-d6a839cca4c3')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Vendor_Marketplace_Product_Access', '18f04377-eb3d-48a5-9e87-d6a839cca4c3', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Integration_Marketplace_Product_Access' AND ActiveDirectoryId = 'f558a93a-e41f-472a-9bd1-cc95b0d38eca')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Integration_Marketplace_Product_Access', 'f558a93a-e41f-472a-9bd1-cc95b0d38eca', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ILM_Lead_Management_Product_Access' AND ActiveDirectoryId = 'a7d09e5f-fec5-4081-afe3-958c10d139e1')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ILM_Lead_Management_Product_Access', 'a7d09e5f-fec5-4081-afe3-958c10d139e1', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ILM_Leasing_Analytics_Product_Access' AND ActiveDirectoryId = '00bf95f3-9838-4e0a-b361-0216bf39cdfd')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ILM_Leasing_Analytics_Product_Access', '00bf95f3-9838-4e0a-b361-0216bf39cdfd', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Portfolio_Management_Product_Access' AND ActiveDirectoryId = '032e95fd-167c-4d87-9e0e-614419e3d85e')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Portfolio_Management_Product_Access', '032e95fd-167c-4d87-9e0e-614419e3d85e', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_CIMPL_Product_Access' AND ActiveDirectoryId = '6fe70b5c-a034-4e25-8351-fff286160db6')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_CIMPL_Product_Access', '6fe70b5c-a034-4e25-8351-fff286160db6', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Deposit_Alternative_Product_Access' AND ActiveDirectoryId = '6a46ffbd-cd78-4cd9-9cec-30a11ce77fb7')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Deposit_Alternative_Product_Access', '6a46ffbd-cd78-4cd9-9cec-30a11ce77fb7', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ClickPay_Product_Access' AND ActiveDirectoryId = 'a00799ea-95dd-4fff-ac7c-10faf150f5ab')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ClickPay_Product_Access', 'a00799ea-95dd-4fff-ac7c-10faf150f5ab', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Waste_Product_Access' AND ActiveDirectoryId = '0d925c5f-2fab-48bf-87ce-5aeb5605a288')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Waste_Product_Access', '0d925c5f-2fab-48bf-87ce-5aeb5605a288', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Energy_Product_Access' AND ActiveDirectoryId = 'bf589209-434b-472e-8d17-266b14555da7')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Energy_Product_Access', 'bf589209-434b-472e-8d17-266b14555da7', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Water_Product_Access' AND ActiveDirectoryId = '75c6e3a1-783c-40de-88ad-c44aab35da76')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Water_Product_Access', '75c6e3a1-783c-40de-88ad-c44aab35da76', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Migo_-_Flexible_Living_Product_Access' AND ActiveDirectoryId = 'c2cc3340-aded-42b3-97a7-977a430b8b4d')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Migo_-_Flexible_Living_Product_Access', 'c2cc3340-aded-42b3-97a7-977a430b8b4d', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Hands-On_Training_System_Product_Access' AND ActiveDirectoryId = '439f63ac-f9a6-44cd-8459-6da72dbc52db')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Hands-On_Training_System_Product_Access', '439f63ac-f9a6-44cd-8459-6da72dbc52db', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Self-Guided_Tour_Product_Access' AND ActiveDirectoryId = '5353e4de-76a0-45c5-88a3-453a915f7847')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Self-Guided_Tour_Product_Access', '5353e4de-76a0-45c5-88a3-453a915f7847', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Waste_Commercial_Product_Access' AND ActiveDirectoryId = 'c344cd71-a470-451f-a099-e2598a2fed0d')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Waste_Commercial_Product_Access', 'c344cd71-a470-451f-a099-e2598a2fed0d', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access' AND ActiveDirectoryId = 'de94d94c-4bda-49c3-8a36-c2563b440b2d')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Non-Prod_All_Products_Access', 'de94d94c-4bda-49c3-8a36-c2563b440b2d', 'Q4 Script', GETUTCDATE()
END

DECLARE @ADGroupId INT
DECLARE @ProductId INT

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'RealPage IA'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'OneSite'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'Realpage Support Login'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'OneSite'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Asset_Optimization_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Asset Optimization'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Lead2Lease_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Lead2Lease'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Financial_Suite_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Financial Suite'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Marketing_Center_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Marketing Center'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Prospect_Contact_Center_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Prospect Contact Center'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Spend_Management_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Spend Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Renters_Insurance_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Renters Insurance'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Vendor_Credentialing_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Vendor Credentialing'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Resident_Portals_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Resident Portals'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Utility_Management_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Utility Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Document_Director_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Document Director'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_On-Site_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'On-Site'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Unified_Amenities_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Unified Amenities'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Vendor_Marketplace_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Vendor Marketplace'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Integration_Marketplace_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Integration Marketplace'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ILM_Lead_Management_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ILM Lead Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ILM_Leasing_Analytics_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ILM Leasing Analytics'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Portfolio_Management_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Portfolio Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_CIMPL_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'CIMPL'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Deposit_Alternative_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Deposit Alternative'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ClickPay_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ClickPay'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Waste_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Waste'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Energy_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Energy'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Water_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Water'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Migo_-_Flexible_Living_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Migo - Flexible Living'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Hands-On_Training_System_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Hands-On Training System'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Self-Guided_Tour_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Self-Guided Tour'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Waste_Commercial_Product_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Waste Commercial'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'OneSite'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Asset Optimization'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Lead2Lease'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Financial Suite'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Marketing Center'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Prospect Contact Center'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Spend Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Renters Insurance'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Vendor Credentialing'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Resident Portals'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Utility Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Document Director'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'On-Site'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Unified Amenities'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Vendor Marketplace'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Integration Marketplace'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ILM Lead Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ILM Leasing Analytics'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Portfolio Management'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'CIMPL'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Deposit Alternative'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'ClickPay'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Waste'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Energy'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Water'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Migo - Flexible Living'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Hands-On Training System'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Self-Guided Tour'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_Access'

SELECT @ProductId = ProductId
FROM [Enterprise].[Product]
WHERE [Name] = 'Smart Waste Commercial'

IF @ADGroupId IS NOT NULL AND @ProductId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupProduct] where ADGroupId = @ADGroupId AND ProductId = @ProductId)
BEGIN
INSERT INTO [Security].[ADGroupProduct](ADGroupId, ProductId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @ProductId, 'Q4 Script', GETUTCDATE()
END
GO
--END UserStory 944608
--Start UserStory 944833
IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Asset_Optimization_User_Management_Access' AND ActiveDirectoryId = 'b274fa8a-d362-45a7-a81e-fa6794a1df20')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Asset_Optimization_User_Management_Access', 'b274fa8a-d362-45a7-a81e-fa6794a1df20', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Lead2Lease_User_Management_Access' AND ActiveDirectoryId = '3eb7ae1a-e382-4689-8e1d-4e268d9b9d8e')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Lead2Lease_User_Management_Access', '3eb7ae1a-e382-4689-8e1d-4e268d9b9d8e', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Financial_Suite_User_Management_Access' AND ActiveDirectoryId = '04f20c63-9b54-4f2e-8414-8f2957b32412')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Financial_Suite_User_Management_Access', '04f20c63-9b54-4f2e-8414-8f2957b32412', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Marketing_Center_User_Management_Access' AND ActiveDirectoryId = '9e961d3b-c711-48cc-bb43-e6614f85de1b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Marketing_Center_User_Management_Access', '9e961d3b-c711-48cc-bb43-e6614f85de1b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Prospect_Contact_Center_User_Management_Access' AND ActiveDirectoryId = '75dbd894-38e4-43bf-8e63-0c7f033d7a29')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Prospect_Contact_Center_User_Management_Access', '75dbd894-38e4-43bf-8e63-0c7f033d7a29', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Spend_Management_User_Management_Access' AND ActiveDirectoryId = '83c35377-b925-4c62-99cb-d4e983b48d46')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Spend_Management_User_Management_Access', '83c35377-b925-4c62-99cb-d4e983b48d46', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Renters_Insurance_User_Management_Access' AND ActiveDirectoryId = '124d049a-5aef-4082-9468-66dcb8ef9ad3')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Renters_Insurance_User_Management_Access', '124d049a-5aef-4082-9468-66dcb8ef9ad3', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Vendor_Credentialing_User_Management_Access' AND ActiveDirectoryId = '196d5c96-87b7-4d88-a71b-161757338212')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Vendor_Credentialing_User_Management_Access', '196d5c96-87b7-4d88-a71b-161757338212', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Resident_Portals_User_Management_Access' AND ActiveDirectoryId = '3b3cee05-8875-4f9a-8c16-e35680ae37be')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Resident_Portals_User_Management_Access', '3b3cee05-8875-4f9a-8c16-e35680ae37be', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Utility_Management_User_Management_Access' AND ActiveDirectoryId = '0c161351-2189-41af-bf5f-3903a4235679')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Utility_Management_User_Management_Access', '0c161351-2189-41af-bf5f-3903a4235679', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Document_Director_User_Management_Access' AND ActiveDirectoryId = 'e258c7d2-cdbb-403c-bbb7-d1e882240a4b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Document_Director_User_Management_Access', 'e258c7d2-cdbb-403c-bbb7-d1e882240a4b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_On-Site_User_Management_Access' AND ActiveDirectoryId = '7b527306-7a62-460e-8925-a3bae2a3945c')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_On-Site_User_Management_Access', '7b527306-7a62-460e-8925-a3bae2a3945c', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Unified_Amenities_User_Management_Access' AND ActiveDirectoryId = 'bf12d414-65c9-4d50-b0ce-1b13254e5418')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Unified_Amenities_User_Management_Access', 'bf12d414-65c9-4d50-b0ce-1b13254e5418', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Vendor_Marketplace_User_Management_Access' AND ActiveDirectoryId = 'e815cf94-2520-4a4d-bd3d-6259306f976a')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Vendor_Marketplace_User_Management_Access', 'e815cf94-2520-4a4d-bd3d-6259306f976a', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Integration_Marketplace_User_Management_Access' AND ActiveDirectoryId = '09e31891-6d0c-48d0-92c5-7eb3653fde91')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Integration_Marketplace_User_Management_Access', '09e31891-6d0c-48d0-92c5-7eb3653fde91', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ILM_Lead_Management_User_Management_Access' AND ActiveDirectoryId = 'e1d00491-5a2b-47ec-9866-86b38c82e350')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ILM_Lead_Management_User_Management_Access', 'e1d00491-5a2b-47ec-9866-86b38c82e350', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ILM_Leasing_Analytics_User_Management_Access' AND ActiveDirectoryId = 'ceed6191-1304-4329-ab07-84cfe24980e0')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ILM_Leasing_Analytics_User_Management_Access', 'ceed6191-1304-4329-ab07-84cfe24980e0', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Portfolio_Management_User_Management_Access' AND ActiveDirectoryId = '57529544-ef7a-4ee0-8403-4dd7ce8911ef')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Portfolio_Management_User_Management_Access', '57529544-ef7a-4ee0-8403-4dd7ce8911ef', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_CIMPL_User_Management_Access' AND ActiveDirectoryId = '37e1abb7-ff16-401d-aa02-b43e55fa6f00')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_CIMPL_User_Management_Access', '37e1abb7-ff16-401d-aa02-b43e55fa6f00', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Deposit_Alternative_User_Management_Access' AND ActiveDirectoryId = '67da53b6-c769-4f67-b67a-832ebd663da1')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Deposit_Alternative_User_Management_Access', '67da53b6-c769-4f67-b67a-832ebd663da1', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_ClickPay_User_Management_Access' AND ActiveDirectoryId = '036d6062-20dc-40e4-8280-cc76abcdfbc4')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_ClickPay_User_Management_Access', '036d6062-20dc-40e4-8280-cc76abcdfbc4', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Waste_User_Management_Access' AND ActiveDirectoryId = '011be4d8-2050-4755-b3f6-f5cf7078d94f')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Waste_User_Management_Access', '011be4d8-2050-4755-b3f6-f5cf7078d94f', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Energy_User_Management_Access' AND ActiveDirectoryId = '4a44aae0-4d98-41c7-977e-7d475d2725fb')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Energy_User_Management_Access', '4a44aae0-4d98-41c7-977e-7d475d2725fb', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Water_User_Management_Access' AND ActiveDirectoryId = '54341644-d474-4797-b298-04366c1a8682')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Water_User_Management_Access', '54341644-d474-4797-b298-04366c1a8682', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Migo_-_Flexible_Living_User_Management_Access' AND ActiveDirectoryId = '2e58ab04-5277-45ec-9d00-8cf8dc43ecb7')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Migo_-_Flexible_Living_User_Management_Access', '2e58ab04-5277-45ec-9d00-8cf8dc43ecb7', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Hands-On_Training_System_User_Management_Access' AND ActiveDirectoryId = '7258d4cc-53a2-438f-95bf-878be493b35b')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Hands-On_Training_System_User_Management_Access', '7258d4cc-53a2-438f-95bf-878be493b35b', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Self-Guided_Tour_User_Management_Access' AND ActiveDirectoryId = '60d1c6c6-6414-4d45-97bb-983aa0920612')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Self-Guided_Tour_User_Management_Access', '60d1c6c6-6414-4d45-97bb-983aa0920612', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Smart_Waste_Commercial_User_Management_Access' AND ActiveDirectoryId = 'c321aa5d-5a9c-400b-a791-17c2a920f788')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Smart_Waste_Commercial_User_Management_Access', 'c321aa5d-5a9c-400b-a791-17c2a920f788', 'Q4 Script', GETUTCDATE()
END

IF NOT EXISTS (Select * from  [Security].[ADGroup] where DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access' AND ActiveDirectoryId = '00b3cba8-1ff5-46ba-97a9-42cb0fcdc140')
BEGIN
INSERT INTO [Security].[ADGroup](DisplayName, ActiveDirectoryId, CreatedBy, CreatedDate)
SELECT 'AGAa-UP_Non-Prod_All_Products_User_Management_Access', '00b3cba8-1ff5-46ba-97a9-42cb0fcdc140', 'Q4 Script', GETUTCDATE()
END

DECLARE @ADGroupId INT
DECLARE @RightId INT

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'RealPage IA'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOneSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'Realpage Support Login'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOneSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'RealPage IA'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOneSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Asset_Optimization_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageAssetOptimizationProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Lead2Lease_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageLead2LeaseProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Financial_Suite_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageAccountingProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Marketing_Center_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageMarketingCenterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Prospect_Contact_Center_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ProspectContactCenterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Spend_Management_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSpendManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Renters_Insurance_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageRentersInsuranceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Vendor_Credentialing_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageVendorComplianceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Resident_Portals_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AddEditResidentPortalUser'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Utility_Management_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageUtilityManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Document_Director_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageDocumentManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_On-Site_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOnSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Unified_Amenities_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageUnifiedAmenitiesProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Vendor_Marketplace_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AccessVendorMarketplace'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Integration_Marketplace_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AccessIntegrationMarketplace'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ILM_Lead_Management_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageILMLeadManagemementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ILM_Leasing_Analytics_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageILMLeasingAnalyticsProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Portfolio_Management_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManagePortfolioManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_CIMPL_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageCIMPLQuestions'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Deposit_Alternative_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageDepositAlternativeProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_ClickPay_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageClickPayProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Waste_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingTrashProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Energy_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingEnergyProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Water_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingWaterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Migo_-_Flexible_Living_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageHospitalityServiceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Hands-On_Training_System_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageHandsOnTrainingSystemProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Self-Guided_Tour_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSGTourProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Smart_Waste_Commercial_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSmartWasteCommercialProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOneSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageAssetOptimizationProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageLead2LeaseProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageAccountingProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageMarketingCenterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ProspectContactCenterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSpendManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageRentersInsuranceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageVendorComplianceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AddEditResidentPortalUser'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END
--
SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AddEditResidentPortalUser'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageUtilityManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageDocumentManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageOnSiteProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageUnifiedAmenitiesProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AccessVendorMarketplace'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AccessIntegrationMarketplace'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageILMLeadManagemementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageILMLeasingAnalyticsProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManagePortfolioManagementProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageCIMPLQuestions'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageDepositAlternativeProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageClickPayProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingTrashProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingEnergyProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageIntelligentBuildingWaterProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageHospitalityServiceProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageHandsOnTrainingSystemProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSGTourProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END

SELECT @ADGroupId = ADGroupId
FROM [Security].[ADGroup]
WHERE DisplayName = 'AGAa-UP_Non-Prod_All_Products_User_Management_Access'

SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'ManageSmartWasteCommercialProductAccess'

IF @ADGroupId IS NOT NULL AND @RightId IS NOT NULL AND NOT EXISTS (Select * from  [Security].[ADGroupRight] where ADGroupId = @ADGroupId AND RightId = @RightId)
BEGIN
INSERT INTO [Security].[ADGroupRight](ADGroupId, RightId, CreatedBy, CreatedDate)
SELECT @ADGroupId, @RightId, 'Q4 Script', GETUTCDATE()
END
GO
--End UserStory 944833
--Start UserStory 947855
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CheckADGroupProductAccess')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CheckADGroupProductAccess', 'Require AD Group check for RP Employees', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE()

if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 1
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'CheckADGroupProductAccess'
	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 1
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 1, productsettingtypeid, '1', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'CheckADGroupProductAccess'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END
GO
--End UserStory 947855
--Start UserStory 947864
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CheckADGroupUserMgmt')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CheckADGroupUserMgmt', 'Require AD Group managemet check for RP Employees', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE()

if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 1
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'CheckADGroupUserMgmt'
	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 1
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 1, productsettingtypeid, '1', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'CheckADGroupUserMgmt'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END
--End UserStory 947864


--Start Userstory 936667
---Script to add AzureTokenAddress configuration
DECLARE @AzureTokenAddress NVARCHAR(max) = 'https://login.microsoftonline.com/2c94bed6-d675-4d3d-a53b-7b461fd6acc2/oauth2/v2.0'

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'AzureTokenAddress')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AzureTokenAddress', 'The api endpoint for Azure Token Address APIs', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureTokenAddress' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @AzureTokenAddress, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'AzureTokenAddress'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureTokenAddress' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add AzureTokenAddress configuration
DECLARE @AzureUnifiedLoginUserClientSecret NVARCHAR(max) = '1JCckXFT1uR2BOr.sPMY37G91w_q5-4~D8'

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'AzureUnifiedLoginUserClientSecret')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AzureUnifiedLoginUserClientSecret', 'Azure UnifiedLogin User Client Secret', 1 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientSecret' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @AzureUnifiedLoginUserClientSecret, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'AzureUnifiedLoginUserClientSecret'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientSecret' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add AzureTokenAddress configuration
DECLARE @AzureUnifiedLoginUserClientId NVARCHAR(max) = '7930bfd6-d0b0-45dd-93fb-162eae96365f'

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'AzureUnifiedLoginUserClientId')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AzureUnifiedLoginUserClientId', 'Azure UnifiedLogin User Client Id', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientId' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @AzureUnifiedLoginUserClientId, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'AzureUnifiedLoginUserClientId'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientId' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add AzureTokenAddress configuration
DECLARE @AzureUnifiedLoginUserClientScopes NVARCHAR(max) = 'https://graph.microsoft.com/.default'

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'AzureUnifiedLoginUserClientScopes')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AzureUnifiedLoginUserClientScopes', 'Azure UnifiedLogin User Client Scopes', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientScopes' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @AzureUnifiedLoginUserClientScopes, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'AzureUnifiedLoginUserClientScopes'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUnifiedLoginUserClientScopes' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add AzureTokenAddress configuration
DECLARE @AzureUserGraphAPI NVARCHAR(max) = 'https://graph.microsoft.com'

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'AzureUserGraphAPI')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AzureUserGraphAPI', 'Azure User Graph API', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUserGraphAPI' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @AzureUserGraphAPI, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'AzureUserGraphAPI'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AzureUserGraphAPI' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add TimeIntervelToCallAzureADGroupAPI 
DECLARE @TimeIntervelToCallAzureADGroupAPI int = 120

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'TimeIntervelToCallAzureADGroupAPI')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'TimeIntervelToCallAzureADGroupAPI', 'Time Intervel To Call AzureADGroup API in Minutes', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'TimeIntervelToCallAzureADGroupAPI' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @TimeIntervelToCallAzureADGroupAPI, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'TimeIntervelToCallAzureADGroupAPI'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'TimeIntervelToCallAzureADGroupAPI' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add EnableAzureADGroupUserImport 
DECLARE @EnableAzureADGroupUserImport bit = 1

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'EnableAzureADGroupUserImport')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'EnableAzureADGroupUserImport', 'Enable Azure ADGroup User Import', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'EnableAzureADGroupUserImport' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @EnableAzureADGroupUserImport, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'EnableAzureADGroupUserImport'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'EnableAzureADGroupUserImport' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO
--End Userstory 936667
--Start Userstory 920585
IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'CheckADGroupProductAccessGroupNames')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'CheckADGroupProductAccessGroupNames', 'Check ADGroup Group Names to verify Employee Product Access', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'CheckADGroupProductAccessGroupNames' and ps.ProductId= 1)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 1, ProductSettingTypeId, 'Product_Access,RealPage IA,Realpage Support Login,Products_Access', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'CheckADGroupProductAccessGroupNames'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'CheckADGroupProductAccessGroupNames' and ps.ProductId= 1

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 1 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO
IF NOT EXISTS (SELECT TOP (1)  1 FROM Enterprise.ProductUserDependency WHERE ProductId = 75 and DependentProductId = 1)
BEGIN
	INSERT INTO Enterprise.ProductUserDependency(ProductId,DependentProductId)
	VALUES(75,1)
END
GO
GO

--START : script for userstory #944865
--LockOnProductAccessRight 
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'LockOnProductAccessRight' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'LockOnProductAccessRight', 'Warn On Product Error', 0)
end

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(1,	 'LockOnProductAccessRight', 'ManageOneSiteProductAccess' ),
	(4,	 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(6,	 'LockOnProductAccessRight', 'ManageLead2LeaseProductAccess' ),
	(8,	 'LockOnProductAccessRight', 'ManageAccountingProductAccess' ),
	(9,	 'LockOnProductAccessRight', 'ManageMarketingCenterProductAccess' ),
	(10, 'LockOnProductAccessRight', 'ProspectContactCenterProductAccess' ),
	(13, 'LockOnProductAccessRight', 'ManageSpendManagementProductAccess' ),
	(14, 'LockOnProductAccessRight', 'ManageClientPortalProductAccess' ),
	(15, 'LockOnProductAccessRight', 'ManageRentersInsuranceProductAccess' ),
	(16, 'LockOnProductAccessRight', 'ManageVendorComplianceProductAccess' ),
	(17, 'LockOnProductAccessRight', 'AddEditResidentPortalUser' ),
	(18, 'LockOnProductAccessRight', 'ManageUtilityManagementProductAccess' ),
	(20, 'LockOnProductAccessRight', 'ManageDocumentManagementProductAccess' ),
	(23, 'LockOnProductAccessRight', 'ManageOnSiteProductAccess' ),
	(26, 'LockOnProductAccessRight', 'ManageUnifiedAmenitiesProductAccess' ),
	(29, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(30, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(31, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(32, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(33, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(34, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(39, 'LockOnProductAccessRight', 'AccessIntegrationMarketplace' ),
	(40, 'LockOnProductAccessRight', 'ManageILMLeadManagemementProductAccess' ),
	(41, 'LockOnProductAccessRight', 'ManageILMLeasingAnalyticsProductAccess' ),
	(44, 'LockOnProductAccessRight', 'ManagePortfolioManagementProductAccess' ),
	(45, 'LockOnProductAccessRight', 'ManagePlatFormSecurity' ),
	(46, 'LockOnProductAccessRight', 'ManageCustomFields' ),
	(47, 'LockOnProductAccessRight', 'ManageDepositAlternativeProductAccess' ),
	(48, 'LockOnProductAccessRight', 'ManageClickPayProductAccess' ),
	(49, 'LockOnProductAccessRight', 'ManageUnifiedSettings' ),
	(50, 'LockOnProductAccessRight', 'ManageSeniorLeadManagement' ),
	(51, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(52, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(53, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(54, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(55, 'LockOnProductAccessRight', 'ManageRenovationManager' ),
	(57, 'LockOnProductAccessRight', 'ManageIntelligentBuildingTrashProductAccess' ),
	(58, 'LockOnProductAccessRight', 'ManageIntelligentBuildingEnergyProductAccess' ),
	(59, 'LockOnProductAccessRight', 'ManageIntelligentBuildingWaterProductAccess' ),
	(60, 'LockOnProductAccessRight', 'ManageHomeSharingProductAccess' ),
	(63, 'LockOnProductAccessRight', 'ManageHandsOnTrainingSystemProductAccess' ),
	(65, 'LockOnProductAccessRight', 'ManageSGTourProductAccess' ),
	(66, 'LockOnProductAccessRight', 'ManageAssetOptimizationProductAccess' ),
	(67, 'LockOnProductAccessRight', 'Reporting' ),
	(68, 'LockOnProductAccessRight', 'ManageLeaseLabsProductAccess' ),
	(69, 'LockOnProductAccessRight', 'ManageLeadScoringProductAccess' ),
	(70, 'LockOnProductAccessRight', 'ManageSmartWasteCommercialProductAccess' )
	
	
--select * from @productlist

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @CurrentProductId INT = 1

select @MAX_ID = max(entid) from @productlist

while @Current_ID <= @MAX_ID
begin
	declare @currentSettingType varchar(500)
	declare @currentsettingValue varchar(2000)

	select @CurrentProductId = productid , @currentSettingType = productsettingtype, @currentSettingValue = productsettingvalue
		from @productlist where entid = @Current_ID

	--print 'productid = ' + convert(varchar,@currentproductid)
	if exists ( select top 1 1 from enterprise.product WHERE ProductId = @CurrentProductId )
	begin
		if not exists (
		select top 1 1 
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = @CurrentProductId  
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
			AND pst.Name = @currentSettingType
		)
		begin
			declare @currentproductconfigurationid INT
			select distinct top 1 @currentproductconfigurationid = pc.configurationid
				FROM Enterprise.GlobalProductConfiguration gpc  
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
					WHERE  gpc.ProductId = @CurrentProductId
				AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
				AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
				AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
			order by pc.ConfigurationId desc

			if (@currentproductconfigurationid is not null)
			begin
				insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
					select @CurrentProductId, productsettingtypeid, @currentSettingValue, GETUTCDATE()
						from enterprise.ProductSettingType where name = @currentSettingType
				insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
					values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
			end
		end
	end	
	set @Current_ID = @Current_ID + 1
end
GO
--END : script for userstory #944865




Declare @RightId bigint,@RoleID bigint,@UserId bigint;

SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'

If Not Exists (Select Top 1 1 from Security.[Right] where RightName = 'ManageCommunityRewardsProductAccess')
Begin
insert into Security.[Right] values ('ManageCommunityRewardsProductAccess', 'Manage Community Rewards Product Access','Manage Community Rewards Product Access',13,9,3,77,6357,GETDATE())
END

Select @RightId = RightId from Security.[Right] where RightName = 'ManageCommunityRewardsProductAccess';
Select @RoleID = RoleID from Security.Role where RoleName = 'User Administrator';

If Not Exists (Select Top 1 1 from Security.RoleRight where RoleId =@RoleID and RightID = @RightId)
Begin
 Insert into Security.RoleRight values (@RoleID,@RightId,@UserId,GETDATE())
End

GO
--Start: Script for 965199
DECLARE @CreatedById bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint
SELECT @CreatedById = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%'

IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'EmployeeAccessToInternalRolesAndRightsSetup')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('EmployeeAccessToInternalRolesAndRightsSetup', 'Ability to view and edit internal Roles & Rights', 'Employee Access to Internal Roles & Rights Setup', 13, 10, 3, 3, @CreatedById, GETDATE())
END

--RoleRight
SELECT @RightId = RightId 
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccessToInternalRolesAndRightsSetup'

SELECT @RoleId = RoleId 
FROM [Security].[Role]
WHERE RoleName = 'User Administrator' AND ShortName = 'SuperUser' and OrgPartyID IS NULL

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId = @RightId)
BEGIN
	INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
	VALUES (@RoleId, @RightId, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @PartyId = O.PartyId
FROM [Enterprise].[Organization] O
    INNER JOIN [Enterprise].[Party] P ON P.PartyId = O.PartyId
WHERE p.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
BEGIN
	INSERT INTO [Security].[OrganizationOverRideRight]
           ([RightId]
           ,[OrgPartyId]
           ,[VisibilityStatusId]
           ,[CreatedBy]
           ,[CreatedDate]) 
           VALUES	(@RightId, @PartyId, 9, @CreatedById, @Now)
END

GO
--End: Script for 965199

-- 981690
DECLARE @rolesandrightssetuprightid INT = 0, @rolesrightsnavigationmenuid INT = 0, @sidemenurouteid INT = 0, @UserId BIGINT = 0, @configmenuid INT = 0;

SELECT @configmenuid = id FROM Enterprise.NavigationMenu WHERE Title = 'Configurations'
SELECT @rolesandrightssetuprightid = rightid FROM security.[RIGHT] WHERE RightName = 'EmployeeAccessToInternalRolesAndRightsSetup'
SELECT @sidemenurouteid = routeid FROM security.Route WHERE RouteValue = 'SideMenu'
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.NavigationMenu WHERE Title = 'Roles & Rights Setup' )
BEGIN
	INSERT INTO enterprise.NavigationMenu (Title,PageId,Icon,URL,OrderIndex,ParentId,Origin)
	VALUES ( 'Roles & Rights Setup','rolesandrightssetup', NULL, '/home/roles-rights-setup', 129, @configmenuid, N'unified-login' )
END

SELECT @rolesrightsnavigationmenuid = Id FROM Enterprise.NavigationMenu WHERE URL = '/home/roles-rights-setup'

IF @rolesandrightssetuprightid <> 0 AND @rolesrightsnavigationmenuid <> 0 AND NOT EXISTS (SELECT TOP 1 1 FROM enterprise.NavigationMenuRights WHERE NavigationMenuId = @rolesrightsnavigationmenuid AND RightId = @rolesandrightssetuprightid)
BEGIN
	INSERT INTO enterprise.NavigationMenuRights ( NavigationMenuId, RightId )
	VALUES (@rolesrightsnavigationmenuid, @rolesandrightssetuprightid)
END

IF @rolesandrightssetuprightid <> 0 AND @sidemenurouteid <> 0 AND NOT EXISTS (SELECT TOP 1 1 FROM Security.RightRoute WHERE RightId = @rolesandrightssetuprightid AND RouteId = @sidemenurouteid)
BEGIN
	INSERT INTO security.RightRoute ( RightId,RouteId,RightName,CreatedBy,CreatedDate )
	VALUES ( @rolesandrightssetuprightid, @sidemenurouteid, 'EmployeeAccessToInternalRolesAndRightsSetup', @UserId, GETDATE() )
END

GO
-- 981690
