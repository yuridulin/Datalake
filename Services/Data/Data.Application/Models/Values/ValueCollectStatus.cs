﻿namespace Datalake.Data.Application.Models.Values;

/// <summary>
/// Состояние работы
/// </summary>
public record ValueCollectStatus
{
	public DateTime Date { get; set; }

	public bool HasError { get; set; }

	public string? ErrorMessage { get; set; }
}
