using LinqToDB;
using LinqToDB.Data;

namespace Logger.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext(): base("Default") { }

		// Объекты

		public ITable<Station> Stations
			=> this.GetTable<Station>();

		public ITable<StationConfig> StationsConfigs
			=> this.GetTable<StationConfig>();

		public ITable<StationSpec> StationsSpecs
			=> this.GetTable<StationSpec>();

		public ITable<LogFilter> LogsFilters
			=> this.GetTable<LogFilter>();

		public ITable<Log> Logs
			=> this.GetTable<Log>();

		public ITable<LogReaction> LogsReactions
			=> this.GetTable<LogReaction>();

		public ITable<Channel> Channels
			=> this.GetTable<Channel>();

		public ITable<ActionPing> ActionsPings
			=> this.GetTable<ActionPing>();

		public ITable<ActionWatcher> ActionsWatchers
			=> this.GetTable<ActionWatcher>();

		public ITable<ActionSql> ActionsSql
			=> this.GetTable<ActionSql>();

		public ITable<Settings> Settings
			=> this.GetTable<Settings>();

		// Связи

		public ITable<Rel_StationConfig_LogFilter> Rel_StationConfig_LogFilter
			=> this.GetTable<Rel_StationConfig_LogFilter>();

		public ITable<Rel_StationConfig_ActionPing> Rel_StationConfig_ActionPing
			=> this.GetTable<Rel_StationConfig_ActionPing>();

		public ITable<Rel_StationConfig_ActionWatcher> Rel_StationConfig_ActionWatcher
			=> this.GetTable<Rel_StationConfig_ActionWatcher>();

		public ITable<Rel_LogFilter_Channel> Rel_LogFilter_Channel
			=> this.GetTable<Rel_LogFilter_Channel>();

		public ITable<Rel_Log_WebView> Rel_Log_WebView
			=> this.GetTable<Rel_Log_WebView>();

		public ITable<Rel_Log_Telegram> Rel_Log_Telegram
			=> this.GetTable<Rel_Log_Telegram>();

		public ITable<Rel_StationConfig_ActionSql> Rel_StationConfig_ActionSql
			=> this.GetTable<Rel_StationConfig_ActionSql>();
	}
}
