CREATE TABLE Enterprise.RightDependency
	(
	RightValueTypeId int NULL,
	DependentRightValueTypeId int NULL,
	[LastUpdateDateTime] DATETIME NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT FK_RightDependency_RightValueType FOREIGN KEY (RightValueTypeId) REFERENCES Enterprise.RightValueType(RightValueTypeId) ON UPDATE  NO ACTION  ON DELETE  NO ACTION ,
	CONSTRAINT FK_RightDependency_RightValueType1 FOREIGN KEY (DependentRightValueTypeId) REFERENCES Enterprise.RightValueType(RightValueTypeId) ON UPDATE  NO ACTION  ON DELETE  NO ACTION 
	)  
GO
CREATE NONCLUSTERED INDEX [IX_RightDependency_Dep] ON [Enterprise].[RightDependency]
(
	[DependentRightValueTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE UNIQUE CLUSTERED INDEX [IX_RightDependency_cls] ON Enterprise.RightDependency
	(
	RightValueTypeId,
	DependentRightValueTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO