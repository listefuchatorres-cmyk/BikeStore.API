namespace BikeStore.API.Models
{
    public class DetalleVenta
    {
        public int IdDetalle { get; set; }

        public int IdVenta { get; set; }

        public int IdBicicleta { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal SubtotalDetalle { get; set; }
    }
}
