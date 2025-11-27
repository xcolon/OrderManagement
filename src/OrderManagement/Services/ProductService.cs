using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderManagement.Data;
using OrderManagement.DTOs;
using OrderManagement.Models;

namespace OrderManagement.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string AllProductsCacheKey = "AllProducts";
    private const string ProductCacheKeyPrefix = "Product_";
    private const string CategoryCacheKeyPrefix = "Category_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ProductService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        if (_cache.TryGetValue(AllProductsCacheKey, out IEnumerable<ProductDto>? cachedProducts) && cachedProducts != null)
        {
            return cachedProducts;
        }

        var products = await _context.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        _cache.Set(AllProductsCacheKey, products, CacheDuration);
        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}{id}";

        if (_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
        {
            return cachedProduct;
        }

        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (product != null)
        {
            _cache.Set(cacheKey, product, CacheDuration);
        }

        return product;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        var cacheKey = $"{CategoryCacheKeyPrefix}{category}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<ProductDto>? cachedProducts) && cachedProducts != null)
        {
            return cachedProducts;
        }

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Category == category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        _cache.Set(cacheKey, products, CacheDuration);
        return products;
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            Category = productDto.Category,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        InvalidateProductCache();

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, ProductUpdateDto productDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return null;
        }

        if (productDto.Name != null)
            product.Name = productDto.Name;
        if (productDto.Description != null)
            product.Description = productDto.Description;
        if (productDto.Price.HasValue)
            product.Price = productDto.Price.Value;
        if (productDto.StockQuantity.HasValue)
            product.StockQuantity = productDto.StockQuantity.Value;
        if (productDto.Category != null)
            product.Category = productDto.Category;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        InvalidateProductCache(id, product.Category);

        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        InvalidateProductCache(id, product.Category);

        return true;
    }

    public async Task<bool> UpdateStockAsync(int id, int quantity)
    {
        if (quantity < 0)
        {
            return false;
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        InvalidateProductCache(id, product.Category);

        return true;
    }

    private void InvalidateProductCache(int? productId = null, string? category = null)
    {
        _cache.Remove(AllProductsCacheKey);

        if (productId.HasValue)
        {
            _cache.Remove($"{ProductCacheKeyPrefix}{productId.Value}");
        }

        if (!string.IsNullOrEmpty(category))
        {
            _cache.Remove($"{CategoryCacheKeyPrefix}{category}");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
