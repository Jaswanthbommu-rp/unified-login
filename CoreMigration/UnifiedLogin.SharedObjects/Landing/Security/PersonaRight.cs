namespace UnifiedLogin.SharedObjects.Landing.Security
{
    /// <summary>
    /// Persona Action Right
    /// </summary>
    public class PersonaActionRight
    {
        /// <summary>
        /// Action Id
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Object Type
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Parent Action Id
        /// </summary>
        public int? ParentActionId { get; set; }

        /// <summary>
        /// Action value Type Id
        /// </summary>
        public int ActionvalueTypeId { get; set; }

        /// <summary>
        /// ProductId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Is Exclude From Impersonation
        /// </summary>
        public bool IsExcludeRightFromImpersonation { get; set; }
    }
}
