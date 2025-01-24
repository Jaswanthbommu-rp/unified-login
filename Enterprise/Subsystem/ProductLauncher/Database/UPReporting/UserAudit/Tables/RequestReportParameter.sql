CREATE TABLE [UserAudit].[RequestReportParameter] (
    [RequestReportParameterId] INT            IDENTITY (1, 1) NOT NULL,
    [RequestId]                BIGINT         NULL,
    [ReportParameterId]        INT            NULL,
    [SelectedValue]            VARCHAR (1000) NULL,
    [CreatedDate]              DATETIME2 (7)  DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK__RequestR__7D888356DE5ECAC3] PRIMARY KEY CLUSTERED ([RequestReportParameterId] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_RequestReportParameter_RequestId]
ON [UserAudit].[RequestReportParameter] ([RequestId])
GO