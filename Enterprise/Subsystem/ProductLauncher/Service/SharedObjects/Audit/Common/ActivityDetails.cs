using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common
{
    /// <summary>
    /// Class for activity details copied from Audit.Core
    /// </summary>
    [Serializable]
    public class ActivityDetails
    {
        protected ActivityDetails(ActivityDetails activityDetails)
        {
            AdditionalInformation = activityDetails.AdditionalInformation;
            LogActivityTypeName = activityDetails.LogActivityTypeName;
            LogCategoryName = activityDetails.LogCategoryName;
            Message = activityDetails.Message;
            OrganizationPartyId = activityDetails.OrganizationPartyId;
            BooksMasterOrganizationId = activityDetails.BooksMasterOrganizationId;
            CorrelationId = activityDetails.CorrelationId;
            BooksProductCode = activityDetails.BooksProductCode;

            FromUserLoginName = activityDetails.FromUserLoginName;
            FromUserFirstName = activityDetails.FromUserFirstName;
            FromUserLastName = activityDetails.FromUserLastName;
            FromUserLoginId = activityDetails.FromUserLoginId;
            FromUserRealpageId = activityDetails.FromUserRealpageId;

            ToUserLoginName = activityDetails.ToUserLoginName;
            ToUserLoginId = activityDetails.ToUserLoginId;
            ToUserFirstName = activityDetails.ToUserFirstName;
            ToUserLastName = activityDetails.ToUserLastName;
            ToUserRealpageId = activityDetails.ToUserRealpageId;

            ContextId = activityDetails.ContextId;
            ContextReferenceId = activityDetails.ContextReferenceId;
        }

        public ActivityDetails()
        {
        }

        public string LogCategoryName { get; set; }
        public string LogActivityTypeName { get; set; }
        public string CorrelationId { get; set; }
        public string Message { get; set; }
        public string ContextId { get; set; }
        public string ContextReferenceId { get; set; }
        public string FromUserLoginName { get; set; }
        public long FromUserLoginId { get; set; }
        public string FromUserFirstName { get; set; }
        public string FromUserLastName { get; set; }
        public string FromUserRealpageId { get; set; }

        public string ToUserLoginName { get; set; }
        public long? ToUserLoginId { get; set; }
        public string ToUserFirstName { get; set; }
        public string ToUserLastName { get; set; }
        public string ToUserRealpageId { get; set; }

        public long OrganizationPartyId { get; set; }
        public long BooksMasterOrganizationId { get; set; }
        public string BooksProductCode { get; set; }
        public List<AdditionalParameters> AdditionalInformation { get; set; }
    }

    [Serializable]
    public class ActivityDetailMessage : ActivityDetails // We have to keep this class public as Serialization won't work with internal or private class
    {
        private ActivityDetailMessage()
        { }

        public ActivityDetailMessage(ActivityDetails activityDetails) : base(activityDetails)
        {
        }
        public string ServerName { get; set; }
        public DateTime ApplicationTimestamp { get; set; }
    }

    [Serializable]
    public class AdditionalParameters
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
