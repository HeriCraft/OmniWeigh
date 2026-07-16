namespace OmniWeigh.Core.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Barcode is used in the desktop app to store image filename
        public string Barcode { get; set; } = string.Empty;
        // Prix unitaire obligatoire (par défaut 0)
        public decimal UnitPrice { get; set; } = 0m;

        // Devise, par défaut MGA
        public string Currency { get; set; } = "MGA";

        public virtual ICollection<WeighingHistory> WeighingHistories { get; set; } = new List<WeighingHistory>();
    }
}
