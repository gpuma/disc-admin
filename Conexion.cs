using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DiscAdmin
{
    class Conexion
    {
        public static string ConexionString { get; set; }
        private static SQLiteConnection _conexion;

        /// <summary>
        /// Ejecuta una consulta que retorna un solo valor.
        /// </summary>
        /// <param name="query">La instrucción SQL.</param>
        /// <returns>Retorna el resultado SQL o nulo si hubo un error</returns>
        public static object EjecutarEscalar(string query)
        {
            using (_conexion = new SQLiteConnection(ConexionString))
            {
                var cmd = new SQLiteCommand(query, _conexion);
                try
                {
                    _conexion.Open();
                    return cmd.ExecuteScalar();
                }
                catch (SQLiteException e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                    return null;
                }
            }
        }

        /// <summary>
        /// Ejecuta una consulta que retorna un grupo de datos.
        /// </summary>
        /// <param name="query">La instrucción SQL</param>
        /// <returns>Retorna un DataSet con la información o null si hubo error</returns>
        public static DataSet EjecutarQuery(string query)
        {
            using (_conexion = new SQLiteConnection(ConexionString))
            {
                var cmd = new SQLiteCommand(query, _conexion);
                var da = new SQLiteDataAdapter(cmd);
                var ds = new DataSet();
                try
                {
                    da.Fill(ds);
                }
                catch (SQLiteException e)
                {
                    /*
                    MessageBox.Show(
                        "No existe la base de datos.",
                        "Error cargando la base de datos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    Environment.Exit(-1);*/
                    MessageBox.Show(e.Message);
                    throw;
                    return null;
                }
                return ds;
            }
        }

        /// <summary>
        /// Ejecuta una instrucción SQL.
        /// </summary>
        /// <param name="noquery">La instrucción a ejecutar</param>
        /// <returns>Retorna el número de filas afectadas o -1 si hubo un error</returns>
        public static int EjecutarNoQuery(string noquery)
        {
            using (_conexion = new SQLiteConnection(ConexionString))
            {
                var cmd = new SQLiteCommand(noquery, _conexion);
                try
                {
                    _conexion.Open();
                    return cmd.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                    return -1;
                }
            }
        }
    }
}
