﻿using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.UpdateSource;

public record UpdateSourceCommand : ICommandRequest
{
	public required UserAccessEntity User { get; init; }
	public required int SourceId { get; init; }
	public required string Name { get; init; }
	public string? Description { get; init; }
	public string? Address { get; init; }
	public required SourceType Type { get; init; }
	public required bool IsDisabled { get; init; }
}
