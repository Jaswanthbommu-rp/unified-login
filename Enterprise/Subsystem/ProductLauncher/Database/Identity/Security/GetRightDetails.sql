CREATE Procedure [Security].[GetRightDetails] (@rightId int)
AS
Begin  
 select r.RightId, RightName, r.Description, r.Value,  
 p.ProductId, p.Name as ProductName, r.TargetProductId, p1.Name as TargetProductName, st.StatusTypeId, st.Name as StatusType,   
 r.VisibilityStatusId, st1.Name as Visibility, r.IsExcludeRightFromImpersonation 
 from Security.[Right] r  
 Join Enterprise.Product p on p.ProductId = r.ProductId  
 Join Enterprise.Product p1 on p1.ProductId = r.TargetProductId  
 Join Enterprise.StatusType st on st.StatusTypeId = r.StatusTypeId  
 Join Enterprise.StatusType st1  on st1.StatusTypeId = r.VisibilityStatusId  
 where r.RightId = @rightId  
End
