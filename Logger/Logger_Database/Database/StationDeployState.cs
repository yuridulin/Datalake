namespace Logger.Database
{
	public enum StationDeployState
	{
		No_Info,

		WaitForInstall,
		Installing,
		Installed,
		ErrorWhenInstall,

		WaitForUninstall,
		Uninstalling,
		Uninstalled,
		ErrorWhenUninstall,
	}
}
