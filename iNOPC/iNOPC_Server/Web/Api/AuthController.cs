using iNOPC.Server.Models.Configurations;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace iNOPC.Server.Web.Api
{
	public class AuthController : Controller
	{
		public object Login(LoginPass user)
		{
			var auth = Program.Configuration.Access
				.Where(x => x.Login == user.Login)
				.FirstOrDefault();

			if (auth == null)
			{
				return new { Warning = "Указанная учетная запись не найдена" };
			}
			else if (auth.Hash != user.Hash)
			{
				return new { Warning = "Введенный пароль не подходит" };
			}
			else
			{
				var session = new Session
				{
					Login = auth.Login,
					Token = new Random().Next().ToString(),
					AccessType = auth.AccessType,
					Expire = DateTime.Now.AddDays(7),
				};

				lock (Http.Sessions)
				{
					Http.Sessions.Add(session);
				}

				// Записываем в куки клиента его сессионный токен
				Response.Headers.Add("Inopc-Access-Token", session.Token);

				// Авторизовываем, чтобы после перезагрузки вебки клиент сразу вошел
				// честно говоря лишняя деталь на случай, когда вместо полного релоада будут перегружаться конкретные компоненты вебки
				//response.Headers.Add("Inopc-Login", user.Login);
				//response.Headers.Add("Inopc-Access-Type", ((int)session.AccessType).ToString());

				return new { Done = "Вход успешно выполнен" };
			}
		}

		public object Logout(Session session)
		{
			lock (Http.Sessions)
			{
				var old = Http.Sessions.Where(x => x.Token == session.Token).FirstOrDefault();
				if (old != null)
				{
					Http.Sessions.Remove(old);
				}
			}

			return new { Done = "Выход произведён успешно" };
		}

		public object Users()
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration)
			{
				return Program.Configuration.Access
					.Select(x => new
					{
						x.Login,
						x.AccessType,
					});
			}
		}

		public object Create(LoginPass user)
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.FIRST)
			{
				return new { Warning = "Нет доступа" };
			}

			var access = new AccessRecord
			{
				Login = user.Login,
				AccessType = user.AccessType,
				Hash = user.Hash,
			};

			lock (Program.Configuration)
			{
				var another = Program.Configuration.Access
					.FirstOrDefault(x => x.Login == access.Login);

				if (another != null) return new { Warning = "Такой пользователь уже существует" };

				Program.Configuration.Access.Add(access);
				Program.Configuration.SaveToFile();
			}

			return new { Done = "Пользователь успешно добавлен" };
		}

		public object Delete(LoginPass user)
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration)
			{
				var access = Program.Configuration.Access
					.FirstOrDefault(x => x.Login == user.Login);

				if (access == null) return new { Warning = "Такой пользователь не существует" };

				Program.Configuration.Access.Remove(access);
				Program.Configuration.SaveToFile();
			}

			return new { Done = "Пользователь успешно удалён" };
		}
	}
}
