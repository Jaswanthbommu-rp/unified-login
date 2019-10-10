CREATE TABLE [Enterprise].[ContactMechanismType] (
    [ContactMechanismTypeID] INT           IDENTITY (1, 1) NOT NULL,
    [Description]            NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ContactMechanismType] PRIMARY KEY CLUSTERED ([ContactMechanismTypeID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Identity Column for Contact Mechanisms', @level0type = N'SCHEMA', @level0name = N'Enterprise', @level1type = N'TABLE', @level1name = N'ContactMechanismType', @level2type = N'COLUMN', @level2name = N'ContactMechanismTypeID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'A contact mechanism is an agency or means by which two or more persons, groups (parties), or other item (facility) are placed in communication with each other.', @level0type = N'SCHEMA', @level0name = N'Enterprise', @level1type = N'TABLE', @level1name = N'ContactMechanismType';

