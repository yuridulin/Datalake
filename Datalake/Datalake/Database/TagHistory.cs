using Datalake.Enums;

namespace Datalake.Database
{
	public class TagHistory : V0.TagHistory
	{
		public object Value()
		{
			switch (Type)
			{
				case TagType.Number: return Number;
				case TagType.Boolean: return Number.HasValue && Number.Value != 0;
				default: return Text;
			}
		}
	}
}
