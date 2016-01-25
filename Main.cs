using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data.SQLite;
using Microsoft.VisualBasic;

namespace DiscAdmin
{
    struct Item
    {
        public string EstucheId { get; set; }
        public string Contenido { get; set; }
        public int Pagina { get; set; }
        public int Numero { get; set; }
    }
    public partial class Main : Form
    {
        private readonly IList<Item> _items = new List<Item>();
        public Main()
        {
            InitializeComponent();
            Cargar();
        }

        private void CargarEstuches()
        {
            Conexion.ConexionString = @"Data Source=DISCDATA.s3db;Version=3";
            var ds = Conexion.EjecutarQuery("select ID from Estuche");
            cmbEstuche.DataSource = ds.Tables[0];
            cmbEstuche.DisplayMember = "ID";
        }

        private static int BuscarPosicion(string estucheid, int pagina, int numero)
        {
            Conexion.ConexionString = @"Data Source=DISCDATA.s3db;Version=3";
            var posicionid = Conexion.EjecutarEscalar(
                //al no ingresar el estuche siepmre lo ingresaba en el A
                String.Format("select id from POSICION where pagina={0} and numero={1} and estucheid='{2}'", pagina, numero, estucheid));
            return posicionid != null ? Convert.ToInt32(posicionid) : -1;
        }

        private bool AgregarData(string estucheid, int pagina, int numero)
        {
            var posicionid = BuscarPosicion(estucheid, pagina, numero);
            var contenido = txtContenido.Text;
            var query = String.Format("insert into DATA(CASEID,CONTENIDO) values({0},'{1}');", posicionid, contenido);
            Conexion.ConexionString = @"Data Source=DISCDATA.s3db;Version=3";
            Conexion.EjecutarNoQuery(query);
            //query = "update POSICION set estado=1 where ID=" + posicionid;
            //Conexion.EjecutarNoQuery(query);
            return true;
        }

        private int GetCaseidItem(string estucheid, string pagina, string numero)
        {
            //obtenemos el caseid para saber donde esta ubicado el dato en la base de datos
            var querycaseid = String.Format(
                "select ID" +
                " from POSICION" +
                " where ESTUCHEID='{0}'" +
                " and PAGINA={1}" +
                " and NUMERO={2}",
                lblEstuche.Text,
                lblPagina.Text,
                lblNumero.Text);

            return Convert.ToInt32(Conexion.EjecutarEscalar(querycaseid));
        }

        private bool BorrarData()
        {
            var caseid = GetCaseidItem(lblEstuche.Text, lblPagina.Text, lblNumero.Text);

            //con el case id y el nombre del objeto a eliminar podemos borrar el registro
            var queryborrar = String.Format(
                "delete" +
                " from DATA" +
                " where CASEID={0}" +
                " and CONTENIDO='{1}'",
                caseid,
                ((Item)lstItems.SelectedItem).Contenido);

            var resultado = Conexion.EjecutarNoQuery(queryborrar);
            //si es diferente de menos 1 fue completado
            return (resultado != -1);
        }

        private bool ModificarContenido(string estucheid, string pagina, string numero, string original, string nuevo)
        {
            var caseid = GetCaseidItem(estucheid, pagina, numero);

            var modificarquery = String.Format(
                "update DATA" +
                " set CONTENIDO='{0}'" +
                " where CASEID={1}" +
                " and CONTENIDO='{2}'",
                nuevo, caseid, original);

            //si no hubo error
            return (Conexion.EjecutarNoQuery(modificarquery)!=-1);
        }

        /// <summary>
        /// Limpia el ListBox y añade todos los items de nuevo.
        /// </summary>
        private void CargarItemsListBox()
        {
            var w = new System.Diagnostics.Stopwatch();
            //w.Start();
            lstItems.Items.Clear();
            /*w.Stop();
            MessageBox.Show(w.ElapsedMilliseconds.ToString());*/
            foreach (var i in _items)
            {
                lstItems.Items.Add(i);
            }
        }

        private void CargarItems()
        {
            Conexion.ConexionString = @"Data Source=DISCDATA.s3db;Version=3";
            //como la busqueda es global, podemos usar el View ITEMS
            //al principio pense que la busqueda es por estuches pero no
            //tiene sentido porque al buscar un item no sabemos donde esta
            var dt = Conexion.EjecutarQuery("select * from ITEMS").Tables[0];
            if (dt == null)
            {
                return;
            }

            foreach (DataRow r in dt.Rows)
            {
                var cont = r[1].ToString();
                var estuche = r[2].ToString();
                var pag = Convert.ToInt32(r[3]);
                var num = Convert.ToInt32(r[4]);
                _items.Add(new Item { Contenido = cont, EstucheId = estuche, Pagina = pag, Numero = num });
            }
            CargarItemsListBox();
        }

        private void Cargar()
        {
            CargarItems();
            CargarEstuches();
            lstItems.DisplayMember = "Contenido";
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            var estucheid = cmbEstuche.Text;
            var pagina = Convert.ToInt32(txtPagina.Text);
            var numero = Convert.ToInt32(txtPosicion.Text);
            if (!AgregarData(estucheid, pagina, numero))
            {
                return;
            }
            CargarItems();
            //ActualizarContadores();
            txtContenido.Clear();
        }

        private void btnAgregarEstuche_Click(object sender, EventArgs e)
        {
            //cambiado para que espere que se ingrese un nuevo estuche para cargarlos
            new Estuches().ShowDialog();
            CargarEstuches();
        }

        private void Buscar(string patron)
        {
            if (patron == "")
                return;

            IList<Item> aEliminar = new List<Item>();
            var watch = new System.Diagnostics.Stopwatch();
            foreach (var i in lstItems.Items)
            {
                var item = ((Item)i);
                if (!item.Contenido.ToLower().Contains(patron))
                {
                    aEliminar.Add(item);
                }
            }

            foreach (var i in aEliminar)
            {
                lstItems.Items.Remove(i);
            }
            watch.Stop();
            this.Text = (watch.ElapsedMilliseconds.ToString());
        }

        private void ActualizarInfo(Item item)
        {
            lblEstuche.Text = item.EstucheId;
            lblPagina.Text = item.Pagina.ToString();
            lblNumero.Text = item.Numero.ToString();
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = lstItems.SelectedItem;
            //cuando se borra puede ser nulo
            if (item == null)
            {
                lstItems.SelectedIndex = 0;
                item = lstItems.SelectedItem;
            }
            ActualizarInfo((Item)item);
        }

        private void Tick(object sender, EventArgs e)
        {
            Buscar();
        }

        private void Buscar()
        {
            timer.Stop();
            timeriniciado = false;
            //no busca si no esta modo busqueda por que es muy lento :(
            if (chkBusqueda.Checked != true)
                return;
            CargarItemsListBox();
            //en el programa tratamos todas las cadenas como minusculas
            var patron = txtContenido.Text.ToLower();
            Buscar(patron);
        }

        bool timeriniciado = false;

        private void txtContenido_TextChanged(object sender, EventArgs e)
        {
            if (timeriniciado == true)
            {
                //Reseteamos el timer si se añade una letra
                timer.Stop();
                timer.Start();
            }
            else
            {
                //cuando se empieza la busqueda (primera letra)
                timer.Start();
                timeriniciado = true;
            }
        }

        private void Borrar()
        {
            var res = MessageBox.Show(
                "Seguro que desea eliminar este item?",
                "Advertencia",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (res == DialogResult.OK)
            {
                BorrarData();
                lstItems.Items.RemoveAt(lstItems.SelectedIndex);
            }
        }

        private void lstItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Borrar();
            }
            else if (e.KeyCode == Keys.F2)
            {
                Modificar();
            }
        }

        private void Modificar()
        {
            var nuevo=Interaction.InputBox("Reemplazar:");
            //si esta vacio
            if (nuevo == "")
                return;
            var estucheid = lblEstuche.Text;
            var pagina = lblPagina.Text;
            var numero = lblNumero.Text;
            var original = ((Item)lstItems.SelectedItem).Contenido;
            ModificarContenido(estucheid, pagina, numero, original, nuevo);

            //no debo modificar structs
            //tengo que deshacerme de eso...
            CargarItems();
        }

        private void lstItems_DoubleClick(object sender, EventArgs e)
        {
            Modificar();
        }
    }
}
