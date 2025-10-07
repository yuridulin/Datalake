namespace Datalake.Data.Application.Models.Values;

internal record TimeSettings
{
	internal DateTime? Old { get; set; }

	internal DateTime? Young { get; set; }

	internal DateTime? Exact { get; set; }
}
