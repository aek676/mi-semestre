namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for product information.
    /// </summary>
    public record ProductDto(
        string Id,
        string Name,
        double Price,
        int Quantity);
}