using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers
{
	public static class List
	{
		public static void Start(CancellationToken token)
		{
			Task.Run(() => Cache.CacheWorker.Start(token));
			Task.Run(() => Calculator.CalculatorWorker.Start(token));
			Task.Run(() => Collector.CollectorWorker.Start(token));
		}
	}
}
