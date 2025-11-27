using OrderManagement.DTOs;
using OrderManagement.Models;

namespace OrderManagement.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
    Task<ProductDto> CreateProductAsync(ProductCreateDto productDto);
    Task<ProductDto?> UpdateProductAsync(int id, ProductUpdateDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateStockAsync(int id, int quantity);
}
