using Datalake.Data.Application.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Domain.Extensions;

namespace Datalake.Data.Application.Models.Values;

internal class ValueTrustedWriteRequest
{
	internal required TagSettingsDto Tag { get; init; }

	internal object? Value { get; set; }

	internal DateTime? Date { get; set; } = DateTimeExtension.GetCurrentDateTime();

	internal TagQuality? Quality { get; set; }
}
