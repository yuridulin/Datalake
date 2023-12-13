using Datalake.Enums;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;

namespace Datalake.Database.V2
{
	public static class Migrator
	{
		public static int Migrate(DatabaseContext db)
		{
			// реализация изменений в схеме базы данных
			// нам нужно понять, что есть в базе данных
			// это базовая вещь, которая предполагает создание структуры и в том случае, если это первый запуск

			var provider = db.DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(db);

			// если там нет проверяемой таблицы, просто создаем её
			// если она есть, нужно выполнить миграцию данных

			string tableName = "Logs";
			string tempName = tableName + "_migrate";
			var old = db.GetTable<V0.Log>();

			try
			{
				var records = old.Count();

				// из-за специфики linq2db миграция будет через временную таблицу
				if (schema.Tables.Any(x => x.TableName == tempName)) db.DropTable<V0.Log>(tempName);
				var temp = db.CreateTable<V0.Log>(tempName);

				// копируем данные в временную таблицу, чтобы не потерять
				var copytoTemp = temp.BulkCopy(old.ToList());
				if (copytoTemp.RowsCopied != records)
				{
					throw new Exception($"Число перенесённых во временную таблицу записей [{copytoTemp.RowsCopied}] не совпало с числом записей в старой таблице [{records}]");
				}

				// удаляем старую версию
				db.DropTable<V0.Log>(tableName);

				// создаём новую версию
				var actual = db.CreateTable<Log>();

				// выполняем перенос данных из временной таблицы
				var copyToActual = actual.BulkCopy(temp.Select(x => new Log
				{
					Category = x.Category,
					Date = x.Date,
					Details = x.Details,
					Ref = x.Ref,
					Text = x.Text,
					Type = x.Type,
					User = null,
				}).ToList());

				if (copyToActual.RowsCopied != records)
				{
					throw new Exception($"Число перенесённых в новую таблицу записей [{copyToActual.RowsCopied}] не совпало с числом записей во временной таблице [{records}]");
				}

				// удаляем временную таблицу
				db.DropTable<V0.Log>(tableName + "_migrate");

				return 2;
			}
			catch (Exception e)
			{
				db.Log(new Database.Log
				{
					Category = LogCategory.Database,
					Type = LogType.Error,
					Text = $"Ошибка при миграции таблицы {tableName} из v0 в v1",
					Exception = e
				});

				return 1;
			}
		}
	}
}
