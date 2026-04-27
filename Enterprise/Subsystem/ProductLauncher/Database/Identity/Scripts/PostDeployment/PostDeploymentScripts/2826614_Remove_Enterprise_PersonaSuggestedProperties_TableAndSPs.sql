-- Drop Enterprise.AddPersonaSuggestedProperties
IF EXISTS (SELECT 1 
           FROM sys.objects 
           WHERE object_id = OBJECT_ID(N'[Enterprise].[AddPersonaSuggestedProperties]') 
             AND type = N'P')
BEGIN
    DROP PROCEDURE [Enterprise].[AddPersonaSuggestedProperties];
END
GO

-- Drop Enterprise.DeletePersonaSuggestedProperties
IF EXISTS (SELECT 1 
           FROM sys.objects 
           WHERE object_id = OBJECT_ID(N'[Enterprise].[DeletePersonaSuggestedProperties]') 
             AND type = N'P')
BEGIN
    DROP PROCEDURE [Enterprise].[DeletePersonaSuggestedProperties];
END
GO

-- Drop Enterprise.ListSuggestedPropertiesForPersona
IF EXISTS (SELECT 1 
           FROM sys.objects 
           WHERE object_id = OBJECT_ID(N'[Enterprise].[ListSuggestedPropertiesForPersona]') 
             AND type = N'P')
BEGIN
    DROP PROCEDURE [Enterprise].[ListSuggestedPropertiesForPersona];
END
GO

-- Drop the table only if it exists
IF OBJECT_ID(N'[Enterprise].[PersonaSuggestedProperties]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [Enterprise].[PersonaSuggestedProperties];
END
GO