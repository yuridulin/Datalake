using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Application.Models.Tags;

namespace Datalake.Data.Application.Models.Values;

internal class ValueTrustedWriteRequest
{
	internal required TagSettingsDto Tag { get; init; }

	internal object? Value { get; set; }

	internal DateTime? Date { get; set; } = DateTimeExtension.GetCurrentDateTime();

	internal TagQuality? Quality { get; set; }
}
