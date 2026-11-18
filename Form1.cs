using System;
using System.Windows.Forms;

namespace TaqueriaProyecto
{
    public partial class Form1 : Form
    {
        private Conexion conexion = new Conexion();

        public Form1()
        {
            InitializeComponent();

            comboBoxTipo.Items.Clear();
            comboBoxTipo.Items.Add("Taco");
            comboBoxTipo.Items.Add("Bebida");
            comboBoxTipo.DropDownStyle = ComboBoxStyle.DropDownList;

            try
            {
                conexion.AbrirConexion();
                MessageBox.Show("Conexión exitosa con MySQL");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.CerrarConexion();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarProductos();
            ActualizarVisibilidad();
        }

        private void CargarProductos()
        {
            try
            {
                dataGridView1.DataSource = conexion.Observar();
                ConfigurarColumnas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ConfigurarColumnas()
        {
            if (dataGridView1.Columns["id_producto"] != null) dataGridView1.Columns["id_producto"].HeaderText = "ID";
            if (dataGridView1.Columns["nombre"] != null) dataGridView1.Columns["nombre"].HeaderText = "Nombre";
            if (dataGridView1.Columns["precio"] != null)
            {
                dataGridView1.Columns["precio"].HeaderText = "Precio";
                dataGridView1.Columns["precio"].DefaultCellStyle.Format = "C2";
            }
            if (dataGridView1.Columns["tipo"] != null) dataGridView1.Columns["tipo"].HeaderText = "Tipo";
            if (dataGridView1.Columns["disponible"] != null) dataGridView1.Columns["disponible"].HeaderText = "Disponible";
            if (dataGridView1.Columns["guiso"] != null) dataGridView1.Columns["guiso"].HeaderText = "Guiso";
            if (dataGridView1.Columns["tortilla"] != null) dataGridView1.Columns["tortilla"].HeaderText = "Tortilla";
            if (dataGridView1.Columns["tipo_bebida"] != null) dataGridView1.Columns["tipo_bebida"].HeaderText = "Tipo bebida";
            if (dataGridView1.Columns["tamano"] != null) dataGridView1.Columns["tamano"].HeaderText = "Tamaño";
        }

        private void ActualizarVisibilidad()
        {
            bool esTaco = string.Equals(comboBoxTipo.Text, "Taco", StringComparison.OrdinalIgnoreCase);
            bool esBebida = string.Equals(comboBoxTipo.Text, "Bebida", StringComparison.OrdinalIgnoreCase);

            label5.Visible = esTaco;
            txtTortilla.Visible = esTaco;
            label6.Visible = esTaco;
            txtGuiso.Visible = esTaco;

            label7.Visible = esBebida;
            txtTipoBebida.Visible = esBebida;
            label8.Visible = esBebida;
            txtTamanio.Visible = esBebida;
        }

        private void comboBoxTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarVisibilidad();
        }

        private bool ValidarProducto()
        {
            if (textNombre.Text.Trim() == "")
            {
                MessageBox.Show("Escribe el nombre del producto.");
                textNombre.Focus();
                return false;
            }

            decimal precio;

            if (!decimal.TryParse(textPrecio.Text, out precio) || precio <= 0)
            {
                MessageBox.Show("Escribe un precio válido mayor que cero.");
                textPrecio.Focus();
                return false;
            }

            if (!string.Equals(comboBoxTipo.Text, "Taco", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(comboBoxTipo.Text, "Bebida", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Selecciona el tipo: Taco o Bebida.");
                comboBoxTipo.Focus();
                return false;
            }

            if (string.Equals(comboBoxTipo.Text, "Taco", StringComparison.OrdinalIgnoreCase))
            {
                if (txtTortilla.Text.Trim() == "" || txtGuiso.Text.Trim() == "")
                {
                    MessageBox.Show("Completa la tortilla y el guiso del taco.");
                    return false;
                }
            }
            else
            {
                if (txtTipoBebida.Text.Trim() == "" || txtTamanio.Text.Trim() == "")
                {
                    MessageBox.Show("Completa el tipo y el tamaño de la bebida.");
                    return false;
                }
            }

            return true;
        }

        private Producto CrearProductoDesdeFormulario()
        {
            decimal precio = Convert.ToDecimal(textPrecio.Text);
            string tipo = string.Equals(comboBoxTipo.Text, "Taco", StringComparison.OrdinalIgnoreCase) ? "Taco" : "Bebida";

            if (tipo == "Taco")
            {
                Taco taco = new Taco();
                taco.Nombre = textNombre.Text.Trim();
                taco.Precio = precio;
                taco.Tipo = tipo;
                taco.Disponible = Disponible.Checked;
                taco.Guiso = txtGuiso.Text.Trim();
                taco.Tortilla = txtTortilla.Text.Trim();
                return taco;
            }
            else
            {
                Bebida bebida = new Bebida();
                bebida.Nombre = textNombre.Text.Trim();
                bebida.Precio = precio;
                bebida.Tipo = tipo;
                bebida.Disponible = Disponible.Checked;
                bebida.TipoB = txtTipoBebida.Text.Trim();
                bebida.Tamanio = txtTamanio.Text.Trim();
                return bebida;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarProducto())
            {
                return;
            }

            try
            {
                Producto producto = CrearProductoDesdeFormulario();
                int id = conexion.InsertarProducto(producto);

                MessageBox.Show("Producto guardado correctamente. ID: " + id + "\n" + producto.ObtenerDescripcion());
                LimpiarFormulario();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Actualizar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un producto primero.");
                return;
            }

            if (!ValidarProducto())
            {
                return;
            }

            try
            {
                int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id_producto"].Value);
                string tipoAnterior = dataGridView1.CurrentRow.Cells["tipo"].Value.ToString();

                Producto producto = CrearProductoDesdeFormulario();
                producto.IdProducto = id;

                if (conexion.ActualizarProducto(producto, tipoAnterior))
                {
                    MessageBox.Show("Producto actualizado correctamente.\n" + producto.ObtenerDescripcion());
                    LimpiarFormulario();
                    CargarProductos();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el producto.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Eliminar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un producto primero.");
                return;
            }

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id_producto"].Value);

            if (MessageBox.Show("¿Eliminar el producto seleccionado?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (conexion.Eliminar(id))
                {
                    MessageBox.Show("Producto eliminado correctamente.");
                    LimpiarFormulario();
                    CargarProductos();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el producto.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow fila = dataGridView1.Rows[e.RowIndex];

            textNombre.Text = fila.Cells["nombre"].Value == DBNull.Value ? "" : fila.Cells["nombre"].Value.ToString();
            textPrecio.Text = fila.Cells["precio"].Value == DBNull.Value ? "" : fila.Cells["precio"].Value.ToString();

            string tipo = fila.Cells["tipo"].Value == DBNull.Value ? "" : fila.Cells["tipo"].Value.ToString();

            if (string.Equals(tipo, "Taco", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxTipo.Text = "Taco";
            }
            else if (string.Equals(tipo, "Bebida", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxTipo.Text = "Bebida";
            }
            else
            {
                comboBoxTipo.Text = tipo;
            }

            Disponible.Checked = fila.Cells["disponible"].Value != DBNull.Value && Convert.ToBoolean(fila.Cells["disponible"].Value);

            txtTortilla.Text = fila.Cells["tortilla"].Value == DBNull.Value ? "" : fila.Cells["tortilla"].Value.ToString();
            txtGuiso.Text = fila.Cells["guiso"].Value == DBNull.Value ? "" : fila.Cells["guiso"].Value.ToString();
            txtTipoBebida.Text = fila.Cells["tipo_bebida"].Value == DBNull.Value ? "" : fila.Cells["tipo_bebida"].Value.ToString();
            txtTamanio.Text = fila.Cells["tamano"].Value == DBNull.Value ? "" : fila.Cells["tamano"].Value.ToString();

            ActualizarVisibilidad();
        }

        private void LimpiarFormulario()
        {
            textNombre.Clear();
            textPrecio.Clear();
            comboBoxTipo.SelectedIndex = -1;
            Disponible.Checked = false;
            txtTortilla.Clear();
            txtGuiso.Clear();
            txtTipoBebida.Clear();
            txtTamanio.Clear();
            dataGridView1.ClearSelection();
            ActualizarVisibilidad();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            FormClientes form = new FormClientes();
            form.ShowDialog();
        }

        private void btnEmpleados_Click(object sender, EventArgs e)
        {
            FormEmpleados form = new FormEmpleados();
            form.ShowDialog();
        }

        private void btnOrdenes_Click(object sender, EventArgs e)
        {
            FormOrdenes form = new FormOrdenes();
            form.ShowDialog();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }
    }
}
