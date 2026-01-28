namespace UnifiedLogin.LandingAPIEnterprise.Services.Role
{
    /// <summary>
    /// Service for querying product roles and rights.
    /// </summary>
    public interface IRoleQueryService
    {
        /// <summary>
        /// Gets roles assigned to a specific user for a product.
        /// </summary>
        ActionResultEnvelope GetUserProductRoles(Guid realPageId, string productCode);

        /// <summary>
        /// Gets all roles for a product.
        /// </summary>
        ActionResultEnvelope GetProductRoles(string productCode);

        /// <summary>
        /// Gets rights for a specific role.
        /// </summary>
        ActionResultEnvelope GetRightsForRole(string productCode, int roleId);
    }

    /// <summary>
    /// Wrapper for action result data with status.
    /// </summary>
    public sealed class ActionResultEnvelope
    {
        public static ActionResultEnvelope Ok(object value) =>
            new ActionResultEnvelope(ActionResultKind.Ok, value, null);

        public static ActionResultEnvelope NotFound() =>
            new ActionResultEnvelope(ActionResultKind.NotFound, null, null);

        public static ActionResultEnvelope BadRequest(object value) =>
            new ActionResultEnvelope(ActionResultKind.BadRequest, value, null);

        private ActionResultEnvelope(ActionResultKind kind, object value, string errorMessage)
        {
            Kind = kind;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public ActionResultKind Kind { get; }
        public object Value { get; }
        public string ErrorMessage { get; }
    }

    /// <summary>
    /// Type of action result.
    /// </summary>
    public enum ActionResultKind
    {
        Ok = 1,
        NotFound = 2,
        BadRequest = 3
    }
}
