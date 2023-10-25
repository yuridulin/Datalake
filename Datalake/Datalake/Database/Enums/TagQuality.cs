namespace Datalake.Database.Enums
{
	public enum TagQuality
	{
		Bad = 0,
		Bad_NoConnect = 4,
		Bad_NoValues = 8,
		Good = 192,
		Good_ManualWrite = 216,
		Unknown = -1,
	}
}
