CREATE PROCEDURE [Enterprise].UpdateRight
(@RightId     INT,
 @Rightname   NVARCHAR(50),
 @Description NVARCHAR(200) NULL
)
AS
     BEGIN
         IF @Description IS NULL
            OR @Description = ''
             BEGIN
                 SELECT @Description = Description
                 FROM Enterprise.[RightValueType]
                 WHERE RightValueTypeId = @RightId;
         END;
         IF @Rightname IS NULL
            OR @Rightname = ''
             BEGIN
                 SELECT @Rightname = Value
                 FROM Enterprise.[RightValueType]
                 WHERE RightValueTypeId = @RightId;
         END;
         BEGIN TRY
             SET NOCOUNT ON;
             UPDATE Enterprise.[RightValueType]
               SET
                   value = @RightName,
                   Description = @Description
             WHERE RightValueTypeID = @RightId;
         END TRY
         BEGIN CATCH
             DECLARE @ErrorLogID INT;
             EXEC dbo.LogError
                  @ErrorLogID = @ErrorLogID OUTPUT;
             SELECT 0 AS Id,
                    ErrorMessage
             FROM [dbo].ErrorLog
             WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
     END;	
