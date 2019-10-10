
INSERT INTO AuditDB.logging.Product (ProductName)
select a.name from [identity].enterprise.product a
    left join auditdb.logging.product  b
    on a.name = b.productname
    where b.productname is null 