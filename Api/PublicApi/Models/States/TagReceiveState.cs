namespace Datalake.PublicApi.Models.States;

/// <summary>
/// Состояние работы
/// </summary>
/// <param name="Date">Дата последней записи</param>
/// <param name="Message">Сообщение о последней ошибке, если есть</param>
public record TagReceiveState(
	DateTime Date,
	string? Message);
