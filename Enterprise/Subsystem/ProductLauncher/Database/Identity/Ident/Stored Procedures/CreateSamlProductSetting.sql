CREATE PROCEDURE [Ident].[CreateSamlProductSetting](
	 @ProductId int
	,@LoginUri nvarchar(500)
	,@SigningCertificateThumbprint nvarchar(50)
	,@SubjectIdSamlAttribute nvarchar(20)
	,@SamlProductSettingsId int OUTPUT
	)
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

           INSERT INTO [Ident].[SamlProductSettings]
				([ProductId]
				,[LoginUri]
				,[SigningCertificateThumbprint]
				,[SubjectIdSamlAttribute])
			OUTPUT Inserted.[SamlProductSettingsId] AS Id ,
                   '' AS ErrorMessage
			VALUES
				(@ProductId
				,@LoginUri
				,@SigningCertificateThumbprint
				,@SubjectIdSamlAttribute)
            COMMIT;

			SET @SamlProductSettingsId = SCOPE_IDENTITY();

        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;
    END;