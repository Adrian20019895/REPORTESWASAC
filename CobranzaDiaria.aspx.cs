using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Text;

public partial class CobranzaDiaria : System.Web.UI.Page
{
    private string connectionString = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Establecer fechas por defecto (último mes)
            txtFechaHasta.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaDesde.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            
            CargarSucursales();
            CargarProgramas();
            CargarOperacionesCobranza();
        }
    }

    private void CargarSucursales()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                
                // Forzar limpieza completa del dropdown
                ddlSucursal.Items.Clear();
                ddlSucursal.ClearSelection();
                ddlSucursal.Items.Add(new ListItem("Todas las Sucursales", ""));
                
                string query = @"
                    SELECT DISTINCT 
                        s.fiSucursal,
                        ISNULL(s.fcDescripcion, 'Sucursal ' + CAST(s.fiSucursal AS VARCHAR)) as NombreSucursal
                    FROM SIAC.DBO.SIVcSucursal s
                    INNER JOIN dbo.SIVaSolicitudCaja sc ON s.fiSucursal = sc.fiSucursal
                    WHERE sc.flActivo = 1 AND s.flActivo = 1
                    ORDER BY s.fcDescripcion";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int contador = 0;
                        while (reader.Read())
                        {
                            string nombreSucursal = reader["NombreSucursal"].ToString();
                            string idSucursal = reader["fiSucursal"].ToString();
                            
                            ddlSucursal.Items.Add(new ListItem(nombreSucursal, idSucursal));
                            contador++;
                        }
                        
                        // Log para debug
                        if (contador == 0)
                        {
                            Response.Write("<script>console.log('No se encontraron sucursales en la consulta principal');</script>");
                        }
                        else
                        {
                            Response.Write("<script>console.log('Sucursales cargadas: " + contador + "');</script>");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Si hay error con la consulta compleja, usar consulta directa de SIVcSucursal
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    ddlSucursal.Items.Clear();
                    ddlSucursal.ClearSelection();
                    ddlSucursal.Items.Add(new ListItem("Todas las Sucursales", ""));
                    
                    string querySimple = @"
                        SELECT 
                            fiSucursal,
                            ISNULL(fcDescripcion, 'Sucursal ' + CAST(fiSucursal AS VARCHAR)) as NombreSucursal
                        FROM SIAC.DBO.SIVcSucursal 
                        WHERE flActivo = 1
                        ORDER BY fcDescripcion";
                    
                    using (SqlCommand cmd = new SqlCommand(querySimple, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlSucursal.Items.Add(new ListItem(
                                    reader["NombreSucursal"].ToString(),
                                    reader["fiSucursal"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                Response.Write("<script>alert('Error al cargar sucursales: " + ex2.Message.Replace("'", "\\'") + "');</script>");
            }
        }
    }

    private void CargarProgramas()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                
                // Forzar limpieza completa del dropdown
                ddlPrograma.Items.Clear();
                ddlPrograma.ClearSelection();
                ddlPrograma.Items.Add(new ListItem("Todos los Programas", ""));
                
                string query = @"
                    SELECT DISTINCT 
                        fiSubprograma,
                        'Programa ' + CAST(fiSubprograma AS VARCHAR) as NombrePrograma
                    FROM dbo.SIVaSolicitudCaja
                    WHERE fiSubprograma IS NOT NULL AND flActivo = 1
                    ORDER BY fiSubprograma";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ddlPrograma.Items.Add(new ListItem(
                                reader["NombrePrograma"].ToString(),
                                reader["fiSubprograma"].ToString()
                            ));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Response.Write("<script>alert('Error al cargar programas: " + ex.Message.Replace("'", "\\'") + "');</script>");
        }
    }

    private void CargarOperacionesCobranza()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                
                DateTime fechaDesde, fechaHasta;
                try
                {
                    fechaDesde = DateTime.ParseExact(txtFechaDesde.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    fechaHasta = DateTime.ParseExact(txtFechaHasta.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    
                    // Validar que el rango no sea muy amplio
                    TimeSpan diferencia = fechaHasta - fechaDesde;
                    // Sin alertas por rango de fechas o límite de registros
                }
                catch
                {
                    // Si hay error en las fechas, usar último mes
                    fechaHasta = DateTime.Now;
                    fechaDesde = DateTime.Now.AddDays(-30);
                }

                // Consulta con filtros de fecha corregidos - incluye activos e inactivos
                string query = @"
                    SELECT
                        CONVERT(VARCHAR, sc.fecMod, 103) as Fecha,
                        CONVERT(VARCHAR, sc.fecMod, 108) as Hora,
                        CAST(sc.fiSolicitudCaja AS VARCHAR) as NumeroOperacion,
                        CAST(sc.fiBeneficiario AS VARCHAR) as NombreBeneficiario,
                        ISNULL(sc.usrAlta, 'N/A') as Cobratario,
                        'Programa ' + CAST(sc.fiSubprograma AS VARCHAR) as Programa,
                        ISNULL(s.fcDescripcion, 'Sucursal ' + CAST(sc.fiSucursal AS VARCHAR)) as Sucursal,
                        ISNULL(sc.fiAmort, 0) as MontoCapital,
                        ISNULL(sc.fiReal, 0) + ISNULL(sc.fiInteresF, 0) as MontoInteres,
                        ISNULL(sc.fiMoratorio, 0) as MontoMoratorio,
                        ISNULL(sc.fiGastosAdmon, 0) + ISNULL(sc.fiSeguro, 0) + ISNULL(sc.fiOtroSeguro, 0) as OtrosGastos,
                        ISNULL(sc.fiDescuento, 0) + ISNULL(sc.fiSubcidio, 0) as Descuentos,
                        ISNULL(sc.fiTotal, 0) as TotalOperacion,
                        ISNULL(tp.fcTipoPago, 'Tipo ' + CAST(sc.fiTipoPago AS VARCHAR)) as TipoOperacion,
                        ISNULL(sc.fcObservacion, '') as Observaciones,
                        sc.flActivo as Estado
                    FROM dbo.SIVaSolicitudCaja sc
                    LEFT JOIN SIAC.DBO.SIVcSucursal s ON sc.fiSucursal = s.fiSucursal
                    LEFT JOIN SIAC.DBO.SIVcTipoPago tp ON sc.fiTipoPago = tp.fiTipoPago
                    WHERE (sc.flActivo = 1 OR sc.flActivo = 0)
                        AND CAST(sc.fecMod AS DATE) >= @FechaDesde 
                        AND CAST(sc.fecMod AS DATE) <= @FechaHasta";

                // Agregar filtros adicionales
                if (!string.IsNullOrEmpty(ddlSucursal.SelectedValue))
                {
                    query += " AND sc.fiSucursal = @Sucursal";
                }
                if (!string.IsNullOrEmpty(ddlPrograma.SelectedValue))
                {
                    query += " AND sc.fiSubprograma = @Programa";
                }

                query += " ORDER BY sc.fecMod DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Date);
                    cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.Date);

                    if (!string.IsNullOrEmpty(ddlSucursal.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue("@Sucursal", ddlSucursal.SelectedValue);
                    }
                    if (!string.IsNullOrEmpty(ddlPrograma.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue("@Programa", ddlPrograma.SelectedValue);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        
                        gvCobranza.DataSource = dt;
                        gvCobranza.DataBind();

                        if (dt.Rows.Count > 0)
                        {
                            pnlResumen.Visible = true;
                            MostrarResumenReal(dt);
                        }
                        else
                        {
                            pnlResumen.Visible = false;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Error simple sin diagnóstico
            Response.Write("<script>alert('Error al cargar datos: " + ex.Message.Replace("'", "\\'") + "');</script>");
        }
    }

    private void MostrarResumen(DataTable dt)
    {
        try
        {
            int totalOperaciones = dt.Rows.Count;
            decimal totalCobrado = 0;
            decimal capitalCobrado = 0;
            decimal interesesCobrados = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal capital = Convert.ToDecimal(row["MontoCapital"]);
                decimal interes = Convert.ToDecimal(row["MontoInteres"]);
                decimal moratorio = Convert.ToDecimal(row["MontoMoratorio"]);
                decimal otros = Convert.ToDecimal(row["OtrosGastos"]);

                capitalCobrado += capital;
                interesesCobrados += interes + moratorio;
                totalCobrado += capital + interes + moratorio + otros;
            }

            lblTotalOperaciones.Text = totalOperaciones.ToString("N0");
            lblTotalCobrado.Text = totalCobrado.ToString("C2");
            lblCapitalCobrado.Text = capitalCobrado.ToString("C2");
            lblInteresesCobrados.Text = interesesCobrados.ToString("C2");
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error", 
                "alert('Error al calcular resumen: " + ex.Message.Replace("'", "\\'") + "');", true);
        }
    }

    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        CargarOperacionesCobranza();
    }

    protected void btnVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("VerDatos.aspx");
    }

    protected void btnExportarExcel_Click(object sender, EventArgs e)
    {
        try
        {
            // Exportar como CSV (compatible con Excel)
            StringBuilder sb = new StringBuilder();
            
            // Encabezados
            sb.AppendLine("Fecha,Hora,Numero Operacion,Beneficiario,Cobratario,Programa,Sucursal,Capital,Interes,Moratorio,Otros Gastos,Descuentos,Total,Tipo");
            
            // Datos
            foreach (GridViewRow row in gvCobranza.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string[] campos = new string[row.Cells.Count];
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        string cellText = row.Cells[i].Text.Replace("&nbsp;", "").Replace(",", ";");
                        campos[i] = "\"" + cellText + "\"";
                    }
                    sb.AppendLine(string.Join(",", campos));
                }
            }
            
            // Configurar respuesta HTTP
            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("content-disposition", 
                "attachment; filename=CobranzaDiaria_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
            Response.Charset = "UTF-8";
            Response.ContentEncoding = Encoding.UTF8;
            
            // Agregar BOM para UTF-8
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());
            Response.Write(sb.ToString());
            Response.End();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error", 
                "alert('Error al exportar: " + ex.Message.Replace("'", "\\'") + "');", true);
        }
    }

    protected void gvCobranza_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Obtener el estado del registro (activo/inactivo)
            DataRowView rowView = (DataRowView)e.Row.DataItem;
            bool esActivo = Convert.ToBoolean(rowView["Estado"]);
            
            // Si el registro está inactivo, aplicar estilo rojo
            if (!esActivo)
            {
                e.Row.CssClass += " registro-inactivo";
            }
            
            // Aplicar estilos específicos a las columnas
            if (e.Row.Cells.Count > 0)
            {
                e.Row.Cells[0].CssClass = "fecha-column"; // Fecha
                if (e.Row.Cells.Count > 7)
                {
                    e.Row.Cells[7].CssClass = "monto-column"; // Capital
                    e.Row.Cells[8].CssClass = "monto-column"; // Interés
                    e.Row.Cells[9].CssClass = "monto-column"; // Moratorio
                    e.Row.Cells[10].CssClass = "monto-column"; // Otros Gastos
                    e.Row.Cells[11].CssClass = "monto-column"; // Descuentos
                    e.Row.Cells[12].CssClass = "monto-column"; // Total
                }
                if (e.Row.Cells.Count > 6)
                {
                    e.Row.Cells[6].CssClass = "sucursal-column"; // Sucursal
                }
                if (e.Row.Cells.Count > 3)
                {
                    e.Row.Cells[3].CssClass = "beneficiario-column"; // Beneficiario
                }
                // Estilo para la columna de observaciones (última columna)
                if (e.Row.Cells.Count > 14)
                {
                    e.Row.Cells[14].CssClass = "observaciones-column";
                    e.Row.Cells[14].Style.Add("max-width", "200px");
                    e.Row.Cells[14].Style.Add("word-wrap", "break-word");
                    e.Row.Cells[14].Style.Add("font-size", "0.85em");
                }
            }
        }
    }

    private void MostrarResumenReal(DataTable dt)
    {
        try
        {
            int totalOperaciones = dt.Rows.Count;
            int totalOperacionesActivas = 0;
            int totalOperacionesInactivas = 0;
            decimal totalCobrado = 0;
            decimal capitalCobrado = 0;
            decimal interesesCobrados = 0;

            foreach (DataRow row in dt.Rows)
            {
                // Verificar si el registro está activo
                bool esActivo = Convert.ToBoolean(row["Estado"]);
                
                if (esActivo)
                {
                    totalOperacionesActivas++;
                    
                    // Solo sumar los montos si el registro está activo
                    decimal capital = Convert.ToDecimal(row["MontoCapital"]);
                    decimal interes = Convert.ToDecimal(row["MontoInteres"]);
                    decimal moratorio = Convert.ToDecimal(row["MontoMoratorio"]);
                    decimal otros = Convert.ToDecimal(row["OtrosGastos"]);

                    capitalCobrado += capital;
                    interesesCobrados += interes + moratorio;
                    totalCobrado += Convert.ToDecimal(row["TotalOperacion"]);
                }
                else
                {
                    totalOperacionesInactivas++;
                }
            }

            // Mostrar totales (solo operaciones activas se suman en montos)
            lblTotalOperaciones.Text = totalOperacionesActivas.ToString("N0") + " activas" + 
                (totalOperacionesInactivas > 0 ? " (" + totalOperacionesInactivas.ToString("N0") + " anuladas)" : "");
            lblTotalCobrado.Text = totalCobrado.ToString("C2");
            lblCapitalCobrado.Text = capitalCobrado.ToString("C2");
            lblInteresesCobrados.Text = interesesCobrados.ToString("C2");
        }
        catch (Exception ex)
        {
            Response.Write("<script>alert('Error al calcular resumen: " + ex.Message.Replace("'", "\\'") + "');</script>");
        }
    }
}
