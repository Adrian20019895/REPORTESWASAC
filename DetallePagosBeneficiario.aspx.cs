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

public partial class DetallePagosBeneficiario : System.Web.UI.Page
{
    // Cadena de conexión a la base de datos
    private string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
    private int idBeneficiario = 0;
    private int idSubprograma = 0;
    private string nombreBeneficiario = "";
    private string programa = "";
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Recupera los parámetros de la QueryString
            if (Request.QueryString["IdBeneficiario"] != null && Request.QueryString["IdSubprograma"] != null)
            {
                idBeneficiario = Convert.ToInt32(Request.QueryString["IdBeneficiario"]);
                idSubprograma = Convert.ToInt32(Request.QueryString["IdSubprograma"]);
                
                // Obtener información del beneficiario
                ObtenerInformacionBeneficiario();
                
                // Cargar el resumen de pagos
                CargarResumenPagos();
                
                // Cargar el detalle de pagos
                CargarDetallePagos();
            }
            else
            {
                // Redirige a la página anterior si no hay parámetros
                Response.Redirect("DetalleSaldos.aspx");
            }
        }
    }

    // Obtiene la información básica del beneficiario
    private void ObtenerInformacionBeneficiario()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT TOP 1
                    Nombre + ' ' + Apaterno + ' ' + Amaterno AS NombreCompleto,
                    Programa,
                    MontoAsignado
                FROM [SIAC].[dbo].[SIVvwPagosConDatosGenerales] 
                WHERE fiBeneficiario = @IdBeneficiario AND fiSubprograma = @IdSubprograma";
            
            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@IdBeneficiario", idBeneficiario);
            cmd.Parameters.AddWithValue("@IdSubprograma", idSubprograma);
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                nombreBeneficiario = reader["NombreCompleto"].ToString();
                programa = reader["Programa"].ToString();
                decimal montoAsignado = reader["MontoAsignado"] != DBNull.Value ? Convert.ToDecimal(reader["MontoAsignado"]) : 0;
                
                lblNombreBeneficiario.Text = nombreBeneficiario;
                lblPrograma.Text = programa;
                lblMontoAsignado.Text = "$" + montoAsignado.ToString("N2");
                
                // Obtener y mostrar el saldo para liquidar
                decimal saldoLiquidar = ObtenerSaldoParaLiquidar();
                lblSaldoLiquidar.Text = "$" + saldoLiquidar.ToString("N2");
            }
            
            reader.Close();
        }
    }

    // Carga el resumen de pagos del beneficiario
    private void CargarResumenPagos()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    COUNT(*) AS TotalPagos,
                    SUM(fiTotal) AS MontoTotalPagado,
                    MAX(fecAlta) AS UltimoPago
                FROM [SIAC].[dbo].[SIVaSolicitudCaja] 
                WHERE fiBeneficiario = @IdBeneficiario 
                AND fiSubprograma = @IdSubprograma 
                AND fiTipoPago <> 3";
            
            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@IdBeneficiario", idBeneficiario);
            cmd.Parameters.AddWithValue("@IdSubprograma", idSubprograma);
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                int totalPagos = reader["TotalPagos"] != DBNull.Value ? Convert.ToInt32(reader["TotalPagos"]) : 0;
                decimal montoTotalPagado = reader["MontoTotalPagado"] != DBNull.Value ? Convert.ToDecimal(reader["MontoTotalPagado"]) : 0;
                DateTime ultimoPago = reader["UltimoPago"] != DBNull.Value ? Convert.ToDateTime(reader["UltimoPago"]) : DateTime.MinValue;
                
                lblTotalPagos.Text = totalPagos.ToString("N0");
                lblMontoTotalPagado.Text = "$" + montoTotalPagado.ToString("N2");
                lblUltimoPago.Text = ultimoPago != DateTime.MinValue ? ultimoPago.ToString("dd/MM/yyyy") : "N/A";
            }
            
            reader.Close();
        }
    }

    // Carga el detalle de todos los pagos del beneficiario
    private void CargarDetallePagos()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY fecAlta ASC, fiSolicitudCaja ASC) as fiPago,
                    fecAlta as fcFecha,
                    fiTotal as fiMonto,
                    fiAmort as fiAmortizacion,
                    fiReal as fiInteresReal,
                    ISNULL(fiMoratorio, 0) as fiMoratorio,
                    ISNULL(fiDescuento, 0) as fiDescuento,
                    ISNULL(usrAlta, '') as fcCobratorio,
                    ISNULL(fcObservacion, '') as fcObservaciones
                FROM [SIAC].[dbo].[SIVaSolicitudCaja] 
                WHERE fiBeneficiario = @IdBeneficiario 
                AND fiSubprograma = @IdSubprograma 
                AND fiTipoPago <> 3
                ORDER BY fecAlta ASC, fiSolicitudCaja ASC";
            
            SqlDataAdapter da = new SqlDataAdapter(consulta, con);
            da.SelectCommand.Parameters.AddWithValue("@IdBeneficiario", idBeneficiario);
            da.SelectCommand.Parameters.AddWithValue("@IdSubprograma", idSubprograma);
            
            DataTable dt = new DataTable();
            da.Fill(dt);
            
            // Agregar una columna calculada para el saldo
            dt.Columns.Add("fiSaldo", typeof(decimal));
            
            // Obtener el monto asignado del beneficiario para calcular el saldo
            decimal montoAsignado = ObtenerMontoAsignado();
            decimal capitalAcumulado = 0;
            
            // Calcular el saldo después de cada pago (basado en capital acumulado)
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                decimal capitalPago = Convert.ToDecimal(dt.Rows[i]["fiAmortizacion"]);
                capitalAcumulado += capitalPago;
                dt.Rows[i]["fiSaldo"] = montoAsignado - capitalAcumulado;
            }
            
            GridViewPagos.DataSource = dt;
            GridViewPagos.DataBind();
        }
    }
    
    // Obtiene el monto asignado del beneficiario
    private decimal ObtenerMontoAsignado()
    {
        decimal montoAsignado = 0;
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT TOP 1 MontoAsignado 
                FROM [SIAC].[dbo].[SIVvwPagosConDatosGenerales] 
                WHERE fiBeneficiario = @IdBeneficiario 
                AND fiSubprograma = @IdSubprograma";
            
            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@IdBeneficiario", idBeneficiario);
            cmd.Parameters.AddWithValue("@IdSubprograma", idSubprograma);
            
            con.Open();
            object result = cmd.ExecuteScalar();
            
            if (result != null && result != DBNull.Value)
            {
                montoAsignado = Convert.ToDecimal(result);
            }
        }
        return montoAsignado;
    }

    // Obtiene el saldo para liquidar usando el mismo procedimiento que ConsultaSaldos
    private decimal ObtenerSaldoParaLiquidar()
    {
        decimal saldoLiquidar = 0;
        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand("spSIVCnListaSaldoActBeneficiario", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pfiSubprograma", idSubprograma);
            cmd.Parameters.AddWithValue("@pfiBeneficiario", idBeneficiario);
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                // Usar la liquidación inmediata como saldo para liquidar
                saldoLiquidar = reader["fiLiquidacionIme"] != DBNull.Value ? Convert.ToDecimal(reader["fiLiquidacionIme"]) : 0;
            }
            
            reader.Close();
        }
        return saldoLiquidar;
    }

    // Formatea las columnas de montos en el GridView
    protected void GridViewPagos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Formatea la columna Monto Pagado (índice 2)
            decimal montoPagado;
            if (decimal.TryParse(e.Row.Cells[2].Text, out montoPagado))
            {
                e.Row.Cells[2].Text = "$" + montoPagado.ToString("N2");
            }

            // Formatea la columna Capital (índice 3)
            decimal capital;
            if (decimal.TryParse(e.Row.Cells[3].Text, out capital))
            {
                e.Row.Cells[3].Text = "$" + capital.ToString("N2");
            }

            // Formatea la columna Interés Real (índice 4)
            decimal interesReal;
            if (decimal.TryParse(e.Row.Cells[4].Text, out interesReal))
            {
                e.Row.Cells[4].Text = "$" + interesReal.ToString("N2");
            }

            // Formatea la columna Moratorio (índice 5)
            decimal moratorio;
            if (decimal.TryParse(e.Row.Cells[5].Text, out moratorio))
            {
                e.Row.Cells[5].Text = "$" + moratorio.ToString("N2");
            }

            // Formatea la columna Descuento (índice 6)
            decimal descuento;
            if (decimal.TryParse(e.Row.Cells[6].Text, out descuento))
            {
                e.Row.Cells[6].Text = "$" + descuento.ToString("N2");
            }

            // Formatea la columna Saldo (índice 7)
            decimal saldo;
            if (decimal.TryParse(e.Row.Cells[7].Text, out saldo))
            {
                e.Row.Cells[7].Text = "$" + saldo.ToString("N2");
                
                // Aplica color condicional al saldo
                if (saldo <= 0)
                {
                    e.Row.Cells[7].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    e.Row.Cells[7].ForeColor = System.Drawing.Color.Red;
                }
            }
        }
    }

    // Botón para volver a la página anterior
    protected void btnVolver_Click(object sender, EventArgs e)
    {
        if (Request.QueryString["Programa"] != null)
        {
            Response.Redirect("DetalleSaldos.aspx?Programa=" + Request.QueryString["Programa"]);
        }
        else
        {
            Response.Redirect("DetalleSaldos.aspx");
        }
    }

    // Botón para ir a ConsultaSaldos con la información del usuario actual
    protected void btnConsultaSaldos_Click(object sender, EventArgs e)
    {
        // Obtener los parámetros directamente del QueryString original
        string beneficiario = Request.QueryString["IdBeneficiario"];
        string subprograma = Request.QueryString["IdSubprograma"];
        
        if (!string.IsNullOrEmpty(beneficiario) && !string.IsNullOrEmpty(subprograma))
        {
            // Redirigir a ConsultaSaldos.aspx con los parámetros del usuario actual
            string url = string.Format("ConsultaSaldos.aspx?IdBeneficiario={0}&IdSubprograma={1}", 
                                       beneficiario, subprograma);
            Response.Redirect(url);
        }
        else
        {
            // Si no hay parámetros, usar las variables de clase como respaldo
            string url = string.Format("ConsultaSaldos.aspx?IdBeneficiario={0}&IdSubprograma={1}", 
                                       idBeneficiario, idSubprograma);
            Response.Redirect(url);
        }
    }

    // Exporta los datos a PDF
    protected void btnExportarPDF_Click(object sender, EventArgs e)
    {
        GridViewPagos.AllowPaging = false;
        CargarDetallePagos();

        Response.ContentType = "application/pdf";
        Response.AddHeader("content-disposition", "attachment;filename=HistorialPagos_" + nombreBeneficiario.Replace(" ", "_") + ".pdf");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);

        using (MemoryStream ms = new MemoryStream())
        {
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);
            pdfDoc.Open();

            // --- AGREGAR LOGO INSTITUCIONAL MEJORADO ---
            string logo1Path = Server.MapPath("~/img/logocodesvi.png");
            if (File.Exists(logo1Path))
            {
                iTextSharp.text.Image logo1 = iTextSharp.text.Image.GetInstance(logo1Path);
                logo1.ScaleToFit(60f, 60f); // Tamaño proporcionalmente correcto
                PdfPTable headerTable = new PdfPTable(3);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1, 3, 1 });

                PdfPCell cellLogo1 = new PdfPCell(logo1) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE };
                
                // Título mejorado con subtítulo
                Paragraph mainTitle = new Paragraph("HISTORIAL DE PAGOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK));
                mainTitle.Alignment = Element.ALIGN_CENTER;
                Paragraph subTitle = new Paragraph(nombreBeneficiario, FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY));
                subTitle.Alignment = Element.ALIGN_CENTER;
                subTitle.SpacingBefore = 5f;
                
                PdfPCell cellTitle = new PdfPCell() { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                cellTitle.AddElement(mainTitle);
                cellTitle.AddElement(subTitle);
                
                PdfPCell cellEmpty = new PdfPCell(new Phrase("")) { Border = 0 };
                headerTable.AddCell(cellLogo1);
                headerTable.AddCell(cellTitle);
                headerTable.AddCell(cellEmpty);
                pdfDoc.Add(headerTable);
                pdfDoc.Add(new Paragraph(" ")); // Espacio después del encabezado
            }

            // === SECCIÓN 1: INFORMACIÓN DEL BENEFICIARIO ===
            Paragraph tituloInfo = new Paragraph("INFORMACIÓN DEL BENEFICIARIO", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.RED));
            tituloInfo.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(tituloInfo);
            pdfDoc.Add(new Paragraph(" "));

            // Información del beneficiario
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 1, 1 });

            Font fontInfoHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            Font fontInfoValue = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

            PdfPCell cellBeneficiarioHeader = new PdfPCell(new Phrase("Beneficiario", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };
            PdfPCell cellProgramaHeader = new PdfPCell(new Phrase("Programa", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };

            infoTable.AddCell(cellBeneficiarioHeader);
            infoTable.AddCell(cellProgramaHeader);

            PdfPCell cellBeneficiarioValue = new PdfPCell(new Phrase(nombreBeneficiario, fontInfoValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };
            PdfPCell cellProgramaValue = new PdfPCell(new Phrase(programa, fontInfoValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };

            infoTable.AddCell(cellBeneficiarioValue);
            infoTable.AddCell(cellProgramaValue);

            pdfDoc.Add(infoTable);
            pdfDoc.Add(new Paragraph(" "));

            // === SECCIÓN 2: RESUMEN DE PAGOS ===
            Paragraph tituloResumen = new Paragraph("RESUMEN DE PAGOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.RED));
            tituloResumen.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(tituloResumen);
            pdfDoc.Add(new Paragraph(" "));

            PdfPTable resumenTable = new PdfPTable(4);
            resumenTable.WidthPercentage = 100;
            resumenTable.SetWidths(new float[] { 1, 1, 1, 1 });

            PdfPCell cellTotalPagosHeader = new PdfPCell(new Phrase("Total de Pagos", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };
            PdfPCell cellMontoTotalHeader = new PdfPCell(new Phrase("Monto Total Pagado", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };
            PdfPCell cellUltimoPagoHeader = new PdfPCell(new Phrase("Ultimo Pago", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };
            PdfPCell cellMontoAsignadoHeader = new PdfPCell(new Phrase("Monto Asignado", fontInfoHeader))
            { BackgroundColor = new BaseColor(216, 38, 40), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };

            resumenTable.AddCell(cellTotalPagosHeader);
            resumenTable.AddCell(cellMontoTotalHeader);
            resumenTable.AddCell(cellUltimoPagoHeader);
            resumenTable.AddCell(cellMontoAsignadoHeader);

            Font fontResumenValue = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

            PdfPCell cellTotalPagosValue = new PdfPCell(new Phrase(lblTotalPagos.Text, fontResumenValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };
            PdfPCell cellMontoTotalValue = new PdfPCell(new Phrase(lblMontoTotalPagado.Text, fontResumenValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };
            PdfPCell cellUltimoPagoValue = new PdfPCell(new Phrase(lblUltimoPago.Text, fontResumenValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };
            PdfPCell cellMontoAsignadoValue = new PdfPCell(new Phrase(lblMontoAsignado.Text, fontResumenValue))
            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10, BackgroundColor = BaseColor.LIGHT_GRAY };

            resumenTable.AddCell(cellTotalPagosValue);
            resumenTable.AddCell(cellMontoTotalValue);
            resumenTable.AddCell(cellUltimoPagoValue);
            resumenTable.AddCell(cellMontoAsignadoValue);

            pdfDoc.Add(resumenTable);
            pdfDoc.Add(new Paragraph(" "));

            // === SECCIÓN 3: GRÁFICO DE PROGRESO ===
            Paragraph tituloGrafico = new Paragraph("PROGRESO DE PAGOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.RED));
            tituloGrafico.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(tituloGrafico);
            pdfDoc.Add(new Paragraph(" "));

            // Crear gráfico de progreso usando barras
            decimal montoAsignado = 0, montoTotalPagado = 0;
            if (decimal.TryParse(lblMontoAsignado.Text.Replace("$", "").Replace(",", ""), out montoAsignado) &&
                decimal.TryParse(lblMontoTotalPagado.Text.Replace("$", "").Replace(",", ""), out montoTotalPagado))
            {
                decimal porcentajePagado = montoAsignado > 0 ? (montoTotalPagado / montoAsignado) * 100 : 0;
                decimal porcentajePendiente = 100 - porcentajePagado;

                // Crear representación visual mejorada del progreso
                PdfPTable graficoTable = new PdfPTable(1);
                graficoTable.WidthPercentage = 80;
                graficoTable.HorizontalAlignment = Element.ALIGN_CENTER;

                // Información del progreso
                string progresoTexto = string.Format("Progreso de Pagos: {0:F1}% completado\n\n", porcentajePagado);
                progresoTexto += string.Format("■ Monto Pagado: {0} ({1:F1}%)\n", lblMontoTotalPagado.Text, porcentajePagado);
                progresoTexto += string.Format("■ Monto Pendiente: {0:C} ({1:F1}%)\n", (montoAsignado - montoTotalPagado), porcentajePendiente);
                progresoTexto += string.Format("■ Monto Total Asignado: {0}", lblMontoAsignado.Text);

                PdfPCell cellProgreso = new PdfPCell(new Phrase(progresoTexto, FontFactory.GetFont(FontFactory.HELVETICA, 10)))
                { 
                    Border = 1, 
                    HorizontalAlignment = Element.ALIGN_LEFT, 
                    VerticalAlignment = Element.ALIGN_MIDDLE, 
                    Padding = 15,
                    BackgroundColor = new BaseColor(248, 249, 250)
                };

                graficoTable.AddCell(cellProgreso);
                pdfDoc.Add(graficoTable);
            }

            pdfDoc.Add(new Paragraph(" "));
            pdfDoc.Add(new Paragraph(" "));

            // === SECCIÓN 4: HISTORIAL DETALLADO ===
            Paragraph tituloHistorial = new Paragraph("HISTORIAL DETALLADO DE PAGOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.RED));
            tituloHistorial.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(tituloHistorial);
            pdfDoc.Add(new Paragraph(" "));

            // --- TABLA DE DATOS ---
            int colCount = GridViewPagos.HeaderRow.Cells.Count;
            PdfPTable table = new PdfPTable(colCount);

            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 0.8f, 1f, 1.2f, 1.2f, 1.2f, 1f, 1f, 1.2f, 1.5f, 2f }); // Ajustar anchos
            Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);

            // Encabezados
            for (int i = 0; i < colCount; i++)
            {
                TableCell cell = GridViewPagos.HeaderRow.Cells[i];
                PdfPCell pdfCell = new PdfPCell(new Phrase(cell.Text, fontHeader));
                pdfCell.BackgroundColor = new BaseColor(216, 38, 40);
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.Padding = 5;
                table.AddCell(pdfCell);
            }
            table.HeaderRows = 1;

            // Filas de datos
            Font fontData = FontFactory.GetFont(FontFactory.HELVETICA, 7);
            foreach (GridViewRow row in GridViewPagos.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    PdfPCell pdfCell = new PdfPCell(new Phrase(row.Cells[i].Text, fontData));
                    pdfCell.Padding = 3;
                    
                    // Alternar colores de fila
                    if (row.RowIndex % 2 == 0)
                        pdfCell.BackgroundColor = new BaseColor(248, 249, 250);
                    
                    // Alineación especial para diferentes columnas
                    if (i == 0) // Número de pago
                        pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    else if (i >= 2 && i <= 7) // Columnas de montos
                        pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    else if (i == 1) // Fecha
                        pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    
                    // Color especial para saldos
                    if (i == 7) // Saldo después del pago
                    {
                        string saldoText = row.Cells[i].Text.Replace("$", "").Replace(",", "");
                        decimal saldo;
                        if (decimal.TryParse(saldoText, out saldo))
                        {
                            if (saldo <= 0)
                                pdfCell.BackgroundColor = new BaseColor(200, 230, 201); // Verde claro
                            else
                                pdfCell.BackgroundColor = new BaseColor(255, 205, 210); // Rojo claro
                        }
                    }
                    
                    table.AddCell(pdfCell);
                }
            }

            pdfDoc.Add(table);

            pdfDoc.Add(new Paragraph(" "));

            // === SECCIÓN 5: ANÁLISIS Y OBSERVACIONES ===
            if (GridViewPagos.Rows.Count > 0)
            {
                Paragraph tituloAnalisis = new Paragraph("ANÁLISIS DE PAGOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.RED));
                tituloAnalisis.Alignment = Element.ALIGN_LEFT;
                pdfDoc.Add(tituloAnalisis);
                pdfDoc.Add(new Paragraph(" "));

                // Calcular algunos datos interesantes
                int totalPagos = GridViewPagos.Rows.Count;
                DateTime? primerPago = null, ultimoPago = null;
                
                if (GridViewPagos.Rows.Count > 0)
                {
                    string fechaPrimerPago = GridViewPagos.Rows[0].Cells[1].Text;
                    string fechaUltimoPago = GridViewPagos.Rows[GridViewPagos.Rows.Count - 1].Cells[1].Text;
                    
                    DateTime fp;
                    if (DateTime.TryParse(fechaPrimerPago, out fp))
                        primerPago = fp;
                    DateTime up;
                    if (DateTime.TryParse(fechaUltimoPago, out up))
                        ultimoPago = up;
                }

                string analisisTexto = "• Total de pagos realizados: " + totalPagos + "\n";
                if (primerPago.HasValue)
                    analisisTexto += "• Primer pago: " + primerPago.Value.ToString("dd/MM/yyyy") + "\n";
                if (ultimoPago.HasValue)
                    analisisTexto += "• Último pago: " + ultimoPago.Value.ToString("dd/MM/yyyy") + "\n";
                if (primerPago.HasValue && ultimoPago.HasValue)
                {
                    int diasTranscurridos = (ultimoPago.Value - primerPago.Value).Days;
                    analisisTexto += "• Período de pagos: " + diasTranscurridos + " días\n";
                }

                Paragraph parrafoAnalisis = new Paragraph(analisisTexto, FontFactory.GetFont(FontFactory.HELVETICA, 10));
                pdfDoc.Add(parrafoAnalisis);
            }

            // Pie de página con fecha y hora
            pdfDoc.Add(new Paragraph(" "));
            Paragraph piePagina = new Paragraph("Reporte generado el: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), 
                FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY));
            piePagina.Alignment = Element.ALIGN_RIGHT;
            pdfDoc.Add(piePagina);

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
        GridViewPagos.AllowPaging = false;
        CargarDetallePagos();

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=HistorialPagos_" + nombreBeneficiario.Replace(" ", "_") + ".xls");
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";
        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                // Escribe la información del beneficiario
                hw.Write("<table style='width:100%;'>");
                hw.Write("<tr>");
                hw.Write("<td colspan='10' style='text-align:center; font-size:16pt; font-weight:bold;'>Historial de Pagos - " + nombreBeneficiario + "</td>");
                hw.Write("</tr>");
                hw.Write("<tr>");
                hw.Write("<td style='text-align:center; font-weight:bold;'>Programa: " + programa + "</td>");
                hw.Write("<td colspan='3'></td>");
                hw.Write("<td style='text-align:center; font-weight:bold;'>Total de Pagos</td>");
                hw.Write("<td style='text-align:center; font-weight:bold;'>Monto Total Pagado</td>");
                hw.Write("<td style='text-align:center; font-weight:bold;'>Ultimo Pago</td>");
                hw.Write("<td style='text-align:center; font-weight:bold;'>Monto Asignado</td>");
                hw.Write("<td colspan='2'></td>");
                hw.Write("</tr>");
                hw.Write("<tr>");
                hw.Write("<td colspan='4'></td>");
                hw.Write("<td style='text-align:center; font-weight:bold; color:red;'>" + lblTotalPagos.Text + "</td>");
                hw.Write("<td style='text-align:center; font-weight:bold; color:red;'>" + lblMontoTotalPagado.Text + "</td>");
                hw.Write("<td style='text-align:center; font-weight:bold; color:red;'>" + lblUltimoPago.Text + "</td>");
                hw.Write("<td style='text-align:center; font-weight:bold; color:red;'>" + lblMontoAsignado.Text + "</td>");
                hw.Write("<td colspan='2'></td>");
                hw.Write("</tr>");
                hw.Write("</table>");
                hw.Write("<br /><br />");

                // Escribe el GridView
                GridViewPagos.RenderControl(hw);
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
