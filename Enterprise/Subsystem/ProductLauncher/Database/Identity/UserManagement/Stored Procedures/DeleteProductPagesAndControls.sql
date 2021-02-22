CREATE PROCEDURE [UserManagement].DeleteProductPagesAndControls (                
  @ProductId int  
)                 
AS                 
begin              
 BEGIN TRY                
        BEGIN TRANSACTION;   
    --get all the product controls  
   WITH  SelfJoinCTE AS (   
     SELECT    
    c.[ControlId]   
     FROM [UserManagement].[Control]  c  
    INNER JOIN [UserManagement].[ControlType] ct ON c.[ControlTypeId] = ct.[ControlTypeId]    
    inner join UserManagement.ProductPageControl pc on pc.ControlId=c.ControlId  
    inner join UserManagement.ProductPage pp on pp.ProductPageId=pc.ProductPageId  
     WHERE pp.ProductId=@ProductId  
     UNION ALL    
     SELECT    
     [Control].[ControlId]   
     FROM [UserManagement].[Control]    
    INNER JOIN [UserManagement].[ControlType] ON [Control].[ControlTypeId] = [ControlType].[ControlTypeId]    
    INNER JOIN SelfJoinCTE ON ([UserManagement].[Control].[ParentControlId] = SelfJoinCTE.[ControlId])    
    )    
    select DISTINCT CTE.[ControlId] into #conrolids FROM SelfJoinCTE CTE  
    
   --delete the pages  
   delete a from   
    UserManagement.ProductPageControl a join  
    UserManagement.ProductPage b   
     on a.ProductPageId=b.ProductPageId  
    where b.ProductId=@ProductId  
  
   delete from   
    UserManagement.ProductPage  
    where ProductId=@ProductId   
  
   --delete the controls  
   delete from [UserManagement].[ControlDependency]   
   where MasterControlId in   
    (SELECT controlid from #conrolids);  
  
   delete from [UserManagement].ControlAttribute   
   where ControlId in   
    (SELECT controlid from #conrolids );   
   
   delete from [UserManagement].Control   
   where ControlId in   
    (SELECT controlid from #conrolids );  
  
  COMMIT;                
 END TRY                  
 BEGIN CATCH           
  print ERROR_MESSAGE()            
        ROLLBACK;          
        DECLARE @ErrorLogID INT;                
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;                
        SELECT  0 AS Id,                
   ErrorMessage                
        FROM    dbo.ErrorLog                
        WHERE   ErrorLogID = @ErrorLogID;                
 END CATCH                
END;