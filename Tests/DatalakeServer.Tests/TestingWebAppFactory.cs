using Microsoft.AspNetCore.Mvc.Testing;


namespace DatalakeServer.TestRunner;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
}
