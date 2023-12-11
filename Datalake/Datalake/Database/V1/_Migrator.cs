using Datalake.Enums;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;

namespace Datalake.Database.V1
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

			if (!schema.Tables.Any(t => t.TableName == db.Tags.TableName))
			{
				db.CreateTable<Tag>();
			}
			else
			{
				db.Tags.Where(x => x.IsCalculating).Set(x => x.SourceId, CustomSourcesIdentity.Calculated).Update();
				db.Tags.Where(x => x.Name == "INSERTING").Delete();
			}

			if (!schema.Tables.Any(t => t.TableName == db.Logs.TableName))
			{
				db.CreateTable<Log>();
			}

			db.Log(new Log
			{
				Category = LogCategory.Core,
				Type = LogType.Information,
				Text = $"Запуск",
			});

			if (!schema.Tables.Any(t => t.TableName == db.Settings.TableName))
			{
				db.CreateTable<Settings>();
			}

			if (!schema.Tables.Any(t => t.TableName == db.Users.TableName))
			{
				db.CreateTable<User>();
			}
			else
			{
				try
				{
					var old = db.Users;
					var records = old.Count();

					// из-за специфики linq2db миграция будет через временную таблицу
					var temp = db.CreateTable<V0.User>("Users_migrate");

					// копируем данные в временную таблицу, чтобы не потерять
					var copytoTemp = temp.BulkCopy(db.Users);
					if (copytoTemp.RowsCopied != records)
					{
						throw new Exception($"Число перенесённых во временную таблицу записей [{copytoTemp.RowsCopied}] не совпало с числом записей в старой таблице [{records}]");
					}

					// удаляем старую версию
					db.DropTable<V0.User>("Users");

					// создаём новую версию
					var actual = db.CreateTable<User>();

					// выполняем перенос данных из временной таблицы
					var copyToActual = actual.BulkCopy(temp
						.Select(x => new User
						{
							Name = x.Name,
							AccessType = x.AccessType,
							FullName = x.Name,
							Hash = x.Hash,
						})
						.ToList());
					if (copyToActual.RowsCopied != records)
					{
						throw new Exception($"Число перенесённых в новую таблицу записей [{copyToActual.RowsCopied}] не совпало с числом записей во временной таблице [{records}]");
					}

					// удаляем временную таблицу
					db.DropTable<V0.User>("Users_migrate");
				}
				catch (Exception e)
				{
					db.Log(new Log
					{
						Category = LogCategory.Database,
						Type = LogType.Error,
						Text = "Ошибка при миграции таблицы Users из v0 в v1",
						Exception = e
					});
				}
			}

			if (!schema.Tables.Any(t => t.TableName == db.Sources.TableName))
			{
				db.CreateTable<Source>();
			}

			if (!schema.Tables.Any(t => t.TableName == db.Blocks.TableName))
			{
				db.CreateTable<Block>();
			}

			if (!schema.Tables.Any(t => t.TableName == db.Rel_Tag_Input.TableName))
			{
				db.CreateTable<Rel_Tag_Input>();
			}

			if (!schema.Tables.Any(t => t.TableName == db.Rel_Block_Tag.TableName))
			{
				db.CreateTable<Rel_Block_Tag>();
			}

			return 1;
		}
	}
}
