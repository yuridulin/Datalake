using LinqToDB.Mapping;
using System;

namespace Logger.Database
{
	[Table(Name = "Stations")]
	public class Station
	{
		[Column, NotNull]
		public string Endpoint { get; set; }

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column]
		public int StationConfigId { get; set; } = 0;

		[Column]
		public DateTime LastTimeAlive { get; set; } = DateTime.MinValue;

		[Column]
		public string AgentVersion { get; set; } = string.Empty;

		[Column]
		public StationDeployState DeployStatus { get; set; } = StationDeployState.No_Info;

		[Column]
		public string DeployMessage { get; set; } = string.Empty;

		[Column]
		public DateTime DeployTime { get; set; } = DateTime.MinValue;


		public bool IsAlive => (DateTime.Now - LastTimeAlive) < TimeSpan.FromMinutes(1);

		public string DeployText()
		{
			switch (DeployStatus)
			{
				case StationDeployState.WaitForInstall: return "Ожидает установки";
				case StationDeployState.Installing: return "В процессе установки";
				case StationDeployState.Installed: return "Установлен";
				case StationDeployState.ErrorWhenInstall: return "Не установлен (ошибка)";
				case StationDeployState.WaitForUninstall: return "Ожидает удаления";
				case StationDeployState.Uninstalling: return "Удаляется";
				case StationDeployState.Uninstalled: return "Удалён";
				case StationDeployState.ErrorWhenUninstall: return "Не удалён (ошибка)";
				default: return "Не обнаружен";
			}
		}
	}
}
