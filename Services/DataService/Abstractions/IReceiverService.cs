using Datalake.DataService.Services.Receiver.Models;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Служба получения данных из источников по сети
/// </summary>
/// <param name="logger">Служба сообщений</param>
public interface IReceiverService
{
	/// <summary>
	/// Запрос данных из ноды Datalake
	/// </summary>
	/// <param name="tags">Список названий запрашиваемых тегов</param>
	/// <param name="address">Адрес ноды</param>
	/// <returns>Ответ с данными</returns>
	Task<ReceiveResponse> AskDatalake(Guid[] tags, string address);

	/// <summary>
	/// Запрос данных из сервера INOPC
	/// </summary>
	/// <param name="tags">Список имен запрашиваемых тегов</param>
	/// <param name="address">Адрес сервера</param>
	/// <returns>Ответ с данными</returns>
	Task<ReceiveResponse> AskInopc(string[] tags, string address);
}