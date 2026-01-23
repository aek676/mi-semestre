using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    /// <summary>
    /// Represents a product entity.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// The unique identifier of the product.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string Id { get; set; } = default!;
        /// <summary>
        /// The name of the product.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// The price of the product.
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// The quantity of the product in stock.
        /// </summary>
        public int Quantity { get; set; }

    }
}