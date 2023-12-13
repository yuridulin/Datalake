using Datalake.Database;
using Datalake.Enums;
using Datalake.Web.Attributes;
using Datalake.Web.Models;
using LinqToDB.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Datalake.Web
{
	public class Router
	{
		HttpListenerRequest Request { get; set; }

		HttpListenerResponse Response { get; set; }

		public void Resolve(HttpListenerContext context)
		{
			Request = context.Request;
			Response = context.Response;
			var headers = new Dictionary<string, string>();
			Answer res;

			var url = Request.Url.LocalPath.Substring(1);
			if (string.IsNullOrEmpty(url)) url = "index.html";

			if (!url.StartsWith("api"))
			{
				// Запрошен статический документ
				res = Document(url);

				#if DEBUG
				Debug.WriteLine($"[HTTP] DOC {url} - {res.StatusCode}");
				#endif
			}
			else
			{
				// Заголовки, передающие информацию о авторизации
				#if DEBUG
				Response.Headers.Add("Access-Control-Allow-Origin", "*");
				Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
				Response.Headers.Add("Access-Control-Allow-Headers", "x-requested-with, Content-Type, origin, authorization, accept, x-access-token" 
					+ $", {Headers.LoginHeader}, {Headers.AccessHeader}, {Headers.TokenHeader}");
				Response.Headers.Add("Access-Control-Expose-Headers", $"{Headers.LoginHeader}, {Headers.AccessHeader}, {Headers.TokenHeader}");
				#endif

				// Тело запроса, переданное через POST
				string requestBody = Request.HasEntityBody
					? new StreamReader(Request.InputStream).ReadToEnd()
					: "";

				User user = new User
				{
					Name = Request.RemoteEndPoint.Address.ToString(),
					AccessType = AccessType.NOT,
				};

				try
				{
					// авторизация запроса по токену, хранящемуся в куках. Если запрос - аутентификация, то обработчик перезапишет это значение
					var token = Request.Headers.Get(Headers.TokenHeader);
					headers.Add(Headers.TokenHeader, token);

					var session = Server.Sessions.FirstOrDefault(x => token != null && x.Token == token && x.Expire > DateTime.Now);
					if (session != null)
					{
						// Пользователь авторизован по сессионному токену
						user.Name = session.Name;
						user.AccessType = session.AccessType;
					}
					else
					{
						// Пользователь не авторизован по сессионному токену
						List<User> users;

						using (var db = new DatabaseContext())
						{
							users = db.Users.ToList();
						}

						if (users.Count == 0)
						{
							// Ни одна учётная запись не создана, выдается специальное разрешение FIRST для создания первой учётной записи
							user.Name = AccessType.FIRST.ToString();
							user.AccessType = AccessType.FIRST;
						}
						else
						{
							// Пользователь не авторизован
							user.AccessType = AccessType.NOT;
						}
					}

					headers.Add(Headers.LoginHeader, user.Name);
					headers.Add(Headers.AccessHeader, ((int)user.AccessType).ToString());

					res = Action(url.Replace("api/", ""), requestBody, user);
					
					#if DEBUG
					Debug.WriteLine($"[HTTP] API {url} by {user.Name} as {user.AccessType} - {res.StatusCode}");
					#endif
				}
				catch (Exception e)
				{
					res = new Answer
					{
						StatusCode = HttpStatusCode.InternalServerError,
						ContentType = "application/json",
						String = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace })
					};

					#if DEBUG
					Debug.WriteLine($"[HTTP] API {url} by {user.Name} as {user.AccessType} - {res.StatusCode}\nERROR:\n{e.Message}\n{e.StackTrace}");
					#endif
				}
			}

			foreach (var h in headers)
			{
				if (Response.Headers.GetValues(h.Key) == null) Response.Headers.Add(h.Key, h.Value);
			}

			// Отправка ответа клиенту
			Response.StatusCode = (int)res.StatusCode;
			Response.ContentType = res.ContentType;
			Response.ContentLength64 = res.Bytes.Length;
			using (var output = Response.OutputStream)
			{
				output.Write(res.Bytes, 0, res.Bytes.Length);
				output.Close();
			}
		}

		Answer Action(string methodName, string body, User user)
		{
			if (Request.HttpMethod == "OPTIONS")
			{
				return new Answer
				{
					StatusCode = HttpStatusCode.OK,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { })
				};
			}

			var methodParts = methodName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

			var controller = Type.GetType($"{nameof(Datalake)}.{nameof(Web)}.{nameof(Api)}.{methodParts[0]}Controller", false, true);
			if (controller == null)
			{
				return new Answer
				{
					StatusCode = HttpStatusCode.NotImplemented,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { Error = "Контроллер " + methodParts[0] + " не найден" })
				};
			}

			var instance = (Controller)Activator.CreateInstance(controller);
			instance.Request = Request;
			instance.Response = Response;
			instance.User = user;

			var action = controller.GetMethod(methodParts[1], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (action == null)
			{
				return new Answer
				{
					StatusCode = HttpStatusCode.NotImplemented,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { Error = "Метод " + methodParts[1] + " не найден" })
				};
			}

			var auth = (AuthAttribute)action.GetCustomAttribute(typeof(AuthAttribute), false);
			if (auth != null)
			{
				if (!auth.Types.Contains(user.AccessType))
				{
					return new Answer
					{
						StatusCode = HttpStatusCode.Forbidden,
						ContentType = "application/json",
						String = JsonConvert.SerializeObject(new { Error = "Нет доступа" })
					};
				}
			}

			var attType = typeof(AsyncStateMachineAttribute);
			var attrib = (AsyncStateMachineAttribute)action.GetCustomAttribute(attType);

			object result;
			var methodParams = action.GetParameters();

			if (methodParams.Length > 0)
			{
				var jsonParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

				if (jsonParams != null)
				{
					jsonParams = jsonParams
						.ToList()
						.Select(x => new { Key = x.Key.ToLower(), x.Value })
						.ToDictionary(x => x.Key, x => x.Value);
				}
				else
				{
					jsonParams = new Dictionary<string, object>();
				}

				var invokeParams = new List<object>();
				var missedParams = new Dictionary<string, Type>();

				foreach (var p in methodParams)
				{
					var paramType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;

					if (jsonParams.ContainsKey(p.Name.ToLower()))
					{
						var jsonParam = jsonParams[p.Name.ToLower()];
						if (jsonParam != null)
						{
							var jsonType = Nullable.GetUnderlyingType(jsonParam.GetType()) ?? jsonParam.GetType();

							try
							{
								if (jsonType == typeof(JObject))
								{
									var param = (jsonParam as JObject).ToObject(p.ParameterType);
									invokeParams.Add(param);
								}
								else if (jsonType == typeof(JArray))
								{
									var param = (jsonParam as JArray).ToObject(p.ParameterType);
									invokeParams.Add(param);
								}
								else
								{

									var param = Convert.ChangeType(jsonParam, paramType);
									invokeParams.Add(param);
								}
							}
							catch
							{
								Console.WriteLine("Не получилось определить JSON тип: " + jsonType);
								missedParams.Add(p.Name, paramType);
							}
						}
						else if (paramType.IsNullable())
						{
							invokeParams.Add(null);
						}
					}
					else if (p.DefaultValue != DBNull.Value)
					{
						invokeParams.Add(p.DefaultValue);
					}
					else
					{
						missedParams.Add(p.Name, paramType);
					}
				}

				if (missedParams.Count > 0)
				{
					string err = "\"Ожидаются, но не переданы параметры: ";

					foreach (var kv in missedParams)
					{
						err += "\n" + kv.Key + " : " + kv.Value;
					}

					throw new Exception(err);
				}
				else
				{
					result = action.Invoke(instance, invokeParams.ToArray());
				}
			}
			else
			{
				result = (Result)action.Invoke(instance, new object[0]);
			}

			if (attrib != null)
			{
				result = ((Task<object>)result).Result;
			}

			Result answer = (Result)result;

			return new Answer
			{
				StatusCode = answer.StatusCode,
				ContentType = "application/json",
				String = answer.ToJson(),
			};
		}

		Answer Document(string filename)
		{
			#if DEBUG
			string message = $"[HTTP] DOC: {filename}";
			#endif

			string basePath = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\";
			string filePath = (basePath + filename.Replace("/", "\\")).Replace(@"\\", "\\");
			string type = "text/html";

			if (filePath[filePath.Length - 1] == '\\')
			{
				filePath = filePath.Substring(0, filePath.Length - 1);
			}

			if (filePath.LastIndexOf('.') > -1)
			{
				string fileType = filePath.Substring(filePath.LastIndexOf('.') + 1);
				switch (fileType)
				{
					case "png":
						type = "image/png";
						break;
					case "ico":
						type = "image/x-icon";
						break;
					case "css":
						type = "text/css";
						break;
					case "js":
						type = "application/javascript";
						break;
					case "ttf":
						type = "font/ttf";
						break;
				}
			}
			else
			{
				filePath = basePath + "index.html";
			}

			if (File.Exists(filePath))
			{
				var result = File.ReadAllBytes(filePath);

				return new Answer
				{
					StatusCode = HttpStatusCode.OK,
					ContentType = type,
					Bytes = result
				};
			}
			else
			{
				return new Answer
				{
					StatusCode = HttpStatusCode.NotFound,
				};
			}
		}
	}
}