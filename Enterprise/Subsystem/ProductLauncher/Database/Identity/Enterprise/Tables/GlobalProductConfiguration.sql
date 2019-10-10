CREATE TABLE [Enterprise].[GlobalProductConfiguration]
(
	[GlobalProductConfigurationId] INT NOT NULL IDENTITY(1,1),
	[ConfigurationId] INT NOT NULL , 
    [ProductId] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_GlobalProductConfiguration] PRIMARY KEY ([GlobalProductConfigurationId]), 
    CONSTRAINT [AK_GlobalProductConfiguration_ConfigurationId_ProductId_ThruDate] UNIQUE ([ConfigurationId], [ProductId], [ThruDate] ),
	CONSTRAINT [FK_GlobalProductConfiguration_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT [FK_GlobalProductConfiguration_Configuration] FOREIGN KEY([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON UPDATE CASCADE ON DELETE CASCADE
)
