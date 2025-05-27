namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Right detail
	/// </summary>
    public class Right : IRight
	{
		/// <summary>
		/// Unique RightId
		/// </summary>
		public int RightId { get; set; }

		/// <summary>
		/// Right Name
		/// </summary>
		public string RightName { get; set; }


		/// <summary>
		/// Right Description
		/// </summary>
		public string RightDescription { get; set; }
 
		/// <summary>
		/// Right Value TypeId
		/// </summary>
		public int RightValueTypeId { get; set; }

		/// <summary>
		/// Right ShortName (NickName)
		/// </summary>
        public string RightNickName { get; set; }

        /// <summary>
        /// Is Exclude From Impersonation
        /// </summary>
        public bool IsExcludeRightFromImpersonation { get; set; }
    }
}
