using Azure.Core.Serialization;
using FunctionApp2;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TestProject1;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1 : FunctionUnitTestBase
    {
        //Borde gå att bygga en egen function host här inne så man kan mocka och sedan starta skiten.. bra ide? vet ej....
        //public override string FunctionName => "FunctionApp2";

        //[TestMethod]
        //public async Task TestMethod1()
        //{
        //    var httpClient = new HttpClient();
        //    var response = await httpClient.GetAsync("http://localhost:7071/api/Function1");
        //    var result = await response.Content.ReadAsStringAsync();

        //    Assert.AreEqual("Welcome to Azure Functions!", result);
        //}


        [TestMethod]
        public async Task TestMethod2()
        {
            //Arrange
            var function = new Function1();

            //Act
            var result = function.Run(HttpRequestDataMock.Object);
            result.Body.Position = 0;

            var reader = new StreamReader(result.Body, Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            Assert.AreEqual("Welcome to Azure Functions!", body);
        }
    }
}


[TestClass]
public abstract class FunctionUnitTestBase
{
    public virtual Mock<FunctionContext> FunctionContextMock { get; set; }
    public virtual Mock<HttpRequestData> HttpRequestDataMock { get; set; }
    public virtual Mock<HttpResponseData> HttpResponseDataMock { get; set; }

    [TestInitialize]
    public virtual async Task BaseTestInitialize()
    {
        SetupMocks();
    }

    [TestCleanup]
    public void TearDown()
    {
    }

    private void SetupMocks()
    {
        FunctionContextMock = new Mock<FunctionContext>();
        HttpRequestDataMock = new Mock<HttpRequestData>(FunctionContextMock.Object);
        HttpResponseDataMock = new Mock<HttpResponseData>(FunctionContextMock.Object);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<Function1>>();
        //loggerFactoryMock.Setup(f => f.CreateLogger<Function1>()).Returns(logger.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Options.Create(new WorkerOptions { Serializer = new JsonObjectSerializer() }));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        FunctionContextMock.Setup(x => x.InstanceServices).Returns(serviceProvider);

        HttpRequestDataMock.Setup(x => x.Headers).Returns(new HttpHeadersCollection());
        HttpResponseDataMock.Setup(x => x.Headers).Returns(new HttpHeadersCollection());
        HttpResponseDataMock.Setup(x => x.Body).Returns(new MemoryStream());
        HttpRequestDataMock.Setup(x => x.CreateResponse()).Returns(HttpResponseDataMock.Object);
    }
}

[TestClass]
public abstract class FunctionTestBase
{
    public abstract string FunctionName { get; }

    [TestInitialize]
    public async Task BaseTestInitialize()
    {
        FunctionHostHelper.StartFuncHostProcess(FunctionName);

        await Task.Delay(4000);
    }

    [TestCleanup]
    public void TearDown()
    {
        FunctionHostHelper.KillFuncHosts();
    }
}