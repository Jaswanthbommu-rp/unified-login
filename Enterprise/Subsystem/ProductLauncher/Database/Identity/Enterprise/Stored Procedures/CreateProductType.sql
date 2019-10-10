CREATE PROCEDURE [Enterprise].[CreateProductType](  
	 @ProductTypeId int   
	,@ParentProductTypeId int   
	,@Name varchar(50)  
	,@Description varchar(1000)  
	,@ProductTypeGUID uniqueidentifier
 )   
AS  
BEGIN  

	SET NOCOUNT ON;  
	   
	BEGIN TRY  
		BEGIN TRAN;  
		INSERT INTO [Enterprise].[ProductType]  
		(
			[ProductTypeId]
           ,[ParentProductTypeId]
           ,[Name]
           ,[Description]
           ,[ProductTypeGUID]
		)
		OUTPUT	Inserted.ProductTypeId AS Id,
				'' AS ErrorMessage  
		VALUES
		(
			 @ProductTypeId  
			,@ParentProductTypeId  
			,@Name
			,@Description
			,@ProductTypeGUID
		)
		COMMIT;  
	END TRY  
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
				ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH; 
END;