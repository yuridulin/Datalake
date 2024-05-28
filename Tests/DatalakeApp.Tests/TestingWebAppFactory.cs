using Microsoft.AspNetCore.Mvc.Testing;


namespace DatalakeServer.Tests;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
}
