using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public class ClientService : IClientService
    {
        public async Task<IEnumerable<ClientDto>> GetAllAsync()
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            await EnsureReferenceColumnExistsAsync(db);

            var clients = await db.Clients.AsNoTracking().ToListAsync();

            System.Diagnostics.Debug.WriteLine($"ClientService: loaded {clients.Count} clients from DB");

            return clients.Select(c => MapToDto(c));
        }

        public async Task<ClientDto> AddAsync(ClientDto client)
        {
            using var db = new OmniDbContext();
            await db.Database.EnsureCreatedAsync();

            await EnsureReferenceColumnExistsAsync(db);

            var model = new Client
            {
                Name = client.Name,
                ContactInfo = client.ContactInfo ?? string.Empty
            };

            db.Clients.Add(model);
            await db.SaveChangesAsync();

            // After insert we have the generated Id. Persist the Reference as C-{Id} padded to 5 digits (SMALLINT range) in the DB.
            model.Reference = $"C-{model.Id:D5}";
            await db.SaveChangesAsync();

            client.Id = model.Id;
            client.Reference = model.Reference;
            return client;
        }

        private static ClientDto MapToDto(Client c)
        {
            var dto = new ClientDto
            {
                Id = c.Id,
                Reference = string.IsNullOrWhiteSpace(c.Reference) ? $"C-{c.Id}" : c.Reference,
                Name = c.Name,
                ContactInfo = c.ContactInfo
            };

            // Attempt to parse common contact fields from JSON ContactInfo
            try
            {
                if (!string.IsNullOrWhiteSpace(c.ContactInfo))
                {
                    using var doc = JsonDocument.Parse(c.ContactInfo);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("phone", out var p)) dto.Phone = p.GetString() ?? string.Empty;
                    if (root.TryGetProperty("email", out var e)) dto.Email = e.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // ignore invalid JSON stored in ContactInfo
            }

            return dto;
        }

        private static async Task EnsureReferenceColumnExistsAsync(OmniDbContext db)
        {
            try
            {
                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info('Clients')";
                using var reader = await cmd.ExecuteReaderAsync();
                bool hasReference = false;
                while (await reader.ReadAsync())
                {
                    var name = reader[1]?.ToString(); // second column is name
                    if (string.Equals(name, "Reference", System.StringComparison.OrdinalIgnoreCase))
                    {
                        hasReference = true;
                        break;
                    }
                }

                if (!hasReference)
                {
                    // SQLite supports adding a new column with ALTER TABLE ... ADD COLUMN
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Clients ADD COLUMN Reference TEXT");
                }

                // Normalize Reference for all existing rows to padded format C-00001 (5 digits)
                // Use printf('%05d', Id) which is available in SQLite to format integers with leading zeros
                // Update all rows to ensure legacy non-padded values (e.g. 'C-3') are normalized
                await db.Database.ExecuteSqlRawAsync("UPDATE Clients SET Reference = 'C-' || printf('%05d', Id)");
            }
            catch
            {
                // Non-blocking: if schema update fails, higher-level operations will fallback to generating reference on the fly.
            }
        }
    }
}
