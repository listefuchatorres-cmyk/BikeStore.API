using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BikeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BicicletasController : ControllerBase
    {
        private readonly string _cadenaConexion;

        public BicicletasController(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException(
                    "No se encontró ninguna conexión."
                );
        }

        // GET: api/Bicicletas
        [HttpGet]
        public async Task<ActionResult<List<Bicicleta>>> ObtenerTodas()
        {
            var bicicletas = new List<Bicicleta>();

            const string consulta = """
                SELECT
                    IdBicicleta,
                    IdCategoria,
                    Marca,
                    Modelo,
                    Precio,
                    Stock,
                    Estado
                FROM Bicicletas
                ORDER BY IdBicicleta;
                """;


            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var bicicleta = new Bicicleta
                {
                    IdBicicleta = lector.GetInt32(
                        lector.GetOrdinal("IdBicicleta")
                    ),

                    IdCategoria = lector.GetInt32(
                        lector.GetOrdinal("IdCategoria")
                    ),

                    Marca = lector.GetString(
                        lector.GetOrdinal("Marca")
                    ),

                    Modelo = lector.GetString(
                        lector.GetOrdinal("Modelo")
                    ),

                    Precio = lector.GetDecimal(
                        lector.GetOrdinal("Precio")
                    ),

                    Stock = lector.GetInt32(
                        lector.GetOrdinal("Stock")
                    ),

                    Estado = lector.GetString(
                        lector.GetOrdinal("Estado")
                    )
                };

                bicicletas.Add(bicicleta);
            }

            return Ok(bicicletas);
        }

        // GET: api/Bicicletas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Bicicleta>> ObtenerPorId(int id)
        {
            const string consulta = """
        SELECT
            IdBicicleta,
            IdCategoria,
            Marca,
            Modelo,
            Precio,
            Stock,
            Estado
        FROM Bicicletas
        WHERE IdBicicleta = @IdBicicleta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add("@IdBicicleta", System.Data.SqlDbType.Int).Value = id;

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "La bicicleta solicitada no existe.",
                    id = id
                });
            }

            var bicicleta = new Bicicleta
            {
                IdBicicleta = lector.GetInt32(
                    lector.GetOrdinal("IdBicicleta")
                ),

                IdCategoria = lector.GetInt32(
                    lector.GetOrdinal("IdCategoria")
                ),

                Marca = lector.GetString(
                    lector.GetOrdinal("Marca")
                ),

                Modelo = lector.GetString(
                    lector.GetOrdinal("Modelo")
                ),

                Precio = lector.GetDecimal(
                    lector.GetOrdinal("Precio")
                ),

                Stock = lector.GetInt32(
                    lector.GetOrdinal("Stock")
                ),

                Estado = lector.GetString(
                    lector.GetOrdinal("Estado")
                )
            };

            return Ok(bicicleta);
        }

        // POST: api/Bicicletas
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] Bicicleta nuevaBicicleta)
        {
            if (nuevaBicicleta.Precio <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio debe ser mayor que 0."
                });
            }

            if (nuevaBicicleta.Stock < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El stock no puede ser negativo."
                });
            }

            const string sentencia = """
        INSERT INTO Bicicletas
        (
            IdCategoria,
            Marca,
            Modelo,
            Precio,
            Stock,
            Estado
        )
        OUTPUT INSERTED.IdBicicleta
        VALUES
        (
            @IdCategoria,
            @Marca,
            @Modelo,
            @Precio,
            @Stock,
            @Estado
        );
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(sentencia, conexion);

            comando.Parameters.Add(
                "@IdCategoria",
                System.Data.SqlDbType.Int
            ).Value = nuevaBicicleta.IdCategoria;

            comando.Parameters.Add(
                "@Marca",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = nuevaBicicleta.Marca.Trim();

            comando.Parameters.Add(
                "@Modelo",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = nuevaBicicleta.Modelo.Trim();

            var parametroPrecio = comando.Parameters.Add(
                "@Precio",
                System.Data.SqlDbType.Decimal
            );

            parametroPrecio.Precision = 10;
            parametroPrecio.Scale = 2;
            parametroPrecio.Value = nuevaBicicleta.Precio;

            comando.Parameters.Add(
                "@Stock",
                System.Data.SqlDbType.Int
            ).Value = nuevaBicicleta.Stock;

            comando.Parameters.Add(
                "@Estado",
                System.Data.SqlDbType.VarChar,
                20
            ).Value = nuevaBicicleta.Estado.Trim();

            await conexion.OpenAsync();

            object? resultado = await comando.ExecuteScalarAsync();

            int idGenerado = Convert.ToInt32(resultado);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = idGenerado },
                new
                {
                    mensaje = "Bicicleta registrada correctamente.",
                    idBicicleta = idGenerado
                }
            );
        }

        // PUT: api/Bicicletas/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarBicicleta(
            int id,
            [FromBody] Bicicleta bicicleta)
        {
            if (id != bicicleta.IdBicicleta)
            {
                return BadRequest(new
                {
                    mensaje = "El Id de la URL no coincide con el Id de la bicicleta."
                });
            }

            if (bicicleta.Precio <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio debe ser mayor que 0."
                });
            }

            if (bicicleta.Stock < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El stock no puede ser negativo."
                });
            }

            const string consulta = """
        UPDATE Bicicletas
        SET
            IdCategoria = @IdCategoria,
            Marca = @Marca,
            Modelo = @Modelo,
            Precio = @Precio,
            Stock = @Stock,
            Estado = @Estado
        WHERE IdBicicleta = @IdBicicleta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdBicicleta",
                System.Data.SqlDbType.Int
            ).Value = id;

            comando.Parameters.Add(
                "@IdCategoria",
                System.Data.SqlDbType.Int
            ).Value = bicicleta.IdCategoria;

            comando.Parameters.Add(
                "@Marca",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = bicicleta.Marca.Trim();

            comando.Parameters.Add(
                "@Modelo",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = bicicleta.Modelo.Trim();

            var parametroPrecio = comando.Parameters.Add(
                "@Precio",
                System.Data.SqlDbType.Decimal
            );

            parametroPrecio.Precision = 10;
            parametroPrecio.Scale = 2;
            parametroPrecio.Value = bicicleta.Precio;

            comando.Parameters.Add(
                "@Stock",
                System.Data.SqlDbType.Int
            ).Value = bicicleta.Stock;

            comando.Parameters.Add(
                "@Estado",
                System.Data.SqlDbType.VarChar,
                20
            ).Value = bicicleta.Estado.Trim();

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "La bicicleta solicitada no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Bicicleta actualizada correctamente.",
                id = id
            });
        }

        // DELETE: api/Bicicletas/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarBicicleta(int id)
        {
            const string consulta = """
        DELETE FROM Bicicletas
        WHERE IdBicicleta = @IdBicicleta;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdBicicleta",
                System.Data.SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "La bicicleta solicitada no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Bicicleta eliminada correctamente.",
                id = id
            });
        }

    }
}