
CREATE TABLE [Enterprise].[ProductLoginActivitybyUser]
(UserAuditProductLoginId BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
ProductId INT NOT NULL CONSTRAINT FK_ProductLoginActivitybyUser_Product FOREIGN KEY (ProductId) REFERENCES ENTERPRISE.PRODUCT(ProductId), 
PersonaId BIGINT NOT NULL CONSTRAINT FK_ProductLoginActivitybyUser_Persona FOREIGN KEY (PersonaId) REFERENCES PERSON.PERSONA(PersonaId),  
ImpersonatorUserId BIGINT NOT NULL,
CreateDate DATETIME DEFAULT GETUTCDATE())
GO
CREATE NONCLUSTERED INDEX IDX_ProductLoginActivitybyUser_PersonaId
ON [Enterprise].[ProductLoginActivitybyUser] ([PersonaId])
INCLUDE ([ProductId],[ImpersonatorUserId],[CreateDate])
GO