
--UPDATE Password Policy Expiratio Date to 90 days.

UPDATE Ident.PasswordPolicy
    SET PasswordExpirationPeriodInDays = 90;

UPDATE Enterprise.Action SET ProductId = 3 WHERE ProductId = 3

IF EXISTS (SELECT 1 FROM Person.Persona WHERE UserId = -1)
BEGIN
    DELETE FROM Person.Persona WHERE UserId = -1
END



EXEC sys.sp_updateextendedproperty @name=N'Build', @value='28'

