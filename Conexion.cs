using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace TaqueriaProyecto
{
    internal class Conexion
    {
        private string servidor = "localhost";
        private string puerto = "3308";
        private string baseDatos = "taqueria";
        private string usuario = "root";
        private string password = "";

        private MySqlConnection conexion;

        public string Servidor { get => servidor; set => servidor = value; }
        public string Puerto { get => puerto; set => puerto = value; }
        public string BaseDatos { get => baseDatos; set => baseDatos = value; }
        public string Usuario { get => usuario; set => usuario = value; }
        public string Password { get => password; set => password = value; }

        public Conexion()
        {
            string cadenaConexion = $"Server={servidor};Port={puerto};Database={baseDatos};Uid={usuario};Pwd={password};";
            conexion = new MySqlConnection(cadenaConexion);
        }

        public MySqlConnection AbrirConexion()
        {
            try
            {
                if (conexion.State == ConnectionState.Closed)
                {
                    conexion.Open();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al abrir la conexión: " + ex.Message);
            }

            return conexion;
        }

        public MySqlConnection CerrarConexion()
        {
            try
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                {
                    conexion.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión: " + ex.Message);
            }

            return conexion;
        }

        private DataTable EjecutarConsulta(string query, string mensajeError)
        {
            DataTable dt = new DataTable();

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(mensajeError + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }

            return dt;
        }

        // ==================== PRODUCTOS ====================

        public bool Insertar(string nombre, decimal precio, string tipo, bool disponible)
        {
            string query = "INSERT INTO productos (nombre, precio, tipo, disponible) VALUES (@nombre, @precio, @tipo, @disponible)";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@tipo", tipo);
                    cmd.Parameters.AddWithValue("@disponible", disponible);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar producto: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DataTable Observar()
        {
            string query = @"SELECT p.id_producto, p.nombre, p.precio, p.tipo, p.disponible,
                                    t.guiso, t.tortilla,
                                    b.tipo AS tipo_bebida, b.tamano
                             FROM productos p
                             LEFT JOIN tacos t ON p.id_producto = t.id_producto
                             LEFT JOIN bebidas b ON p.id_producto = b.id_producto
                             ORDER BY p.id_producto";

            return EjecutarConsulta(query, "Error al obtener los productos: ");
        }

        public bool Actualizar(int id, string nombre, decimal precio, string tipo, bool disponible)
        {
            string query = "UPDATE productos SET nombre = @nombre, precio = @precio, tipo = @tipo, disponible = @disponible WHERE id_producto = @id";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@tipo", tipo);
                    cmd.Parameters.AddWithValue("@disponible", disponible);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar producto: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public int InsertarProducto(Producto producto)
        {
            string queryProducto = "INSERT INTO productos (nombre, precio, tipo, disponible) VALUES (@nombre, @precio, @tipo, @disponible)";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int idProducto;

                        using (MySqlCommand cmd = new MySqlCommand(queryProducto, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                            cmd.Parameters.AddWithValue("@precio", producto.Precio);
                            cmd.Parameters.AddWithValue("@tipo", producto.Tipo);
                            cmd.Parameters.AddWithValue("@disponible", producto.Disponible);

                            cmd.ExecuteNonQuery();
                            idProducto = Convert.ToInt32(cmd.LastInsertedId);
                        }

                        producto.IdProducto = idProducto;

                        if (producto is Taco taco)
                        {
                            string queryTaco = "INSERT INTO tacos (id_producto, guiso, tortilla) VALUES (@id, @guiso, @tortilla)";

                            using (MySqlCommand cmd = new MySqlCommand(queryTaco, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idProducto);
                                cmd.Parameters.AddWithValue("@guiso", taco.Guiso);
                                cmd.Parameters.AddWithValue("@tortilla", taco.Tortilla);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (producto is Bebida bebida)
                        {
                            string queryBebida = "INSERT INTO bebidas (id_producto, tipo, tamano) VALUES (@id, @tipo, @tamano)";

                            using (MySqlCommand cmd = new MySqlCommand(queryBebida, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idProducto);
                                cmd.Parameters.AddWithValue("@tipo", bebida.TipoB);
                                cmd.Parameters.AddWithValue("@tamano", bebida.Tamanio);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return idProducto;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar producto: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public bool ActualizarProducto(Producto producto, string tipoAnterior)
        {
            string queryProducto = "UPDATE productos SET nombre = @nombre, precio = @precio, tipo = @tipo, disponible = @disponible WHERE id_producto = @id";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(queryProducto, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                            cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                            cmd.Parameters.AddWithValue("@precio", producto.Precio);
                            cmd.Parameters.AddWithValue("@tipo", producto.Tipo);
                            cmd.Parameters.AddWithValue("@disponible", producto.Disponible);

                            cmd.ExecuteNonQuery();
                        }

                        // Si el producto cambió de tipo, se elimina la fila de la tabla anterior.
                        if (string.Equals(tipoAnterior, "Taco", StringComparison.OrdinalIgnoreCase) && !(producto is Taco))
                        {
                            using (MySqlCommand cmd = new MySqlCommand("DELETE FROM tacos WHERE id_producto = @id", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (string.Equals(tipoAnterior, "Bebida", StringComparison.OrdinalIgnoreCase) && !(producto is Bebida))
                        {
                            using (MySqlCommand cmd = new MySqlCommand("DELETE FROM bebidas WHERE id_producto = @id", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (producto is Taco taco)
                        {
                            string updateTaco = "UPDATE tacos SET guiso = @guiso, tortilla = @tortilla WHERE id_producto = @id";
                            int filasTaco;

                            using (MySqlCommand cmd = new MySqlCommand(updateTaco, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                cmd.Parameters.AddWithValue("@guiso", taco.Guiso);
                                cmd.Parameters.AddWithValue("@tortilla", taco.Tortilla);

                                filasTaco = cmd.ExecuteNonQuery();
                            }

                            if (filasTaco == 0)
                            {
                                string insertTaco = "INSERT INTO tacos (id_producto, guiso, tortilla) VALUES (@id, @guiso, @tortilla)";

                                using (MySqlCommand cmd = new MySqlCommand(insertTaco, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                    cmd.Parameters.AddWithValue("@guiso", taco.Guiso);
                                    cmd.Parameters.AddWithValue("@tortilla", taco.Tortilla);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else if (producto is Bebida bebida)
                        {
                            string updateBebida = "UPDATE bebidas SET tipo = @tipo, tamano = @tamano WHERE id_producto = @id";
                            int filasBebida;

                            using (MySqlCommand cmd = new MySqlCommand(updateBebida, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                cmd.Parameters.AddWithValue("@tipo", bebida.TipoB);
                                cmd.Parameters.AddWithValue("@tamano", bebida.Tamanio);

                                filasBebida = cmd.ExecuteNonQuery();
                            }

                            if (filasBebida == 0)
                            {
                                string insertBebida = "INSERT INTO bebidas (id_producto, tipo, tamano) VALUES (@id, @tipo, @tamano)";

                                using (MySqlCommand cmd = new MySqlCommand(insertBebida, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@id", producto.IdProducto);
                                    cmd.Parameters.AddWithValue("@tipo", bebida.TipoB);
                                    cmd.Parameters.AddWithValue("@tamano", bebida.Tamanio);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar producto: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public bool Eliminar(int id)
        {
            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM detalle_orden WHERE id_producto = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            int enOrdenes = Convert.ToInt32(cmd.ExecuteScalar());

                            if (enOrdenes > 0)
                            {
                                throw new Exception("No se puede eliminar el producto porque está incluido en una o más órdenes. Márcalo como no disponible.");
                            }
                        }

                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM tacos WHERE id_producto = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM bebidas WHERE id_producto = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        int filas;

                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM productos WHERE id_producto = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            filas = cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return filas > 0;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar producto: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        // ==================== TACOS ====================

        public DataTable ObservarTacos()
        {
            string query = @"SELECT p.id_producto, p.nombre, p.precio, p.tipo, p.disponible,
                                    t.guiso, t.tortilla
                             FROM productos p
                             INNER JOIN tacos t ON p.id_producto = t.id_producto
                             ORDER BY p.id_producto";

            return EjecutarConsulta(query, "Error al obtener los tacos: ");
        }

        // ==================== BEBIDAS ====================

        public DataTable ObservarBebidas()
        {
            string query = @"SELECT p.id_producto, p.nombre, p.precio, p.tipo, p.disponible,
                                    b.tipo AS tipo_bebida, b.tamano
                             FROM productos p
                             INNER JOIN bebidas b ON p.id_producto = b.id_producto
                             ORDER BY p.id_producto";

            return EjecutarConsulta(query, "Error al obtener las bebidas: ");
        }

        // ==================== CLIENTES ====================

        public bool InsertarCliente(Cliente cliente)
        {
            string query = "INSERT INTO clientes (nombre, telefono) VALUES (@nombre, @telefono)";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@telefono", cliente.Telefono);

                    cmd.ExecuteNonQuery();
                    cliente.IdCliente = Convert.ToInt32(cmd.LastInsertedId);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar cliente: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DataTable ObservarClientes()
        {
            string query = "SELECT id_cliente, nombre, telefono FROM clientes ORDER BY id_cliente";

            return EjecutarConsulta(query, "Error al obtener los clientes: ");
        }

        public bool ActualizarCliente(Cliente cliente)
        {
            string query = "UPDATE clientes SET nombre = @nombre, telefono = @telefono WHERE id_cliente = @id";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", cliente.IdCliente);
                    cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@telefono", cliente.Telefono);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar cliente: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public bool EliminarCliente(int id)
        {
            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Las órdenes pueden quedar sin cliente porque id_cliente acepta NULL.
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE ordenes SET id_cliente = NULL WHERE id_cliente = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        int filas;

                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM clientes WHERE id_cliente = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            filas = cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return filas > 0;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar cliente: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        // ==================== EMPLEADOS ====================

        public bool InsertarEmpleado(Empleado empleado)
        {
            string query = "INSERT INTO empleados (nombre, puesto) VALUES (@nombre, @puesto)";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", empleado.Nombre);
                    cmd.Parameters.AddWithValue("@puesto", empleado.Puesto);

                    cmd.ExecuteNonQuery();
                    empleado.IdEmpleado = Convert.ToInt32(cmd.LastInsertedId);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar empleado: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DataTable ObservarEmpleados()
        {
            string query = "SELECT id_empleado, nombre, puesto FROM empleados ORDER BY id_empleado";

            return EjecutarConsulta(query, "Error al obtener los empleados: ");
        }

        public bool ActualizarEmpleado(Empleado empleado)
        {
            string query = "UPDATE empleados SET nombre = @nombre, puesto = @puesto WHERE id_empleado = @id";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", empleado.IdEmpleado);
                    cmd.Parameters.AddWithValue("@nombre", empleado.Nombre);
                    cmd.Parameters.AddWithValue("@puesto", empleado.Puesto);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar empleado: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public bool EliminarEmpleado(int id)
        {
            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM ordenes WHERE id_empleado = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            int enOrdenes = Convert.ToInt32(cmd.ExecuteScalar());

                            if (enOrdenes > 0)
                            {
                                throw new Exception("No se puede eliminar el empleado porque tiene órdenes registradas.");
                            }
                        }

                        int filas;

                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM empleados WHERE id_empleado = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            filas = cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return filas > 0;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar empleado: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        // ==================== ÓRDENES ====================

        public int InsertarOrden(Orden orden, List<DetalleOrden> detalles)
        {
            if (detalles == null || detalles.Count == 0)
            {
                throw new Exception("La orden debe tener al menos un producto.");
            }

            decimal total = 0;

            foreach (DetalleOrden detalle in detalles)
            {
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                total += detalle.Subtotal;
            }

            orden.Total = total;

            string queryOrden = "INSERT INTO ordenes (id_cliente, id_empleado, fecha, total) VALUES (@id_cliente, @id_empleado, @fecha, @total)";
            string queryDetalle = "INSERT INTO detalle_orden (id_orden, id_producto, cantidad, precio_unitario, subtotal) VALUES (@id_orden, @id_producto, @cantidad, @precio_unitario, @subtotal)";

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int idOrden;

                        using (MySqlCommand cmd = new MySqlCommand(queryOrden, conn, trans))
                        {
                            MySqlParameter parametroCliente = cmd.Parameters.Add("@id_cliente", MySqlDbType.Int32);

                            if (orden.IdCliente > 0)
                            {
                                parametroCliente.Value = orden.IdCliente;
                            }
                            else
                            {
                                parametroCliente.Value = DBNull.Value;
                            }

                            cmd.Parameters.AddWithValue("@id_empleado", orden.IdEmpleado);
                            cmd.Parameters.AddWithValue("@fecha", orden.Fecha);
                            cmd.Parameters.AddWithValue("@total", orden.Total);

                            cmd.ExecuteNonQuery();
                            idOrden = Convert.ToInt32(cmd.LastInsertedId);
                        }

                        orden.IdOrden = idOrden;

                        foreach (DetalleOrden detalle in detalles)
                        {
                            detalle.IdOrden = idOrden;

                            using (MySqlCommand cmd = new MySqlCommand(queryDetalle, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id_orden", idOrden);
                                cmd.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                                cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                                cmd.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                                cmd.Parameters.AddWithValue("@subtotal", detalle.Subtotal);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return idOrden;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar la orden: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DataTable ObservarOrdenes()
        {
            string query = @"SELECT o.id_orden, o.id_cliente,
                                    IFNULL(c.nombre, '(Sin cliente)') AS cliente,
                                    o.id_empleado, e.nombre AS empleado,
                                    o.fecha, o.total
                             FROM ordenes o
                             LEFT JOIN clientes c ON o.id_cliente = c.id_cliente
                             INNER JOIN empleados e ON o.id_empleado = e.id_empleado
                             ORDER BY o.id_orden DESC";

            return EjecutarConsulta(query, "Error al obtener las órdenes: ");
        }

        public DataTable ObservarDetalleOrden(int idOrden)
        {
            string query = @"SELECT d.id_detalle, d.id_orden, d.id_producto,
                                    p.nombre AS producto,
                                    d.cantidad, d.precio_unitario, d.subtotal
                             FROM detalle_orden d
                             INNER JOIN productos p ON d.id_producto = p.id_producto
                             WHERE d.id_orden = @id_orden
                             ORDER BY d.id_detalle";

            DataTable dt = new DataTable();

            try
            {
                MySqlConnection conn = AbrirConexion();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_orden", idOrden);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el detalle de la orden: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }

            return dt;
        }

        public DataTable ObservarProductosDisponibles()
        {
            string query = @"SELECT p.id_producto, p.nombre, p.precio, p.tipo,
                                    CONCAT(p.nombre, ' (', p.tipo, ') - $', p.precio) AS descripcion
                             FROM productos p
                             WHERE p.disponible = 1
                             ORDER BY p.tipo, p.nombre";

            return EjecutarConsulta(query, "Error al obtener los productos disponibles: ");
        }
    }
}
