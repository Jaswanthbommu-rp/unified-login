using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserStatus
    {
		private DateTime _fromDate;
		private bool _fromDateNull = true;
		private DateTime _thruDate;
		private bool _thruDateNull = true;

		public int UserStatusId { get; set; }
        public string Name { get; set; }
        
        public DateTime FromDate {
			get
			{
				return DateTime.SpecifyKind(_fromDate, DateTimeKind.Utc);
			}

			set
			{
				_fromDate = value;
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
					_thruDateNull = true;
				}
			}
		}
		public int UserId { get; set; }
    }
} 