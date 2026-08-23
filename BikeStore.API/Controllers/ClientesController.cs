using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BikeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly string _cadenaConexion;

        public ClientesController(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException(
                    "No se encontró ninguna conexión."
                );
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> ObtenerTodos()
        {
            var clientes = new List<Cliente>();

            const string consulta = """
                SELECT
                    IdCliente,
                    Cedula,
                    Nombres,
                    Apellidos,
                    Telefono,
                    Correo
                FROM Clientes
                ORDER BY IdCliente;
                """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var cliente = new Cliente
                {
                    IdCliente = lector.GetInt32(
                        lector.GetOrdinal("IdCliente")
                    ),

                    Cedula = lector.GetString(
                        lector.GetOrdinal("Cedula")
                    ),

                    Nombres = lector.GetString(
                        lector.GetOrdinal("Nombres")
                    ),

                    Apellidos = lector.GetString(
                        lector.GetOrdinal("Apellidos")
                    ),

                    Telefono = lector.IsDBNull(
                        lector.GetOrdinal("Telefono")
                    )
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("Telefono")
                    ),

                    Correo = lector.IsDBNull(
                        lector.GetOrdinal("Correo")
                    )
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("Correo")
                    )
                };

                clientes.Add(cliente);
            }

            return Ok(clientes);
        }

        // GET: api/Clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> ObtenerPorId(int id)
        {
            const string consulta = """
        SELECT
            IdCliente,
            Cedula,
            Nombres,
            Apellidos,
            Telefono,
            Correo
        FROM Clientes
        WHERE IdCliente = @IdCliente;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCliente",
                System.Data.SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "El cliente solicitado no existe.",
                    id = id
                });
            }

            var cliente = new Cliente
            {
                IdCliente = lector.GetInt32(
                    lector.GetOrdinal("IdCliente")
                ),

                Cedula = lector.GetString(
                    lector.GetOrdinal("Cedula")
                ),

                Nombres = lector.GetString(
                    lector.GetOrdinal("Nombres")
                ),

                Apellidos = lector.GetString(
                    lector.GetOrdinal("Apellidos")
                ),

                Telefono = lector.IsDBNull(
                    lector.GetOrdinal("Telefono")
                )
                ? null
                : lector.GetString(
                    lector.GetOrdinal("Telefono")
                ),

                Correo = lector.IsDBNull(
                    lector.GetOrdinal("Correo")
                )
                ? null
                : lector.GetString(
                    lector.GetOrdinal("Correo")
                )
            };

            return Ok(cliente);
        }

        // GET: api/Clientes/buscar/cedula/{cedula}
        [HttpGet("buscar/cedula/{cedula}")]
        public async Task<ActionResult<Cliente>> BuscarPorCedula(string cedula)
        {
            const string consulta = """
        SELECT
            IdCliente,
            Cedula,
            Nombres,
            Apellidos,
            Telefono,
            Correo
        FROM Clientes
        WHERE Cedula = @Cedula;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@Cedula",
                System.Data.SqlDbType.VarChar,
                10
            ).Value = cedula.Trim();

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
            {
                return NotFound(new
                {
                    mensaje = "No se encontró ningún cliente con esa cédula.",
                    cedula = cedula
                });
            }

            var cliente = new Cliente
            {
                IdCliente = lector.GetInt32(
                    lector.GetOrdinal("IdCliente")
                ),

                Cedula = lector.GetString(
                    lector.GetOrdinal("Cedula")
                ),

                Nombres = lector.GetString(
                    lector.GetOrdinal("Nombres")
                ),

                Apellidos = lector.GetString(
                    lector.GetOrdinal("Apellidos")
                ),

                Telefono = lector.IsDBNull(
                    lector.GetOrdinal("Telefono")
                )
                ? null
                : lector.GetString(
                    lector.GetOrdinal("Telefono")
                ),

                Correo = lector.IsDBNull(
                    lector.GetOrdinal("Correo")
                )
                ? null
                : lector.GetString(
                    lector.GetOrdinal("Correo")
                )
            };

            return Ok(cliente);
        }

        // GET: api/Clientes/buscar/apellido/{apellido}
        [HttpGet("buscar/apellido/{apellido}")]
        public async Task<ActionResult<List<Cliente>>> BuscarPorApellido(string apellido)
        {
            var clientes = new List<Cliente>();

            const string consulta = """
        SELECT
            IdCliente,
            Cedula,
            Nombres,
            Apellidos,
            Telefono,
            Correo
        FROM Clientes
        WHERE Apellidos LIKE @Apellidos
        ORDER BY Apellidos, Nombres;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@Apellidos",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = "%" + apellido.Trim() + "%";

            await conexion.OpenAsync();

            await using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var cliente = new Cliente
                {
                    IdCliente = lector.GetInt32(
                        lector.GetOrdinal("IdCliente")
                    ),

                    Cedula = lector.GetString(
                        lector.GetOrdinal("Cedula")
                    ),

                    Nombres = lector.GetString(
                        lector.GetOrdinal("Nombres")
                    ),

                    Apellidos = lector.GetString(
                        lector.GetOrdinal("Apellidos")
                    ),

                    Telefono = lector.IsDBNull(
                        lector.GetOrdinal("Telefono")
                    )
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("Telefono")
                    ),

                    Correo = lector.IsDBNull(
                        lector.GetOrdinal("Correo")
                    )
                    ? null
                    : lector.GetString(
                        lector.GetOrdinal("Correo")
                    )
                };

                clientes.Add(cliente);
            }

            return Ok(clientes);
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] Cliente nuevoCliente)
        {
            if (string.IsNullOrWhiteSpace(nuevoCliente.Cedula))
            {
                return BadRequest(new
                {
                    mensaje = "La cédula es obligatoria."
                });
            }

            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombres))
            {
                return BadRequest(new
                {
                    mensaje = "Los nombres son obligatorios."
                });
            }

            if (string.IsNullOrWhiteSpace(nuevoCliente.Apellidos))
            {
                return BadRequest(new
                {
                    mensaje = "Los apellidos son obligatorios."
                });
            }

            const string sentencia = """
        INSERT INTO Clientes
        (
            Cedula,
            Nombres,
            Apellidos,
            Telefono,
            Correo
        )
        OUTPUT INSERTED.IdCliente
        VALUES
        (
            @Cedula,
            @Nombres,
            @Apellidos,
            @Telefono,
            @Correo
        );
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(sentencia, conexion);

            comando.Parameters.Add(
                "@Cedula",
                System.Data.SqlDbType.VarChar,
                10
            ).Value = nuevoCliente.Cedula.Trim();

            comando.Parameters.Add(
                "@Nombres",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = nuevoCliente.Nombres.Trim();

            comando.Parameters.Add(
                "@Apellidos",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = nuevoCliente.Apellidos.Trim();

            comando.Parameters.Add(
                "@Telefono",
                System.Data.SqlDbType.VarChar,
                15
            ).Value = string.IsNullOrWhiteSpace(nuevoCliente.Telefono)
                ? DBNull.Value
                : nuevoCliente.Telefono.Trim();

            comando.Parameters.Add(
                "@Correo",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = string.IsNullOrWhiteSpace(nuevoCliente.Correo)
                ? DBNull.Value
                : nuevoCliente.Correo.Trim();

            await conexion.OpenAsync();

            object? resultado = await comando.ExecuteScalarAsync();

            int idGenerado = Convert.ToInt32(resultado);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = idGenerado },
                new
                {
                    mensaje = "Cliente registrado correctamente.",
                    idCliente = idGenerado
                }
            );
        }

        // PUT: api/Clientes/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarCliente(
            int id,
            [FromBody] Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return BadRequest(new
                {
                    mensaje = "El Id de la URL no coincide con el Id del cliente."
                });
            }

            const string consulta = """
        UPDATE Clientes
        SET
            Cedula = @Cedula,
            Nombres = @Nombres,
            Apellidos = @Apellidos,
            Telefono = @Telefono,
            Correo = @Correo
        WHERE IdCliente = @IdCliente;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCliente",
                System.Data.SqlDbType.Int
            ).Value = id;

            comando.Parameters.Add(
                "@Cedula",
                System.Data.SqlDbType.VarChar,
                10
            ).Value = cliente.Cedula.Trim();

            comando.Parameters.Add(
                "@Nombres",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = cliente.Nombres.Trim();

            comando.Parameters.Add(
                "@Apellidos",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = cliente.Apellidos.Trim();

            comando.Parameters.Add(
                "@Telefono",
                System.Data.SqlDbType.VarChar,
                15
            ).Value = string.IsNullOrWhiteSpace(cliente.Telefono)
                ? DBNull.Value
                : cliente.Telefono.Trim();

            comando.Parameters.Add(
                "@Correo",
                System.Data.SqlDbType.VarChar,
                100
            ).Value = string.IsNullOrWhiteSpace(cliente.Correo)
                ? DBNull.Value
                : cliente.Correo.Trim();

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "El cliente solicitado no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Cliente actualizado correctamente.",
                id = id
            });
        }

        // DELETE: api/Clientes/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarCliente(int id)
        {
            const string consulta = """
        DELETE FROM Clientes
        WHERE IdCliente = @IdCliente;
        """;

            await using var conexion = new SqlConnection(_cadenaConexion);
            await using var comando = new SqlCommand(consulta, conexion);

            comando.Parameters.Add(
                "@IdCliente",
                System.Data.SqlDbType.Int
            ).Value = id;

            await conexion.OpenAsync();

            int filasAfectadas = await comando.ExecuteNonQueryAsync();

            if (filasAfectadas == 0)
            {
                return NotFound(new
                {
                    mensaje = "El cliente solicitado no existe.",
                    id = id
                });
            }

            return Ok(new
            {
                mensaje = "Cliente eliminado correctamente.",
                id = id
            });
        }
    }
}
