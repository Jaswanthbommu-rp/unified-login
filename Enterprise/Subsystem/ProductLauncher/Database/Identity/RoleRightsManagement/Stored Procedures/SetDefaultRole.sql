Create PROCEDURE [Security].[SetDefaultRole](@RoleId INT,@PartyId bigint,@CreatedBy nvarchar(50))
AS
     BEGIN
         DECLARE @DefaultRoleId INT;
         DECLARE @SaveEnterpriseRoleData varchar(10);
		 
		Select @SaveEnterpriseRoleData = PS.Value from Enterprise.ProductSetting PS
		Join Enterprise.ProductSettingType PST ON
			PS.ProductSettingTypeId = PST.ProductSettingTypeId
		where PS.ProductId = 3
		And PST.Name = 'SaveRoleDataInEnterprise'
         
		IF @PartyId IS NOT NULL
		BEGIN
			BEGIN TRY
				IF EXISTS (SELECT 1 FROM Security.OrganizationDefaultRole Where OrgPartyId = @PartyId)
				BEGIN
					UPDATE Security.OrganizationDefaultRole SET RoleId = @RoleId,
																CreatedBy = @CreatedBy,
																CreatedDate = GETDATE()
					Where OrgPartyId = @PartyId
				END
				ELSE
				BEGIN
					INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
					SELECT @PartyId,@RoleId,@CreatedBy,GETDATE()
				END
				 
				SELECT @RoleId AS RoleId,
					'' AS ErrorMessage;

				IF (@SaveEnterpriseRoleData = 'True')
				BEGIN
					SET @DefaultRoleId = @RoleId
					IF (@RoleId = 1)
					Begin
						Select @DefaultRoleId = RoleId From Enterprise.Role R
						Join Enterprise.RoleValueType RV ON
							RV.RoleValueTypeId = R.RoleValueTypeId
						Where RV.Value = 'User Administrator'
						And R.PartyID = @PartyId
					End
					IF (@RoleId = 2)
					Begin
						Select @DefaultRoleId = RoleId From Enterprise.Role R
						Join Enterprise.RoleValueType RV ON
							RV.RoleValueTypeId = R.RoleValueTypeId
						Where RV.Value = 'Basic End User'
						And R.PartyID = @PartyId
					End
					EXEC [Enterprise].[SetDefaultRole] @DefaultRoleId
				END
			END TRY
			BEGIN CATCH
				DECLARE @ErrorLogID INT;
				EXEC dbo.LogError
					@ErrorLogID = @ErrorLogID OUTPUT;
				SELECT @RoleId AS Id,
					ErrorMessage
				FROM [dbo].ErrorLog
				WHERE ErrorLogID = @ErrorLogID;
			END CATCH;			
		END
     END;
