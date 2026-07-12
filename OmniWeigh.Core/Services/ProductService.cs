using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public class ProductService : IProductService
    {
        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            var products = await db.Products.AsNoTracking().ToListAsync();

            // Normalize Reference if missing
            foreach (var p in products.Where(p => string.IsNullOrWhiteSpace(p.Barcode)))
            {
                // no-op; keep Barcode as-is
            }

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Reference = $"P-{p.Id:D5}",
                Name = p.Name,
                ImageFileName = string.IsNullOrWhiteSpace(p.Barcode) ? null : p.Barcode
            });
        }

        public async Task<ProductDto> AddAsync(ProductDto dto, string? imageSourcePath = null)
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            var model = new Product
            {
                Name = dto.Name,
                Barcode = dto.ImageFileName ?? string.Empty
            };

            db.Products.Add(model);
            await db.SaveChangesAsync();

            // If an image path is supplied, save it to app images folder and set Barcode to filename
            if (!string.IsNullOrWhiteSpace(imageSourcePath) && File.Exists(imageSourcePath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var imgFolder = Path.Combine(appData, "OmniWeigh", "images");
                Directory.CreateDirectory(imgFolder);

                var ext = Path.GetExtension(imageSourcePath).ToLowerInvariant();
                var storedFilename = $"P-{model.Id:D5}" + ext;
                var dest = Path.Combine(imgFolder, storedFilename);
                File.Copy(imageSourcePath, dest, true);

                model.Barcode = storedFilename;
                await db.SaveChangesAsync();
            }

            return new ProductDto
            {
                Id = model.Id,
                Reference = $"P-{model.Id:D5}",
                Name = model.Name,
                ImageFileName = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode
            };
        }
    }
}
