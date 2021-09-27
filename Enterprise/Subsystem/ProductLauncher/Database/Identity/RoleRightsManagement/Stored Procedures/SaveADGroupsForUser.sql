CREATE PROCEDURE [Security].[SaveADGroupsForUser]  
(  
 @PersonaId BIGINT,
 @json  varchar(max)  
)  
AS  
BEGIN
 DELETE FROM Security.ADGroupUser WHERE PersonaId =  @PersonaId  
 INSERT INTO Security.ADGroupUser(  
  ADGroupId  
  ,PersonaId
  ,CreatedBy
  ,CreatedDate
  )  
 SELECT    
  userAdGroups.ADGroupId 
  ,@PersonaId as PersonaId 
  ,'System'
  ,GETUTCDATE() as CreateDate  
   
 FROM  OPENJSON (@json)  
   WITH(  
    ADGroupId INT 
   ) AS userAdGroups
   
END