using Datalake.Data.Application.Models.Sources;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollectorFactory
{
	IDataCollector? Create(SourceSettingsDto sourceSettings);
}
