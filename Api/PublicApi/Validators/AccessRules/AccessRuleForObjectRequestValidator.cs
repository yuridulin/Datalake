using Datalake.PublicApi.Models.AccessRules;
using FluentValidation;

namespace Datalake.PublicApi.Validators.AccessRules;

/// <summary>
/// Валидатор для <see cref="AccessRuleForObjectRequest"/>
/// </summary>
public class AccessRuleForObjectRequestValidator : AbstractValidator<AccessRuleForObjectRequest>
{
	/// <summary>
	/// Валидация
	/// </summary>
	public AccessRuleForObjectRequestValidator()
	{
		RuleFor(x => x.AccessType)
			.IsInEnum()
			.WithMessage("Тип доступа должен быть допустимым значением");

		// Должен быть указан ровно один актор
		RuleFor(x => x)
			.Must(x => new[] { x.UserGuid, x.UserGroupGuid }.Count(id => id.HasValue) == 1)
			.WithMessage("Должен быть указан ровно один актор (UserGuid или UserGroupGuid)");

		// Валидация GUID'ов
		When(x => x.UserGuid.HasValue, () =>
		{
			RuleFor(x => x.UserGuid).NotEqual(Guid.Empty);
		});

		When(x => x.UserGroupGuid.HasValue, () =>
		{
			RuleFor(x => x.UserGroupGuid).NotEqual(Guid.Empty);
		});
	}
}

