using Microsoft.EntityFrameworkCore;

namespace DatalakeDb.Models
{
	/*
	 * Логика этой штуки такова и более никакова.
	 * Это связь между объектом (сущностью) и локальной версией тега
	 * Если объект привнесён извне, такая связь должна генерироваться на основе json, где будут прописаны uuid объекта и связанных тегов
	 * Можно сделать всё через uuid? Да, как вариант. Продумать о необходимости. Для импорта/экспорта можно использовать uuid вариант, в быту - обычные числовые id
	 */
	[Keyless]
	public class EntityTag
	{
		public int EntityId { get; set; } = 0;

		public int TagId { get; set; } = 0;

		public string Name { get; set; } = string.Empty;
	}
}
