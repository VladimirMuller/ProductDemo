using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProductDemo.Dto;
using ProductDemo.Models;
using ProductDemo.Repositories;
using System.Collections;
using System.Web;

namespace ProductDemo.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository repository;

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
        public IActionResult GetAll()
        {
            try
            {
                return Ok(repository.Get().ToList());
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
        public IActionResult Get(int id)
        {
            try
            {
                var result = repository.Get(id);
                if (result == null) 
                    return NotFound($"Product with Id = {id} not found");
                else
                    return Ok(result);
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
        public IActionResult Create([FromBody] ProductDto product)
        {
            try
            {
                if (product == null)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object");

                product.Name = HttpUtility.HtmlEncode(product.Name);
                product.Description = HttpUtility.HtmlEncode(product.Description);

                var createdProduct = repository.Add(product);

                return CreatedAtAction(nameof(Get),
                    new { Id = createdProduct.Id }, createdProduct);
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
        public IActionResult Update(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                    return BadRequest("Product ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object");

                product.Name = HttpUtility.HtmlEncode(product.Name);
                product.Description = HttpUtility.HtmlEncode(product.Description);

                var productToUpdate = repository.Get(id);

                if (productToUpdate == null)
                    return NotFound($"Product with Id = {id} not found");

                return Ok(repository.Update(product));
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
        public IActionResult Delete(int id)
        {
            try
            {
                var productToDelete = repository.Get(id);

                if (productToDelete == null)
                {
                    return NotFound($"Product with Id = {id} not found");
                }
                return Ok(repository.Delete(id));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
