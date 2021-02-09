CREATE PROCEDURE [Enterprise].[CreatePropertyInstance]

(
	 @Name			NVARCHAR(250)
	,@Address		NVARCHAR(200)	= NULL
	,@City			NVARCHAR(60)	= NULL
	,@State			NVARCHAR(20)	= NULL
	,@PostalCode	NVARCHAR(25)	= NULL
	,@Country		NVARCHAR(25)	= NULL
	,@County		NVARCHAR(60)	= NULL
	,@Latitude		DECIMAL(9,6)	= 0
	,@Longitude		DECIMAL(9,6)	= 0
	,@CustomerPropertyId	BIGINT  = 0
	,@Domain		NVARCHAR(50) = 'UNKNOWN'
)
AS
BEGIN  
 SET NOCOUNT ON  
 DECLARE @PropertyInstanceId BIGINT  
   
  IF NOT EXISTS(select 1 from Enterprise.PropertyInstance where CustomerPropertyId= @CustomerPropertyId and Domain = @Domain)
  BEGIN
		INSERT INTO [Enterprise].[PropertyInstance]  
	  ([Name]  
	  ,[Address]  
	  ,[City]  
	  ,[State]  
	  ,[PostalCode]  
	  ,[Country]  
	  ,[County]  
	  ,[Latitude]  
	  ,[Longitude]  
	  ,[CustomerPropertyId]  
	  ,[Domain])  
		VALUES  
	 (  
	  @Name  
	  ,@Address  
	  ,@City  
	  ,@State  
	  ,@PostalCode  
	  ,@Country  
	  ,@County  
	  ,@Latitude  
	  ,@Longitude  
	  ,@CustomerPropertyId  
	  ,@Domain  
	 )  
  
	 SET @PropertyInstanceId = SCOPE_IDENTITY();  
  
	 SELECT   
	  @PropertyInstanceId AS Id,  
	  InstanceId AS RealPageId,  
	  '' AS ErrorMessage  
	 FROM Enterprise.PropertyInstance  
	 WHERE  
	  PropertyInstanceId = @PropertyInstanceId  
  END
  ELSE
  BEGIN
	 SELECT   
	  PropertyInstanceId AS Id,  
	  InstanceId AS RealPageId,  
	  'A Platform Property Already Exist with the Same Domain.' AS ErrorMessage  
	 FROM Enterprise.PropertyInstance
	 WHERE 
		CustomerPropertyId= @CustomerPropertyId and Domain = @Domain
  END  
END
GO
