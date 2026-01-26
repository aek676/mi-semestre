using backend.Models;

namespace backend.Repositories
{
    /// <summary>
    /// Interface for product repository data access operations.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Retrieves all products from the repository.
        /// </summary>
        /// <returns>A collection of all products.</returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();
        /// <summary>
        /// Retrieves a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The product if found, otherwise null.</returns>
        Task<Product> GetProductByIdAsync(string id);
        /// <summary>
        /// Adds a new product to the repository.
        /// </summary>
        /// <param name="product">The product to add.</param>
        Task AddProductAsync(Product product);
        /// <summary>
        /// Updates an existing product in the repository.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        Task UpdateProductAsync(Product product);
        /// <summary>
        /// Deletes a product from the repository.
        /// </summary>
        /// <param name="id">The identifier of the product to delete.</param>
        Task DeleteProductAsync(string id);
    }
}