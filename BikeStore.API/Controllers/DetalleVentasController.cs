using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BikeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleVentasController : ControllerBase
    {
        private readonly string _cadenaConexion;

        public DetalleVentasController(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException(
                    "No se encontró ninguna conexión."
                );
        }

        // GET: api/DetalleVentas
        [HttpGet]
        public async Task<ActionResult<List<DetalleVenta>>> ObtenerTodos()
        {
            var detalles = new List<DetalleVenta>();

            const string consulta = """
                SELECT
                    IdDetalle,
                    IdVenta,
                    IdBicicleta,
                    Cantidad,
                    PrecioUnitario,
                    SubtotalDetalle
                FROM DetalleVentas
                ORDER BY IdDetalle;
                """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var detalle = new DetalleVenta
                {
                    IdDetalle = lector.GetInt32(
                        lector.GetOrdinal("IdDetalle")
                    ),

                    IdVenta = lector.GetInt32(
                        lector.GetOrdinal("IdVenta")
                    ),

                    IdBicicleta = lector.GetInt32(
                        lector.GetOrdinal("IdBicicleta")
                    ),

                    Cantidad = lector.GetInt32(
                        lector.GetOrdinal("Cantidad")
                    ),

                    PrecioUnitario = lector.GetDecimal(
                        lector.GetOrdinal("PrecioUnitario")
                    ),

                    SubtotalDetalle = lector.GetDecimal(
                        lector.GetOrdinal("SubtotalDetalle")
                    )
                };

                detalles.Add(detalle);
            }

            return Ok(detalles);
        }

        // GET: api/DetalleVentas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleVenta>> ObtenerPorId(int id)
        {
            const string consulta = """
        SELECT
            IdDetalle,
            IdVenta,
            IdBicicleta,
            Cantidad,
            PrecioUnitario,
            SubtotalDetalle
        FROM DetalleVentas
        WHERE IdDetalle = @IdDetalle;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdDetalle",
                SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "El detalle de venta solicitado no existe.",
                    id = id
                });
            }

            var detalle = new DetalleVenta
            {
                IdDetalle = lector.GetInt32(
                    lector.GetOrdinal("IdDetalle")
                ),

                IdVenta = lector.GetInt32(
                    lector.GetOrdinal("IdVenta")
                ),

                IdBicicleta = lector.GetInt32(
                    lector.GetOrdinal("IdBicicleta")
                ),

                Cantidad = lector.GetInt32(
                    lector.GetOrdinal("Cantidad")
                ),

                PrecioUnitario = lector.GetDecimal(
                    lector.GetOrdinal("PrecioUnitario")
                ),

                SubtotalDetalle = lector.GetDecimal(
                    lector.GetOrdinal("SubtotalDetalle")
                )
            };

            return Ok(detalle);
        }

        // POST: api/DetalleVentas
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] DetalleVenta nuevoDetalle)
        {
            if (nuevoDetalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            if (nuevoDetalle.PrecioUnitario < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio unitario no puede ser negativo."
                });
            }

            decimal subtotalDetalle =
                nuevoDetalle.Cantidad * nuevoDetalle.PrecioUnitario;

            const string sentencia = """
        INSERT INTO DetalleVentas
        (
            IdVenta,
            IdBicicleta,
            Cantidad,
            PrecioUnitario,
            SubtotalDetalle
        )
        OUTPUT INSERTED.IdDetalle
        VALUES
        (
            @IdVenta,
            @IdBicicleta,
            @Cantidad,
            @PrecioUnitario,
            @SubtotalDetalle
        );
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(sentencia, conexion);

            comando.Parameters.Add(
                "@IdVenta",
                SqlDbType.Int
            ).Value = nuevoDetalle.IdVenta;

            comando.Parameters.Add(
                "@IdBicicleta",
                SqlDbType.Int
            ).Value = nuevoDetalle.IdBicicleta;

            comando.Parameters.Add(
                "@Cantidad",
                SqlDbType.Int
            ).Value = nuevoDetalle.Cantidad;

            var parametroPrecio = comando.Parameters.Add(
                "@PrecioUnitario",
                SqlDbType.Decimal
            );

            parametroPrecio.Precision = 10;
            parametroPrecio.Scale = 2;
            parametroPrecio.Value = nuevoDetalle.PrecioUnitario;

            var parametroSubtotal = comando.Parameters.Add(
                "@SubtotalDetalle",
                SqlDbType.Decimal
            );

            parametroSubtotal.Precision = 10;
            parametroSubtotal.Scale = 2;
            parametroSubtotal.Value = subtotalDetalle;

            try
            {
                await conexion.OpenAsync();

                object? resultado = await comando.ExecuteScalarAsync();

                int idGenerado = Convert.ToInt32(resultado);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = idGenerado },
                    new
                    {
                        mensaje = "Detalle de venta registrado correctamente.",
                        idDetalle = idGenerado,
                        subtotalDetalle = subtotalDetalle
                    }
                );
            }
            catch (SqlException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo registrar el detalle. Verifique que la venta y la bicicleta existan."
                });
            }
        }

        // PUT: api/DetalleVentas/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarDetalle(
            int id,
            [FromBody] DetalleVenta detalle)
        {
            if (id != detalle.IdDetalle)
            {
                return BadRequest(new
                {
                    mensaje = "El Id de la URL no coincide con el Id del detalle."
                });
            }

            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            if (detalle.PrecioUnitario < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio unitario no puede ser negativo."
                });
            }

            decimal subtotalDetalle =
                detalle.Cantidad * detalle.PrecioUnitario;

            const string consulta = """
        UPDATE DetalleVentas
        SET
            IdVenta = @IdVenta,
            IdBicicleta = @IdBicicleta,
            Cantidad = @Cantidad,
            PrecioUnitario = @PrecioUnitario,
            SubtotalDetalle = @SubtotalDetalle
        WHERE IdDetalle = @IdDetalle;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdDetalle",
                SqlDbType.Int
            ).Value = id;

            comando.Parameters.Add(
                "@IdVenta",
                SqlDbType.Int
            ).Value = detalle.IdVenta;

            comando.Parameters.Add(
                "@IdBicicleta",
                SqlDbType.Int
            ).Value = detalle.IdBicicleta;

            comando.Parameters.Add(
                "@Cantidad",
                SqlDbType.Int
            ).Value = detalle.Cantidad;

            var parametroPrecio = comando.Parameters.Add(
                "@PrecioUnitario",
                SqlDbType.Decimal
            );

            parametroPrecio.Precision = 10;
            parametroPrecio.Scale = 2;
            parametroPrecio.Value = detalle.PrecioUnitario;

            var parametroSubtotal = comando.Parameters.Add(
                "@SubtotalDetalle",
                SqlDbType.Decimal
            );

            parametroSubtotal.Precision = 10;
            parametroSubtotal.Scale = 2;
            parametroSubtotal.Value = subtotalDetalle;

            try
            {
                await conexion.OpenAsync();

                int filasAfectadas = await comando.ExecuteNonQueryAsync();

                if (filasAfectadas == 0)
                {
                    return NotFound(new
                    {
                        mensaje = "El detalle de venta solicitado no existe.",
                        id = id
                    });
                }

                return Ok(new
                {
                    mensaje = "Detalle de venta actualizado correctamente.",
                    id = id,
                    subtotalDetalle = subtotalDetalle
                });
            }
            catch (SqlException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo actualizar el detalle. Verifique que la venta y la bicicleta existan."
                });
            }
        }

        // DELETE: api/DetalleVentas/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarDetalle(int id)
        {
            const string consulta = """
        DELETE FROM DetalleVentas
        WHERE IdDetalle = @IdDetalle;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdDetalle",
                SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "El detalle de venta solicitado no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Detalle de venta eliminado correctamente.",
                id = id
            });
        }
    }
}
