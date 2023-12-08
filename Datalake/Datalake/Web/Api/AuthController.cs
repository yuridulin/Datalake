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
		public object Login(LoginPass auth)
		{
			using (var db = new DatabaseContext())
			{
				var user = db.Users
					.Where(x => x.Name == auth.Name)
					.FirstOrDefault();

				if (user == null)
				{
					if (User.AccessType == AccessType.FIRST && db.Users.Count() == 0)
					{
						// Если в базе нет пользователей, создаем администратора и авторизовываем его
						Create(auth);

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
						x.Name, x.AccessType,
					})
					.ToList();

				return users;
			}
		}

		[Auth(AccessType.ADMIN, AccessType.FIRST)]
		public object Create(LoginPass auth)
		{
			var user = new User
			{
				Name = auth.Name,
				AccessType = User.AccessType == AccessType.FIRST ? AccessType.ADMIN : auth.AccessType,
				Hash = auth.Hash,
			};

			using (var db = new DatabaseContext())
			{
				if (db.Users.Any(x => x.Name == user.Name)) return Error("Такой пользователь уже существует");

				db.Users
					.Value(x => x.Name, user.Name)
					.Value(x => x.AccessType, user.AccessType)
					.Value(x => x.Hash, user.Hash)
					.Insert();

				return Done("Пользователь успешно добавлен");
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
