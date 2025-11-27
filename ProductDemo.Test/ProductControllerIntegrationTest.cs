using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NuGet.Protocol;
using ProductDemo;
using ProductDemo.Controllers;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ProductDemo.Controllers.ProductController;


namespace ProductDemo.Test;

public class ProductControllerIntegrationTest : IDisposable
{
    private ApplicationDbContext? applicationDbContext;
    private readonly ProductController controller;

    public ProductControllerIntegrationTest()
    {
        // Note: It used an another name for database as in ProductDemo!
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("memoryTest").Options;
        applicationDbContext = new ApplicationDbContext(contextOptions);

        IProductRepository repository = new ProductRepository(applicationDbContext);
        controller = new(repository);
    }

    [Fact]
    public async Task Create_GetAll_Get_Update_Delete()
    {
        Assert.Equal(0, applicationDbContext?.Products.Count() ?? -1);

        ProductDtoCreateRequest productDto = new (Name: "name1", Description: "desc1");

        // create
        var createdResult = await controller.Create(productDto) as ObjectResult;
        Assert.NotNull(createdResult);  
        Assert.Equal((int)HttpStatusCode.Created, createdResult.StatusCode);
        var createdProductResult = Assert.IsAssignableFrom<ProductDtoResponse>(createdResult.Value);

        Assert.Equal(1, applicationDbContext?.Products.Count());
        var firstRecord = applicationDbContext?.Products.FirstOrDefault();
        Assert.NotNull(firstRecord);
        Assert.Equal("name1", firstRecord.Name);
        Assert.Equal("desc1", firstRecord.Description);

        // get all
        var getAllResult = await controller.GetAll() as ObjectResult;
        Assert.NotNull(getAllResult);
        Assert.Equal((int)HttpStatusCode.OK, getAllResult.StatusCode);
        var listGetAllResult = Assert.IsAssignableFrom<List<ProductDtoResponse>>(getAllResult.Value);
        List<ProductDtoResponse> expectedProducts = [createdProductResult];
        Assert.Equal(expectedProducts.ToJson().ToString(), listGetAllResult.ToJson().ToString());

        Assert.Equal(1, applicationDbContext?.Products.Count());

        // get one by Id
        var getResult = await controller.Get(createdProductResult.Id) as ObjectResult;
        Assert.NotNull(getResult);
        Assert.Equal((int)HttpStatusCode.OK, getResult.StatusCode);
        var getProduct = Assert.IsAssignableFrom<ProductDtoResponse>(getResult.Value);
        Assert.Equal(createdProductResult.ToJson().ToString(), getProduct.ToJson().ToString());

        Assert.Equal(1, applicationDbContext?.Products.Count());


        // update
        ProductDtoUpdateRequest productToUpdate = new(Id: createdProductResult.Id, Name: "new name1", Description: "new description1");
        var updatedResult = await controller.Update(createdProductResult.Id, productToUpdate) as OkResult;
        Assert.NotNull(updatedResult);
        //Assert.Equal((int)HttpStatusCode.OK, updatedResult.StatusCode);
        //Assert.NotNull(updatedResult.Value);

        // after update
        var getResultAfterUpdate = await controller.Get(createdProductResult.Id) as ObjectResult;
        Assert.NotNull(getResultAfterUpdate);
        Assert.Equal((int)HttpStatusCode.OK, getResultAfterUpdate.StatusCode);
        var getProductAfterUpdate = Assert.IsAssignableFrom<ProductDtoResponse>(getResultAfterUpdate.Value);

        Assert.Equal("new name1", getProductAfterUpdate.Name);
        Assert.Equal("new description1", getProductAfterUpdate.Description);

        Assert.Equal(1, applicationDbContext?.Products.Count());
        firstRecord = applicationDbContext?.Products.FirstOrDefault();
        Assert.NotNull(firstRecord);
        Assert.Equal("new name1", firstRecord.Name);
        Assert.Equal("new description1", firstRecord.Description);

        // delete
        var deletedResult = await controller.Delete(createdProductResult.Id) as OkResult;
        Assert.NotNull(deletedResult);
        //Assert.Equal((int)HttpStatusCode.OK, deletedResult.StatusCode);

        Assert.Equal(0, applicationDbContext?.Products.Count());    
    }

    public void Dispose()
    {
        if (applicationDbContext is not null)
        {
            applicationDbContext.Dispose();
            applicationDbContext = null;
        }
        GC.SuppressFinalize(this);
    }
}
