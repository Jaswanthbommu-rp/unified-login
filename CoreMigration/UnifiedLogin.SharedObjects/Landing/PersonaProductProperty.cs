using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class PersonaProductProperty
	{
		/// <summary>
		/// PersonaProductPropertyId
		/// </summary>
		public int PersonaProductPropertyId { get; set; }

		/// <summary>
		/// PersonaId
		/// </summary>
		public int PersonaId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// PropertyId
		/// </summary>
		public string PropertyId { get; set; }

		/// <summary>
		/// PropertyInstanceId
		/// </summary>
		public string PropertyInstanceId { get; set; }
	}
}
