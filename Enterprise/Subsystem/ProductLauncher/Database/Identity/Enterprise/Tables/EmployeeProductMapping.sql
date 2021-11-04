CREATE TABLE [Enterprise].[EmployeeProductMapping]
(
	[EmployeeProductMappingId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[PersonaId] BIGINT NOT NULL,
	[ProductId] INT NOT NULL,
	[AdGroupId] INT NOT NULL,
	[CreatedDate] DATETIME2 NOT NULL 
)
GO

ALTER TABLE [Enterprise].[EmployeeProductMapping] ADD  CONSTRAINT [DF_EmployeeProductMapping_CreatedDate]  DEFAULT (GETUTCDATE()) FOR [CreatedDate]
GO

ALTER TABLE [Enterprise].[EmployeeProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeProductMapping_Persona] FOREIGN KEY([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
GO

ALTER TABLE [Enterprise].[EmployeeProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeProductMapping_ADGroup] FOREIGN KEY([ADGroupId]) REFERENCES [Security].[ADGroup] ([ADGroupId]) ON DELETE CASCADE ON UPDATE CASCADE
GO

ALTER TABLE [Enterprise].[EmployeeProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeProductMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
GO
