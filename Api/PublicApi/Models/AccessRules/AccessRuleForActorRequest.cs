﻿using Datalake.PublicApi.Enums;

namespace Datalake.PublicApi.Models.AccessRules;

/// <summary>
/// Необходимая информация для создания правила доступа на конкретный объект
/// </summary>
/// <param name="AccessType">Тип доступа</param>
/// <param name="BlockId">Идентификатор блока</param>
/// <param name="SourceId">Идентификатор источника данных</param>
/// <param name="TagId">Идентификатор тега</param>
public record AccessRuleForActorRequest(
	AccessType AccessType,
	int? BlockId = null,
	int? SourceId = null,
	int? TagId = null);
