using iNOPC.Library;
using System;

namespace iNOPC.Server.Storage
{
	public class Tag : DefField
	{
		public uint TagHandle { get; set; } = 0;

		public TagType Type { get; set; }
	}
}