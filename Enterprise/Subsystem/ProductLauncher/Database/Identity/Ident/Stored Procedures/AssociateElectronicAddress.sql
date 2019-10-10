CREATE PROCEDURE [Ident].[AssociateElectronicAddress]
    @RealPageId UNIQUEIDENTIFIER ,
    @ElectronicAddress VARCHAR(255) ,
    @ElectronicAddressType VARCHAR(50) ,
    @ElectronicAddressTypeUsage VARCHAR(50)
AS
    DECLARE @ContactMechanismId INT;
    DECLARE @PartyId BIGINT;
    DECLARE @PartyContactMechanismId INT;
    DECLARE @ContactMechanismUsageTypeId INT;
    DECLARE @ErrorLogID INT;
    
    SELECT  @ContactMechanismId = ea.ContactMechanismID
    FROM    Enterprise.ElectronicAddress ea
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = ea.ContactMechanismID
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.ContactMechanismId = ea.ContactMechanismID
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   ea.ElectronicAddressString = @ElectronicAddress AND p.RealPageId = @RealPageId;

    SELECT  @ContactMechanismUsageTypeId = ContactMechanismUsageTypeID
    FROM    Enterprise.ContactMechanismUsageType
    WHERE   Name = @ElectronicAddressTypeUsage;


    SELECT  @PartyId = p.PartyId
    FROM    Enterprise.Party p
    WHERE   p.RealPageId = @RealPageId;

    IF @ContactMechanismId IS NULL
        BEGIN TRY
            BEGIN TRAN;
            INSERT  INTO Enterprise.ContactMechanism
                    DEFAULT VALUES;

            SET @ContactMechanismId = SCOPE_IDENTITY();

            INSERT  INTO Enterprise.ElectronicAddress
                    ( ContactMechanismID ,
                      ElectronicAddressString ,
                      ElectronicAddressType
		            )
            VALUES  ( @ContactMechanismId ,
                      @ElectronicAddress ,
                      @ElectronicAddressType
		            );

            INSERT  INTO Enterprise.PartyContactMechanism
                    ( PartyId ,
                      ContactMechanismId ,
                      FromDate ,
                      ThruDate
			        )
            VALUES  ( @PartyId , -- PartyId - bigint
                      @ContactMechanismId , -- ContactMechanismId - int
                      GETUTCDATE() , -- FromDate - datetime
                      NULL -- ThruDate - datetime
			        );

            INSERT  INTO Enterprise.ContactMechanismUsage
                    ( PartyContactMechanismID ,
                      ContactMechanismUsageTypeID
			        )
            VALUES  ( @PartyContactMechanismId , -- PartyContactMechanismID - bigint
                      @ContactMechanismUsageTypeId  -- ContactMechanismUsageTypeID - int
			        );

            COMMIT;
        END TRY  
        BEGIN CATCH
            ROLLBACK;

            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;
        END CATCH;	
    ELSE
        BEGIN TRY
            BEGIN TRAN;
            INSERT  INTO Enterprise.PartyContactMechanism
                    ( PartyId ,
                      ContactMechanismId ,
                      FromDate ,
                      ThruDate
			        )
            VALUES  ( @PartyId , -- PartyId - bigint
                      @ContactMechanismId , -- ContactMechanismId - int
                      GETUTCDATE() , -- FromDate - datetime
                      NULL -- ThruDate - datetime
			        );
					 
            INSERT  INTO Enterprise.ContactMechanismUsage
                    ( PartyContactMechanismID ,
                      ContactMechanismUsageTypeID
			        )
            VALUES  ( @PartyContactMechanismId , -- PartyContactMechanismID - bigint
                      @ContactMechanismUsageTypeId  -- ContactMechanismUsageTypeID - int
			        );	
        END TRY  
        BEGIN CATCH
            ROLLBACK;

            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;
        END CATCH;					    


