CREATE PROCEDURE [Security].[GetADGroupsForRight](
    @RightId int
)
AS
    BEGIN
		SELECT ADG.ActiveDirectoryId,
			   ADG.ADGroupId,
			   ADG.CreatedBy,
			   ADG.CreatedDate,
			   ADG.DisplayName,
			   ADG.IsActive
		FROM [Security].ADGroupRight ADR
		INNER JOIN [Security].[ADGroup] ADG ON ADG.ADGroupId = ADR.ADGroupId
		WHERE ADR.RightId = @RightId
	END
