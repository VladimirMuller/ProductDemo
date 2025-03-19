using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Moq.Protected;
using ProductDemo.Controllers;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System.Net;
using ProductDemo.Dto;
using System.Web;
using NuGet.Protocol;

namespace ProductDemo.Test;

public class ProductControllerUnitTest
{
    [Fact]
    public void GetAll_SomeRecords()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.GetAll() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var listResult = Assert.IsAssignableFrom<List<Product>>(result.Value);
        Assert.Equal(list.ToJson().ToString(), listResult.ToJson().ToString());
    }

    [Fact]
    public void GetAll_EmptyRepository()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
       
        repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.GetAll() as ObjectResult;
        
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var listResult = Assert.IsAssignableFrom<List<Product>>(result.Value);
        Assert.Equal(list.ToJson().ToString(), listResult.ToJson().ToString());
    }

    [Fact]
    public void GetAll_SimulateError()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();

        repositoryMock.Setup(o => o.Get()).Throws(new Exception("Test"));

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.GetAll() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Equal("Error retrieving data from the database", result.Value);
    }

    [Fact]
    public void Get_SomeRecords()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());
        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 2) ? list[1] : null);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Get(2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.Equal(list[1].ToJson().ToString(), productResult.ToJson().ToString());
    }

    [Fact]
    public void Get_NotFoundRecord()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());
        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == -10) ? null : list.FirstOrDefault());

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Get(-10) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Product with Id = -10 not found", result.Value);
    }


    [Fact]
    public void Create_OneRecord()
    {
        var repositoryMock = new Mock<IProductRepository>();
        //var list = new List<Product>();
        //list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        //list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        ProductDto productDtoToCreate = new ProductDto("new product", "new product description");
        Product? productCreatedMock = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());
        repositoryMock.Setup(x => x.Add(It.IsAny<ProductDto>())).Returns<ProductDto>(a => productCreatedMock = new Product(3, a));
        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 3) ? productCreatedMock : null);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Create(productDtoToCreate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.Equal(productCreatedMock.ToJson().ToString(), productResult.ToJson().ToString());
    }

    [Fact]
    public void Create_SimulateError()
    {
        var repositoryMock = new Mock<IProductRepository>();
        //var list = new List<Product>();
        //list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        //list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        ProductDto productDtoToCreate = new ProductDto("new product", "new product description");
        Product productCreatedMock = new Product() { Id = 3, Name = productDtoToCreate.Name, Description = productDtoToCreate.Description };

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());
        repositoryMock.Setup(x => x.Add(It.IsAny<ProductDto>())).Throws(new Exception("Test"));
        //repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 3) ? productCreatedMock : null);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Create(productDtoToCreate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Equal("Error creating new product record", result.Value);
    }

    [Fact]
    public void Create_XSSAttack()
    {
        var repositoryMock = new Mock<IProductRepository>();
        //var list = new List<Product>();
        //list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        //list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        ProductDto productDtoToCreate = new ProductDto("<img src = x onerror=alert('XSS name!')>", "<img src = x onerror=alert('XSS desc!')>");
        Product? productCreatedMock = null; 

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());
        repositoryMock.Setup(x => x.Add(It.IsAny<ProductDto>())).Returns<ProductDto>(a => productCreatedMock = new Product(3, a));
        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 3) ? productCreatedMock : null);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Create(productDtoToCreate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.NotNull(productCreatedMock);

        string jsonProductCreatedMock = productCreatedMock.ToJson().ToString();
        Assert.Equal(jsonProductCreatedMock, productResult.ToJson().ToString());
        Assert.DoesNotContain('<', jsonProductCreatedMock.ToArray());
        Assert.DoesNotContain('>', jsonProductCreatedMock.ToArray());
        Assert.DoesNotContain('\'', jsonProductCreatedMock.ToArray());
        //Assert.DoesNotContain('\"', jsonProductCreatedMock.ToArray());
    }

    [Fact]
    public void Update_OneRecord()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product productToUpdate = new Product() { Id = 2, Name = "new TestName2", Description = "new TestDescription2" };
        Product? productToUpdated = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 2) ? list[1] : null);
        repositoryMock.Setup(x => x.Update(It.IsAny<Product>())).Returns<Product>(a => productToUpdated = a);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Update(2, productToUpdate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.NotNull(productToUpdated);
        Assert.Equal(productToUpdated.ToJson().ToString(), productResult.ToJson().ToString());
    }

    [Fact]
    public void Update_XSSAttack()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product productToUpdate = new Product() { Id = 2, Name = "<img src = x onerror=alert('XSS name!')>", 
            Description = "<img src = x onerror=alert('XSS desc!')>" };
        Product? productUpdated = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 2) ? list[1] : null);
        repositoryMock.Setup(x => x.Update(It.IsAny<Product>())).Returns<Product>(a => productUpdated = a);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Update(2, productToUpdate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.NotNull(productUpdated);
        Assert.Equal(productUpdated.ToJson().ToString(), productResult.ToJson().ToString());

        string jsonProductUpdated = productUpdated.ToJson().ToString();
        Assert.Equal(jsonProductUpdated, productResult.ToJson().ToString());
        Assert.DoesNotContain('<', jsonProductUpdated.ToArray());
        Assert.DoesNotContain('>', jsonProductUpdated.ToArray());
        Assert.DoesNotContain('\'', jsonProductUpdated.ToArray());
        //Assert.DoesNotContain('\"', jsonProductUpdated.ToArray());
    }

    [Fact]
    public void Update_SimulateError()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product productToUpdate = new Product() { Id = 2, Name = "new TestName2", Description = "new TestDescription2" };
        //Product? productToUpdated = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => (a == 2) ? list[1] : null);
        repositoryMock.Setup(x => x.Update(It.IsAny<Product>())).Throws(new Exception("test"));
        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Update(2, productToUpdate) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Error updating data", result.Value);
    }



    [Fact]
    public void Delete_OneRecord()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product? productToDelete = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => productToDelete = (a == 2) ? list[1] : null);
        repositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Returns<int>(a => productToDelete);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Delete(2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Value);

        var productResult = Assert.IsAssignableFrom<Product>(result.Value);
        Assert.NotNull(productToDelete);
    }

    [Fact]
    public void Delete_NotFound()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product? productToDelete = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => productToDelete = (a == 10) ? null : list[0]);
        repositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Returns<int>(a => productToDelete);

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Delete(10) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Product with Id = 10 not found", result.Value);
    }

    [Fact]
    public void Delete_SimulateError()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var list = new List<Product>();
        list.Add(new Product() { Id = 1, Name = "TestName1", Description = "TestDescription1" });
        list.Add(new Product() { Id = 2, Name = "TestName2", Description = "TestDescription2" });

        Product? productToDelete = null;

        // repositoryMock.Setup(o => o.Get()).Returns(list.AsQueryable());

        repositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns<int>(a => productToDelete = (a == 1) ? list[0] : null);
        repositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Throws(new Exception("test"));

        ProductController controller = new ProductController(repositoryMock.Object);

        var result = controller.Delete(1) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Error deleting data", result.Value);
    }
}
