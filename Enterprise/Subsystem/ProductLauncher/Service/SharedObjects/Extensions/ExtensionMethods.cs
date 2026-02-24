using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
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
        #region "Common"

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
        /// Validate a boolean value state 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A boolean value</returns>
        public static bool GetBooleanValue(this bool? value)
        {
            if (value.HasValue && value.Value == true)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region "Audit"

        /// <summary>
        /// Generate the necessary information for the audit log when update an entity
        /// </summary>
        /// <typeparam name="O">Old entity type</typeparam>
        /// <typeparam name="N">New entity type</typeparam>
        /// <param name="oldEntity">Old entity</param>
        /// <param name="newEntity">New entity</param>
        /// <param name="entityAffected">Name of the affected entity</param>
        /// <returns>A list of audit records</returns>
        public static List<AuditRecord> GenerateUpdateAudit<O, N>(O oldEntity, N newEntity, string entityAffected, bool isRealPageUser) where O : N
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

                            var formatedOldValue = oldValue != null ? (oldValue.ToString() != string.Empty ? oldValue.ToString() : null) : null;
                            var formatedNewValue = newValue != null ? (newValue.ToString() != string.Empty ? newValue.ToString() : null) : null;

                            bool oldValueIsDate = oldValue != null && oldValue.GetType() == Type.GetType("System.DateTime") ? true : false;
                            bool newValueIsDate = newValue != null && newValue.GetType() == Type.GetType("System.DateTime") ? true : false;

                            if (oldValueIsDate)
                            {
                                oldValue = TimeZoneInfo.ConvertTime(Convert.ToDateTime(oldValue), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            }

                            if (newValueIsDate)
                            {
                                if (String.Format("{0:MM/dd/yyyy}", newValue).Equals(DateTime.MaxValue.ToString("MM/dd/yyyy")))
                                {
                                    newValue = null;
                                    formatedNewValue = null;
                                }
                                else
                                {
                                    newValue = TimeZoneInfo.ConvertTime(Convert.ToDateTime(newValue), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
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
                                auditRecord.ColumnName = internalAttribute.ColumnName;

                                if (!string.IsNullOrWhiteSpace(internalAttribute.Message))
                                {
                                    Dictionary<string, string> deserializedObject = (Dictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>(internalAttribute.Message);

                                    if (deserializedObject.ContainsKey(newValue.ToString()))
                                    {
                                        auditRecord.AuditMessage = deserializedObject.Where(p => p.Key == newValue.ToString()).FirstOrDefault().Value;

                                        if (!string.IsNullOrWhiteSpace(auditRecord.AuditMessage))
                                        {
                                            result.Add(auditRecord);
                                        }
                                    }
                                }
                                else
                                {
                                    
                                    auditRecord.AuditMessage = string.Concat((isRealPageUser ? "RealPage User " : String.Empty), "{2} updated the ",
                                       internalAttribute.ColumnName,
                                       newValueIsDate || oldValueIsDate ? " date from " :" from ", auditRecord.OldValue == null ? internalAttribute.NullBlankValue : auditRecord.OldValue.ToString() == string.Empty ? internalAttribute.NullBlankValue : auditRecord.OldValue,
                                       " to ", auditRecord.NewValue == null ? internalAttribute.NullBlankValue : auditRecord.NewValue.ToString() == string.Empty ? internalAttribute.NullBlankValue : auditRecord.NewValue,
                                       ((newValueIsDate || oldValueIsDate) && (auditRecord.NewValue != null && !string.IsNullOrEmpty(auditRecord.NewValue.ToString()))) ? " CST" : "", newValueIsDate || oldValueIsDate ? string.Empty : " on the " + entityAffected,
                                       " for {0} {1}.");

                                    result.Add(auditRecord);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the changes on custom fields for the audit log
        /// </summary>
        /// <param name="oldCustomField">Old custom fields</param>
        /// <param name="newCustomField">New custom fields</param>
        /// <returns>List of audit records</returns>
        public static List<AuditRecord> GetCustomFieldsAudit(IList<CustomFieldValue> oldCustomField, IList<CustomFieldValue> newCustomField)
        {
            newCustomField = newCustomField != null ? newCustomField : new List<CustomFieldValue>();
            List<AuditRecord> result = new List<AuditRecord>();
         
            foreach (CustomFieldValue oldCustomFieldValue in oldCustomField)
            {
                foreach (CustomFieldValue newCustomFieldValue in newCustomField)
                {
                    if (oldCustomFieldValue.FieldId == newCustomFieldValue.FieldId && oldCustomFieldValue.FieldValueId == newCustomFieldValue.FieldValueId)
                    {
                        string oldValue = string.IsNullOrEmpty(oldCustomFieldValue.Value) ? string.Empty : oldCustomFieldValue.Value;
                        string newValue = string.IsNullOrEmpty(newCustomFieldValue.Value) ? string.Empty : newCustomFieldValue.Value;

                        if (oldValue != newValue)
                        {

                            AuditRecord auditRecord = new AuditRecord();
                            auditRecord.OldValue = oldValue;
                            auditRecord.NewValue = newValue;
                            auditRecord.ColumnName = newCustomFieldValue.Name;
                            auditRecord.AuditMessage = string.Concat("{2} updated the ",
                                                  oldCustomFieldValue.Name,
                                                  " information from ", string.IsNullOrEmpty(oldValue) ? "a blank value" : oldValue,
                                                  " to ", string.IsNullOrEmpty(newValue) ? "a blank value" : newValue,
                                                  " for {0} {1}.");

                            auditRecord.LogActivityType = LogActivityTypeConstants.UPDATE_USER;

                            result.Add(auditRecord);
                        }
                    }
                }
            }



            List<CustomFieldValue> newEnabledFields = newCustomField.Where(n => !oldCustomField.Any(o => o.FieldId == n.FieldId)).ToList();

            foreach (CustomFieldValue customField in newEnabledFields)
            {
                if (string.IsNullOrEmpty(customField.Value) == false)
                {
                    AuditRecord auditRecord = new AuditRecord();

                    auditRecord.AuditMessage = string.Concat("{2} updated the ",
                                          customField.Name,
                                          " information from a blank value",
                                          " to ", customField.Value,
                                          " on the user profile",
                                          " for {0} {1}.");

                    auditRecord.LogActivityType = LogActivityTypeConstants.UPDATE_USER;

                    result.Add(auditRecord);
                }
            }

            return result;
        }

        #endregion

    }
}

