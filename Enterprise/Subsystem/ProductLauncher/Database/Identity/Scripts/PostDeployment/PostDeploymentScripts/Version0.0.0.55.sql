--DECLARE @RightValueTypeId INT

--SELECT @RightValueTypeId = RightValueTypeId FROM Enterprise.RightValueType WHERE Value = 'Access to Green Book Migration Tool'

--IF EXISTS(SELECT 1 FROM Enterprise.[Right] WHERE RightValueTypeId = @RightValueTypeId)
--BEGIN
--	DELETE FROM Enterprise.[Right] WHERE RightValueTypeId = @RightValueTypeId
--	DELETE FROM Enterprise.[RightValueType] WHERE RightValueTypeId = @RightValueTypeId
--END


UPDATE Enterprise.RightValueType 
SET Value = 'Access to Unified Login Migration Tool'
WHERE Value = 'Access to Green Book Migration Tool'

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='56'