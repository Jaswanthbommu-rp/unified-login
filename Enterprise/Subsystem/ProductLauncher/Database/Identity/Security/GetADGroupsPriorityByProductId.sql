Create Procedure [Security].[GetADGroupsPriorityByProductId] (@productId int)
As
Begin
select ad.ADGroupId as Id, p.ProductId, ad.DisplayName as Name, ap.AssignmentOrder
from Security.ADGroupProduct ap
join Enterprise.Product p on ap.ProductId = p.ProductId
join Security.ADGroup ad on ad.ADGroupId = ap.ADGroupId
where ap.ProductId = @productId
order by AssignmentOrder
End