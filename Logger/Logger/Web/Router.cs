using Logger.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Logger.Web
{
	public class Router
	{
		HttpListenerRequest Request { get; set; }

		HttpListenerResponse Response { get; set; }

		public void Resolve(HttpListenerContext context)
		{
			Request = context.Request;
			Response = context.Response;
			Answer res;

			var url = Request.Url.LocalPath.Substring(1);
			if (string.IsNullOrEmpty(url)) url = "index.html";

			if (!url.StartsWith("api"))
			{
				// Запрошен статический документ
				res = Document(url);
			}
			else
			{
				// Тело запроса, переданное через POST
				string requestBody = Request.HasEntityBody
					? new StreamReader(Request.InputStream).ReadToEnd()
					: "";

				// NTLM идентификация
				var user = context.User as WindowsPrincipal;
			
				try
				{
					res = Action(url.Replace("api/", ""), requestBody, user);
				}
				catch (Exception e)
				{
					res = new Answer
					{
						StatusCode = 200,
						ContentType = "application/json",
						String = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace })
					};
				}
			}

			#if DEBUG
			Response.AddHeader("Access-Control-Allow-Origin", "*");
			Response.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
			Response.AddHeader("Access-Control-Allow-Headers", "x-requested-with, Content-Type, origin, authorization, accept, x-access-token");
			#endif

			// Отправка ответа клиенту
			Response.StatusCode = res.StatusCode;
			Response.ContentType = res.ContentType;
			Response.ContentLength64 = res.Bytes.Length;
			using (var output = Response.OutputStream)
			{
				output.Write(res.Bytes, 0, res.Bytes.Length);
				output.Close();
			}
		}

		Answer Action(string methodName, string body, WindowsPrincipal user)
		{
			Console.WriteLine($"[HTTP API] {methodName}");

			if (Request.HttpMethod == "OPTIONS")
			{
				return new Answer
				{
					StatusCode = 200,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { })
				};
			}

			var methodParts = methodName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

			var controller = Type.GetType($"{nameof(Logger)}.{nameof(Web)}.{nameof(Api)}.{methodParts[0]}Controller", false, true);
			if (controller == null)
			{
				return new Answer
				{
					StatusCode = 501,
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
					StatusCode = 501,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { Error = "Метод " + methodParts[1] + " не найден" })
				};
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

					var invokeParams = new List<object>();

					foreach (var p in methodParams)
					{
						if (jsonParams.ContainsKey(p.Name.ToLower()))
						{
							var jsonParam = jsonParams[p.Name.ToLower()];
							var jsonType = jsonParam.GetType();

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
									var param = Convert.ChangeType(jsonParam, p.ParameterType);
									invokeParams.Add(param);
								}
							}
							catch
							{
								Console.WriteLine("Не получилось определить JSON тип: " + jsonType);
							}
						}
					}

					result = action.Invoke(instance, invokeParams.ToArray());
				}
				else
				{
					result = action.Invoke(instance, null);
				}
			}
			else
			{
				result = action.Invoke(instance, null);
			}

			if (attrib != null)
			{
				result = ((Task<object>)result).Result;
			}

			return new Answer
			{
				StatusCode = 200,
				ContentType = "application/json",
				String = JsonConvert.SerializeObject(result),
			};
		}

		Answer Document(string filename)
		{
			Console.WriteLine($"[HTTP DOC] {filename}");

			string basePath = AppDomain.CurrentDomain.BaseDirectory + "\\Web\\Content\\";
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
					StatusCode = 200,
					ContentType = type,
					Bytes = result
				};
			}
			else
			{
				return new Answer
				{
					StatusCode = 404,
				};
			}
		}
	}
}