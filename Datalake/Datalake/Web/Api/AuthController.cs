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
					.Where(x => x.Name == name)
					.FirstOrDefault();

				if (user == null)
				{
					if (User.AccessType == AccessType.FIRST && db.Users.Count() == 0)
					{
						// Если в базе нет пользователей, создаем администратора и авторизовываем его
						Create(name, password, name, AccessType.ADMIN);

						user = db.Users
							.Where(x => x.Name == auth.Name)
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
				Response.Headers.Add(Headers.LoginHeader, session.Name);
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
		public object Users()
		{
			using (var db = new DatabaseContext())
			{
				var users = db.Users
					.Select(x => new
					{
						x.Name,
						x.FullName,
						x.AccessType,
					})
					.ToList();

				return users;
			}
		}

		[Auth(AccessType.ADMIN, AccessType.FIRST)]
		public object Create(string name, string password, string fullName, AccessType access)
		{
			var auth = new LoginPass { Password = password };

			using (var db = new DatabaseContext())
			{
				if (db.Users.Any(x => x.Name == name)) return Error("Такой пользователь уже существует");

				db.Users
					.Value(x => x.Name, name)
					.Value(x => x.FullName, fullName)
					.Value(x => x.AccessType, access)
					.Value(x => x.Hash, auth.Hash)
					.Insert();

				return Done("Пользователь успешно добавлен");
			}
		}

		[Auth(AccessType.ADMIN, AccessType.FIRST)]
		public object Update(string name, string newName = null, string password = null, string fullName = null, AccessType? access = null)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Users.Any(x => x.Name == name)) return Error("Такой пользователь не существует");

				if (!string.IsNullOrEmpty(newName))
				{
					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.Name, newName)
						.Update();

					name = newName;
				}

				if (!string.IsNullOrEmpty(password))
				{
					var auth = new LoginPass { Password = password };

					db.Users
						.Where(x => x.Name == name)
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
					db.Users
						.Where(x => x.Name == name)
						.Set(x => x.AccessType, access.Value)
						.Update();
				}

				return Done("Пользователь успешно сохранён");
			}
		}

		[Auth(AccessType.ADMIN)]
		public object Delete(LoginPass auth)
		{
			using (var db = new DatabaseContext())
			{
				var user = db.Users
					.Where(x => x.Name == auth.Name)
					.ToList();

				if (user == null) return Error("Такой пользователь не существует");

				db.Users
					.Where(x => x.Name == auth.Name)
					.Delete();

				return Done("Пользователь успешно удалён");
			}
		}
	}
}
