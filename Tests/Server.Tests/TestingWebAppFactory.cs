using Microsoft.AspNetCore.Mvc.Testing;


namespace Datalake.Server.Tests;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
}
