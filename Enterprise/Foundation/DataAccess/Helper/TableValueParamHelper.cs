using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;

namespace RP.Enterprise.Foundation.DataAccess.Component.Helper
{
    public static class TableValueParamHelper
    {
		/// <summary>
		/// Wrapper to convert a generic list to a DataTable object which can be used for passing 
		/// a list of parameters to a stored procedure.
		/// https://www.codeproject.com/articles/835519/passing-table-valued-parameters-with-dapper
		/// </summary>
		/// <typeparam name="T">type of enumerbale</typeparam>
		/// <param name="enumerable">list of values</param>
		/// <param name="typeName">database type name</param>
		/// <param name="orderedColumnNames">if more than one column in a TVP, 
		/// <returns>a custom query parameter</returns>
		public static SqlMapper.ICustomQueryParameter ConvertToTableValuedParameter<T>
            (this IEnumerable<T> enumerable, string typeName, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();

            if (typeof(T).IsValueType || typeof(T).FullName.Equals("System.String"))
            {
                dataTable.Columns.Add(orderedColumnNames == null ?
                    "NONAME" : orderedColumnNames.First(), typeof(T));
                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties
                    (BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where
                    (w => w.CanRead).ToArray();
                if (readableProperties.Length > 1 && orderedColumnNames == null)
                    throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
                var columnNames = (orderedColumnNames ??
                    readableProperties.Select(s => s.Name)).ToArray();
                foreach (string name in columnNames)
                {
                    dataTable.Columns.Add(name, readableProperties.Single
                        (s => s.Name.Equals(name)).PropertyType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single
                            (s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }

            return dataTable.AsTableValuedParameter(typeName);
        }
    }
}
