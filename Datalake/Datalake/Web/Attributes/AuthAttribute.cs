using Datalake.Enums;
using System;
using System.Collections.Generic;

namespace Datalake.Web.Attributes
{
	class AuthAttribute : Attribute
	{
		public List<AccessType> Types { get; }

		public AuthAttribute(AccessType type1)
		{
			Types = new List<AccessType> { type1 };
			Fill();
		}

		public AuthAttribute(AccessType type1, AccessType type2)
		{
			Types = new List<AccessType> { type1, type2 };
			Fill();
		}

		public AuthAttribute(AccessType type1, AccessType type2, AccessType type3)
		{
			Types = new List<AccessType> { type1, type2, type3 };
			Fill();
		}

		public AuthAttribute(AccessType type1, AccessType type2, AccessType type3, AccessType type4)
		{
			Types = new List<AccessType> { type1, type2, type3, type4 };
			Fill();
		}

		void Fill()
		{
			if (Types.Contains(AccessType.NOT))
			{
				Types.Add(AccessType.USER);
				Types.Add(AccessType.ADMIN);
			}

			if (Types.Contains(AccessType.USER))
			{
				Types.Add(AccessType.ADMIN);
			}
		}
	}
}
