CREATE PROCEDURE [Ident].[CreateSamlAttributeType]
    @AttributeTypeName NVARCHAR(100)
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Ident.SamlAttributeType ( [Name] )
            OUTPUT Inserted.SamlAttributeTypeId AS Id ,
                   '' AS ErrorMessage
            VALUES ( @AttributeTypeName );

            COMMIT;
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
