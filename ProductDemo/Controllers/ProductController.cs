using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System.Collections;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;

namespace ProductDemo.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository repository;

        public sealed record ProductDtoResponse(int Id, string Name, string? Description) 
        {
            public ProductDtoResponse(Product product) : this(product.Id, product.Name, product.Description) { }
        }

        public sealed record ProductDtoCreateRequest(string Name, string? Description)
        {
            public Product ToEntity() => new Product { Name = Name, Description = Description };
        }

        public sealed record ProductDtoUpdateRequest(int Id, string Name, string? Description)
        {
            public Product ToEntity() => new Product { Id = Id, Name = Name, Description = Description };
        }


        public ProductController(IProductRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Get all products.
        /// curl "http://localhost:5136/api/products"
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await repository.GetAllAsync(cancellationToken);
                var productDtos = products.Select(p => new ProductDtoResponse(p)).ToList();
                return Ok(productDtos);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        /// <summary>
        /// Get one product by Id.
        /// curl "http://localhost:5136/api/products/1"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await repository.GetAsync(id);
                if (result is null) 
                    return NotFound($"Product with Id = {id} not found");
                else
                    return Ok(new ProductDtoResponse(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        /// <summary>
        /// Create one new product.
        /// curl -X "POST" "http://localhost:5136/api/products" -H "accept: */*"  -H "Content-Type: application/json" -d "{""name"": ""test1"",""description"": ""desc1""}"
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(ProductDtoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ProductDtoCreateRequest productDto)
        {
            try
            {
                if (productDto == null)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object");

                var productToAdd = productDto.ToEntity();

                productToAdd.Name = HttpUtility.HtmlEncode(productToAdd.Name);
                productToAdd.Description = HttpUtility.HtmlEncode(productToAdd.Description);

                await repository.AddAsync(productToAdd);

                return CreatedAtAction(
                    actionName: nameof(Get),
                    routeValues: new { Id = productToAdd.Id },
                    value: new ProductDtoResponse(productToAdd));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new product record");
            }
        }

        /// <summary>
        /// Update a product.
        /// curl -X PUT "http://localhost:5136/api/products/2" -H "accept: */*" -H "Content-Type: application/json" -d "{ ""name"": ""new name"", ""description"": ""new description"", ""id"": 2}"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        //[ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDtoUpdateRequest productDto)
        {
            try
            {
                //if (id != product.Id)
                //    return BadRequest("Product ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object");

                var productToUpdate = await repository.GetAsync(id);

                if (productToUpdate is null)
                    return NotFound($"Product with Id = {id} not found");

                //productToUpdate = productDto.ToEntity();
                productToUpdate.Name = HttpUtility.HtmlEncode(productDto.Name);
                productToUpdate.Description = HttpUtility.HtmlEncode(productDto.Description);

                await repository.UpdateAsync(productToUpdate);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        /// <summary>
        /// Delete one product.
        /// curl -X DELETE http://localhost:5136/api/products/1  -H 'accept: */*'
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        //[ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var productToDelete = await repository.GetAsync(id);

                if (productToDelete is null)
                {
                    return NotFound($"Product with Id = {id} not found");
                }
                await repository.DeleteAsync(id);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
