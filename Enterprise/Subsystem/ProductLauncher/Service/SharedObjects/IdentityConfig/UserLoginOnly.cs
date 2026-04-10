using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class UserLoginOnly : UserLoginCommon, IUserLoginOnly, IUserLoginCommon
    {
        private DateTime _LastLogin;
        private bool _LastLoginNull = true;
        /// <summary>
        /// PartyId
        /// </summary>
        [JsonProperty(PropertyName = "PartyId")]
        public long PartyId { get; set; }

        /// <summary>
        /// RealPageId
        /// </summary>
        [JsonProperty(PropertyName = "RealPageId")]
        public Guid RealPageId { get; set; }

        /// <summary>
        /// PersonaId
        /// </summary>
        [JsonProperty(PropertyName = "PersonaId")]
        public long PersonaId { get; set; }

        /// <summary>
        ///  Not an input. Used to determine if the loginname is an email. Used only for the UI.
        /// </summary>
        public string LoginNameType { get; set; }

        /// <summary>
        /// PasswordHash
        /// </summary>
        [JsonIgnore]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Password Salt
        /// </summary>
        [JsonIgnore]
        public string PasswordSalt { get; set; }


        /// <summary>
        /// Password Modified Date
        /// </summary>
        [JsonProperty(PropertyName = "PasswordModifiedDate")]
        public DateTime? PasswordModifiedDate { get; set; }

        /// <summary>
        /// User last login date
        /// </summary>
        [JsonProperty(PropertyName = "LastLogin")]
        //public DateTime? LastLogin { get; set; }
        public DateTime? LastLogin
        {
            get
            {
                if (!_LastLoginNull)
                {
                    return DateTime.SpecifyKind(_LastLogin, DateTimeKind.Utc);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value.HasValue)
                {
                    _LastLogin = value.Value;
                    _LastLoginNull = false;
                }
                else
                {
                    _LastLoginNull = true;
                }
            }
        }

        // <summary>
        /// Clear text password used to create new or update user 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// UserRoleType
        /// </summary>
        //public UserRoleType? UserRoleType { get; set; }

        /// <summary>
        /// Use third party identity service provider.  Default to true so that the toggle switch is on on the Add new User.
        /// </summary>
        public bool Is3rdPartyIDP { get; set; } = true;

        /// <summary>
        /// Is LoginName Null or WhiteSpace
        /// </summary>
        [JsonIgnore]
        public bool IsLoginNameNullOrWhiteSpace
        {
            get
            {
                return String.IsNullOrWhiteSpace(LoginName);
            }
        }

        /// <summary>
        /// Used to get an example of a get user list
        /// </summary>
        /// <returns></returns>
        public static List<UserLoginOnly> GetUserOutputResultExample()
        {
            DateTime dToday = DateTime.UtcNow;

            List<UserLoginOnly> userLoginList = new List<UserLoginOnly>();
            userLoginList.Add(new UserLoginOnly()
            {
                UserId = 43,
                PartyId = 1,
                LoginName = "test@test.com",
                PasswordHash = "",
                //IsSuperUser = false,
            });
            userLoginList.Add(new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "notme@test.com",
                PasswordHash = "",
                //sSuperUser = true,
            });
            return userLoginList;
        }

        #region Examples
        /// <summary>
        /// Example for New UserLogin method
        /// </summary>
        /// <returns>Newly Created User Id</returns>
        public static UserLoginStatusOutputResult GetNewUserLoginExample()
        {
            UserLoginStatusOutputResult result = new UserLoginStatusOutputResult();
            result.NewUserId = 1;
            return result;
        }

        /// <summary>
        /// Output result for New UserLogin
        /// </summary>
        public class UserLoginStatusOutputResult
        {
            /// <summary>
            /// Represents the newly created User Id
            /// </summary>
            public long NewUserId { get; set; }
        }
        #endregion
    }
}
