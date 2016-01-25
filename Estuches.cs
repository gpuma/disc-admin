using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DiscAdmin
{
    public partial class Estuches : Form
    {
        /*
        private string _id;
        private string _capacidad;
        private string _discoxpag;
        private string _paginas;
         * */
        public Estuches()
        {
            InitializeComponent();
        }

        private string InsertarEstuche()
        {
            var id = txtID.Text;
            var discosxpag = txtDiscosxpag.Text;
            var paginas = txtPaginas.Text;
            var capacidad = txtCapacidad.Text;
            var query = String.Format("insert into Estuche values('{0}',{1},{2},{3})",
                                      id,
                                      paginas,
                                      discosxpag,
                                      capacidad);

            Conexion.ConexionString = @"Data Source=DISCDATA.s3db;Version=3";
            return Conexion.EjecutarNoQuery(query) == -1 ? null : id;
        }


        private bool ValoresValidos()
        {
            return true;
        }

        private static void CrearCases(int paginas, int discosxpag, string estucheid)
        {
            using (var conexion = new SQLiteConnection(Conexion.ConexionString))
            {
                var queryBuilder = new StringBuilder();
                for (var pag = 1; pag <= paginas; pag++)
                {
                    for (var num = 1; num <= discosxpag; num++)
                    {
                        queryBuilder.Append(
                            String.Format("insert into POSICION (ESTUCHEID,PAGINA,NUMERO) values('{0}',{1},{2})",
                                          estucheid, pag, num)+"\n;");
                    }
                }
                var query = queryBuilder.ToString();
                var cmd = new SQLiteCommand(query, conexion);
                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                }
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            lblEstado.Text = "Creando posiciones...";
            var paginas = Convert.ToInt32(txtPaginas.Text);
            var discosxpag = Convert.ToInt32(txtDiscosxpag.Text);
            if (!ValoresValidos())
            {
                return;
            }
            var estucheid = InsertarEstuche().ToString();
            if (estucheid==null)
            {
                return;
            }
            
            CrearCases(paginas,discosxpag,estucheid);
            lblEstado.Text = "";
            Close();
        }
    }
}
