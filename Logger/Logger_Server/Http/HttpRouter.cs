using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Logger_Server.Http
{
	public class HttpRouter
	{
		HttpListenerRequest Request { get; set; }

		HttpListenerResponse Response { get; set; }

		public void Resolve(HttpListenerContext context)
		{
			Request = context.Request;
			Response = context.Response;

			HttpResponse res;

			try
			{
				var url = Request.Url.LocalPath.Substring(1);
				if (string.IsNullOrEmpty(url)) url = "index.html";

				if (url.StartsWith("api"))
				{
					string requestBody = "";
					if (Request.InputStream != Stream.Null)
					{
						using (var reader = new StreamReader(Request.InputStream, Request.ContentEncoding))
						{
							requestBody = reader.ReadToEnd();
						}
					}

					res = Action(url.Replace("api", ""), requestBody);
				}
				else
				{
					res = Document(url);
				}
			}
			catch (Exception e)
			{
				res = new HttpResponse
				{
					StatusCode = 501,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace })
				};
			}

			Response.StatusCode = res.StatusCode;
			Response.ContentType = res.ContentType;
			Response.ContentLength64 = res.Bytes.Length;
			using (var output = Response.OutputStream)
			{
				output.Write(res.Bytes, 0, res.Bytes.Length);
				output.Close();
			}
		}

		HttpResponse Action(string methodName, string body)
		{
			Console.WriteLine("Action: " + methodName);

			try
			{
				object result = new { Error = "Запрошенный метод не существует", StackTrace = "Http.HttpRouter.Action" };

				var methodParts = methodName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

				var controller = Type.GetType("Logger_Server.Http.Api." + methodParts[ 0 ], false, true);
				var action = controller.GetMethod(methodParts[ 1 ], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

				if (action != null)
				{
					var parameters = action.GetParameters();
					if (parameters.Length > 0)
					{
						var type = parameters[ 0 ].ParameterType;
						var obj = JsonConvert.DeserializeObject(body, type);
						result = action.Invoke(null, new object[] { obj });
					}
					else
					{
						result = action.Invoke(null, null);
					}

					return new HttpResponse
					{
						StatusCode = 200,
						ContentType = "application/json",
						String = JsonConvert.SerializeObject(result),
					};
				}
				else
				{
					return new HttpResponse
					{
						StatusCode = 501,
						ContentType = "application/json",
						String = JsonConvert.SerializeObject(new { Error = "Метод " + methodParts[ 1 ] + " не найден" }),
					};
				}
			}
			catch (Exception e)
			{
				return new HttpResponse
				{
					StatusCode = 501,
					ContentType = "application/json",
					String = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace }),
				};
			}
		}

		HttpResponse Document(string filename)
		{
			string type = "text/html";
			string filePath = (Environment.CurrentDirectory + "\\http\\content\\" + filename.Replace("/", "\\")).Replace(@"\\", "\\"); 
			
			if (filePath[ filePath.Length - 1 ] == '\\')
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
					default:
						type = "text/html";
						break;
				}
			}
			else
			{
				filePath += ".html";
			}

			Console.WriteLine("Document: " + filePath + ", type " + type);

			if (File.Exists(filePath))
			{
				var result = File.ReadAllBytes(filePath);

				return new HttpResponse
				{
					StatusCode = 200,
					ContentType = type,
					Bytes = result
				};
			}
			else
			{
				return new HttpResponse
				{
					StatusCode = 404,
				};
			}
		}
	}
}
