CREATE TABLE [Enterprise].[CommunicationEmailTemplate] (
    [CommunicationEmailTemplateID] INT            IDENTITY (1, 1) NOT NULL,
	[CommunicationEventAudienceTypeId] INT NOT NULL,
	[CommunicationEventPurposeTypeId] INT NOT NULL,
    [Subject]                      NVARCHAR (255) NOT NULL,
    [Body]                         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Enterprise.CommunicationEmailTemplate] PRIMARY KEY CLUSTERED ([CommunicationEmailTemplateID] ASC),
	CONSTRAINT [FK_EmailTemplate_AudienceType] FOREIGN KEY (CommunicationEventAudienceTypeId) REFERENCES Enterprise.CommunicationEventAudienceType (CommunicationEventAudienceTypeId) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT [FK_EmailTemplate_PurposeType] FOREIGN KEY (CommunicationEventPurposeTypeId) REFERENCES Enterprise.CommunicationEventPurposeType (CommunicationEventPurposeTypeId) ON UPDATE CASCADE ON DELETE CASCADE
);

