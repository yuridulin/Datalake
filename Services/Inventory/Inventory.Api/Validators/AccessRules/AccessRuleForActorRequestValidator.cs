using Datalake.Inventory.Api.Models.AccessRules;
using FluentValidation;

namespace Datalake.Inventory.Api.Validators.AccessRules;

/// <summary>
/// Валидатор для <see cref="AccessRuleForActorRequest"/>
/// </summary>
public class AccessRuleForActorRequestValidator : AbstractValidator<AccessRuleForActorRequest>
{
	/// <summary>
	/// Валидация
	/// </summary>
	public AccessRuleForActorRequestValidator()
	{
		RuleFor(x => x.AccessType)
			.IsInEnum()
			.WithMessage("Тип доступа должен быть допустимым значением");

		// Должен быть указан ровно один актор
		RuleFor(x => x)
			.Must(x => new[] { x.BlockId, x.TagId, x.SourceId }.Count(id => id.HasValue) == 1)
			.WithMessage("Должен быть указан ровно один объект (тег, блок или источник)");

		// Валидация идентификаторов
		When(x => x.BlockId.HasValue, () =>
		{
			RuleFor(x => x.BlockId).GreaterThan(0);
		});

		When(x => x.TagId.HasValue, () =>
		{
			RuleFor(x => x.TagId).GreaterThan(0);
		});

		When(x => x.SourceId.HasValue, () =>
		{
			RuleFor(x => x.SourceId).GreaterThan(0);
		});
	}
}

