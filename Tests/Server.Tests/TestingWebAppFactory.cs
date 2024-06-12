using Microsoft.AspNetCore.Mvc.Testing;


namespace Datalake.Server.TestRunner;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
}
