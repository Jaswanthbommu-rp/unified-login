--User Story 1203781: UL: Update logic for "last login" - should be based on the user's last log in to the PMC - DEV/QA

IF NOT EXISTS( select TOP 1 1 from Ident.UserLoginPersona where LastLoginDate IS NOT NULL)
BEGIN
 update ulp
 set ulp.LastLoginDate = ul.LastLoginDate
 from Ident.UserLogin UL 
 inner join ident.userloginpersona ulp on ul.userid = ulp.userloginid and ulp.PrimaryOrganization = 1 
END
Go