CREATE PROCEDURE Enterprise.ManageRight
(@RightName NVARCHAR(200),
 @Status    NVARCHAR(20),
 @PartyId   INT           = NULL
)
AS
/*
Change the state of the Right for all organizations or single
*/     
	 BEGIN
         SELECT @Status = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = @Status
               AND StatusTypeCategoryType.Name = 'Security';
         UPDATE UA
           SET
               UA.Status = @Status
         FROM Enterprise.[Right] R
              INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
              INNER JOIN Enterprise.UserActions UA ON UA.RightId = R.RightId
         WHERE RVT.VALUE = @RightName
               AND (R.PartyId = @PartyId
                    OR @PartyId IS NULL);
     END;