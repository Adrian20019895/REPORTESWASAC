
// Página principal para mostrar y exportar montos por programa
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Web;
using System.Web.UI.WebControls;

// Clase parcial que representa la página VerDatos
public partial class VerDatos : System.Web.UI.Page
{
    // Carga inicial de la página: muestra datos y oculta mensaje de carga
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "document.getElementById('loadingMessage').style.display = 'block';", true);
            CargarMunicipios(); // Cargar municipios antes de mostrar datos
            MostrarDatos(); // Carga datos y totales
            ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "document.getElementById('loadingMessage').style.display = 'none';", true);
        }
    }

    // Calcula y muestra los totales generales o de un programa específico
    private void MostrarTotalesGenerales(string programa = null, string municipio = null)
    {
        ActualizarTituloResumen(programa, municipio);
        // Consulta SQL para totales generales
        string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consultaTotales = @"
                SELECT 
                    SUM(r.MontoAsignado) AS TotalAsignado, 
                    SUM(r.AmortizacionPag) AS MontoPagadoCapital,
                    SUM(r.InteresRealPag + r.InteresFijoPag + r.MoratorioPag - r.DescuentoPag) AS MontoPagadoIntereses,
                    SUM(r.TotalPag) AS TotalPagado,
                    SUM(r.MontoAsignado) - SUM(r.AmortizacionPag) AS SaldoPendienteCapital,
                    SUM(r.GastosAdmonPag + r.SeguroPag + r.OtroSeguroPag - r.SubsidioPag) AS OtrosGastos,
                    (ISNULL(SUM(r.fiLiquidacionOrd), 0) - ISNULL(SUM(r.fiInteresMoratorio), 0) - (SUM(r.MontoAsignado) - SUM(r.AmortizacionPag))) AS SaldoPendienteInteresReal,
                    ISNULL(SUM(r.fiInteresMoratorio), 0) AS SaldoPendienteInteresesMoratorios,
                    ISNULL(SUM(r.fiLiquidacionOrd), 0) AS TotalSaldoPendiente,
                    COUNT(DISTINCT r.fiBeneficiario) AS TotalBeneficiarios
                FROM 
                    [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE r.Estatus = 'Aceptado por Cobranza'";
                
            if (!string.IsNullOrEmpty(programa) && programa != "Vista general")
                consultaTotales += " AND r.Programa = @Programa";
                
            if (!string.IsNullOrEmpty(municipio) && municipio != "-- Todos los Municipios --")
                consultaTotales += " AND r.MunicipioContrato = @Municipio";
                
            SqlCommand cmd = new SqlCommand(consultaTotales, con);
            cmd.CommandTimeout = 180;
            
            if (!string.IsNullOrEmpty(programa) && programa != "Vista general")
                cmd.Parameters.AddWithValue("@Programa", programa);
                
            if (!string.IsNullOrEmpty(municipio) && municipio != "-- Todos los Municipios --")
                cmd.Parameters.AddWithValue("@Municipio", municipio);
                
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Asigna totales a los labels
                lblTotalAsignadoGeneral.Text = "$" + (reader["TotalAsignado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAsignado"]).ToString("N2") : "0.00");
                lblMontoPagadoCapitalGeneral.Text = "$" + (reader["MontoPagadoCapital"] != DBNull.Value ? Convert.ToDecimal(reader["MontoPagadoCapital"]).ToString("N2") : "0.00");
                lblMontoPagadoInteresesGeneral.Text = "$" + (reader["MontoPagadoIntereses"] != DBNull.Value ? Convert.ToDecimal(reader["MontoPagadoIntereses"]).ToString("N2") : "0.00");
                lblTotalPagadoGeneral.Text = "$" + (reader["TotalPagado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPagado"]).ToString("N2") : "0.00");
                lblSaldoPendienteCapitalGeneral.Text = "$" + (reader["SaldoPendienteCapital"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteCapital"]).ToString("N2") : "0.00");
                lblOtrosGastosGeneral.Text = "$" + (reader["OtrosGastos"] != DBNull.Value ? Convert.ToDecimal(reader["OtrosGastos"]).ToString("N2") : "0.00");
                // NUEVAS COLUMNAS EN EL RESUMEN
                lblSaldoPendienteInteresRealGeneral.Text = "$" + (reader["SaldoPendienteInteresReal"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteInteresReal"]).ToString("N2") : "0.00");
                lblSaldoPendienteInteresesMoratoriosGeneral.Text = "$" + (reader["SaldoPendienteInteresesMoratorios"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteInteresesMoratorios"]).ToString("N2") : "0.00");
                lblSaldoPendienteGeneral.Text = "$" + (reader["TotalSaldoPendiente"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSaldoPendiente"]).ToString("N2") : "0.00");
                lblTotalBeneficiariosGeneral.Text = reader["TotalBeneficiarios"] != DBNull.Value ? Convert.ToInt32(reader["TotalBeneficiarios"]).ToString() : "0";
            }
            reader.Close();
        }
    }

    // Consulta y muestra los datos en el GridView y llena el DropDownList de programas
    private void MostrarDatos()
    {
        // Consulta SQL para obtener resumen por programa
        string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    r.Programa, 
                    SUM(r.MontoAsignado) AS [Total Monto Asignado], 
                    SUM(r.AmortizacionPag) AS [Monto Pagado a Capital],
                    SUM(r.InteresRealPag + r.InteresFijoPag + r.MoratorioPag - r.DescuentoPag) AS [Monto Pagado a Intereses],
                    SUM(r.GastosAdmonPag + r.SeguroPag + r.OtroSeguroPag - r.SubsidioPag) AS [Otros Gastos],
                    SUM(r.TotalPag) AS [Total Monto Pagado],
                    SUM(r.MontoAsignado) - SUM(r.AmortizacionPag) AS [Saldo Pendiente a Capital],
                    (ISNULL(SUM(r.fiLiquidacionOrd), 0) - ISNULL(SUM(r.fiInteresMoratorio), 0) - (SUM(r.MontoAsignado) - SUM(r.AmortizacionPag))) AS [Saldo pendiente Interes Real],
                    ISNULL(SUM(r.fiInteresMoratorio), 0) AS [Saldo pendiente intereses moratorios],
                    ISNULL(SUM(r.fiLiquidacionOrd), 0) AS [Total Saldo Pendiente],
                    COUNT(DISTINCT r.fiBeneficiario) AS [Total Beneficiarios]
                FROM 
                    [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE r.Estatus = 'Aceptado por Cobranza'";
                
            // Agregar filtro de municipio si se ha seleccionado uno
            if (ddlMunicipios != null && !string.IsNullOrEmpty(ddlMunicipios.SelectedValue) && ddlMunicipios.SelectedValue != "-- Todos los Municipios --")
            {
                consulta += " AND r.MunicipioContrato = @Municipio";
            }
                
            consulta += @"
                GROUP BY 
                    r.Programa
                ORDER BY 
                    r.Programa";
                    
            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.CommandTimeout = 180;
            
            // Agregar parámetro de municipio si aplica
            if (ddlMunicipios != null && !string.IsNullOrEmpty(ddlMunicipios.SelectedValue) && ddlMunicipios.SelectedValue != "-- Todos los Municipios --")
            {
                cmd.Parameters.AddWithValue("@Municipio", ddlMunicipios.SelectedValue);
            }
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            con.Open();
            da.Fill(dt);
            con.Close();
            // Llenar el DropDownList solo la primera vez
            if (!IsPostBack)
            {
                ddlProgramas.Items.Clear();
                ddlProgramas.Items.Add("Vista general");
                foreach (DataRow row in dt.Rows)
                {
                    string programa = row["Programa"].ToString();
                    if (!ddlProgramas.Items.Contains(new System.Web.UI.WebControls.ListItem(programa)))
                        ddlProgramas.Items.Add(new System.Web.UI.WebControls.ListItem(programa));
                }
            }
            // Asigna los datos al GridView
            GridView2.DataSource = dt;
            GridView2.DataBind();
            
            // Obtener municipio seleccionado para los totales
            string municipioSeleccionado = ddlMunicipios != null && !string.IsNullOrEmpty(ddlMunicipios.SelectedValue) ? ddlMunicipios.SelectedValue : null;
            MostrarTotalesGenerales(null, municipioSeleccionado);
        }
    }

    // Actualiza el título del resumen según el programa y municipio seleccionado
    private void ActualizarTituloResumen(string programa = null, string municipio = null)
    {
        string titulo = "Resumen General de Programas";
        
        if (!string.IsNullOrEmpty(programa) && programa != "Vista general")
        {
            titulo = string.Format("Resumen del Programa: {0}", programa);
        }
        
        if (!string.IsNullOrEmpty(municipio) && municipio != "-- Todos los Municipios --")
        {
            if (!string.IsNullOrEmpty(programa) && programa != "Vista general")
                titulo += string.Format(" - Municipio: {0}", municipio);
            else
                titulo = string.Format("Resumen General - Municipio: {0}", municipio);
        }
        
        lblTituloResumen.Text = titulo;
    }

    // Exporta el GridView a PDF con formato y logos
    protected void btnExportarPDF_Click(object sender, EventArgs e)
    {
        GridView2.AllowPaging = false;
        MostrarDatos();
        // Configura respuesta PDF
        Response.ContentType = "application/pdf";
        Response.AddHeader("content-disposition", "attachment;filename=MontosPorPrograma.pdf");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        using (MemoryStream ms = new MemoryStream())
        {
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);
            pdfDoc.Open();
            // Agrega logos y encabezado
            string logo1Path = Server.MapPath("~/img/logocodesvi.png");
            if (File.Exists(logo1Path))
            {
                iTextSharp.text.Image logo1 = iTextSharp.text.Image.GetInstance(logo1Path);
                logo1.ScaleAbsolute(120, 120);
                PdfPTable headerTable = new PdfPTable(3);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1, 2, 1 });
                PdfPCell cellLogo1 = new PdfPCell(logo1) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE };
                PdfPCell cellTitle = new PdfPCell(new Phrase("Montos totales asignados por programa", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18)))
                { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                headerTable.AddCell(cellLogo1);
                headerTable.AddCell(cellTitle);
                pdfDoc.Add(headerTable);
                pdfDoc.Add(new Paragraph(" "));
            }
            // Tabla de datos
            int colCount = GridView2.HeaderRow.Cells.Count;
            int accionesIndex = 10;
            PdfPTable table = new PdfPTable(colCount - 1);
            table.WidthPercentage = 100;
            Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            for (int i = 0; i < colCount; i++)
            {
                if (i == accionesIndex) continue;
                TableCell cell = GridView2.HeaderRow.Cells[i];
                PdfPCell pdfCell = new PdfPCell(new Phrase(cell.Text, fontHeader));
                pdfCell.BackgroundColor = new BaseColor(216, 38, 40);
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.Padding = 5;
                table.AddCell(pdfCell);
            }
            table.HeaderRows = 1;
            foreach (GridViewRow row in GridView2.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    if (i == accionesIndex) continue;
                    string text = row.Cells[i].Text;
                    // Formatea montos como $N2
                    if (GridView2.HeaderRow.Cells[i].Text.Contains("Total Monto Asignado") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Monto Pagado a Capital") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Monto Pagado a Intereses") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Total Monto Pagado") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Saldo Pendiente a Capital") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Otros Gastos") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Saldo Pendiente a Intereses") ||
                        GridView2.HeaderRow.Cells[i].Text.Contains("Total Saldo Pendiente"))
                    {
                        decimal monto;
                        if (decimal.TryParse(text, out monto))
                            text = "$" + monto.ToString("N2");
                    }
                    PdfPCell pdfCell = new PdfPCell(new Phrase(text));
                    pdfCell.Padding = 5;
                    table.AddCell(pdfCell);
                }
            }
            pdfDoc.Add(table);
            pdfDoc.Close();
            byte[] bytes = ms.ToArray();
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            Response.Flush();
            Response.End();
        }
    }


    // Exporta el GridView a Excel (formato simple)
    protected void btnExportarExcel_Click(object sender, EventArgs e)
    {
        GridView2.AllowPaging = false;
        MostrarDatos();
        
        // Oculta columna de acciones
        GridView2.Columns[10].Visible = false;
        
        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=MontosPorPrograma.xls");
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";
        
        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                // Crear una tabla HTML para envolver el contenido del GridView
                hw.Write("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel'>");
                hw.Write("<head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /></head><body>");
                
                // Renderizar el GridView
                GridView2.RenderControl(hw);
                
                hw.Write("</body></html>");
                
                // Escribir en la respuesta
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }
        
        // Vuelve a mostrar la columna después de exportar
        GridView2.Columns[10].Visible = true;
    }

    // Requerido por ASP.NET para exportar controles de servidor
    public override void VerifyRenderingInServerForm(Control control) { }

    // Filtra los datos por programa seleccionado y actualiza el resumen
    protected void btnGenerarReporte_Click(object sender, EventArgs e)
    {
        string seleccion = ddlProgramas.SelectedValue;
        string municipioSeleccionado = ddlMunicipios != null && !string.IsNullOrEmpty(ddlMunicipios.SelectedValue) ? ddlMunicipios.SelectedValue : null;
        
        if (seleccion != "Vista general")
        {
            MostrarTotalesGenerales(seleccion, municipioSeleccionado);
            // Consulta y muestra solo el programa seleccionado
            string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string consulta = @"
                    SELECT 
                        r.Programa, 
                        SUM(r.MontoAsignado) AS [Total Monto Asignado], 
                        SUM(r.AmortizacionPag) AS [Monto Pagado a Capital],
                        SUM(r.InteresRealPag + r.InteresFijoPag + r.MoratorioPag - r.DescuentoPag) AS [Monto Pagado a Intereses],
                        SUM(r.GastosAdmonPag + r.SeguroPag + r.OtroSeguroPag - r.SubsidioPag) AS [Otros Gastos],
                        SUM(r.TotalPag) AS [Total Monto Pagado],
                        SUM(r.MontoAsignado) - SUM(r.AmortizacionPag) AS [Saldo Pendiente a Capital],
                        (ISNULL(SUM(r.fiLiquidacionOrd), 0) - ISNULL(SUM(r.fiInteresMoratorio), 0) - (SUM(r.MontoAsignado) - SUM(r.AmortizacionPag))) AS [Saldo pendiente Interes Real],
                        ISNULL(SUM(r.fiInteresMoratorio), 0) AS [Saldo pendiente intereses moratorios],
                        ISNULL(SUM(r.fiLiquidacionOrd), 0) AS [Total Saldo Pendiente],
                        COUNT(DISTINCT r.fiBeneficiario) AS [Total Beneficiarios]
                    FROM 
                        [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                    WHERE
                        r.Programa = @Programa AND r.Estatus = 'Aceptado por Cobranza'";
                        
                // Agregar filtro de municipio si se ha seleccionado uno
                if (!string.IsNullOrEmpty(municipioSeleccionado) && municipioSeleccionado != "-- Todos los Municipios --")
                {
                    consulta += " AND r.MunicipioContrato = @Municipio";
                }
                        
                consulta += @"
                    GROUP BY 
                        r.Programa
                    ORDER BY 
                        r.Programa";
                        
                SqlCommand cmd = new SqlCommand(consulta, con);
                cmd.CommandTimeout = 180;
                cmd.Parameters.AddWithValue("@Programa", seleccion);
                
                // Agregar parámetro de municipio si aplica
                if (!string.IsNullOrEmpty(municipioSeleccionado) && municipioSeleccionado != "-- Todos los Municipios --")
                {
                    cmd.Parameters.AddWithValue("@Municipio", municipioSeleccionado);
                }
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GridView2.DataSource = dt;
                GridView2.DataBind();
            }
        }
        else
        {
            MostrarDatos();
        }
    }

    // Da formato visual a las filas del GridView (colores, negritas, moneda)
    protected void GridView2_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Formato moneda para columnas de montos
            for (int i = 1; i <= 9; i++)
                if (e.Row.Cells.Count > i)
                {
                    decimal valor;
                    if (decimal.TryParse(e.Row.Cells[i].Text, out valor))
                        e.Row.Cells[i].Text = "$" + valor.ToString("N2");
                }
            // Colores y negritas por columna
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
                e.Row.Cells[1].ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                e.Row.Cells[1].Font.Bold = true;
            }
            for (int i = 2; i <= 5; i++)
                if (e.Row.Cells.Count > i)
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(200, 230, 201);
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.FromArgb(27, 94, 32);
                }
            if (e.Row.Cells.Count > 5)
                e.Row.Cells[5].Font.Bold = true;
            for (int i = 6; i <= 9; i++)
                if (e.Row.Cells.Count > i)
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(255, 205, 210);
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                }
            // Negrita solo en Total Saldo Pendiente (9), sin negrita en intereses moratorios (8)
            if (e.Row.Cells.Count > 8)
                e.Row.Cells[8].Font.Bold = false;
            if (e.Row.Cells.Count > 9)
                e.Row.Cells[9].Font.Bold = true;
        }
    }

    // Redirige a la página de detalle de saldos al hacer clic en el botón correspondiente
    protected void GridView2_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "VerDetalleSaldos")
        {
            string programa = e.CommandArgument.ToString();
            Response.Redirect("DetalleSaldos.aspx?Programa=" + programa);
        }
    }

    // Botón para ir a la página de Beneficiarios No Aceptados
    protected void btnBeneficiariosNoAceptados_Click(object sender, EventArgs e)
    {
        Response.Redirect("BeneficiariosNoAceptados.aspx");
    }
    
    // Botón para ir a la página de Notificaciones
    protected void btnNotificaciones_Click(object sender, EventArgs e)
    {
        Response.Redirect("Notificaciones.aspx");
    }
    
    // Carga la lista de municipios disponibles en todos los programas
    private void CargarMunicipios()
    {
        string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"SELECT DISTINCT MunicipioContrato 
                               FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] 
                               WHERE Estatus = 'Aceptado por Cobranza' 
                               AND MunicipioContrato IS NOT NULL AND MunicipioContrato <> ''
                               ORDER BY MunicipioContrato";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.CommandTimeout = 180;
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            ddlMunicipios.Items.Clear();
            ddlMunicipios.Items.Add(new System.Web.UI.WebControls.ListItem("-- Todos los Municipios --", ""));

            while (reader.Read())
            {
                string municipio = reader["MunicipioContrato"].ToString().Trim();
                if (!string.IsNullOrEmpty(municipio))
                {
                    ddlMunicipios.Items.Add(new System.Web.UI.WebControls.ListItem(municipio, municipio));
                }
            }

            reader.Close();
        }
    }
    
    // Evento para el cambio de selección del municipio
    protected void ddlMunicipios_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Mostrar indicador de carga
        ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "document.getElementById('loadingMessage').style.display = 'block';", true);
        
        // Recargar datos con el filtro de municipio
        // Si hay un programa seleccionado diferente a "Vista general", mantener esa selección
        string programaSeleccionado = ddlProgramas.SelectedValue;
        if (!string.IsNullOrEmpty(programaSeleccionado) && programaSeleccionado != "Vista general")
        {
            btnGenerarReporte_Click(sender, e);
        }
        else
        {
            MostrarDatos();
        }
        
        // Ocultar indicador de carga
        ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "document.getElementById('loadingMessage').style.display = 'none';", true);
    }

    // Evento del botón de Cobranza Diaria
    protected void btnCobranza_Click(object sender, EventArgs e)
    {
        Response.Redirect("CobranzaDiaria.aspx");
    }
}
