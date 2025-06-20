using Datalake.PublicApi.Enums;

namespace Datalake.Database.Tables;
public interface IReadOnlySource
{
	string? Address { get; set; }
	string? Description { get; set; }
	int Id { get; set; }
	bool IsDeleted { get; set; }
	string Name { get; set; }
	SourceType Type { get; set; }
}