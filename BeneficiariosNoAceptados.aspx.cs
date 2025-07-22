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

public partial class BeneficiariosNoAceptados : System.Web.UI.Page
{
    // Cadena de conexión a la base de datos
    private string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Cargar datos iniciales
            CargarEstatusDisponibles();
            CargarBeneficiarios();
            CargarEstadisticas();
        }
    }

    // Carga los estatus disponibles (excluyendo "Aceptado por Cobranza")
    private void CargarEstatusDisponibles()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT DISTINCT Estatus 
                FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] 
                WHERE Estatus IS NOT NULL AND Estatus <> 'Aceptado por Cobranza'
                ORDER BY Estatus";

            SqlCommand cmd = new SqlCommand(consulta, con);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string estatus = reader["Estatus"].ToString();
                // Usar la referencia específica para evitar ambigüedad
                ddlEstatus.Items.Add(new System.Web.UI.WebControls.ListItem(estatus, estatus));
            }
            
            reader.Close();
        }
    }

    // Carga el listado de beneficiarios no aceptados
    private void CargarBeneficiarios()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    r.fiBeneficiario,
                    (r.Nombre + ' ' + r.Apaterno + ' ' + ISNULL(r.Amaterno, '')) AS NombreCompleto,
                    r.Programa,
                    r.Subprograma,
                    r.Estatus,
                    r.MontoAsignado,
                    GETDATE() AS FechaRegistro,
                    CASE 
                        WHEN r.Estatus = 'Pendiente' THEN 'Solicitud en proceso de revisión'
                        WHEN r.Estatus = 'Rechazado' THEN 'No cumple con los requisitos'
                        WHEN r.Estatus = 'En Revisión' THEN 'Documentación bajo análisis'
                        ELSE 'Revisar con el área de cobranza'
                    END AS Observaciones,
                    r.fiSubprograma
                FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE r.Estatus IS NOT NULL AND r.Estatus <> 'Aceptado por Cobranza'";

            // Aplicar filtros si están seleccionados
            if (!string.IsNullOrEmpty(ddlEstatus.SelectedValue))
            {
                consulta += " AND r.Estatus = @Estatus";
            }

            if (!string.IsNullOrEmpty(txtBuscar.Text))
            {
                consulta += " AND (r.Nombre LIKE @Buscar OR r.Apaterno LIKE @Buscar OR r.Amaterno LIKE @Buscar OR r.Programa LIKE @Buscar)";
            }

            consulta += " ORDER BY r.Programa, r.Nombre, r.Apaterno";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.CommandTimeout = 180; // 3 minutos para consultas grandes
            
            if (!string.IsNullOrEmpty(ddlEstatus.SelectedValue))
            {
                cmd.Parameters.AddWithValue("@Estatus", ddlEstatus.SelectedValue);
            }
            
            if (!string.IsNullOrEmpty(txtBuscar.Text))
            {
                cmd.Parameters.AddWithValue("@Buscar", "%" + txtBuscar.Text + "%");
            }

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            GridViewBeneficiarios.DataSource = dt;
            GridViewBeneficiarios.DataBind();
        }
    }

    // Carga las estadísticas generales
    private void CargarEstadisticas()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    COUNT(DISTINCT r.fiBeneficiario) AS TotalBeneficiarios,
                    COUNT(DISTINCT r.Programa) AS TotalProgramas,
                    SUM(r.MontoAsignado) AS MontoTotal
                FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE r.Estatus IS NOT NULL AND r.Estatus <> 'Aceptado por Cobranza'";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.CommandTimeout = 180; // 3 minutos para consultas grandes
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int totalBeneficiarios = reader["TotalBeneficiarios"] != DBNull.Value ? Convert.ToInt32(reader["TotalBeneficiarios"]) : 0;
                int totalProgramas = reader["TotalProgramas"] != DBNull.Value ? Convert.ToInt32(reader["TotalProgramas"]) : 0;
                decimal montoTotal = reader["MontoTotal"] != DBNull.Value ? Convert.ToDecimal(reader["MontoTotal"]) : 0;

                lblTotalBeneficiarios.Text = totalBeneficiarios.ToString("N0");
                lblTotalProgramas.Text = totalProgramas.ToString("N0");
                lblMontoTotal.Text = "$" + montoTotal.ToString("N2");
            }
            
            reader.Close();
        }
    }

    // Formato de celdas en GridView
    protected void GridViewBeneficiarios_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Formatear la columna de Monto Asignado (índice 5)
            if (e.Row.Cells.Count > 5)
            {
                decimal valor;
                if (decimal.TryParse(e.Row.Cells[5].Text, out valor))
                {
                    e.Row.Cells[5].Text = "$" + valor.ToString("N2");
                }
            }

            // Formatear la columna de Fecha Registro (índice 6)
            if (e.Row.Cells.Count > 6)
            {
                DateTime fecha;
                if (DateTime.TryParse(e.Row.Cells[6].Text, out fecha))
                {
                    e.Row.Cells[6].Text = fecha.ToString("dd/MM/yyyy");
                }
            }

            // Aplicar colores según el estatus
            if (e.Row.Cells.Count > 4)
            {
                string estatus = e.Row.Cells[4].Text;
                switch (estatus.ToLower())
                {
                    case "pendiente":
                        e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(255, 243, 205);
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(133, 100, 4);
                        break;
                    case "rechazado":
                        e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(255, 205, 210);
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                        break;
                    case "en revisión":
                    case "en revision":
                        e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(225, 242, 253);
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
                        break;
                    default:
                        e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
                        e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                        break;
                }
                e.Row.Cells[4].Font.Bold = true;
            }
        }
    }

    // Botón para buscar beneficiarios
    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        CargarBeneficiarios();
        CargarEstadisticas();
    }

    // Botón para volver a la página anterior
    protected void btnVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("VerDatos.aspx");
    }

    // Exporta los datos a PDF
    protected void btnExportarPDF_Click(object sender, EventArgs e)
    {
        GridViewBeneficiarios.AllowPaging = false;
        CargarBeneficiarios();

        Response.ContentType = "application/pdf";
        Response.AddHeader("content-disposition", "attachment;filename=BeneficiariosNoAceptados_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);

        using (MemoryStream ms = new MemoryStream())
        {
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);
            pdfDoc.Open();

            // Título del documento
            iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
            Paragraph title = new Paragraph("BENEFICIARIOS NO ACEPTADOS POR COBRANZA", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            pdfDoc.Add(title);

            // Tabla de datos
            if (GridViewBeneficiarios.Rows.Count > 0)
            {
                PdfPTable dataTable = new PdfPTable(GridViewBeneficiarios.Columns.Count);
                dataTable.WidthPercentage = 100;

                iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(216, 38, 40);

                // Headers
                for (int i = 0; i < GridViewBeneficiarios.Columns.Count; i++)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(GridViewBeneficiarios.Columns[i].HeaderText, tableHeaderFont));
                    headerCell.BackgroundColor = headerColor;
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5f;
                    dataTable.AddCell(headerCell);
                }

                // Datos
                iTextSharp.text.Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK);
                foreach (GridViewRow row in GridViewBeneficiarios.Rows)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        PdfPCell dataCell = new PdfPCell(new Phrase(row.Cells[i].Text, dataFont));
                        dataCell.Padding = 3f;
                        dataTable.AddCell(dataCell);
                    }
                }
                
                pdfDoc.Add(dataTable);
            }

            pdfDoc.Close();
            byte[] bytes = ms.ToArray();
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            Response.Flush();
            Response.End();
        }
    }

    // Exporta los datos a Excel
    protected void btnExportarExcel_Click(object sender, EventArgs e)
    {
        GridViewBeneficiarios.AllowPaging = false;
        CargarBeneficiarios();

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=BeneficiariosNoAceptados_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                // Escribir encabezado HTML para Excel
                hw.Write("<html><head><meta charset='UTF-8'></head><body>");
                hw.Write("<h2>Beneficiarios No Aceptados por Cobranza</h2>");
                hw.Write("<table border='1' cellpadding='3' cellspacing='0'>");
                
                // Escribir encabezados
                hw.Write("<tr>");
                for (int i = 0; i < GridViewBeneficiarios.Columns.Count; i++)
                {
                    hw.Write("<th style='background-color: #D82628; color: white;'>");
                    hw.Write(GridViewBeneficiarios.Columns[i].HeaderText);
                    hw.Write("</th>");
                }
                hw.Write("</tr>");
                
                // Escribir datos
                if (GridViewBeneficiarios.Rows.Count > 0)
                {
                    foreach (GridViewRow row in GridViewBeneficiarios.Rows)
                    {
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            hw.Write("<tr>");
                            for (int i = 0; i < row.Cells.Count; i++)
                            {
                                hw.Write("<td>");
                                string cellText = row.Cells[i].Text;
                                if (cellText == "&nbsp;")
                                    cellText = "";
                                hw.Write(cellText);
                                hw.Write("</td>");
                            }
                            hw.Write("</tr>");
                        }
                    }
                }
                
                hw.Write("</table></body></html>");
                
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }
    }

    // Requerido por ASP.NET para exportar controles de servidor
    public override void VerifyRenderingInServerForm(Control control)
    {
        // Método vacío para evitar errores de renderizado al exportar
    }
}