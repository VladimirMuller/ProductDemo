using Microsoft.Extensions.Logging;
using ProductDemo.Controllers;

namespace ProductDemo.Test;

public class HomeControllerUnitTest
{
    const Logger<HomeController> loggerMock = null;

    [Fact]
    public void Test1()
    {    
        HomeController homeController = new HomeController(loggerMock);

        Assert.NotNull(homeController.Index());
    }
}