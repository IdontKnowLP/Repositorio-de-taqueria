using System;
using System.Windows.Forms;

namespace TaqueriaProyecto
{
    public partial class FormEmpleados : Form
    {
        private Conexion conexion = new Conexion();

        public FormEmpleados()
        {
            InitializeComponent();
        }

        private void FormEmpleados_Load(object sender, EventArgs e)
        {
            CargarEmpleados();
        }

        private void CargarEmpleados()
        {
            try
            {
                dataGridViewEmpleados.DataSource = conexion.ObservarEmpleados();

                if (dataGridViewEmpleados.Columns["id_empleado"] != null) dataGridViewEmpleados.Columns["id_empleado"].HeaderText = "ID";
                if (dataGridViewEmpleados.Columns["nombre"] != null) dataGridViewEmpleados.Columns["nombre"].HeaderText = "Nombre";
                if (dataGridViewEmpleados.Columns["puesto"] != null) dataGridViewEmpleados.Columns["puesto"].HeaderText = "Puesto";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool Validar()
        {
            if (txtNombreEmpleado.Text.Trim() == "")
            {
                MessageBox.Show("Escribe el nombre del empleado.");
                txtNombreEmpleado.Focus();
                return false;
            }

            if (txtPuestoEmpleado.Text.Trim() == "")
            {
                MessageBox.Show("Escribe el puesto del empleado.");
                txtPuestoEmpleado.Focus();
                return false;
            }

            return true;
        }

        private void btnGuardarEmpleado_Click(object sender, EventArgs e)
        {
            if (!Validar())
            {
                return;
            }

            try
            {
                Empleado empleado = new Empleado();
                empleado.Nombre = txtNombreEmpleado.Text.Trim();
                empleado.Puesto = txtPuestoEmpleado.Text.Trim();

                conexion.InsertarEmpleado(empleado);

                MessageBox.Show("Empleado guardado correctamente. ID: " + empleado.IdEmpleado);
                Limpiar();
                CargarEmpleados();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnActualizarEmpleado_Click(object sender, EventArgs e)
        {
            if (dataGridViewEmpleados.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un empleado primero.");
                return;
            }

            if (!Validar())
            {
                return;
            }

            try
            {
                Empleado empleado = new Empleado();
                empleado.IdEmpleado = Convert.ToInt32(dataGridViewEmpleados.CurrentRow.Cells["id_empleado"].Value);
                empleado.Nombre = txtNombreEmpleado.Text.Trim();
                empleado.Puesto = txtPuestoEmpleado.Text.Trim();

                if (conexion.ActualizarEmpleado(empleado))
                {
                    MessageBox.Show("Empleado actualizado correctamente.");
                    Limpiar();
                    CargarEmpleados();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el empleado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEliminarEmpleado_Click(object sender, EventArgs e)
        {
            if (dataGridViewEmpleados.CurrentRow == null)
            {
                MessageBox.Show("Selecciona un empleado primero.");
                return;
            }

            int id = Convert.ToInt32(dataGridViewEmpleados.CurrentRow.Cells["id_empleado"].Value);

            if (MessageBox.Show("¿Eliminar el empleado seleccionado?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (conexion.EliminarEmpleado(id))
                {
                    MessageBox.Show("Empleado eliminado correctamente.");
                    Limpiar();
                    CargarEmpleados();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el empleado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLimpiarEmpleado_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void dataGridViewEmpleados_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow fila = dataGridViewEmpleados.Rows[e.RowIndex];

            txtNombreEmpleado.Text = fila.Cells["nombre"].Value == DBNull.Value ? "" : fila.Cells["nombre"].Value.ToString();
            txtPuestoEmpleado.Text = fila.Cells["puesto"].Value == DBNull.Value ? "" : fila.Cells["puesto"].Value.ToString();
        }

        private void Limpiar()
        {
            txtNombreEmpleado.Clear();
            txtPuestoEmpleado.Clear();
            dataGridViewEmpleados.ClearSelection();
        }
    }
}
