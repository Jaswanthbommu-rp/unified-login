CREATE PROCEDURE [Enterprise].[Listulmappingpersonaidforproductuserid_v2] (
@UPFMId               UNIQUEIDENTIFIER = NULL,
@ProductId            INT,
@TargetProductUserIds NVARCHAR(max))
AS
  BEGIN
      DECLARE @SamlAttributeId INT;
      DECLARE @ProductUserIdList TABLE
        (
           productuserid NVARCHAR(max)
        );
      DECLARE @TargetProductUserPersonaList TABLE
        (
           personaid BIGINT
        );
      DECLARE @OrgPartyId BIGINT;
      DECLARE @ContactPreference TABLE
        (
           personaid            INT,
           preferredphonenumber VARCHAR(30)
        )
SELECT @OrgPartyId = ep.partyid
      FROM   enterprise.party ep
      WHERE  realpageid = @UPFMId
SELECT @SamlAttributeId = samlattributeid
      FROM   ident.samlattribute
      WHERE  NAME = 'UserId'
INSERT INTO @ProductUserIdList
                  (productuserid)
      (SELECT *
       FROM   String_split(@TargetProductUserIds, ','));
SELECT DISTINCT sua.value            AS ProductUserId,
                      sua.personaid        AS UnifiedLoginPersonaId,
                      cp.preferredphonenumber,
                      ne.notificationemail AS Email
      FROM   ident.samluserattribute sua
             INNER JOIN @ProductUserIdList puid
                     ON sua.value = puid.productuserid
             INNER JOIN person.persona p
                     ON p.personaid = sua.personaid
             INNER JOIN ident.userloginpersona ulp
                     ON p.userloginpersonaid = ulp.userloginpersonaid
             INNER JOIN ident.userlogin ul
                     ON ul.userid = ulp.userloginid
             INNER JOIN person.person pe
                     ON pe.partyid = ul.personpartyid
             LEFT OUTER JOIN (SELECT AP.personaid     AS PersonaId,
                                     Isnull(TM.countrycode, '') + TM.areacode
                                     + TM.phonenumber AS PreferredPhoneNumber
                              FROM   enterprise.telecommunicationsnumber tm
                                     INNER JOIN enterprise.partycontactmechanism
                                                pcm
                                             ON tm.contactmechanismid =
                                                pcm.contactmechanismid
                                     INNER JOIN person.activepersona AP
                                             ON AP.partyid = PCM.partyid
                                     INNER JOIN person.persona p
                                             ON p.personaid = AP.personaid
                                     INNER JOIN ident.userloginpersona ulp
                                             ON p.userloginpersonaid =
                                                ulp.userloginpersonaid
                                     --AND ULP.OrganizationPartyId=@OrgPartyId
                                     INNER JOIN
                                     enterprise.[contactmechanismpreference] CMP
                                             ON CMP.contactmechanismid =
                                                PCM.contactmechanismid
                                                AND ( PCM.thrudate IS NULL
                                                       OR PCM.thrudate >
                                                          Getutcdate()
                                                    )) CP
                          ON CP.personaid = P.personaid
             LEFT OUTER JOIN (SELECT p.partyid,
                                     ea.electronicaddressstring AS
                                     NotificationEmail
                              FROM   enterprise.contactmechanismusage cmu
                                     INNER JOIN enterprise.partycontactmechanism
                                                pcm
                                             ON pcm.partycontactmechanismid =
                                                cmu.partycontactmechanismid
                                     INNER JOIN enterprise.contactmechanism cm
                                             ON cm.contactmechanismid =
                                                pcm.contactmechanismid
                                     INNER JOIN enterprise.electronicaddress ea
                                             ON ea.contactmechanismid =
                                                cm.contactmechanismid
                                     INNER JOIN enterprise.party p
                                             ON p.partyid = pcm.partyid
                              WHERE  ( pcm.thrudate IS NULL
                                        OR pcm.thrudate > Getutcdate() )
                                     AND cmu.contactmechanismusagetypeid = 301)
                             ne
                          ON ne.partyid = pe.partyid
      WHERE  productid = @ProductId
             AND samlattributeid = @SamlAttributeId
             AND ULP.organizationpartyid = @OrgPartyId
			 AND ulp.StatusTypeId = 1
  END; 
