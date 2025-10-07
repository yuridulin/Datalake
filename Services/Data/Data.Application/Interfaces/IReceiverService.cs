using Datalake.Data.Application.Models;

namespace Datalake.Data.Application.Interfaces;

/// <summary>
/// Служба получения данных из источников по сети
/// </summary>
public interface IReceiverService
{
	/// <summary>
	/// Запрос данных из сервера INOPC
	/// </summary>
	/// <param name="tags">Список имен запрашиваемых тегов</param>
	/// <param name="address">Адрес сервера</param>
	/// <returns>Ответ с данными</returns>
	Task<RemoteResponseDto> AskInopc(string[] tags, string address);
}