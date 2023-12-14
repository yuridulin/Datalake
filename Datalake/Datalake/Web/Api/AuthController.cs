using Datalake.Database;
using Datalake.Enums;
using Datalake.Web.Attributes;
using Datalake.Web.Models;
using LinqToDB;
using System;
using System.Linq;

namespace Datalake.Web.Api
{
	public class AuthController : Controller
	{
		public object Login(string name, string password)
		{
			using (var db = new DatabaseContext())
			{
				var auth = new LoginPass { Password = password };

				var user = db.Users
					.Where(x => x.Name == name && x.StaticHost == null)
					.FirstOrDefault();

				if (user == null)
				{
					if (User.AccessType == AccessType.FIRST && db.Users.Where(x => x.StaticHost == null).Count() == 0)
					{
						// Если в базе нет пользователей, создаем администратора и авторизовываем его
						Create(name, name, (int)AccessType.ADMIN, null, password);

						user = db.Users
							.Where(x => x.Name == auth.Name && x.StaticHost == null)
							.FirstOrDefault();

						if (user == null) return Error("Ошибка при создании первого пользователя");
					}
					else
					{
						return Error("Указанная учетная запись не найдена");
					}
				}

				if (user.Hash != auth.Hash)
				{
					return Error("Введенный пароль не подходит");
				}

				// Создаём сессию для авторизованного пользователя
				var session = new UserSession
				{
					Name = user.Name,
					Token = new Random().Next().ToString(),
					AccessType = user.AccessType,
					Expire = DateTime.Now.AddDays(7), // срок жизни сессии
				};

				lock (Server.Sessions)
				{
					Server.Sessions.Add(session);
				}

				// Отдаём информацию о новой сессии
				Response.Headers.Add(Headers.NameHeader, session.Name);
				Response.Headers.Add(Headers.TokenHeader, session.Token);
				Response.Headers.Add(Headers.AccessHeader, ((int)session.AccessType).ToString());
				return Done("Вход успешно выполнен");
			}
		}

		public object Logout(string token)
		{
			lock (Server.Sessions)
			{
				var old = Server.Sessions.Where(x => x.Token == token).FirstOrDefault();
				if (old != null)
				{
					Server.Sessions.Remove(old);
				}
			}

			return Done("Выход произведён успешно");
		}

		[Auth(AccessType.ADMIN)]
		public object UserInfo(string name)
		{
			using (var db = new DatabaseContext())
			{
				var user = db.Users
					.Select(x => new
					{
						x.Name,
						x.FullName,
						x.StaticHost,
						x.Hash,
						AccessType = (int)x.AccessType,
					})
					.FirstOrDefault(x => x.Name == name);

				if (user == null) return Error("Пользователь не найден");

				return Data(user);
			}
		}

		[Auth(AccessType.ADMIN)]
		public object Users()
		{
			using (var db = new DatabaseContext())
			{
				var users = db.Users
					.Select(x => new
					{
						x.Name,
						x.FullName,
						x.StaticHost,
						AccessType = (int)x.AccessType,
					})
					.ToList();

				return Data(users);
			}
		}

		[Auth(AccessType.ADMIN, AccessType.FIRST)]
		public object Create(string name, string fullName, int accessType, string staticHost = null, string password = null)
		{
			using (var db = new DatabaseContext())
			{
				string hash;

				if (!string.IsNullOrEmpty(staticHost))
				{
					var oldHashList = db.Users
						.Where(x => x.StaticHost != null)
						.Select(x => x.Hash)
						.ToList();

					do
					{
						hash = LoginPass.RandomHash();
					}
					while (oldHashList.Any(x => x == hash));
				}
				else if (!string.IsNullOrEmpty(staticHost))
				{
					hash = new LoginPass { Password = password }.Hash;
				}
				else
				{
					return Error("Не предоставлено достаточно данных. Нужно передать или пароль, или адрес сторонней службы");
				}

				if (db.Users.Any(x => x.Name == name)) return Error("Такой пользователь уже существует");

				db.Users
					.Value(x => x.Name, name)
					.Value(x => x.FullName, fullName)
					.Value(x => x.AccessType, (AccessType)accessType)
					.Value(x => x.Hash, hash)
					.Value(x => x.StaticHost, staticHost)
					.Insert();

				return Done("Пользователь успешно добавлен");
			}
		}

		[Auth(AccessType.ADMIN, AccessType.FIRST)]
		public object Update(string name, string newName = null, string staticHost = null, bool newHash = false, string password = null, string fullName = null, AccessType? access = null)
		{
			using (var db = new DatabaseContext())
			{
				var user = db.Users.FirstOrDefault(x => x.Name == name);
				if (user == null) return Error("Такой пользователь не существует");

				if (!string.IsNullOrEmpty(newName))
				{
					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.Name, newName)
						.Update();

					name = newName;
				}

				if (newHash)
				{
					var oldHashList = db.Users
						.Where(x => x.StaticHost != null)
						.Select(x => x.Hash)
						.ToList();

					string hash;

					do
					{
						hash = LoginPass.RandomHash();
					}
					while (oldHashList.Any(x => x == hash));

					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.Hash, hash)
						.Update();
				}

				if (!string.IsNullOrEmpty(staticHost))
				{
					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.StaticHost, staticHost)
						.Set(x => x.Hash, string.IsNullOrEmpty(user.StaticHost) ? LoginPass.RandomHash() : user.Hash)
						.Update();
				}
				else if (!string.IsNullOrEmpty(password))
				{
					var auth = new LoginPass { Password = password };

					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.StaticHost, null as string)
						.Set(x => x.Hash, auth.Hash)
						.Update();
				}

				if (!string.IsNullOrEmpty(fullName))
				{
					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.FullName, fullName)
						.Update();
				}

				if (access != null)
				{
					if (user.AccessType == AccessType.ADMIN && access.Value != AccessType.ADMIN && db.Users.Where(x => x.AccessType == AccessType.ADMIN).Count() == 1)
					{
						return Error("Попытка удалить последнего администратора");
					}

					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.AccessType, access.Value)
						.Update();
				}

				return Done("Пользователь успешно изменён");
			}
		}

		[Auth(AccessType.ADMIN)]
		public object Delete(string name)
		{
			using (var db = new DatabaseContext())
			{
				var user = db.Users
					.Where(x => x.Name == name)
					.FirstOrDefault();

				if (user == null) return Error("Такой пользователь не существует");

				if (user.AccessType == AccessType.ADMIN && db.Users.Count(x => x.AccessType == AccessType.ADMIN) == 1)
					return Error("Попытка удалить последнего администратора");

				if (db.Users.Count() == 1)
					return Error("Попытка удалить последнюю учётную запись");

				db.Users
					.Where(x => x.Name == name)
					.Delete();

				Server.Sessions
					.RemoveAll(x => x.Name == name);

				return Done("Пользователь успешно удалён");
			}
		}
	}
}
