using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace TaqueriaProyecto
{
    public partial class FormOrdenes : Form
    {
        private Conexion conexion = new Conexion();
        private DataTable dtDetalle;

        public FormOrdenes()
        {
            InitializeComponent();
            ConfigurarTablaDetalle();
        }

        private void FormOrdenes_Load(object sender, EventArgs e)
        {
            CargarCombos();
            CargarOrdenes();
            dtpFecha.Value = DateTime.Now;
            lblTotalOrden.Text = "$0.00";
        }

        private void ConfigurarTablaDetalle()
        {
            dtDetalle = new DataTable();
            dtDetalle.Columns.Add("id_producto", typeof(int));
            dtDetalle.Columns.Add("producto", typeof(string));
            dtDetalle.Columns.Add("precio_unitario", typeof(decimal));
            dtDetalle.Columns.Add("cantidad", typeof(int));
            dtDetalle.Columns.Add("subtotal", typeof(decimal));

            dataGridViewDetalle.DataSource = dtDetalle;

            // Evita que al ordenar por una columna el índice visual no coincida con el DataTable.
            foreach (DataGridViewColumn columna in dataGridViewDetalle.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void CargarCombos()
        {
            try
            {
                DataTable productos = conexion.ObservarProductosDisponibles();
                cmbProducto.DataSource = productos;
                cmbProducto.DisplayMember = "descripcion";
                cmbProducto.ValueMember = "id_producto";

                DataTable clientes = conexion.ObservarClientes();
                DataRow sinCliente = clientes.NewRow();
                sinCliente["id_cliente"] = 0;
                sinCliente["nombre"] = "(Sin cliente)";
                sinCliente["telefono"] = "";
                clientes.Rows.InsertAt(sinCliente, 0);

                cmbCliente.DataSource = clientes;
                cmbCliente.DisplayMember = "nombre";
                cmbCliente.ValueMember = "id_cliente";

                DataTable empleados = conexion.ObservarEmpleados();
                cmbEmpleado.DataSource = empleados;
                cmbEmpleado.DisplayMember = "nombre";
                cmbEmpleado.ValueMember = "id_empleado";

                if (empleados.Rows.Count == 0)
                {
                    MessageBox.Show("No hay empleados registrados. Registra un empleado antes de crear órdenes.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CargarOrdenes()
        {
            try
            {
                dataGridViewOrdenes.DataSource = conexion.ObservarOrdenes();

                if (dataGridViewOrdenes.Columns["id_orden"] != null) dataGridViewOrdenes.Columns["id_orden"].HeaderText = "ID";
                if (dataGridViewOrdenes.Columns["id_cliente"] != null) dataGridViewOrdenes.Columns["id_cliente"].Visible = false;
                if (dataGridViewOrdenes.Columns["cliente"] != null) dataGridViewOrdenes.Columns["cliente"].HeaderText = "Cliente";
                if (dataGridViewOrdenes.Columns["id_empleado"] != null) dataGridViewOrdenes.Columns["id_empleado"].Visible = false;
                if (dataGridViewOrdenes.Columns["empleado"] != null) dataGridViewOrdenes.Columns["empleado"].HeaderText = "Empleado";
                if (dataGridViewOrdenes.Columns["fecha"] != null) dataGridViewOrdenes.Columns["fecha"].HeaderText = "Fecha";

                if (dataGridViewOrdenes.Columns["total"] != null)
                {
                    dataGridViewOrdenes.Columns["total"].HeaderText = "Total";
                    dataGridViewOrdenes.Columns["total"].DefaultCellStyle.Format = "C2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAgregarProducto_Click(object sender, EventArgs e)
        {
            if (cmbProducto.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un producto.");
                return;
            }

            DataRowView producto = cmbProducto.SelectedItem as DataRowView;

            if (producto == null)
            {
                MessageBox.Show("Selecciona un producto válido.");
                return;
            }

            int idProducto = Convert.ToInt32(producto["id_producto"]);
            string nombre = producto["nombre"].ToString();
            decimal precio = Convert.ToDecimal(producto["precio"]);
            int cantidad = Convert.ToInt32(numCantidad.Value);

            DataRow[] existentes = dtDetalle.Select("id_producto = " + idProducto);

            if (existentes.Length > 0)
            {
                DataRow fila = existentes[0];
                int nuevaCantidad = Convert.ToInt32(fila["cantidad"]) + cantidad;
                fila["cantidad"] = nuevaCantidad;
                fila["subtotal"] = nuevaCantidad * precio;
            }
            else
            {
                DataRow fila = dtDetalle.NewRow();
                fila["id_producto"] = idProducto;
                fila["producto"] = nombre;
                fila["precio_unitario"] = precio;
                fila["cantidad"] = cantidad;
                fila["subtotal"] = cantidad * precio;
                dtDetalle.Rows.Add(fila);
            }

            RecalcularTotal();
            numCantidad.Value = 1;
        }

        private void btnQuitarDetalle_Click(object sender, EventArgs e)
        {
            if (dataGridViewDetalle.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un renglón del detalle.");
                return;
            }

            int indice = dataGridViewDetalle.CurrentRow.Index;

            if (indice >= 0 && indice < dtDetalle.Rows.Count)
            {
                dtDetalle.Rows.RemoveAt(indice);
                RecalcularTotal();
            }
        }

        private void RecalcularTotal()
        {
            decimal total = 0;

            foreach (DataRow fila in dtDetalle.Rows)
            {
                total += Convert.ToDecimal(fila["subtotal"]);
            }

            lblTotalOrden.Text = total.ToString("C2");
        }

        private void btnGuardarOrden_Click(object sender, EventArgs e)
        {
            if (dtDetalle.Rows.Count == 0)
            {
                MessageBox.Show("Agrega al menos un producto a la orden.");
                return;
            }

            if (cmbEmpleado.SelectedValue == null)
            {
                MessageBox.Show("Selecciona un empleado.");
                return;
            }

            try
            {
                int idCliente = 0;

                if (cmbCliente.SelectedValue != null)
                {
                    idCliente = Convert.ToInt32(cmbCliente.SelectedValue);
                }

                int idEmpleado = Convert.ToInt32(cmbEmpleado.SelectedValue);

                Orden orden = new Orden();
                orden.IdCliente = idCliente;
                orden.IdEmpleado = idEmpleado;
                orden.Fecha = dtpFecha.Value;

                List<DetalleOrden> detalles = new List<DetalleOrden>();

                foreach (DataRow fila in dtDetalle.Rows)
                {
                    DetalleOrden detalle = new DetalleOrden();
                    detalle.IdProducto = Convert.ToInt32(fila["id_producto"]);
                    detalle.Cantidad = Convert.ToInt32(fila["cantidad"]);
                    detalle.PrecioUnitario = Convert.ToDecimal(fila["precio_unitario"]);
                    detalle.Subtotal = Convert.ToDecimal(fila["subtotal"]);
                    detalles.Add(detalle);
                }

                int idOrden = conexion.InsertarOrden(orden, detalles);

                MessageBox.Show("Orden #" + idOrden + " registrada correctamente. Total: " + orden.Total.ToString("C2"));
                LimpiarOrden();
                CargarOrdenes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLimpiarOrden_Click(object sender, EventArgs e)
        {
            LimpiarOrden();
        }

        private void LimpiarOrden()
        {
            dtDetalle.Rows.Clear();
            dataGridViewDetalleOrden.DataSource = null;
            RecalcularTotal();
            dtpFecha.Value = DateTime.Now;
            numCantidad.Value = 1;

            if (cmbCliente.Items.Count > 0) cmbCliente.SelectedIndex = 0;
            if (cmbEmpleado.Items.Count > 0) cmbEmpleado.SelectedIndex = 0;
            if (cmbProducto.Items.Count > 0) cmbProducto.SelectedIndex = 0;
        }

        private void dataGridViewOrdenes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            try
            {
                int idOrden = Convert.ToInt32(dataGridViewOrdenes.Rows[e.RowIndex].Cells["id_orden"].Value);

                dataGridViewDetalleOrden.DataSource = conexion.ObservarDetalleOrden(idOrden);

                if (dataGridViewDetalleOrden.Columns["id_detalle"] != null) dataGridViewDetalleOrden.Columns["id_detalle"].Visible = false;
                if (dataGridViewDetalleOrden.Columns["id_orden"] != null) dataGridViewDetalleOrden.Columns["id_orden"].Visible = false;
                if (dataGridViewDetalleOrden.Columns["id_producto"] != null) dataGridViewDetalleOrden.Columns["id_producto"].Visible = false;
                if (dataGridViewDetalleOrden.Columns["producto"] != null) dataGridViewDetalleOrden.Columns["producto"].HeaderText = "Producto";
                if (dataGridViewDetalleOrden.Columns["cantidad"] != null) dataGridViewDetalleOrden.Columns["cantidad"].HeaderText = "Cantidad";

                if (dataGridViewDetalleOrden.Columns["precio_unitario"] != null)
                {
                    dataGridViewDetalleOrden.Columns["precio_unitario"].HeaderText = "Precio unitario";
                    dataGridViewDetalleOrden.Columns["precio_unitario"].DefaultCellStyle.Format = "C2";
                }

                if (dataGridViewDetalleOrden.Columns["subtotal"] != null)
                {
                    dataGridViewDetalleOrden.Columns["subtotal"].HeaderText = "Subtotal";
                    dataGridViewDetalleOrden.Columns["subtotal"].DefaultCellStyle.Format = "C2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
