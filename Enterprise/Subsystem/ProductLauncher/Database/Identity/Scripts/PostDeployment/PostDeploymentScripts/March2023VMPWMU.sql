DECLARE @productid bigint, 
		@UserId bigint, 
		@Now datetime = getUTCDate(),
		@StatusTypeId int, 
		@VisibilityStatusId int, 
		@TargetProductId int 

SET @productid = 38; --- VPM 
SET @StatusTypeId = 13
SET @VisibilityStatusId = 9
SET @TargetProductId = 38
SET @Now = getUTCDate()

SELECT    @UserId = UserId FROM    Ident.UserLogin WHERE    LoginName LIKE 'realpagead@%'

-----------------------------Start Roles ----------------------------------

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'CredentialingAdministrator' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Credentialing Administrator','CredentialingAdministrator','Credentialing Administrator',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'CredentialingReadOnly' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Credentialing Read Only','CredentialingReadOnly','Credentialing Read Only',3,NULL,@productid, @UserId, getUTCDate()
	END
IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantAdministrator' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Administrator','MerchantAdministrator','Merchant Administrator',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantCatalogManagement' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Catalog Management','MerchantCatalogManagement','Merchant Catalog Management',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantCompanyProfilePaymentAccount' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Company Profile/Payment Account','MerchantCompanyProfilePaymentAccount','Merchant Company Profile/Payment Account',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantCustomerAccountSetup' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Customer Account Setup','MerchantCustomerAccountSetup','Merchant Customer Account Setup',3,NULL,@productid, @UserId, getUTCDate()
	END
IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantOrderInvoiceManagement' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Order/Invoice Management','MerchantOrderInvoiceManagement','Merchant Order/Invoice Management',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'MerchantReadOnly' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'Merchant Read Only','MerchantReadOnly','Merchant Read Only',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'VMPAdministrator' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'VMP Administrator','VMPAdministrator','VMP Administrator',3,NULL,@productid, @UserId, getUTCDate()
	END
IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'VMPReadOnly' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'VMP Read Only','VMPReadOnly','VMP Read Only',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'VMPBidsContractsManagement' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'VMP Bids/Contracts Management','VMPBidsContractsManagement','VMP Bids/Contracts Management',3,NULL,@productid, @UserId, getUTCDate()
	END

IF NOT EXISTS(SELECT 1 FROM   security.role WHERE  ShortName = 'VMPProfileManagement' AND productid = @productid AND orgpartyid IS NULL)
	BEGIN
		INSERT INTO security.role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			SELECT 'VMP Profile Management','VMPProfileManagement','VMP Profile Management',3,NULL,@productid, @UserId, getUTCDate()
	END

------------------- End Roles ----------

------------------- Start Rights  ------


IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='AccesstoBids&ContractsinVendorMarketplace' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('AccesstoBids&ContractsinVendorMarketplace','Access to Bids & Contracts in Vendor Marketplace','Access to Bids & Contracts in Vendor Marketplace',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END
	
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='AccesstoVendorProfileinVendorMarketplace' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('AccesstoVendorProfileinVendorMarketplace','Access to Vendor Profile in Vendor Marketplace','Access to Vendor Profile in Vendor Marketplace',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='CreateEditorWithdrawBids' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('CreateEditorWithdrawBids','Create, Edit or Withdraw Bids','Create, Edit or Withdraw Bids',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='DisputeEditorExecuteContracts' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('DisputeEditorExecuteContracts','Dispute, Edit or Execute Contracts','Dispute, Edit or Execute Contracts',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='EditVendorProfile' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('EditVendorProfile','Edit Vendor Profile','Edit Vendor Profile',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='CredentialingAdministrator' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('CredentialingAdministrator','Credentialing Administrator','Credentialing Administrator',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='CredentialingReadOnly' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('CredentialingReadOnly','CredentialingReadOnly','Credentialing Read Only',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantAdministrator' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantAdministrator','MerchantAdministrator','Merchant Administrator',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantAccounts/CatalogManagement' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantAccounts/CatalogManagement','Merchant Accounts/Catalog Management','Merchant Accounts/Catalog Management',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantCatalogManagement' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantCatalogManagement','MerchantCatalogManagement','Merchant Catalog Management',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantCompanyProfile/PaymentAccount' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantCompanyProfile/PaymentAccount','MerchantCompanyProfilePayment Account','Merchant Company Profile/Payment Account',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantOrder/InvoiceManagement' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantOrder/InvoiceManagement','Merchant Order/Invoice Management','Merchant Order/Invoice Management',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantReadOnly' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantReadOnly','MerchantReadOnlyAccount','Merchant Read Only',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='MerchantCustomerAccountSetup' AND ProductId = @ProductId)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('MerchantCustomerAccountSetup','Merchant Customer Account Setup','Merchant Customer Account Setup',13,9,@ProductId ,@TargetProductId,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='ManageVendorMarketPlaceProductAccess' AND ProductId = 3)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
        VALUES('ManageVendorMarketPlaceProductAccess','Manage Vendor MarketPlace Product access','Manage Vendor MarketPlace Product access',13,9,3 ,@TargetProductId,@UserId,@Now);
END

--select * from Security.[Right] where ProductId=38
----------------------------  End RoleRights -----------------------------------

-- RoleRights

Declare @R1 varchar(100),@R2 varchar(100),@R3 varchar(100),@R4 varchar(100),@RoleId1 int;

Select @RoleId1 =RoleId from Security.Role where ShortName = 'CredentialingAdministrator' and ProductId = @productid;
Select @R1 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R2 = RightId from Security.[Right] where RightName ='CredentialingAdministrator' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId1 and RightId in (@R1,@R2))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId1,@R1,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId1,@R2,@UserId,getUTCDate());


END TRY
Begin Catch 
 
      Declare @ErrorMessage nvarchar(250);
      Declare @ErrorSeverity int;
      Declare @ErrorState int;
      Declare @ErrorLine int;
      Select @ErrorMessage = ERROR_Message()
	         ,@ErrorSeverity = ERROR_SEVERITY()
			 ,@ErrorState = ERROR_STATE()
			 ,@ErrorLine = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage,@ErrorSeverity,@ErrorState,@ErrorLine); 
 END Catch 

Declare @R21 varchar(100),@R22 varchar(100),@RoleId2 int;

Select @RoleId2 =RoleId from Security.Role where  ShortName = 'CredentialingReadOnly' and ProductId = @productid;
Select @R21 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R22 = RightId from Security.[Right] where RightName ='CredentialingReadOnly' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId2 and RightId in (@R21,@R22))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId2,@R21,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId2,@R22,@UserId,getUTCDate());


END TRY
Begin Catch 
 
      Declare @ErrorMessage1 nvarchar(250);
      Declare @ErrorSeverity1 int;
      Declare @ErrorState1 int;
      Declare @ErrorLine1 int;
      Select @ErrorMessage1 = ERROR_Message()
	         ,@ErrorSeverity1 = ERROR_SEVERITY()
			 ,@ErrorState1 = ERROR_STATE()
			 ,@ErrorLine1 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage1,@ErrorSeverity1,@ErrorState1,@ErrorLine1); 
 END Catch 


Declare @R31 varchar(100),@R32 varchar(100),@RoleId3 int;

Select @RoleId3 =RoleId from Security.Role where ShortName = 'MerchantAdministrator' and ProductId = @productid;
Select @R31 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R32 = RightId from Security.[Right] where RightName ='MerchantAdministrator' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId3 and RightId in (@R31,@R32))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId3,@R31,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId3,@R32,@UserId,getUTCDate());


END TRY
Begin Catch 
 
      Declare @ErrorMessage31 nvarchar(250);
      Declare @ErrorSeverity31 int;
      Declare @ErrorState31 int;
      Declare @ErrorLine31 int;
      Select @ErrorMessage31 = ERROR_Message()
	         ,@ErrorSeverity31 = ERROR_SEVERITY()
			 ,@ErrorState31 = ERROR_STATE()
			 ,@ErrorLine31 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage31,@ErrorSeverity31,@ErrorState31,@ErrorLine31); 
 END Catch

Declare @R41 varchar(100),@R42 varchar(100),@RoleId4 int;

Select @RoleId4 =RoleId from Security.Role where ShortName ='MerchantCatalogManagement' and ProductId = @productid;
Select @R41 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R42 = RightId from Security.[Right] where RightName ='MerchantCatalogManagement' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId4 and RightId in (@R41,@R42))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId4,@R41,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId4,@R42,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage41 nvarchar(250);
      Declare @ErrorSeverity41 int;
      Declare @ErrorState41 int;
      Declare @ErrorLine41 int;
      Select @ErrorMessage41 = ERROR_Message()
	         ,@ErrorSeverity41 = ERROR_SEVERITY()
			 ,@ErrorState41 = ERROR_STATE()
			 ,@ErrorLine41 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage41,@ErrorSeverity41,@ErrorState41,@ErrorLine41); 
 END Catch

 Declare @R51 varchar(100),@R52 varchar(100),@RoleId5 int;

Select @RoleId5 =RoleId from Security.Role where ShortName ='MerchantCompanyProfilePaymentAccount' and ProductId = @productid;
Select @R51 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R52 = RightId from Security.[Right] where RightName ='MerchantCompanyProfile/PaymentAccount' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId5 and RightId in (@R51,@R52))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId5,@R51,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId5,@R52,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage51 nvarchar(250);
      Declare @ErrorSeverity51 int;
      Declare @ErrorState51 int;
      Declare @ErrorLine51 int;
      Select @ErrorMessage51 = ERROR_Message()
	         ,@ErrorSeverity51 = ERROR_SEVERITY()
			 ,@ErrorState51 = ERROR_STATE()
			 ,@ErrorLine51 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage51,@ErrorSeverity51,@ErrorState51,@ErrorLine51); 
 END Catch

 Declare @R61 varchar(100),@R62 varchar(100),@RoleId6 int;

Select @RoleId6 =RoleId from Security.Role where ShortName ='MerchantCustomerAccountSetup' and ProductId = @productid;
Select @R61 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R62 = RightId from Security.[Right] where RightName ='MerchantCustomerAccountSetup' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId6 and RightId in (@R61,@R62))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId6,@R61,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId6,@R62,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage61 nvarchar(250);
      Declare @ErrorSeverity61 int;
      Declare @ErrorState61 int;
      Declare @ErrorLine61 int;
      Select @ErrorMessage61 = ERROR_Message()
	         ,@ErrorSeverity61 = ERROR_SEVERITY()
			 ,@ErrorState61 = ERROR_STATE()
			 ,@ErrorLine61 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage61,@ErrorSeverity61,@ErrorState61,@ErrorLine61); 
 END Catch


Declare @R71 varchar(100),@R72 varchar(100),@RoleId7 int;

Select @RoleId7 =RoleId from Security.Role where ShortName ='MerchantOrderInvoiceManagement' and ProductId = @productid;
Select @R71 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R72 = RightId from Security.[Right] where RightName ='MerchantOrder/InvoiceManagement' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId7 and RightId in (@R71,@R72))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId7,@R71,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId7,@R72,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage71 nvarchar(250);
      Declare @ErrorSeverity71 int;
      Declare @ErrorState71 int;
      Declare @ErrorLine71 int;
      Select @ErrorMessage71 = ERROR_Message()
	         ,@ErrorSeverity71 = ERROR_SEVERITY()
			 ,@ErrorState71 = ERROR_STATE()
			 ,@ErrorLine71 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage71,@ErrorSeverity71,@ErrorState71,@ErrorLine71); 
 END Catch


 Declare @R81 varchar(100),@R82 varchar(100),@RoleId8 int;

Select @RoleId8 =RoleId from Security.Role where ShortName ='MerchantReadOnly' and ProductId = @productid;
Select @R81 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R82 = RightId from Security.[Right] where RightName ='MerchantReadOnly' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId8 and RightId in (@R81,@R82))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId8,@R81,@UserId,getUTCDate());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId8,@R82,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage81 nvarchar(250);
      Declare @ErrorSeverity81 int;
      Declare @ErrorState81 int;
      Declare @ErrorLine81 int;
      Select @ErrorMessage81 = ERROR_Message()
	         ,@ErrorSeverity81 = ERROR_SEVERITY()
			 ,@ErrorState81 = ERROR_STATE()
			 ,@ErrorLine81 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage81,@ErrorSeverity81,@ErrorState81,@ErrorLine81); 
 END Catch

  Declare @R91 varchar(100),@R92 varchar(100),@R93 varchar(100),@R94 varchar(100),@R95 varchar(100),@RoleId9 int;

Select @RoleId9 =RoleId from Security.Role where ShortName ='VMPAdministrator' and ProductId = @productid;
Select @R91 = RightId from Security.[Right] where RightName ='AccesstoBids&ContractsinVendorMarketplace' and ProductId = @productid;
Select @R92 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R93 = RightId from Security.[Right] where RightName ='CreateEditorWithdrawBids' and ProductId = @productid;
Select @R94 = RightId from Security.[Right] where RightName ='DisputeEditorExecuteContracts' and ProductId = @productid;
Select @R95 = RightId from Security.[Right] where RightName ='EditVendorProfile' and ProductId = @productid;


IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId9 and RightId in (@R91,@R92,@R93,@R94,@R95))
BEGIN TRY

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId9,@R91,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId9,@R92,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId9,@R93,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId9,@R94,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId9,@R95,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage91 nvarchar(250);
      Declare @ErrorSeverity91 int;
      Declare @ErrorState91 int;
      Declare @ErrorLine91 int;
      Select @ErrorMessage91 = ERROR_Message()
	         ,@ErrorSeverity91 = ERROR_SEVERITY()
			 ,@ErrorState91 = ERROR_STATE()
			 ,@ErrorLine91 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage91,@ErrorSeverity91,@ErrorState91,@ErrorLine91); 
 END Catch


Declare @R101 varchar(100),@R102 varchar(100),@RoleId10 int;

Select @RoleId10 =RoleId from Security.Role where ShortName ='VMPReadOnly' and ProductId = @productid;
Select @R101 = RightId from Security.[Right] where RightName ='AccesstoBids&ContractsinVendorMarketplace' and ProductId = @productid;
Select @R102 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;


IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId10 and RightId in (@R101,@R102))
BEGIN TRY

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId10,@R101,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId10,@R102,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage101 nvarchar(250);
      Declare @ErrorSeverity101 int;
      Declare @ErrorState101 int;
      Declare @ErrorLine101 int;
      Select @ErrorMessage101 = ERROR_Message()
	         ,@ErrorSeverity101 = ERROR_SEVERITY()
			 ,@ErrorState101 = ERROR_STATE()
			 ,@ErrorLine101 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage101,@ErrorSeverity101,@ErrorState101,@ErrorLine101); 
 END Catch

 Declare @R111 varchar(100),@R112 varchar(100),@R113 varchar(100),@R114 varchar(100),@RoleId11 int;

Select @RoleId11 =RoleId from Security.Role where ShortName ='VMPBidsContractsManagement' and ProductId = @productid;
Select @R111 = RightId from Security.[Right] where RightName ='AccesstoBids&ContractsinVendorMarketplace' and ProductId = @productid;
Select @R112 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R113 = RightId from Security.[Right] where RightName ='CreateEditorWithdrawBids' and ProductId = @productid;
Select @R114 = RightId from Security.[Right] where RightName ='DisputeEditorExecuteContracts' and ProductId = @productid;


IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId11 and RightId in (@R111,@R112,@R113,@R114))
BEGIN TRY

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId11,@R111,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId11,@R112,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId11,@R113,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId11,@R114,@UserId,getUTCDate());

END TRY
Begin Catch 
 
      Declare @ErrorMessage111 nvarchar(250);
      Declare @ErrorSeverity111 int;
      Declare @ErrorState111 int;
      Declare @ErrorLine111 int;
      Select @ErrorMessage111 = ERROR_Message()
	         ,@ErrorSeverity111 = ERROR_SEVERITY()
			 ,@ErrorState111 = ERROR_STATE()
			 ,@ErrorLine111 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage111,@ErrorSeverity111,@ErrorState111,@ErrorLine111); 
 END Catch

 
Declare @R121 varchar(100),@R122 varchar(100),@R123 varchar(100),@RoleId12 int;

Select @RoleId12 =RoleId from Security.Role where ShortName ='VMPProfileManagement' and ProductId = @productid;
Select @R121 = RightId from Security.[Right] where RightName ='AccesstoBids&ContractsinVendorMarketplace' and ProductId = @productid;
Select @R122 = RightId from Security.[Right] where RightName ='AccesstoVendorProfileinVendorMarketplace' and ProductId = @productid;
Select @R123 = RightId from Security.[Right] where RightName ='EditVendorProfile' and ProductId = @productid;

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId12 and RightId in (@R121,@R122,@R123))
BEGIN TRY

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId12,@R121,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId12,@R122,@UserId,getUTCDate());
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId12,@R123,@UserId,getUTCDate());
END TRY
Begin Catch 
 
      Declare @ErrorMessage121 nvarchar(250);
      Declare @ErrorSeverity121 int;
      Declare @ErrorState121 int;
      Declare @ErrorLine121 int;
      Select @ErrorMessage121 = ERROR_Message()
	         ,@ErrorSeverity121 = ERROR_SEVERITY()
			 ,@ErrorState121 = ERROR_STATE()
			 ,@ErrorLine121 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage121,@ErrorSeverity121,@ErrorState121,@ErrorLine121); 
 END Catch
 go

-- SEED DATA FOR Security.[RoleOrganizationType]
go
DECLARE @UserId bigint, 
		@Now datetime
	
SET @Now = getUTCDate()
SELECT @UserId = UserId FROM    Ident.UserLogin WHERE    LoginName LIKE 'realpagead@%'

INSERT INTO Security.[RoleOrganizationType] (RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
SELECT DD.RoleId,DD.OrganizationTypeId,@UserId,@Now FROM (SELECT SR.RoleId,EO.OrganizationTypeId FROM Security.[role] SR, Enterprise.OrganizationType EO WHERE EO.ThruDate IS NULL AND SR.ProductId<>38 ) DD
	LEFT JOIN Security.[RoleOrganizationType] ROT ON
	ROT.RoleId = DD. RoleId AND ROT.OrganizationTypeId = DD.OrganizationTypeId 
WHERE ROT.RoleId IS NULL



Declare @R131 varchar(100),@RoleId13 int;
Select @RoleId13 =RoleId from Security.Role where ShortName = 'SuperUser' and ProductId = 3 ;
Select @R131 = RightId from Security.[Right] where RightName ='ManageVendorMarketPlaceProductAccess' and ProductId = 3;
IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId13 and RightId in (@R131))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId13,@R131,@UserId,getUTCDate());
END TRY
Begin Catch 
 
      Declare @ErrorMessage131 nvarchar(250);
      Declare @ErrorSeverity131 int;
      Declare @ErrorState131 int;
      Declare @ErrorLine131 int;
      Select @ErrorMessage131 = ERROR_Message()
	         ,@ErrorSeverity131 = ERROR_SEVERITY()
			 ,@ErrorState131 = ERROR_STATE()
			 ,@ErrorLine131 = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage131,@ErrorSeverity131,@ErrorState131,@ErrorLine131); 
 END Catch 


 IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'VPMForVendorsOrgType')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('VPMForVendorsOrgType', 'OrganizationType name for Vendors company.', 0);
END
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'VendorSuperUserRoleId')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('VendorSuperUserRoleId', 'The role Id to create admin user in  product', 0);
END
GO