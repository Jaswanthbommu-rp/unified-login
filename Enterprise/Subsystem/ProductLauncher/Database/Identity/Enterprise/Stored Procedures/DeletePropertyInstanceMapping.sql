Create Procedure [Enterprise].[DeletePropertyInstanceMapping]
(@PersonaId bigint, @ProductId int)
As
Begin
	Update Enterprise.PropertyInstanceMapping
	Set Active = 0, ThruDate = GETUTCDATE()
	where PersonaId = @personaId
	and ProductId = @ProductId
End