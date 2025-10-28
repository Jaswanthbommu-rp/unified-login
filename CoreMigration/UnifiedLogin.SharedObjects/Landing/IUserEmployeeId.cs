namespace UnifiedLogin.SharedObjects.Landing
{
    public interface IUserEmployeeId
    {
        /// <summary>
        /// UserEmployee
        /// </summary>
        int UserEmployeeId { get; set; }

        /// <summary>
        /// UserLoginPersonaId
        /// </summary>
        long UserLoginPersonaId { get; set; }

        /// <summary>
        /// EmployeeId
        /// </summary>
        string EmployeeId { get; set; }
    }
}
