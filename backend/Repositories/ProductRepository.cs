using MongoDB.Driver;
using backend.Models;
using backend.Data;

namespace backend.Repositories
{
    /// <summary>
    /// Repository implementation for product data access using MongoDB.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly MongoDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ProductRepository class.
        /// </summary>
        /// <param name="context">The MongoDB database context.</param>
        public ProductRepository(MongoDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Retrieves all products from the database.
        /// </summary>
        /// <returns>A collection of all products.</returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.Find(_ => true).ToListAsync();
        }
        /// <summary>
        /// Retrieves a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The product if found, otherwise null.</returns>
        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="product">The product to add.</param>
        public Task AddProductAsync(Product product)
        {
            return _context.Products.InsertOneAsync(product);
        }
        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        public Task UpdateProductAsync(Product product)
        {
            return _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
        }
        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The identifier of the product to delete.</param>
        public Task DeleteProductAsync(string id)
        {
            return _context.Products.DeleteOneAsync(p => p.Id == id);
        }
    }
}
