CREATE TABLE [UserAudit].[ReportConfiguration] (
    [ReportConfigurationId] BIGINT           IDENTITY (1, 1) NOT NULL,
    [ReportKey]             UNIQUEIDENTIFIER NULL,
    [ReportName]            VARCHAR (200)    NULL,
    [OrgPartyId]            BIGINT           NULL,
    [FiltersJson]           VARCHAR (MAX)    NULL,
    PRIMARY KEY CLUSTERED ([ReportConfigurationId] ASC)
);

