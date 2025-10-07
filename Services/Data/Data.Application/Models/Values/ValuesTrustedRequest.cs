﻿using Datalake.Contracts.Public.Enums;
using Datalake.Data.Application.Models.Tags;

namespace Datalake.Data.Application.Models.Values;

internal class ValuesTrustedRequest
{
	internal required string RequestKey { get; set; }

	internal required TagSettingsResponse[] Tags { get; set; } = [];

	internal required TimeSettings Time { get; set; }

	internal TagResolution? Resolution { get; set; } = TagResolution.None;

	internal TagAggregation? Func { get; set; } = TagAggregation.None;
}
