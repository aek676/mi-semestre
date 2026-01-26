using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for updating an existing product.
    /// </summary>
    public record UpdateProductDto(
        [Required] string Name,
        [Range(0.01, double.MaxValue)] double Price,
        [Range(0, int.MaxValue)] int Quantity
    );
}