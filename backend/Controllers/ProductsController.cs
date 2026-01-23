using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Repositories;
using backend.DTOs;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing product operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        /// <summary>
        /// Initializes a new instance of the ProductsController class.
        /// </summary>
        /// <param name="productRepository">The product repository for data access.</param>
        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A collection of all products.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productRepository.GetAllProductsAsync();

            var productDto = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Quantity));

            return Ok(productDto);
        }

        /// <summary>
        /// Retrieves a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The product with the specified identifier or not found if it doesn't exist.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetById(string id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var productDto = new ProductDto(product.Id, product.Name, product.Price, product.Quantity);

            return Ok(productDto);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="createDto">The product creation data transfer object.</param>
        /// <returns>The created product with its assigned identifier.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
        {
            var newProduct = new Product
            {
                Name = createDto.Name,
                Price = createDto.Price,
                Quantity = createDto.Quantity
            };

            await _productRepository.AddProductAsync(newProduct);

            var resultDto = new ProductDto(
                newProduct.Id,
                newProduct.Name,
                newProduct.Price,
                newProduct.Quantity
            );

            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, resultDto);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The identifier of the product to update.</param>
        /// <param name="updateDto">The product update data transfer object.</param>
        /// <returns>No content if successful or not found if product doesn't exist.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProductDto updateDto)
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);

            if (existingProduct == null) return NotFound();

            existingProduct.Name = updateDto.Name;
            existingProduct.Price = updateDto.Price;
            existingProduct.Quantity = updateDto.Quantity;

            await _productRepository.UpdateProductAsync(existingProduct);

            return NoContent();
        }

        /// <summary>
        /// Deletes a product by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the product to delete.</param>
        /// <returns>No content if successful or not found if product doesn't exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);

            if (existingProduct == null) return NotFound();

            await _productRepository.DeleteProductAsync(id);

            return NoContent();
        }
    }
}