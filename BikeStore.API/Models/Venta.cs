namespace BikeStore.API.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }

        public DateTime? Fecha { get; set; }

        public int IdCliente { get; set; }

        public decimal Subtotal { get; set; }

        public decimal IVA { get; set; }

        public decimal Total { get; set; }
    }
}
