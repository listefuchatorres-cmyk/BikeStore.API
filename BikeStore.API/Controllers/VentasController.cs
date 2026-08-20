using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BikeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly string _cadenaConexion;

        public VentasController(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException(
                    "No se encontró ninguna conexión."
                );
        }

        // GET: api/Ventas
        [HttpGet]
        public async Task<ActionResult<List<Venta>>> ObtenerTodos()
        {
            var ventas = new List<Venta>();

            const string consulta = """
                SELECT
                    IdVenta,
                    Fecha,
                    IdCliente,
                    Subtotal,
                    IVA,
                    Total
                FROM Ventas
                ORDER BY IdVenta;
                """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var venta = new Venta
                {
                    IdVenta = lector.GetInt32(
                        lector.GetOrdinal("IdVenta")
                    ),

                    Fecha = lector.IsDBNull(
                        lector.GetOrdinal("Fecha")
                    )
                    ? null
                    : lector.GetDateTime(
                        lector.GetOrdinal("Fecha")
                    ),

                    IdCliente = lector.GetInt32(
                        lector.GetOrdinal("IdCliente")
                    ),

                    Subtotal = lector.GetDecimal(
                        lector.GetOrdinal("Subtotal")
                    ),

                    IVA = lector.GetDecimal(
                        lector.GetOrdinal("IVA")
                    ),

                    Total = lector.GetDecimal(
                        lector.GetOrdinal("Total")
                    )
                };

                ventas.Add(venta);
            }

            return Ok(ventas);
        }

        // GET: api/Ventas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> ObtenerPorId(int id)
        {
            const string consulta = """
        SELECT
            IdVenta,
            Fecha,
            IdCliente,
            Subtotal,
            IVA,
            Total
        FROM Ventas
        WHERE IdVenta = @IdVenta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdVenta",
                SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "La venta solicitada no existe.",
                    id = id
                });
            }

            var venta = new Venta
            {
                IdVenta = lector.GetInt32(
                    lector.GetOrdinal("IdVenta")
                ),

                Fecha = lector.IsDBNull(
                    lector.GetOrdinal("Fecha")
                )
                ? null
                : lector.GetDateTime(
                    lector.GetOrdinal("Fecha")
                ),

                IdCliente = lector.GetInt32(
                    lector.GetOrdinal("IdCliente")
                ),

                Subtotal = lector.GetDecimal(
                    lector.GetOrdinal("Subtotal")
                ),

                IVA = lector.GetDecimal(
                    lector.GetOrdinal("IVA")
                ),

                Total = lector.GetDecimal(
                    lector.GetOrdinal("Total")
                )
            };

            return Ok(venta);
        }

        // POST: api/Ventas
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] Venta nuevaVenta)
        {
            if (nuevaVenta.IdCliente <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El Id del cliente es obligatorio."
                });
            }

            if (nuevaVenta.Subtotal < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El subtotal no puede ser negativo."
                });
            }

            if (nuevaVenta.IVA < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El IVA no puede ser negativo."
                });
            }

            if (nuevaVenta.Total < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El total no puede ser negativo."
                });
            }

            const string sentencia = """
        INSERT INTO Ventas
        (
            IdCliente,
            Subtotal,
            IVA,
            Total
        )
        OUTPUT INSERTED.IdVenta
        VALUES
        (
            @IdCliente,
            @Subtotal,
            @IVA,
            @Total
        );
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(sentencia, conexion);

            comando.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = nuevaVenta.IdCliente;

            var parametroSubtotal = comando.Parameters.Add(
                "@Subtotal",
                SqlDbType.Decimal
            );

            parametroSubtotal.Precision = 10;
            parametroSubtotal.Scale = 2;
            parametroSubtotal.Value = nuevaVenta.Subtotal;

            var parametroIVA = comando.Parameters.Add(
                "@IVA",
                SqlDbType.Decimal
            );

            parametroIVA.Precision = 10;
            parametroIVA.Scale = 2;
            parametroIVA.Value = nuevaVenta.IVA;

            var parametroTotal = comando.Parameters.Add(
                "@Total",
                SqlDbType.Decimal
            );

            parametroTotal.Precision = 10;
            parametroTotal.Scale = 2;
            parametroTotal.Value = nuevaVenta.Total;

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
                        mensaje = "Venta registrada correctamente.",
                        idVenta = idGenerado
                    }
                );
            }
            catch (SqlException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo registrar la venta. Verifique que el cliente exista."
                });
            }
        }

        // PUT: api/Ventas/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarVenta(
            int id,
            [FromBody] Venta venta)
        {
            if (id != venta.IdVenta)
            {
                return BadRequest(new
                {
                    mensaje = "El Id de la URL no coincide con el Id de la venta."
                });
            }

            if (venta.IdCliente <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El Id del cliente es obligatorio."
                });
            }

            if (venta.Subtotal < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El subtotal no puede ser negativo."
                });
            }

            if (venta.IVA < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El IVA no puede ser negativo."
                });
            }

            if (venta.Total < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El total no puede ser negativo."
                });
            }

            const string consulta = """
        UPDATE Ventas
        SET
            IdCliente = @IdCliente,
            Subtotal = @Subtotal,
            IVA = @IVA,
            Total = @Total
        WHERE IdVenta = @IdVenta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdVenta",
                SqlDbType.Int
            ).Value = id;

            comando.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = venta.IdCliente;

            var parametroSubtotal = comando.Parameters.Add(
                "@Subtotal",
                SqlDbType.Decimal
            );

            parametroSubtotal.Precision = 10;
            parametroSubtotal.Scale = 2;
            parametroSubtotal.Value = venta.Subtotal;

            var parametroIVA = comando.Parameters.Add(
                "@IVA",
                SqlDbType.Decimal
            );

            parametroIVA.Precision = 10;
            parametroIVA.Scale = 2;
            parametroIVA.Value = venta.IVA;

            var parametroTotal = comando.Parameters.Add(
                "@Total",
                SqlDbType.Decimal
            );

            parametroTotal.Precision = 10;
            parametroTotal.Scale = 2;
            parametroTotal.Value = venta.Total;

            try
            {
                await conexion.OpenAsync();

                int filasAfectadas = await comando.ExecuteNonQueryAsync();

                if (filasAfectadas == 0)
                {
                    return NotFound(new
                    {
                        mensaje = "La venta solicitada no existe.",
                        id = id
                    });
                }

                return Ok(new
                {
                    mensaje = "Venta actualizada correctamente.",
                    id = id
                });
            }
            catch (SqlException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo actualizar la venta. Verifique que el cliente exista."
                });
            }
        }

        // DELETE: api/Ventas/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarVenta(int id)
        {
            const string consulta = """
        DELETE FROM Ventas
        WHERE IdVenta = @IdVenta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdVenta",
                SqlDbType.Int
            ).Value = id;

            try
            {
                await conexion.OpenAsync();

                int filasAfectadas = await comando.ExecuteNonQueryAsync();

                if (filasAfectadas == 0)
                {
                    return NotFound(new
                    {
                        mensaje = "La venta solicitada no existe.",
                        id = id
                    });
                }

                return Ok(new
                {
                    mensaje = "Venta eliminada correctamente.",
                    id = id
                });
            }
            catch (SqlException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo eliminar la venta."
                });
            }
        }
    }
}