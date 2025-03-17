using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo : TagSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Item { get; set; }

	/// <summary>
	/// Формула, на основе которой вычисляется значение
	/// </summary>
	public string? Formula { get; set; } = string.Empty;

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public required TagInputMinimalInfo[] FormulaInputs { get; set; } = [];

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;

	/// <summary>
	/// Минимальная информация о переменных для расчета значений по формуле
	/// </summary>
	public class TagInputMinimalInfo
	{
		/// <summary>
		/// Идентификатор входного тега
		/// </summary>
		public required int InputTagId { get; set; }

		/// <summary>
		/// Имя переменной
		/// </summary>
		public required string VariableName { get; set; }
	}
}
