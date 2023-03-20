CREATE TABLE [UserAudit].[Request] (
    [RequestId]                 BIGINT        IDENTITY (1, 1) NOT NULL,
    [CreatedDate]               DATETIME2 (7) DEFAULT (getutcdate()) NULL,
    [ReportKey]                 VARCHAR (50)  NULL,
    [ReportInstanceRequestJson] VARCHAR (MAX) NULL,
    [OrgPartyId]                BIGINT        NULL,
    PRIMARY KEY CLUSTERED ([RequestId] ASC)
);

