using Datalake.Enums;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;

namespace Datalake.Database.V3
{
	public static class Migrator
	{
		public static int Version = 3;

		public static int Migrate(DatabaseContext db)
		{
			// реализация изменений в схеме базы данных
			// нам нужно понять, что есть в базе данных
			// это базовая вещь, которая предполагает создание структуры и в том случае, если это первый запуск

			var provider = db.DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(db);

			// если там нет проверяемой таблицы, просто создаем её
			// если она есть, нужно выполнить миграцию данных

			string tableName = db.Users.TableName;
			string tempName = tableName + "_migrate";
			var old = db.GetTable<V1.User>();

			try
			{
				var records = old.Count();

				// из-за специфики linq2db миграция будет через временную таблицу
				if (schema.Tables.Any(x => x.TableName == tempName)) db.DropTable<V1.User>(tempName);
				var temp = db.CreateTable<V1.User>(tempName);

				// копируем данные в временную таблицу, чтобы не потерять
				var copytoTemp = temp.BulkCopy(old.ToList());
				if (copytoTemp.RowsCopied != records)
				{
					throw new Exception($"Число перенесённых во временную таблицу записей [{copytoTemp.RowsCopied}] не совпало с числом записей в старой таблице [{records}]");
				}

				// удаляем старую версию
				db.DropTable<V1.User>(tableName);

				// создаём новую версию
				var actual = db.CreateTable<User>();

				// выполняем перенос данных из временной таблицы
				var copyToActual = actual.BulkCopy(temp.Select(x => new User
				{
					Name = x.Name,
					FullName = x.FullName,
					Hash = x.Hash,
					AccessType = x.AccessType,
					StaticHost = null,
				}).ToList());

				if (copyToActual.RowsCopied != records)
				{
					throw new Exception($"Число перенесённых в новую таблицу записей [{copyToActual.RowsCopied}] не совпало с числом записей во временной таблице [{records}]");
				}

				// удаляем временную таблицу
				db.DropTable<V1.User>(tableName + "_migrate");

				return Version;
			}
			catch (Exception e)
			{
				db.Log(new Log
				{
					Category = LogCategory.Database,
					Type = LogType.Error,
					Text = $"Ошибка при миграции таблицы {tableName} из v0 в v1",
					Exception = e
				});

				return Version - 1;
			}
		}
	}
}
