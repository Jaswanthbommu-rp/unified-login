IF OBJECT_ID('[Ident].[CreateSamlAttribute]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateSamlAttribute];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateSamlAttribute]
    @AttributeName NVARCHAR(100) ,
    @SamlAttributeTypeId INT
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Ident.SamlAttribute (   Name ,
                                                SamlAttributeTypeId
                                            )
            OUTPUT Inserted.SamlAttributeId AS Id ,
                   '' AS ErrorMessage
            VALUES ( @AttributeName, @SamlAttributeTypeId );
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

	GRANT EXECUTE ON  [Ident].[CreateSamlUserAttribute] TO [identityserver]
GO
