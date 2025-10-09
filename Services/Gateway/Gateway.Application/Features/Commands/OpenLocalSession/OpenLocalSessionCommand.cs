﻿using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.OpenLocalSession;

public record OpenLocalSessionCommand : ICommandRequest
{
	public required string Login { get; init; }

	public required string PasswordString { get; init; }
}
