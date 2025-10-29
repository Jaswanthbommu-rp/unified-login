namespace UnifiedLogin.DataAccess.Model;

/// <summary>
/// Information required to execute stored procedures with Table-Valued Parameters (TVP).
/// Contains metadata about the stored procedure, table type definition, and parameter mappings
/// necessary for converting .NET collections to SQL Server table-valued parameters.
/// </summary>
public sealed class TableValueParmInfo
{
    #region Public Properties

    /// <summary>
    /// Gets or sets the name of the stored procedure to execute.
    /// This should be the exact name of the stored procedure as it exists in the database.
    /// </summary>
    /// <value>The stored procedure name (e.g., "InsertMultipleUsers", "UpdateProductsBatch")</value>
    /// <example>InsertUsers</example>
    public string StoredProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ordered collection of column names for the table-valued parameter.
    /// The order must match the column order defined in the SQL Server table type.
    /// This is used to map object properties to table columns correctly.
    /// </summary>
    /// <value>An enumerable collection of column names in the correct order</value>
    /// <example>["UserId", "UserName", "Email", "CreatedDate"]</example>
    public IEnumerable<string> OrderedColumnName { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the SQL Server table type name used for the table-valued parameter.
    /// This must match the user-defined table type created in the database.
    /// </summary>
    /// <value>The table type name as defined in SQL Server (e.g., "UserTableType", "ProductTableType")</value>
    /// <example>UserTableType</example>
    public string TableParamTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter name used in the stored procedure for the table-valued parameter.
    /// This is the name by which the stored procedure expects to receive the table data.
    /// </summary>
    /// <value>The parameter name including the @ prefix (e.g., "@Users", "@ProductData")</value>
    /// <example>@Users</example>
    public string TableVariableName { get; set; } = string.Empty;

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that all required properties are properly configured for TVP operations.
    /// </summary>
    /// <returns><c>true</c> if all required properties are valid; otherwise, <c>false</c></returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(StoredProcedureName) &&
               !string.IsNullOrWhiteSpace(TableParamTypeName) &&
               !string.IsNullOrWhiteSpace(TableVariableName) &&
               OrderedColumnName.Any();
    }

    /// <summary>
    /// Gets validation error messages for any improperly configured properties.
    /// </summary>
    /// <returns>A collection of validation error messages, or empty if validation passes</returns>
    public IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(StoredProcedureName))
            errors.Add($"{nameof(StoredProcedureName)} is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(TableParamTypeName))
            errors.Add($"{nameof(TableParamTypeName)} is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(TableVariableName))
            errors.Add($"{nameof(TableVariableName)} is required and cannot be empty.");

        if (!OrderedColumnName.Any())
            errors.Add($"{nameof(OrderedColumnName)} must contain at least one column name.");

        return errors;
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Returns a string representation of the TableValueParmInfo for debugging purposes.
    /// </summary>
    /// <returns>A formatted string containing the key properties</returns>
    public override string ToString()
    {
        return $"StoredProcedure: {StoredProcedureName}, TableType: {TableParamTypeName}, " +
               $"Parameter: {TableVariableName}, Columns: [{string.Join(", ", OrderedColumnName)}]";
    }

    #endregion
}