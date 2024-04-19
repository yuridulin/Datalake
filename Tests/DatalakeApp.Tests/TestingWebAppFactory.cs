using Microsoft.AspNetCore.Mvc.Testing;


namespace DatalakeApp.Tests;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
}
