using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using NuGet.Protocol;
using ProductDemo.Controllers;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using static ProductDemo.Controllers.ProductController;

namespace ProductDemo.Test;

public class ProductControllerUnitTest
{
    [Fact]
    public async void GetAll_SomeRecords()
    {
        // Arrange        
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        var repositoryMock = new Mock<IProductRepository>();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        repositoryMock.Setup(o => o.GetAllAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(list.AsEnumerable<Product>()));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.GetAll(cancellationTokenSource.Token) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var listResult = Assert.IsAssignableFrom<List<ProductDtoResponse>>(result.Value);
        Assert.Equal(list.ToJson().ToString(), listResult.ToJson().ToString());
    }

    [Fact]
    public async void GetAll_EmptyRepository()
    {
        // Arrange
        var list = new List<Product>();
        var repositoryMock = new Mock<IProductRepository>();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        repositoryMock.Setup(o => o.GetAllAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(list.AsEnumerable<Product>()));

        ProductController controller = new(repositoryMock.Object);

        // Act
        var result = await controller.GetAll(cancellationTokenSource.Token) as ObjectResult;
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var listResult = Assert.IsAssignableFrom<List<ProductDtoResponse>>(result.Value);
        Assert.Equal(list.ToJson().ToString(), listResult.ToJson().ToString());
    }

    [Fact]
    public async void GetAll_SimulateError()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        repositoryMock.Setup(o => o.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test"));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.GetAll(cancellationTokenSource.Token) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Equal("Error retrieving data from the database", result.Value);
    }

    [Fact]
    public async void Get_SomeRecords()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 2) ? list[1] : null));
        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Get(2) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<ProductDtoResponse>(result.Value);
        Assert.Equal(list[1].ToJson().ToString(), productResult.ToJson().ToString());
    }

    [Fact]
    public async Task Get_NotFoundRecord()
    {
        // Arrange
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == -10) ? null : list.FirstOrDefault()));

        ProductController controller = new(repositoryMock.Object);

        // Act
        var result = await controller.Get(-10) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Product with Id = -10 not found", result.Value);
    }


    [Fact]
    public async Task Create_OneRecord()
    {
        // Arrange
        ProductDtoCreateRequest productDtoToCreate = new(Name: "new product", Description: "new product description");
        Product? productCreatedMock = null;

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).Returns<Product>(a => Task.FromResult((a.Id=3, productCreatedMock = a)));
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 3) ? productCreatedMock : null));

        ProductController controller = new(repositoryMock.Object);

        // Act
        var result = await controller.Create(productDtoToCreate) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<ProductDtoResponse>(result.Value);
        Assert.Equal(productCreatedMock.ToJson().ToString(), productResult.ToJson().ToString());
    }

    [Fact]
    public async Task Create_SimulateError()
    {
        // Arrange
        ProductDtoCreateRequest productDtoToCreate = new(Name: "new product", Description: "new product description");
        Product productCreatedMock = new() { Id = 3, Name = productDtoToCreate.Name, Description = productDtoToCreate.Description };
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).ThrowsAsync(new Exception("Test"));

        ProductController controller = new(repositoryMock.Object);

        // Act
        var result = await controller.Create(productDtoToCreate) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Equal("Error creating new product record", result.Value);
    }

    [Fact]
    public async Task Create_XSSAttack()
    {
        // Arrange
        ProductDtoCreateRequest productDtoToCreate = new(Name: "<img src = x onerror=alert('XSS name!')>", Description: "<img src = x onerror=alert('XSS desc!')>");
        Product? productCreatedMock = null;
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).Returns<Product>(a => Task.FromResult((a.Id = 3, productCreatedMock = a)));
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 3) ? productCreatedMock : null));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Create(productDtoToCreate) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<ProductDtoResponse>(result.Value);
        Assert.NotNull(productCreatedMock);

        string jsonProductCreatedMock = productCreatedMock.ToJson().ToString();
        Assert.Equal(jsonProductCreatedMock, productResult.ToJson().ToString());
        Assert.DoesNotContain('<', jsonProductCreatedMock.ToArray());
        Assert.DoesNotContain('>', jsonProductCreatedMock.ToArray());
        Assert.DoesNotContain('\'', jsonProductCreatedMock.ToArray());
        //Assert.DoesNotContain('\"', jsonProductCreatedMock.ToArray());
    }

    [Fact]
    public async Task Update_OneRecord()
    {
        // Arrange
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        ProductDtoUpdateRequest productToUpdate = new(Id: 2, Name: "new TestName2", Description: "new TestDescription2");
        Product? productToUpdated = null;

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 2) ? list[1] : null));
        repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns<Product>(a => Task.FromResult(productToUpdated = a));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Update(2, productToUpdate) as OkResult;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Update_XSSAttack()
    {
        // Arrange
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        ProductDtoUpdateRequest productToUpdate = new(Id: 2, Name: "<img src = x onerror=alert('XSS name!')>", Description: "<img src = x onerror=alert('XSS name!')>");
        Product? productUpdated = null;

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 2) ? list[1] : null));
        repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns<Product>(a => Task.FromResult(productUpdated = a));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Update(2, productToUpdate) as OkResult; // ObjectResult;

        // Assert
        Assert.NotNull(result);
        //Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        //Assert.NotNull(result.Value);

        //var productResult = Assert.IsAssignableFrom<ProductDtoResponse>(result.Value);
        //Assert.NotNull(productUpdated);
        //Assert.Equal(productUpdated.ToJson().ToString(), productResult.ToJson().ToString());

        string jsonProductUpdated = productUpdated.ToJson().ToString();
        //Assert.Equal(jsonProductUpdated, productResult.ToJson().ToString());
        Assert.DoesNotContain('<', jsonProductUpdated.ToArray());
        Assert.DoesNotContain('>', jsonProductUpdated.ToArray());
        Assert.DoesNotContain('\'', jsonProductUpdated.ToArray());
        //Assert.DoesNotContain('\"', jsonProductUpdated.ToArray());
    }

    [Fact]
    public async Task Update_SimulateError()
    {
        // Arrange
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        ProductDtoUpdateRequest productToUpdate = new(Id: 2, Name: "new TestName2", Description: "new TestDescription2");
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult((a == 2) ? list[1] : null));
        repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).ThrowsAsync(new Exception("test"));
        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Update(2, productToUpdate) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Error updating data", result.Value);
    }



    [Fact]
    public async Task Delete_OneRecord()
    {
        // Arrange
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        Product? productToDelete = null;
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult(productToDelete = (a == 2) ? list[1] : null));
        repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>())).Returns<int>(a => Task.FromResult(productToDelete));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Delete(2);

        var okRequestResult = Assert.IsType<OkResult>(result);

        //Assert.IsType<SerializableError>(okRequestResult.Value);

        //Assert.NotNull(result.Value);
        //var productResult = Assert.IsAssignableFrom<ProductDtoResponse>(result.Value);
        //Assert.NotNull(productToDelete);
    }

    [Fact]
    public async Task Delete_NotFound()
    {
        // Arrange
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        Product? productToDelete = null;
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult(productToDelete = (a == 10) ? null : list[0]));
        repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>())).Returns<int>(a => Task.FromResult(productToDelete));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Delete(10) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Product with Id = 10 not found", result.Value);
    }

    [Fact]
    public async Task Delete_SimulateError()
    {
        // Arrange
        var list = new List<Product>
        {
            new() { Id = 1, Name = "TestName1", Description = "TestDescription1" },
            new() { Id = 2, Name = "TestName2", Description = "TestDescription2" }
        };

        Product? productToDelete = null;

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns<int>(a => ValueTask.FromResult(productToDelete = (a == 1) ? list[0] : null));
        repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new Exception("test"));

        ProductController controller = new ProductController(repositoryMock.Object);

        // Act
        var result = await controller.Delete(1) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Error deleting data", result.Value);
    }
}
