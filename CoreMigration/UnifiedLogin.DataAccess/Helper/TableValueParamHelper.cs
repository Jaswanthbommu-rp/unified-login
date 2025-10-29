using System.Data;
using System.Reflection;
using Dapper;

namespace UnifiedLogin.DataAccess.Helper;

/// <summary>
/// Provides extension methods for converting .NET collections to SQL Server Table-Valued Parameters (TVP).
/// Supports both simple types (primitives, strings) and complex objects with multiple properties.
/// Integrates seamlessly with Dapper for high-performance bulk operations.
/// </summary>
public static class TableValueParamHelper
{
    #region Public Extension Methods

    /// <summary>
    /// Converts a generic enumerable collection to a Dapper-compatible Table-Valued Parameter.
    /// This method handles both simple types (int, string, etc.) and complex objects with multiple properties.
    /// For complex objects, it uses reflection to map properties to database columns.
    /// </summary>
    /// <typeparam name="T">The type of objects in the enumerable collection</typeparam>
    /// <param name="enumerable">The collection of objects to convert to a table-valued parameter</param>
    /// <param name="typeName">The SQL Server user-defined table type name (must exist in database)</param>
    /// <param name="orderedColumnNames">
    /// Optional ordered list of column names for complex objects. 
    /// Required when T has multiple properties to ensure correct column mapping.
    /// For simple types, if not provided, a default column name "NONAME" will be used.
    /// </param>
    /// <returns>A <see cref="SqlMapper.ICustomQueryParameter"/> that can be used with Dapper commands</returns>
    /// <exception cref="ArgumentNullException">Thrown when enumerable or typeName is null</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when orderedColumnNames is required but not provided, or when column names don't match object properties
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when property mapping fails or type conversion issues occur</exception>
    /// <example>
    /// <code>
    /// // Simple type example
    /// var ids = new[] { 1, 2, 3, 4, 5 };
    /// var tvp = ids.ConvertToTableValuedParameter("IdTableType");
    /// 
    /// // Complex type example
    /// var users = new[] 
    /// { 
    ///     new User { Id = 1, Name = "John", Email = "john@example.com" },
    ///     new User { Id = 2, Name = "Jane", Email = "jane@example.com" }
    /// };
    /// var userTvp = users.ConvertToTableValuedParameter("UserTableType", new[] { "Id", "Name", "Email" });
    /// 
    /// // Usage with Dapper
    /// var parameters = new DynamicParameters();
    /// parameters.Add("@UserData", userTvp);
    /// connection.Execute("InsertUsers", parameters, commandType: CommandType.StoredProcedure);
    /// </code>
    /// </example>
    /// <remarks>
    /// Based on the implementation from: https://www.codeproject.com/articles/835519/passing-table-valued-parameters-with-dapper
    /// 
    /// Performance considerations:
    /// - Uses reflection for complex objects, which may impact performance for very large collections
    /// - DataTable creation is optimized for SQL Server bulk operations
    /// - Column types are automatically inferred from .NET property types
    /// 
    /// Supported .NET to SQL type mappings:
    /// - int → int
    /// - string → nvarchar
    /// - DateTime → datetime2
    /// - bool → bit
    /// - decimal → decimal
    /// - And other standard CLR to SQL type mappings
    /// </remarks>
    public static SqlMapper.ICustomQueryParameter ConvertToTableValuedParameter<T>(
        this IEnumerable<T> enumerable, 
        string typeName, 
        IEnumerable<string>? orderedColumnNames = null)
    {
        // Input validation
        ArgumentNullException.ThrowIfNull(enumerable, nameof(enumerable));
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName, nameof(typeName));

        var dataTable = new DataTable();

        try
        {
            // Handle simple types (primitives and strings)
            if (IsSimpleType<T>())
            {
                return ProcessSimpleType(enumerable, dataTable, typeName, orderedColumnNames);
            }
            else
            {
                return ProcessComplexType(enumerable, dataTable, typeName, orderedColumnNames);
            }
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Failed to convert enumerable of type {typeof(T).Name} to table-valued parameter '{typeName}'. " +
                "Please check that the type mapping is supported and the database table type exists.", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Determines if the specified type is a simple type (primitive, string, or other basic types).
    /// </summary>
    /// <typeparam name="T">The type to check</typeparam>
    /// <returns><c>true</c> if the type is simple; otherwise, <c>false</c></returns>
    private static bool IsSimpleType<T>()
    {
        var type = typeof(T);
        return type.IsValueType || 
               type == typeof(string) || 
               type == typeof(byte[]) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                type.GetGenericArguments()[0].IsValueType);
    }

    /// <summary>
    /// Processes simple types (primitives, strings) into a single-column DataTable.
    /// </summary>
    private static SqlMapper.ICustomQueryParameter ProcessSimpleType<T>(
        IEnumerable<T> enumerable,
        DataTable dataTable,
        string typeName,
        IEnumerable<string>? orderedColumnNames)
    {
        // Use provided column name or default
        var columnName = orderedColumnNames?.FirstOrDefault() ?? "NONAME";
        dataTable.Columns.Add(columnName, typeof(T));

        // Add each value as a row
        foreach (var item in enumerable)
        {
            dataTable.Rows.Add(item);
        }

        return dataTable.AsTableValuedParameter(typeName);
    }

    /// <summary>
    /// Processes complex objects with multiple properties into a multi-column DataTable.
    /// </summary>
    private static SqlMapper.ICustomQueryParameter ProcessComplexType<T>(
        IEnumerable<T> enumerable,
        DataTable dataTable,
        string typeName,
        IEnumerable<string>? orderedColumnNames)
    {
        // Get readable properties using reflection
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        if (properties.Length == 0)
        {
            throw new InvalidOperationException($"Type {typeof(T).Name} has no readable properties for table-valued parameter mapping.");
        }

        // Validate column names requirement for multi-property objects
        if (properties.Length > 1 && (orderedColumnNames == null || !orderedColumnNames.Any()))
        {
            throw new ArgumentException(
                $"Type {typeof(T).Name} has {properties.Length} properties. " +
                "Ordered column names must be provided when TVP contains more than one column. " +
                $"Available properties: [{string.Join(", ", properties.Select(p => p.Name))}]",
                nameof(orderedColumnNames));
        }

        // Determine column names to use
        var columnNames = (orderedColumnNames?.ToArray()) ?? properties.Select(p => p.Name).ToArray();

        // Validate that all specified column names correspond to actual properties
        var propertyMap = new Dictionary<string, PropertyInfo>();
        foreach (var columnName in columnNames)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                throw new ArgumentException(
                    $"Column name '{columnName}' does not match any property in type {typeof(T).Name}. " +
                    $"Available properties: [{string.Join(", ", properties.Select(p => p.Name))}]",
                    nameof(orderedColumnNames));
            }
            propertyMap[columnName] = property;
        }

        // Create DataTable columns
        foreach (var columnName in columnNames)
        {
            var property = propertyMap[columnName];
            var columnType = GetColumnType(property.PropertyType);
            dataTable.Columns.Add(columnName, columnType);
        }

        // Populate DataTable rows
        foreach (var item in enumerable)
        {
            if (item == null)
            {
                // Handle null items by adding DBNull values for all columns
                var nullRow = new object[columnNames.Length];
                Array.Fill(nullRow, DBNull.Value);
                dataTable.Rows.Add(nullRow);
            }
            else
            {
                var values = columnNames.Select(columnName =>
                {
                    var property = propertyMap[columnName];
                    var value = property.GetValue(item);
                    return value ?? DBNull.Value;
                }).ToArray();

                dataTable.Rows.Add(values);
            }
        }

        return dataTable.AsTableValuedParameter(typeName);
    }

    /// <summary>
    /// Gets the appropriate column type for DataTable based on the property type.
    /// Handles nullable types and common type conversions.
    /// </summary>
    /// <param name="propertyType">The property type to convert</param>
    /// <returns>The corresponding column type for the DataTable</returns>
    private static Type GetColumnType(Type propertyType)
    {
        // Handle nullable types by getting the underlying type
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(propertyType)!;
        }

        return propertyType;
    }

    #endregion
}