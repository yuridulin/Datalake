using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Datalake.Inventory.Infrastructure.Database.Initialization;

[Singleton]
public class DomainStartService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<DomainStartService> logger) : IDomainStartService
{
	public async Task StartAsync()
	{
		logger.LogInformation("Настройка домена запущена");

		// запись необходимых источников в список
		var internalSources = Source.InternalSources
			.Select(x => Source.CreateAsInternal(type: x, name: x.ToString(), description: GetDescription(x)))
			.ToArray();

		using var scope = serviceScopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
		var sourcesRepository = scope.ServiceProvider.GetRequiredService<ISourcesRepository>();
		var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
		var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
		var accessRulesRepository = scope.ServiceProvider.GetRequiredService<IAccessRulesRepository>();
		var auditRepository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();

		await unitOfWork.BeginTransactionAsync();

		try
		{
			var existSources = await sourcesRepository.GetAllAsync();

			foreach (var internalSource in internalSources)
			{
				if (existSources.Any(x => x.Type == internalSource.Type))
					continue;

				await sourcesRepository.AddAsync(internalSource);

				logger.LogDebug("Добавлен отсутствующий внутренний источник данных: {type}", internalSource.Type.ToString());
			}

			var existSettings = await settingsRepository.GetAsync();
			if (existSettings == null)
			{
				var setting = new Settings(string.Empty, string.Empty, string.Empty, string.Empty);
				await settingsRepository.AddAsync(setting);

				logger.LogDebug("Добавлены отсутствующие настройки");
			}

			var defaultAdminUser = await usersRepository.GetByLoginAsync("admin");
			if (defaultAdminUser == null)
			{
				defaultAdminUser = User.CreateFromLoginPassword("admin", "admin");
				await usersRepository.AddAsync(defaultAdminUser);

				var defaultAdminGlobalRule = new AccessRule(AccessType.Admin, defaultAdminUser.Guid);
				await accessRulesRepository.AddAsync(defaultAdminGlobalRule);

				logger.LogDebug("Добавлен отсутствующий администратор по умолчанию");
			}

			var auditLog = new Log(LogCategory.Core, $"Сервис {nameof(Inventory)} запущен", null);
			await auditRepository.AddAsync(auditLog);

			await unitOfWork.SaveChangesAsync();
			await unitOfWork.CommitAsync();

			logger.LogInformation("Настройка домена завершена");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Настройка домена не завершена!");
			await unitOfWork.RollbackAsync();
			throw;
		}
	}

	/// <summary>
	/// Получение описания значения enum
	/// </summary>
	/// <param name="enumValue">Значение</param>
	/// <returns>Описание, записанное в атрибуте Description</returns>
	static string GetDescription(Enum enumValue)
	{
		var field = enumValue.GetType().GetField(enumValue.ToString());
		if (field == null)
			return enumValue.ToString();

		if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
			return attribute.Description;

		return enumValue.ToString();
	}
}
