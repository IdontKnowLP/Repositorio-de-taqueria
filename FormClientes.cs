using System;
using System.Windows.Forms;

namespace TaqueriaProyecto
{
    public partial class FormClientes : Form
    {
        private Conexion conexion = new Conexion();

        public FormClientes()
        {
            InitializeComponent();
        }

        private void FormClientes_Load(object sender, EventArgs e)
        {
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                dataGridViewClientes.DataSource = conexion.ObservarClientes();

                if (dataGridViewClientes.Columns["id_cliente"] != null) dataGridViewClientes.Columns["id_cliente"].HeaderText = "ID";
                if (dataGridViewClientes.Columns["nombre"] != null) dataGridViewClientes.Columns["nombre"].HeaderText = "Nombre";
                if (dataGridViewClientes.Columns["telefono"] != null) dataGridViewClientes.Columns["telefono"].HeaderText = "Teléfono";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool Validar()
        {
            if (txtNombreCliente.Text.Trim() == "")
            {
                MessageBox.Show("Escribe el nombre del cliente.");
                txtNombreCliente.Focus();
                return false;
            }

            if (txtTelefonoCliente.Text.Trim() == "")
            {
                MessageBox.Show("Escribe el teléfono del cliente.");
                txtTelefonoCliente.Focus();
                return false;
            }

            return true;
        }

        private void btnGuardarCliente_Click(object sender, EventArgs e)
        {
            if (!Validar())
            {
                return;
            }

            try
            {
                Cliente cliente = new Cliente();
                cliente.Nombre = txtNombreCliente.Text.Trim();
                cliente.Telefono = txtTelefonoCliente.Text.Trim();

                conexion.InsertarCliente(cliente);

                MessageBox.Show("Cliente guardado correctamente. ID: " + cliente.IdCliente);
                Limpiar();
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnActualizarCliente_Click(object sender, EventArgs e)
        {
            if (dataGridViewClientes.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un cliente primero.");
                return;
            }

            if (!Validar())
            {
                return;
            }

            try
            {
                Cliente cliente = new Cliente();
                cliente.IdCliente = Convert.ToInt32(dataGridViewClientes.CurrentRow.Cells["id_cliente"].Value);
                cliente.Nombre = txtNombreCliente.Text.Trim();
                cliente.Telefono = txtTelefonoCliente.Text.Trim();

                if (conexion.ActualizarCliente(cliente))
                {
                    MessageBox.Show("Cliente actualizado correctamente.");
                    Limpiar();
                    CargarClientes();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el cliente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEliminarCliente_Click(object sender, EventArgs e)
        {
            if (dataGridViewClientes.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un cliente primero.");
                return;
            }

            int id = Convert.ToInt32(dataGridViewClientes.CurrentRow.Cells["id_cliente"].Value);

            if (MessageBox.Show("¿Eliminar el cliente seleccionado? Sus órdenes quedarán sin cliente.",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (conexion.EliminarCliente(id))
                {
                    MessageBox.Show("Cliente eliminado correctamente.");
                    Limpiar();
                    CargarClientes();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el cliente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLimpiarCliente_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void dataGridViewClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow fila = dataGridViewClientes.Rows[e.RowIndex];

            txtNombreCliente.Text = fila.Cells["nombre"].Value == DBNull.Value ? "" : fila.Cells["nombre"].Value.ToString();
            txtTelefonoCliente.Text = fila.Cells["telefono"].Value == DBNull.Value ? "" : fila.Cells["telefono"].Value.ToString();
        }

        private void Limpiar()
        {
            txtNombreCliente.Clear();
            txtTelefonoCliente.Clear();
            dataGridViewClientes.ClearSelection();
        }
    }
}
