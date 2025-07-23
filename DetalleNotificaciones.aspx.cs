using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class DetalleNotificaciones : System.Web.UI.Page
{
    private string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
    private string programaSeleccionado;

    protected void Page_Load(object sender, EventArgs e)
    {
        programaSeleccionado = Request.QueryString["Programa"];
        
        if (string.IsNullOrEmpty(programaSeleccionado))
        {
            Response.Redirect("Notificaciones.aspx");
            return;
        }

        if (!IsPostBack)
        {
            lblProgramaSeleccionado.Text = programaSeleccionado;
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "document.getElementById('loadingMessage').style.display = 'block';", true);
            CargarEstadisticasPrograma();
            CargarBeneficiariosConAtrasos();
            ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "document.getElementById('loadingMessage').style.display = 'none';", true);
        }
    }

    private void CargarEstadisticasPrograma()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    COUNT(DISTINCT r.fiBeneficiario) AS TotalBeneficiarios,
                    CAST(AVG(CAST(r.fiMenAtrasadas AS FLOAT)) AS DECIMAL(10,2)) AS PromedioMensualidades,
                    MAX(r.fiMenAtrasadas) AS MaximoMensualidades,
                    SUM(ISNULL(r.fiLiquidacionOrd, 0)) AS TotalMontoAtrasado
                FROM 
                    [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE 
                    r.Estatus = 'Aceptado por Cobranza'
                    AND r.fiMenAtrasadas > 0
                    AND r.Programa = @Programa
                    AND ISNULL(r.fiLiquidacionOrd, 0) > 0";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@Programa", programaSeleccionado);
            cmd.CommandTimeout = 180;
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                lblTotalBeneficiarios.Text = reader["TotalBeneficiarios"].ToString();
                lblPromedioMensualidades.Text = reader["PromedioMensualidades"].ToString();
                lblMaximoMensualidades.Text = reader["MaximoMensualidades"].ToString();
                
                decimal montoTotal = reader["TotalMontoAtrasado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalMontoAtrasado"]) : 0;
                lblMontoTotalAtrasado.Text = montoTotal.ToString("C2");
            }
            
            reader.Close();
        }
    }

    private void CargarBeneficiariosConAtrasos()
    {
        string filtroMensualidades = ddlFiltroMensualidades.SelectedValue;
        string filtroContacto = ddlFiltroContacto != null ? ddlFiltroContacto.SelectedValue : "todos";
        string ordenamiento = ddlOrdenamiento.SelectedValue;
        
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    r.fiBeneficiario,
                    RTRIM(LTRIM(ISNULL(b.fcNombre, ''))) + ' ' + 
                    RTRIM(LTRIM(ISNULL(b.fcApaterno, ''))) + ' ' + 
                    RTRIM(LTRIM(ISNULL(b.fcAmaterno, ''))) AS NombreCompleto,
                    RTRIM(LTRIM(ISNULL(r.MunicipioContrato, ''))) AS MunicipioContrato,
                    r.fiMenAtrasadas,
                    r.MontoAsignado,
                    r.AmortizacionPag,
                    (r.MontoAsignado - r.AmortizacionPag) AS SaldoPendienteCapital,
                    (ISNULL(r.fiLiquidacionOrd, 0) - ISNULL(r.fiInteresMoratorio, 0) - (r.MontoAsignado - r.AmortizacionPag)) AS SaldoPendienteInteresReal,
                    ISNULL(r.fiInteresMoratorio, 0) AS SaldoPendienteInteresesMoratorios,
                    ISNULL(r.fiLiquidacionOrd, 0) AS SaldoParaLiquidar,
                    r.TotalPag,
                    ISNULL(b.fcEmail, '') AS Email,
                    CASE 
                        WHEN t.fiTelefono IS NOT NULL THEN 
                            CASE 
                                WHEN t.fiLada IS NOT NULL AND t.fiLada != 0 
                                THEN '(' + CAST(t.fiLada AS VARCHAR) + ') ' + CAST(t.fiTelefono AS VARCHAR)
                                ELSE CAST(t.fiTelefono AS VARCHAR)
                            END
                        ELSE ''
                    END AS Telefono,
                    (SELECT ISNULL(CONVERT(varchar, MAX(fecAlta), 103), 'N/A') 
                     FROM [SIAC].[dbo].[SIVaSolicitudCaja] 
                     WHERE fiBeneficiario = r.fiBeneficiario AND fiSubprograma = r.fiSubprograma AND fiTipoPago <> 3) AS fdUltimoPago,
                    RTRIM(LTRIM(ISNULL(b.fcCurp, ''))) AS CURP
                FROM 
                    [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                    LEFT JOIN [SIAC].[dbo].[SIVaBeneficiario] b WITH (NOLOCK) ON r.fiBeneficiario = b.fiBeneficiario
                    LEFT JOIN [SIAC].[dbo].[SIVaTelefonoBeneficiario] t WITH (NOLOCK) ON r.fiBeneficiario = t.fiBeneficiario AND t.flActivo = 1
                WHERE 
                    r.Estatus = 'Aceptado por Cobranza'
                    AND r.Programa = @Programa
                    AND r.fiMenAtrasadas >= @FiltroMensualidades
                    AND ISNULL(r.fiLiquidacionOrd, 0) > 0";

            // Agregar filtro de contacto
            switch (filtroContacto)
            {
                case "sin_contacto":
                    consulta += " AND (t.fiTelefono IS NULL OR t.fiTelefono = '') AND (b.fcEmail IS NULL OR b.fcEmail = '')";
                    break;
                case "con_telefono":
                    consulta += " AND t.fiTelefono IS NOT NULL AND t.fiTelefono != ''";
                    break;
                case "con_email":
                    consulta += " AND b.fcEmail IS NOT NULL AND b.fcEmail != ''";
                    break;
            }

            // Agregar ordenamiento
            switch (ordenamiento)
            {
                case "mensualidades_desc":
                    consulta += " ORDER BY r.fiMenAtrasadas DESC, NombreCompleto";
                    break;
                case "mensualidades_asc":
                    consulta += " ORDER BY r.fiMenAtrasadas ASC, NombreCompleto";
                    break;
                case "monto_desc":
                    consulta += " ORDER BY ISNULL(r.fiLiquidacionOrd, 0) DESC, r.fiMenAtrasadas DESC";
                    break;
                case "monto_asc":
                    consulta += " ORDER BY ISNULL(r.fiLiquidacionOrd, 0) ASC, r.fiMenAtrasadas DESC";
                    break;
                case "nombre_asc":
                    consulta += " ORDER BY NombreCompleto ASC";
                    break;
                case "ultimo_pago":
                    consulta += " ORDER BY fdUltimoPago ASC, r.fiMenAtrasadas DESC";
                    break;
                default:
                    consulta += " ORDER BY r.fiMenAtrasadas DESC, NombreCompleto";
                    break;
            }

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@Programa", programaSeleccionado);
            cmd.Parameters.AddWithValue("@FiltroMensualidades", int.Parse(filtroMensualidades));
            cmd.CommandTimeout = 180;
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            
            con.Open();
            da.Fill(dt);
            con.Close();

            GridViewBeneficiarios.DataSource = dt;
            GridViewBeneficiarios.DataBind();
        }
    }

    protected void GridViewBeneficiarios_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Colorear filas según número de mensualidades atrasadas (columna índice 4)
            if (e.Row.Cells.Count > 4)
            {
                int mensualidades;
                if (int.TryParse(e.Row.Cells[4].Text, out mensualidades))
                {
                    if (mensualidades >= 10)
                    {
                        e.Row.CssClass += " mensualidades-criticas";
                        e.Row.Cells[4].Font.Bold = true;
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                    }
                    else if (mensualidades >= 5)
                    {
                        e.Row.CssClass += " mensualidades-altas";
                        e.Row.Cells[4].Font.Bold = true;
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(245, 124, 0);
                    }
                    else if (mensualidades >= 3)
                    {
                        e.Row.CssClass += " mensualidades-moderadas";
                        e.Row.Cells[4].Font.Bold = true;
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                    }
                }
            }

            // Formatear columnas de montos (aplicar estilo a las columnas de saldos)
            // Columna 5: Monto Asignado
            if (e.Row.Cells.Count > 5) { e.Row.Cells[5].CssClass += " monto-asignado"; }
            
            // Columna 6: Pagado a Capital  
            if (e.Row.Cells.Count > 6) { e.Row.Cells[6].CssClass += " monto-pagado"; }
            
            // Columna 7: Saldo Pendiente Capital
            if (e.Row.Cells.Count > 7) { e.Row.Cells[7].CssClass += " saldo-pendiente"; }
            
            // Columna 8: Saldo Pendiente Interés Real
            if (e.Row.Cells.Count > 8) { e.Row.Cells[8].CssClass += " saldo-pendiente"; }
            
            // Columna 9: Saldo Pendiente Intereses Moratorios
            if (e.Row.Cells.Count > 9) { e.Row.Cells[9].CssClass += " saldo-pendiente"; }
            
            // Columna 10: Saldo para Liquidar (más importante, resaltar si es alto)
            if (e.Row.Cells.Count > 10)
            {
                decimal saldoLiquidar;
                if (decimal.TryParse(e.Row.Cells[10].Text.Replace("$", "").Replace(",", ""), out saldoLiquidar))
                {
                    e.Row.Cells[10].CssClass += " saldo-liquidar";
                    if (saldoLiquidar > 50000)
                    {
                        e.Row.Cells[10].CssClass += " monto-alto";
                        e.Row.Cells[10].Font.Bold = true;
                    }
                }
            }

            // Formatear fecha del último pago (columna índice 11)
            if (e.Row.Cells.Count > 11)
            {
                DateTime ultimoPago;
                if (DateTime.TryParse(e.Row.Cells[11].Text, out ultimoPago))
                {
                    // Si el último pago es muy antiguo (más de 6 meses), resaltarlo
                    if (ultimoPago < DateTime.Now.AddMonths(-6))
                    {
                        e.Row.Cells[11].BackColor = System.Drawing.Color.FromArgb(255, 243, 224);
                        e.Row.Cells[11].ForeColor = System.Drawing.Color.FromArgb(245, 124, 0);
                        e.Row.Cells[11].Font.Bold = true;
                    }
                }
                else if (string.IsNullOrEmpty(e.Row.Cells[11].Text) || e.Row.Cells[11].Text == "&nbsp;" || e.Row.Cells[11].Text == "N/A")
                {
                    e.Row.Cells[11].Text = "Sin pagos";
                    e.Row.Cells[11].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                    e.Row.Cells[11].Font.Bold = true;
                }
            }

            // Resaltar si no tiene información de contacto (Teléfono: columna 12, Email: columna 13)
            if (e.Row.Cells.Count > 12 && e.Row.Cells.Count > 13)
            {
                bool sinTelefono = string.IsNullOrEmpty(e.Row.Cells[12].Text) || e.Row.Cells[12].Text == "&nbsp;";
                bool sinEmail = string.IsNullOrEmpty(e.Row.Cells[13].Text) || e.Row.Cells[13].Text == "&nbsp;";
                
                if (sinTelefono && sinEmail)
                {
                    e.Row.Cells[12].Text = "❌ Sin teléfono";
                    e.Row.Cells[13].Text = "❌ Sin email";
                    e.Row.Cells[12].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                    e.Row.Cells[13].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                }
                else if (sinTelefono)
                {
                    e.Row.Cells[12].Text = "❌ Sin teléfono";
                    e.Row.Cells[12].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                }
                else if (sinEmail)
                {
                    e.Row.Cells[13].Text = "❌ Sin email";
                    e.Row.Cells[13].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                }
            }
        }
    }

    protected void GridViewBeneficiarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridViewBeneficiarios.PageIndex = e.NewPageIndex;
        CargarBeneficiariosConAtrasos();
    }

    protected void ddlFiltroMensualidades_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewBeneficiarios.PageIndex = 0; // Resetear a la primera página
        CargarBeneficiariosConAtrasos();
    }

    protected void ddlFiltroContacto_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewBeneficiarios.PageIndex = 0; // Resetear a la primera página
        CargarBeneficiariosConAtrasos();
    }

    protected void ddlOrdenamiento_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewBeneficiarios.PageIndex = 0; // Resetear a la primera página
        CargarBeneficiariosConAtrasos();
    }

    protected void btnExportarExcel_Click(object sender, EventArgs e)
    {
        // Exportar todos los datos sin paginación
        GridViewBeneficiarios.AllowPaging = false;
        CargarBeneficiariosConAtrasos();

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", string.Format("attachment;filename=Beneficiarios_Atrasados_{0}_{1:yyyyMMdd}.xls", 
            programaSeleccionado.Replace(" ", "_"), DateTime.Now));
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                // Escribir encabezado del archivo
                hw.WriteLine("<html><head><meta charset='UTF-8'></head><body>");
                hw.WriteLine(string.Format("<h2>Beneficiarios con Mensualidades Atrasadas - {0}</h2>", programaSeleccionado));
                hw.WriteLine(string.Format("<p>Reporte generado el: {0}</p>", DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
                hw.WriteLine("<table border='1' style='border-collapse:collapse'>");
                
                // Encabezados
                hw.WriteLine("<tr style='background-color:#3F51B5;color:white;font-weight:bold'>");
                hw.WriteLine("<td>ID Beneficiario</td>");
                hw.WriteLine("<td>Nombre Completo</td>");
                hw.WriteLine("<td>CURP</td>");
                hw.WriteLine("<td>Municipio</td>");
                hw.WriteLine("<td>Mensualidades Atrasadas</td>");
                hw.WriteLine("<td>Monto Asignado</td>");
                hw.WriteLine("<td>Saldo Pendiente</td>");
                hw.WriteLine("<td>Último Pago</td>");
                hw.WriteLine("<td>Teléfono</td>");
                hw.WriteLine("<td>Email</td>");
                hw.WriteLine("</tr>");
                
                // Datos
                foreach (GridViewRow row in GridViewBeneficiarios.Rows)
                {
                    hw.WriteLine("<tr>");
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        hw.WriteLine(string.Format("<td>{0}</td>", row.Cells[i].Text));
                    }
                    hw.WriteLine("</tr>");
                }
                
                hw.WriteLine("</table></body></html>");
                
                Response.Write(sw.ToString());
            }
        }

        Response.End();
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        // Requerido para exportar controles de servidor
    }
}
