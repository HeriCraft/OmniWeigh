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
    public class VehicleService : IVehicleService
    {
        public async Task<IEnumerable<VehicleDto>> GetAllAsync()
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            var vehicles = await db.Vehicles.AsNoTracking().ToListAsync();

            return vehicles.Select(v => new VehicleDto
            {
                Id = v.Id,
                Registration = v.Registration,
                Type = v.Type,
                MaxLoad = v.MaxLoad,
                ImageFileName = string.IsNullOrWhiteSpace(v.ImageFile) ? null : v.ImageFile
            });
        }

        public async Task<VehicleDto> AddAsync(VehicleDto dto, string? imageSourcePath = null)
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            var model = new Vehicle
            {
                Registration = dto.Registration,
                Type = dto.Type,
                MaxLoad = string.IsNullOrWhiteSpace(dto.MaxLoad) ? null : dto.MaxLoad
            };

            db.Vehicles.Add(model);
            await db.SaveChangesAsync();

            string? storedFilename = null;
            if (!string.IsNullOrWhiteSpace(imageSourcePath) && File.Exists(imageSourcePath))
            {
                var sourceExt = Path.GetExtension(imageSourcePath).ToLowerInvariant();
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var imgFolder = Path.Combine(appData, "OmniWeigh", "images");
                Directory.CreateDirectory(imgFolder);

                string dest;
                string filename;
                if (sourceExt == ".webp")
                {
                    filename = $"V-{model.Id:D5}.png";
                    dest = Path.Combine(imgFolder, filename);
                    try
                    {
                        using var img = SixLabors.ImageSharp.Image.Load(imageSourcePath);
                        var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder();
                        using var outStream = File.OpenWrite(dest);
                        img.Save(outStream, encoder);
                    }
                    catch
                    {
                        filename = $"V-{model.Id:D5}{sourceExt}";
                        dest = Path.Combine(imgFolder, filename);
                        File.Copy(imageSourcePath, dest, true);
                    }
                }
                else
                {
                    filename = $"V-{model.Id:D5}" + sourceExt;
                    dest = Path.Combine(imgFolder, filename);
                    File.Copy(imageSourcePath, dest, true);
                }

                model.ImageFile = filename;
                await db.SaveChangesAsync();
                storedFilename = filename;
            }

            return new VehicleDto
            {
                Id = model.Id,
                Registration = model.Registration,
                Type = model.Type,
                MaxLoad = model.MaxLoad,
                ImageFileName = storedFilename
            };
        }
    }
}
