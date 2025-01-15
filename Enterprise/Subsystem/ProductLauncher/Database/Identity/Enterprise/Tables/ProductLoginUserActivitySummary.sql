CREATE TABLE [Enterprise].[ProductLoginUserActivitySummary]
(
[ProductId] [int] NOT NULL,
[PersonaId] [bigint] NOT NULL,
[ImpersonatorUserId] [BIGINT] NOT NULL,
[LoginDate] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [Enterprise].[ProductLoginUserActivitySummary] ADD CONSTRAINT [PK__ProductLoginActivitybyUserCurrent] PRIMARY KEY CLUSTERED ([PersonaId], [ProductId]) ON [PRIMARY]
GO
ALTER TABLE [Enterprise].[ProductLoginUserActivitySummary] WITH NOCHECK ADD CONSTRAINT [FK_ProductLoginActivitybyUserCurrent_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId])
GO
ALTER TABLE [Enterprise].[ProductLoginUserActivitySummary] ADD CONSTRAINT [FK_ProductLoginActivitybyUserCurrent_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
GO