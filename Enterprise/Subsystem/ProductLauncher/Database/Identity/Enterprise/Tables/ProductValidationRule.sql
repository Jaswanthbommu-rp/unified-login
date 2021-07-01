CREATE TABLE [Enterprise].[ProductValidationRule](
	[ProductValidationRuleId] INT IDENTITY(1,1) NOT NULL,
	[ProductId] INT NOT NULL,
	[ProductRuleTypeId] INT NOT NULL,
	[RuleValue] [INT] NOT NULL,
	[ValidationMessage] [nvarchar](255) NULL,	
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ProductValidationRule] PRIMARY KEY CLUSTERED 
(
	[ProductValidationRuleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Enterprise].[ProductValidationRule]  WITH CHECK ADD  CONSTRAINT [FK_ProductValidationRule_ProductId] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO
ALTER TABLE [Enterprise].[ProductValidationRule]  WITH CHECK ADD  CONSTRAINT [FK_ProductValidationRule_ProductRuleTypeId] FOREIGN KEY([ProductRuleTypeId])
REFERENCES [Enterprise].[ProductRuleType] ([ProductRuleTypeId])
GO
