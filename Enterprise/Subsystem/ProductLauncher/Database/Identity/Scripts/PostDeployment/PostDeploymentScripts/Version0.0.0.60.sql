IF OBJECT_ID('tempdb..#BooksProductCode') IS NOT NULL
BEGIN
	DROP TABLE #BooksProductCode;
END;

CREATE TABLE #BooksProductCode
	(ProductId INT, BooksProductCode nvarchar(20))

insert into #BooksProductCode (ProductId, BooksProductCode) VALUES (1,'OS')
,(2,'UI')
,(3,'UL')
,(4,'AO')
,(5,'PW')
,(6,'L2L')
,(7,'YS')
,(8,'ACCT')
,(9,'LS')
,(10,'LVL1')
,(11,'NULL')
,(12,'OPSB')
,(13,'SM')
,(14,'OMS')
,(15,'LD')
,(16,'CD')
,(17,'AB')
,(18,'NWP')
,(19,'LP')
,(20,'DOC')
,(21,'OSC')
,(22,'OC')
,(23,'ONST')
,(24,'RA')
,(25,'SP')
,(26,'UA')
,(27,'MT')
,(28,'NULL')

UPDATE P
	SET P.BooksProductCode = PC.BooksProductCode
FROM Enterprise.Product P
	INNER JOIN #BooksProductCode PC
		ON PC.ProductId = P.ProductId

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='61'