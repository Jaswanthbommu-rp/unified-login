using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// User Object
    /// </summary>
    public class UserLogin : UserLoginCommon, IUserLogin
    {
        private DateTime _LastLogin;
        private bool _LastLoginNull = true;
        private DateTime _fromDate;
        private bool _fromDateNull = true;
        private DateTime _thruDate;
        private bool _thruDateNull = true;
        private DateTime _statusThruDate;
        private bool _statusThruDateNull = true;
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
        ///  Not an input. Used to determine if the loginname is an email. Used only for the UI.
        /// </summary>
        public string LoginNameType { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        [JsonProperty(PropertyName = "IsActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// PasswordHash
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Password Salt
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Is Two Factor Enabled, 0 = off, 1 = on, 2 = pending setup
        /// </summary>
        [JsonProperty(PropertyName = "TwoFactorEnabled")]
        public int TwoFactorEnabled { get; set; }

        /// <summary>
        /// The last date the user was nagged about signing up for MFA
        /// </summary>
        [JsonProperty(PropertyName = "TwoFactorLastNotifyDate")]
        public DateTime TwoFactorLastNotifyDate { get; set; }

        /// <summary>
        /// Is the user account locked
        /// </summary>
        [JsonProperty(PropertyName = "IsLocked")]
        public bool? IsLocked { get; set; }

        /// <summary>
        /// Force users to change password.
        /// </summary>
        [JsonProperty(PropertyName = "IsTainted")]
        public bool? IsTainted { get; set; } // TBD after MVP

        /// <summary>
        /// Indicates if user account is pending
        /// </summary>
        [JsonProperty(PropertyName = "IsPending")]
        public bool? IsPending { get; set; }

        /// <summary>
        /// Is invitation to create user Expired
        /// </summary>
        [JsonProperty(PropertyName = "IsExpired")]
        public bool? IsExpired { get; set; }

        /// <summary>
        /// Force Temporary Password Set
        /// </summary>
        [JsonProperty(PropertyName = "IsForceReSetPassword")]
        public bool? IsForceReSetPassword { get; set; }

        /// <summary>
        /// doNotForceChangePassword
        /// </summary>
        [JsonProperty(PropertyName = "doNotForceChangePassword")]
        public bool doNotForceChangePassword { get; set; }

        /// <summary>
        /// Password Modified Date
        /// </summary>
        [JsonProperty(PropertyName = "PasswordModifiedDate")]
        public DateTime? PasswordModifiedDate { get; set; }

        /// <summary>
        /// Used to store the users offset to UTC for display date requirements
        /// </summary>
        [JsonIgnore]
        public int OffsetMinutes { get; set; } = 0;

        /// <summary>
        /// When the account can be used
        /// </summary>
        [JsonProperty(PropertyName = "FromDate")]
        public DateTime? FromDate
        {
            get
            {
                if (!_fromDateNull)
                {
                    return DateTime.SpecifyKind(_fromDate, DateTimeKind.Utc);
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
                    _fromDate = value.Value;
                    _fromDateNull = false;
                }
                else
                {
                    _fromDateNull = true;
                }
            }
        }

        /// <summary>
        /// When the account can no longer be used
        /// </summary>
        [JsonProperty(PropertyName = "ThruDate")]
        public DateTime? ThruDate
        {
            get
            {
                if (!_thruDateNull)
                {
                    return DateTime.SpecifyKind(_thruDate, DateTimeKind.Utc);
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
                    _thruDate = value.Value;
                    _thruDateNull = false;
                }
                else
                {
                    _thruDateNull = true;//
                }
            }
        }

        /// <summary>
        /// User Status As Of Date time
        /// </summary>
        [JsonProperty(PropertyName = "StatusThruDate")]
        public DateTime? StatusThruDate
        {
            get
            {
                if (!_statusThruDateNull)
                {
                    return DateTime.SpecifyKind(_statusThruDate, DateTimeKind.Utc);
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
                    _statusThruDate = value.Value;
                    _statusThruDateNull = false;
                }
                else
                {
                    _statusThruDateNull = true;
                }
            }
        }

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
        /// <summary>
        /// Is the user a super user
        /// </summary>
        [JsonProperty(PropertyName = "IsSuperUser")]
        public bool IsSuperUser { get; set; }

        /// <summary>
        /// User statuses (Active|Disabled|Pending). Sortable.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public UserUiStatusType Status { get; set; }

        /// <summary>
        /// Clear text password used to create new or update user 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// UserRoleType
        /// </summary>
        public UserRoleType? UserRoleType { get; set; }

        /// <summary>
        /// Use third party identity service provider.  Default to true so that the toggle switch is on on the Add new User.
        /// </summary>
        public bool Is3rdPartyIDP { get; set; } = true;
        /// <summary>
        /// StatusID
        /// </summary>
        public int StatusId { get; set; }

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
        public static List<UserLogin> GetUserOutputResultExample()
        {
            DateTime dToday = DateTime.UtcNow;

            List<UserLogin> userLoginList = new List<UserLogin>();
            userLoginList.Add(new UserLogin()
            {
                UserId = 43,
                PartyId = 1,
                LoginName = "test@test.com",
                IsActive = null,
                PasswordHash = "",
                IsLocked = false,
                IsTainted = false,
                IsExpired = false,
                IsPending = false,
                IsSuperUser = false,
                FromDate = dToday,
                ThruDate = DateTime.MaxValue.ToUniversalTime(),
            });
            userLoginList.Add(new UserLogin()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "notme@test.com",
                IsActive = null,
                PasswordHash = "",
                IsLocked = false,
                IsExpired = false,
                IsPending = false,
                IsTainted = false,
                IsSuperUser = true,
                FromDate = dToday,
                ThruDate = DateTime.MaxValue.ToUniversalTime(),
            });
            return userLoginList;
        }

        #region Examples
        /// <summary>
        /// Example for New UserLogin method
        /// </summary>
        /// <returns>Newly Created User Id</returns>
        public static UserLoginOutputResult GetNewUserLoginExample()
        {
            UserLoginOutputResult result = new UserLoginOutputResult();
            result.NewUserId = 1;
            return result;
        }

        /// <summary>
        /// Output result for New UserLogin
        /// </summary>
        public class UserLoginOutputResult
        {
            /// <summary>
            /// Represents the newly created User Id
            /// </summary>
            public long NewUserId { get; set; }
        }
        #endregion
    }
}
