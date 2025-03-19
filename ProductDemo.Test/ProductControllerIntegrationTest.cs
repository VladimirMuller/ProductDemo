using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NuGet.Protocol;
using ProductDemo;
using ProductDemo.Controllers;
using ProductDemo.Dto;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace ProductDemo.Test;

public class ProductControllerIntegrationTest
{
    [Fact]
    public void Create_GetAll_Get_Update_Delete()
    {
        // Note: It used an another name for database as in ProductDemo!
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("memoryTest").Options;
        var applicationDbContext = new ApplicationDbContext(contextOptions);

        IProductRepository repository = new ProductRepository(applicationDbContext);
        ProductController controller = new ProductController(repository);

        Assert.Equal(0, applicationDbContext.Products.Count());

        ProductDto productDto = new ProductDto() { Name = "name1", Description = "desc1" };

        // create
        var createdResult = controller.Create(productDto) as ObjectResult;
        Assert.NotNull(createdResult);  
        Assert.Equal((int)HttpStatusCode.Created, createdResult.StatusCode);
        var createdProductResult = Assert.IsAssignableFrom<Product>(createdResult.Value);

        Assert.Equal(1, applicationDbContext.Products.Count());
        var firstRecord = applicationDbContext.Products.FirstOrDefault();
        Assert.NotNull(firstRecord);
        Assert.Equal("name1", firstRecord.Name);
        Assert.Equal("desc1", firstRecord.Description);

        // get all
        var getAllResult = controller.GetAll() as ObjectResult;
        Assert.NotNull(getAllResult);
        Assert.Equal((int)HttpStatusCode.OK, getAllResult.StatusCode);
        var listGetAllResult = Assert.IsAssignableFrom<List<Product>>(getAllResult.Value);
        List<Product> expectedProducts = new Product[] { createdProductResult }.ToList();
        Assert.Equal(expectedProducts.ToJson().ToString(), listGetAllResult.ToJson().ToString());

        Assert.Equal(1, applicationDbContext.Products.Count());

        // get one by Id
        var getResult = controller.Get(createdProductResult.Id) as ObjectResult;
        Assert.NotNull(getResult);
        Assert.Equal((int)HttpStatusCode.OK, getResult.StatusCode);
        var getProduct = Assert.IsAssignableFrom<Product>(getResult.Value);
        Assert.Equal(createdProductResult.ToJson().ToString(), getProduct.ToJson().ToString());

        Assert.Equal(1, applicationDbContext.Products.Count());
       



        // update
        Product productToUpdate = new Product() { Id = createdProductResult.Id, Name = "new name1", Description = "new description1" };
        var updatedResult = controller.Update(createdProductResult.Id, productToUpdate) as ObjectResult;
        Assert.NotNull(updatedResult);
        Assert.Equal((int)HttpStatusCode.OK, updatedResult.StatusCode);
        Assert.NotNull(updatedResult.Value);

        // after update
        Assert.Equal("new name1", createdProductResult.Name);
        Assert.Equal("new description1", createdProductResult.Description);
        Assert.Equal(1, applicationDbContext.Products.Count());

        firstRecord = applicationDbContext.Products.FirstOrDefault();
        Assert.NotNull(firstRecord);
        Assert.Equal("new name1", firstRecord.Name);
        Assert.Equal("new description1", firstRecord.Description);

        // delete
        var deletedResult = controller.Delete(createdProductResult.Id) as ObjectResult;
        Assert.NotNull(deletedResult);
        Assert.Equal((int)HttpStatusCode.OK, deletedResult.StatusCode);

        Assert.Equal(0, applicationDbContext.Products.Count());    


        applicationDbContext.Dispose();
    }

}
