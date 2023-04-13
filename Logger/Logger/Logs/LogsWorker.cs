using System.Threading;
using System.Threading.Tasks;

namespace Logger.Logs
{
	public static class LogsWorker
	{
		public static async void Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(1000);
			}
		}
	}
}
