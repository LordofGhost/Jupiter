using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Jupiter.Models;
namespace Jupiter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly Data.Context.ShopDbContext _context;

    public ProductsController(Data.Context.ShopDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ICollection<Models.Products.PRequest>> GetProducts(
        [FromQuery] Models.Products.Category? category = null
    )
    {
        IQueryable<Models.Products.Product> query = _context.Products;

        if (category != null)
        {
            query = query.Where(p => p.Category == category);
        }

        var products = await query.ToListAsync();
        ICollection<Models.Products.PRequest> productRequests = new List<Models.Products.PRequest>();

        foreach (var product in products)
        {
            var stock = await _context.Stock
                .Where(s => s.ProductId == product.ProductId)
                .OrderByDescending(s => s.Day)
                .FirstOrDefaultAsync();

            if (stock == null)
            {
                stock = new Models.Stock.Stock
                {
                    ProductId = product.ProductId,
                    InStock = 0,
                    OnTheShelf = 0,
                    PurchasedToday = 0,
                    Day = DateTime.Today
                };
            }

            productRequests.Add(new Models.Products.PRequest(product, stock));
        }

        return productRequests;
    }

    [HttpGet("{ProductId}")]
    public async Task<ActionResult<Models.Products.PRequest>> GetProduct(ulong ProductId)
    {
        var product = await _context.Products.FindAsync(ProductId);
        var stock = await _context.Stock
            .Where(s => s.ProductId == ProductId)
            .OrderByDescending(s => s.Day)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound("This Product doens't exists!");
        }

        if (stock == null)
        {
            stock = new Models.Stock.Stock
            {
                ProductId = product.ProductId,
                InStock = 0,
                OnTheShelf = 0,
                PurchasedToday = 0,
                Day = DateTime.Today
            };
        }

        return new Models.Products.PRequest(product, stock);
    }

    // When creating a product the Shelf attribute should always be null, because otherwise a cycle is created and a 400 Error is sent
    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<Models.Products.Product>> CreateProduct(Models.Products.Product product)
    {
        if (ProductExists(product.ProductId))
        {
            return BadRequest("A Product with the same id already exists");
        }

        if (product.ShelfId != null && !_context.Shelves.Any(e => e.ShelfId == product.ShelfId))
        {
            return BadRequest("The selected shelf does not exist");
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { product.ProductId }, product);
    }

    [HttpPatch("{ProductId}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<Models.Products.Product>> UpdateProduct(ulong ProductId, Models.Products.Product product)
    {
        if (ProductId != product.ProductId)
        {
            var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == ProductId);
            if (oldProduct == null)
            {
                return NotFound();
            }

            var createResult = await CreateProduct(product);
            if (createResult.Result is not CreatedAtActionResult)
            {
                return createResult;
            }

            await UpdateReferencesInShelves(ProductId, product.ProductId);
            await UpdateReferencesInStock(ProductId, product.ProductId);

            var productToRemove = await _context.Products.FindAsync(ProductId);
            if (productToRemove != null)
            {
                _context.Products.Remove(productToRemove);
                await _context.SaveChangesAsync();
            }

            return createResult;
        }

        var existingProduct = await _context.Products.FindAsync(ProductId);
        if (existingProduct == null)
        {
            return NotFound();
        }

        _context.Entry(existingProduct).CurrentValues.SetValues(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{ProductId}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteProduct(ulong ProductId)
    {
        var product = await _context.Products.FindAsync(ProductId);
        if (product == null)
        {
            return NotFound();
        }

        await DeleteReferencesInStock(ProductId);
        await DeleteReferencesInShelves(ProductId);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(ulong ProductId)
    {
        return _context.Products.Any(e => e.ProductId == ProductId);
    }

    private async Task<bool> DeleteReferencesInShelves(ulong deleteProductId)
    {
        var shelves = await _context.Shelves.ToListAsync();
        foreach (var shelf in shelves)
        {
            for (int index = 0; index < shelf.ProductIds.Length; index++)
            {
                if (shelf.ProductIds[index] != null)
                {
                    if (shelf.ProductIds[index] == deleteProductId)
                    {
                        shelf.ProductIds[index] = null;
                        _context.Entry(shelf).State = EntityState.Modified;
                    }
                }
            }
        }
        return true;
    }

   private async Task<bool> UpdateReferencesInShelves(ulong deleteProductId, ulong newProductId)
    {
        var shelves = await _context.Shelves.ToListAsync();
        foreach (var shelf in shelves)
        {
            for (int index = 0; index < shelf.ProductIds.Length; index++)
            {
                if (shelf.ProductIds[index] != null)
                {
                    if (shelf.ProductIds[index] == deleteProductId)
                    {
                        shelf.ProductIds[index] = newProductId;
                        _context.Entry(shelf).State = EntityState.Modified;
                    }
                }
            }
        }
        return true;
    }

    private async Task<bool> DeleteReferencesInStock(ulong deleteProductId)
    {
        var stocks = await _context.Stock.ToListAsync();
        foreach (var stock in stocks)
        {
            if (stock.ProductId == deleteProductId)
            {
                _context.Stock.Remove(stock);
            }
        }
        return true;
    }

    private async Task<bool> UpdateReferencesInStock(ulong oldProductId, ulong newProductId)
    {
        var stocks = await _context.Stock.ToListAsync();
        foreach (var stock in stocks)
        {
            if (stock.ProductId == oldProductId)
            {
                stock.ProductId = newProductId;
                _context.Entry(stock).State = EntityState.Modified;
            }
        }
        return true;
    }
}