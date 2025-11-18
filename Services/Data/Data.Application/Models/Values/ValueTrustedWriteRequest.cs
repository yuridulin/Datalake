using Datalake.Data.Application.Models.Tags;
using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Models.Values;

internal class ValueTrustedWriteRequest
{
	internal required TagSettingsDto Tag { get; init; }

	internal object? Value { get; set; }

	internal DateTime? Date { get; set; } = DateTime.UtcNow;

	internal TagQuality? Quality { get; set; }
}
