CREATE TABLE [Security].[RoleTemplateProductAdditionalTab]
(
	[RoleTemplateProductAdditionalTabId] 	INT NOT NULL	IDENTITY,
	[RoleTemplateId]						INT		NOT NULL,
	[TabJson] NVARCHAR(MAX),  
	[CreatedDate] Datetime,
	[ModifiedDate] Datetime,
    CONSTRAINT [FK_RoleTemplateProduct_AdditionalTab_RoleTemplateId] FOREIGN KEY ([RoleTemplateId]) REFERENCES [Security].[RoleTemplate] ([RoleTemplateId])
)
