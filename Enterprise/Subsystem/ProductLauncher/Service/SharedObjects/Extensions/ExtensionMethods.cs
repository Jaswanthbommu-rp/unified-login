using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Remove leading, middle, and trailing spaces from a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimWhiteSpace(this String str)
        {
            return new string(str.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        /// <summary>
        /// Generate the necessary information for the audit log when update an entity
        /// </summary>
        /// <typeparam name="O">Old entity type</typeparam>
        /// <typeparam name="N">New entity type</typeparam>
        /// <param name="oldEntity">Old entity</param>
        /// <param name="newEntity">New entity</param>
        /// <param name="entityName">Name of the entity</param>
        /// <returns>A list of audit records</returns>
        public static List<AuditRecord> GenerateUpdateAudit<O, N>(O oldEntity, N newEntity, string entityName) where O : N
        {
            List<AuditRecord> result = new List<AuditRecord>();

            foreach (PropertyInfo oldEntityProperty in oldEntity.GetType().GetProperties())
            {
                PropertyInfo newEntityProperty = newEntity.GetType().GetProperties().Where(p => p.Name == oldEntityProperty.Name).FirstOrDefault();

                foreach (System.Attribute attribute in newEntityProperty.GetCustomAttributes(false))
                {
                    if (attribute.GetType() == typeof(AuditLogAttribute))
                    {
                        if (newEntityProperty.GetType().Name == oldEntityProperty.GetType().Name)
                        {
                            var oldValue = oldEntityProperty.GetValue(oldEntity);

                            var newValue = newEntityProperty.GetValue(newEntity);

                            var formatedOldValue = oldValue != null ? oldValue.ToString() : null;
                            var formatedNewValue = newValue != null ? newValue.ToString() : null;

                            bool oldValueIsDate = oldValue != null && oldValue.GetType() == Type.GetType("System.DateTime") ? true : false;
                            bool newValueIsDate = newValue != null && newValue.GetType() == Type.GetType("System.DateTime") ? true : false;

                            if (newValueIsDate)
                            {
                                if (String.Format("{0:MM/dd/yyyy}", newValue).Equals(DateTime.MaxValue.ToString("MM/dd/yyyy")))
                                { 
                                    formatedNewValue = null;
                                }
                            }

                            if (formatedOldValue != formatedNewValue)
                            {
                                AuditRecord auditRecord = new AuditRecord();
                                AuditLogAttribute internalAttribute = ((AuditLogAttribute)attribute);

                                auditRecord.OldValue = internalAttribute.Format != string.Empty
                                                       ? (formatedOldValue != string.Empty
                                                            ? String.Format(internalAttribute.Format, (oldValueIsDate ? oldValue : formatedOldValue))
                                                            : formatedOldValue)
                                                       : formatedOldValue;

                                auditRecord.NewValue = internalAttribute.Format != string.Empty
                                                       ? (formatedNewValue != string.Empty
                                                            ? String.Format(internalAttribute.Format, (newValueIsDate ? newValue : formatedNewValue))
                                                            : formatedNewValue)
                                                       : formatedNewValue;

                                auditRecord.LogActivityType = internalAttribute.LogActivityTypeConstant;

                                auditRecord.AuditMessage = string.Concat("User {2} {3} updated the ",
                                                                       internalAttribute.ColumnName,
                                                                       " from ", auditRecord.OldValue == null ? internalAttribute.NullBlankValue : auditRecord.OldValue.ToString() == string.Empty ? internalAttribute.NullBlankValue : auditRecord.OldValue,
                                                                       " to ", auditRecord.NewValue == null ? internalAttribute.NullBlankValue : auditRecord.NewValue.ToString() == string.Empty ? internalAttribute.NullBlankValue : auditRecord.NewValue,
                                                                       " on ", entityName,
                                                                       " for {0} {1}");

                                result.Add(auditRecord);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
