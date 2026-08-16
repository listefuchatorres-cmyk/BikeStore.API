using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BikeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly string _cadenaConexion;

        public CategoriasController(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException(
                    "No se encontró ninguna conexión."
                );
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<List<Categoria>>> ObtenerTodas()
        {
            var categorias = new List<Categoria>();

            const string consulta = """
                SELECT
                    IdCategoria,
                    Nombre,
                    Descripcion,
                    Activo
                FROM Categorias
                ORDER BY IdCategoria;
                """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var categoria = new Categoria
                {
                    IdCategoria = lector.GetInt32(
                        lector.GetOrdinal("IdCategoria")
                    ),

                    Nombre = lector.GetString(
                        lector.GetOrdinal("Nombre")
                    ),

                    Descripcion = lector.IsDBNull(
                        lector.GetOrdinal("Descripcion")
                    )
                        ? null
                        : lector.GetString(
                            lector.GetOrdinal("Descripcion")
                        ),

                    Activo = lector.GetBoolean(
                        lector.GetOrdinal("Activo")
                    )
                };

                categorias.Add(categoria);
            }

            return Ok(categorias);
        }

        // GET: api/Categorias/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> ObtenerPorId(int id)
        {
            const string consulta = """
        SELECT
            IdCategoria,
            Nombre,
            Descripcion,
            Activo
        FROM Categorias
        WHERE IdCategoria = @IdCategoria;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCategoria",
                System.Data.SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "La categoría solicitada no existe.",
                    id = id
                });
            }

            var categoria = new Categoria
            {
                IdCategoria = lector.GetInt32(
                    lector.GetOrdinal("IdCategoria")
                ),

                Nombre = lector.GetString(
                    lector.GetOrdinal("Nombre")
                ),

                Descripcion = lector.IsDBNull(
                    lector.GetOrdinal("Descripcion")
                )
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("Descripcion")
                    ),

                Activo = lector.GetBoolean(
                    lector.GetOrdinal("Activo")
                )
            };

            return Ok(categoria);
        }

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] Categoria nuevaCategoria)
        {
            if (string.IsNullOrWhiteSpace(nuevaCategoria.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la categoría es obligatorio."
                });
            }

            const string sentencia = """
        INSERT INTO Categorias
        (
            Nombre,
            Descripcion,
            Activo
        )
        OUTPUT INSERTED.IdCategoria
        VALUES
        (
            @Nombre,
            @Descripcion,
            @Activo
        );
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(sentencia, conexion);

            comando.Parameters.Add(
                "@Nombre",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = nuevaCategoria.Nombre.Trim();

            comando.Parameters.Add(
                "@Descripcion",
                System.Data.SqlDbType.VarChar,
                200
            ).Value = string.IsNullOrWhiteSpace(nuevaCategoria.Descripcion)
                ? DBNull.Value
                : nuevaCategoria.Descripcion.Trim();

            comando.Parameters.Add(
                "@Activo",
                System.Data.SqlDbType.Bit
            ).Value = nuevaCategoria.Activo;

            await conexion.OpenAsync();

            object? resultado = await comando.ExecuteScalarAsync();

            int idGenerado = Convert.ToInt32(resultado);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = idGenerado },
                new
                {
                    mensaje = "Categoría registrada correctamente.",
                    idCategoria = idGenerado
                }
            );
        }

        // PUT: api/Categorias/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarCategoria(
            int id,
            [FromBody] Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return BadRequest(new
                {
                    mensaje = "El Id de la URL no coincide con el Id de la categoría."
                });
            }

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la categoría es obligatorio."
                });
            }

            const string consulta = """
        UPDATE Categorias
        SET
            Nombre = @Nombre,
            Descripcion = @Descripcion,
            Activo = @Activo
        WHERE IdCategoria = @IdCategoria;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCategoria",
                System.Data.SqlDbType.Int
            ).Value = id;

            comando.Parameters.Add(
                "@Nombre",
                System.Data.SqlDbType.VarChar,
                50
            ).Value = categoria.Nombre.Trim();

            comando.Parameters.Add(
                "@Descripcion",
                System.Data.SqlDbType.VarChar,
                200
            ).Value = string.IsNullOrWhiteSpace(categoria.Descripcion)
                ? DBNull.Value
                : categoria.Descripcion.Trim();

            comando.Parameters.Add(
                "@Activo",
                System.Data.SqlDbType.Bit
            ).Value = categoria.Activo;

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "La categoría solicitada no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Categoría actualizada correctamente.",
                id = id
            });
        }

        // DELETE: api/Categorias/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            const string consulta = """
        DELETE FROM Categorias
        WHERE IdCategoria = @IdCategoria;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCategoria",
                System.Data.SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "La categoría solicitada no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Categoría eliminada correctamente.",
                id = id
            });
        }
    }
}