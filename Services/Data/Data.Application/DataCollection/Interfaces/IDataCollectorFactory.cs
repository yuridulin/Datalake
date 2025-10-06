using Datalake.Data.Application.DataCollection.Models;

namespace Datalake.Data.Application.DataCollection.Interfaces;

public interface IDataCollectorFactory
{
	IDataCollector? Create(SourceSettingsDto sourceSettings);
}
