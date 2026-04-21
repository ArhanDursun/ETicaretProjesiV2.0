using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var product = await _productService.GetProductByIdAsync(id, userId);
            return Ok(product);
        }
        [Authorize]
        [HttpPost("add-product")]
        public async Task<IActionResult> Create([FromForm] ProductDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            }
            var userId = Guid.Parse(userIdClaim);
            await _productService.CreateProductAsync(dto, userId);
            return Ok(new {message = "Ürün oluşturuldu" });
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] ProductDto dto)
        {
            await _productService.UpdateProductAsync(id, dto);
            return Ok(new {message = "Güncellendi"});
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteProductByIdAsync(id);
            return Ok(new {message = "Silindi"});
        }
        [HttpGet("filter")]
        public async Task <IActionResult> GetFiltered([FromQuery] Guid? categoryId, [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice, [FromQuery] string? searchTerm, [FromQuery] bool onlyInStock = false, [FromQuery] string? orderBy = null, [FromQuery] int PageNumber =1, [FromQuery] int PageSize = 10)
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 50) PageSize = 50;
            var result = await _productService.GetFilteredProductsAsync(categoryId, minPrice, maxPrice, searchTerm,onlyInStock,orderBy,PageNumber,PageSize);
            return Ok(result);
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("admin-delete/{id}")]
        public async Task<IActionResult> DeleteByAdmin(Guid id)
        {
            var result = await _productService.DeleteProductByAdminAsync(id);
            return result ? Ok() : BadRequest();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminAll()
        {
            var products = await _productService.GetAllProductsForAdminAsync();
            return Ok(products);
        }
        [Authorize]
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Kullanıcı Kimliği Buluanamadı");
            var userId = Guid.Parse(userIdString);

            var products = await _productService.GetMyProductsAsync(userId);
            return Ok(products);    
        }

        [HttpGet("seller-products/{sellerId}")]
        public async Task<IActionResult> GetSellerProducts(string sellerId)
        {
            
            var products = await _productService.GetMyProductsAsync(Guid.Parse(sellerId));
            return Ok(products);
        }
        [HttpGet("showcase")]
        public async Task<IActionResult> GetShowcaseProducts([FromQuery] PaginationParams paginationParams)
        {
            var result = await _productService.GetShowcaseProductsAsync(paginationParams);

            return Ok(result);
        }



    }
}
