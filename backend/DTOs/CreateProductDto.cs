using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new product.
    /// </summary>
    public record CreateProductDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        string Name,
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        double Price,
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        int Quantity
    );
}