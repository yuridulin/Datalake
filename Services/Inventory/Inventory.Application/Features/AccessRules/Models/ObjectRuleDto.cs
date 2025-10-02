﻿using Datalake.Contracts.Public.Enums;

namespace Datalake.Inventory.Application.Features.AccessRules.Models;

public record ObjectRuleDto(
	AccessType Type,
	Guid? UserGuid,
	Guid? UserGroupGuid);
