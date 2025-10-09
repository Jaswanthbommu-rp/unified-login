using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	public class UserLoginPersona : IUserLoginPersona
	{
		#region Private Variables
		private DateTime _fromDate;
		private bool _fromDateNull = true;
		private DateTime _thruDate;
		private bool _thruDateNull = true;
		private DateTime _statusThruDate;
		private bool _statusThruDateNull = true;
		#endregion

		#region Public Properties
		public long UserLoginPersonaId { get; set; }

		public long UserLoginId { get; set; }

		public int StatusTypeId { get; set; }

		public bool PrimaryOrganization { get; set; }

        public bool IsDelegateAdmin { get; set; }

		public bool IsRealPartner { get; set; }

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
		#endregion
	}
}
