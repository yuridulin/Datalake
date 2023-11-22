using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers
{
	public static class WorkersList
	{
		public static void Start(CancellationToken token)
		{
			Task.Run(() => LogsWorker.Start(token));
			Task.Run(() => CollectorWorker.Start(token));
			Task.Run(() => CalculatorWorker.Start(token));
		}
	}
}
