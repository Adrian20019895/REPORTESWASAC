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

// Clase auxiliar para generar PDFs con formato profesional
public class DetalleSaldosPDFExporter
{
    // Clase auxiliar para agregar números de página
    private class PaginationHelper : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfPTable footerTable = new PdfPTable(1);
            footerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            
            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);
            string pageText = string.Format("Página {0}", writer.PageNumber);
            
            PdfPCell cell = new PdfPCell(new Phrase(pageText, footerFont));
            cell.Border = Rectangle.NO_BORDER;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            footerTable.AddCell(cell);
            
            footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, writer.DirectContent);
        }
    }
    
    // Método para generar el PDF completo
    public static void GenerarPDFCompleto(GridView gridView, Label lblPrograma, Label lblTotalAsignado, 
        Label lblMontoPagadoCapital, Label lblMontoPagadoIntereses, Label lblTotalPagado, 
        Label lblSaldoPendienteCapital, Label lblSaldoPendienteInteresReal, Label lblSaldoPendienteInteresesMoratorios, 
        Label lblOtrosGastos, Label lblSaldoPendiente, Label lblTotalBeneficiarios, string programaSeleccionado, 
        HttpResponse Response, HttpServerUtility Server)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Crear documento en orientación horizontal (landscape) para mostrar más columnas
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 15f, 15f, 20f, 20f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);
            
            // Agregar paginación al pie de página
            writer.PageEvent = new PaginationHelper();
            
            pdfDoc.Open();
            
            // Definición de fuentes y colores para el documento
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(216, 38, 40));
            Font subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(44, 62, 80));
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
            Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
            Font smallBoldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);
            Font whiteFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(44, 62, 80));
            
            // Colores para las secciones
            BaseColor redColor = new BaseColor(216, 38, 40);        // Rojo CODESVI
            BaseColor darkRedColor = new BaseColor(183, 28, 28);    // Rojo oscuro
            BaseColor greenColor = new BaseColor(46, 125, 50);      // Verde
            BaseColor blueColor = new BaseColor(21, 101, 192);      // Azul
            BaseColor grayColor = new BaseColor(69, 90, 100);       // Gris oscuro
            BaseColor lightGrayColor = new BaseColor(245, 245, 245); // Gris claro
            BaseColor lightGreenColor = new BaseColor(220, 237, 200); // Verde claro
            BaseColor lightRedColor = new BaseColor(255, 205, 210);  // Rojo claro
            
            // === ENCABEZADO DEL DOCUMENTO ===
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 1, 3 });
            headerTable.SpacingAfter = 15f;
            
            // Logo de CODESVI
            string logoPath = Server.MapPath("/WASAC/img/logocodesvi.png");
            if (File.Exists(logoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleToFit(120f, 80f);
                
                PdfPCell cellLogo = new PdfPCell() { Border = 0, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                cellLogo.AddElement(logo);
                headerTable.AddCell(cellLogo);
            }
            else
            {
                PdfPCell cellLogo = new PdfPCell(new Phrase("CODESVI", boldFont)) { Border = 0, VerticalAlignment = Element.ALIGN_MIDDLE };
                headerTable.AddCell(cellLogo);
            }
            
            // Información del documento
            PdfPCell cellTitle = new PdfPCell() { Border = 0, VerticalAlignment = Element.ALIGN_MIDDLE };
            
            Paragraph title = new Paragraph("REPORTE COMPLETO DE SALDOS POR BENEFICIARIO", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            
            Paragraph subtitle = new Paragraph("Programa: " + lblPrograma.Text, subtitleFont);
            subtitle.Alignment = Element.ALIGN_CENTER;
            subtitle.SpacingBefore = 5f;
            
            Paragraph fechaGeneracion = new Paragraph("Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), normalFont);
            fechaGeneracion.Alignment = Element.ALIGN_CENTER;
            fechaGeneracion.SpacingBefore = 5f;
            
            cellTitle.AddElement(title);
            cellTitle.AddElement(subtitle);
            cellTitle.AddElement(fechaGeneracion);
            headerTable.AddCell(cellTitle);
            
            pdfDoc.Add(headerTable);
            
            // Agregar línea separadora
            Paragraph lineBreak = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, redColor, Element.ALIGN_CENTER, -1)));
            pdfDoc.Add(lineBreak);
            
            // === SECCIÓN 1: RESUMEN GENERAL ===
            Paragraph resumenTitle = new Paragraph("RESUMEN GENERAL DEL PROGRAMA", sectionFont);
            resumenTitle.Alignment = Element.ALIGN_CENTER;
            resumenTitle.SpacingBefore = 15f;
            resumenTitle.SpacingAfter = 10f;
            pdfDoc.Add(resumenTitle);
            
            // 1.1 Tabla de montos principales ampliada
            PdfPTable mainSummaryTable = new PdfPTable(7);
            mainSummaryTable.WidthPercentage = 100;
            mainSummaryTable.SpacingAfter = 15f;
            mainSummaryTable.SetWidths(new float[] { 1.2f, 1f, 1f, 1f, 1f, 1f, 1.2f });
            
            // Encabezados de la tabla principal
            PdfPCell[] mainHeaders = {
                new PdfPCell(new Phrase("TOTAL ASIGNADO", whiteFont)),
                new PdfPCell(new Phrase("PAGADO A CAPITAL", whiteFont)),
                new PdfPCell(new Phrase("PAGADO A INTERESES", whiteFont)),
                new PdfPCell(new Phrase("TOTAL PAGADO", whiteFont)),
                new PdfPCell(new Phrase("SALDO PENDIENTE CAPITAL", whiteFont)),
                new PdfPCell(new Phrase("SALDO PENDIENTE INTERÉS REAL", whiteFont)),
                new PdfPCell(new Phrase("SALDO PENDIENTE INTERESES MORATORIOS", whiteFont))
            };
            
            for (int i = 0; i < mainHeaders.Length; i++) 
            {
                PdfPCell cell = mainHeaders[i];
                cell.BackgroundColor = redColor;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.PaddingTop = 8f;
                cell.PaddingBottom = 8f;
                cell.MinimumHeight = 35f;
                mainSummaryTable.AddCell(cell);
            }
            
            // Valores del resumen principal
            PdfPCell[] mainValues = {
                new PdfPCell(new Phrase(lblTotalAsignado.Text, boldFont)),
                new PdfPCell(new Phrase(lblMontoPagadoCapital.Text, boldFont)),
                new PdfPCell(new Phrase(lblMontoPagadoIntereses.Text, boldFont)),
                new PdfPCell(new Phrase(lblTotalPagado.Text, boldFont)),
                new PdfPCell(new Phrase(lblSaldoPendienteCapital.Text, boldFont)),
                new PdfPCell(new Phrase(lblSaldoPendienteInteresReal.Text, boldFont)),
                new PdfPCell(new Phrase(lblSaldoPendienteInteresesMoratorios.Text, boldFont))
            };
            
            for (int i = 0; i < mainValues.Length; i++) 
            {
                PdfPCell cell = mainValues[i];
                
                if (i == 0) cell.BackgroundColor = lightGrayColor;
                else if (i >= 1 && i <= 3) cell.BackgroundColor = lightGreenColor;
                else cell.BackgroundColor = lightRedColor;
                
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.PaddingTop = 8f;
                cell.PaddingBottom = 8f;
                cell.MinimumHeight = 30f;
                mainSummaryTable.AddCell(cell);
            }
            
            pdfDoc.Add(mainSummaryTable);
            
            // 1.2 Información detallada de beneficiarios
            PdfPTable beneficiariosInfoTable = new PdfPTable(2);
            beneficiariosInfoTable.WidthPercentage = 100;
            beneficiariosInfoTable.SetWidths(new float[] { 1, 1 });
            beneficiariosInfoTable.SpacingAfter = 15f;
            
            // Conteo y cálculo de estadísticas
            int totalBeneficiarios = gridView.Rows.Count;
            int beneficiariosConSaldo = 0;
            int beneficiariosSinSaldo = 0;
            decimal totalSaldoPendiente = 0;
            decimal totalMontoPagado = 0;
            
            foreach (GridViewRow row in gridView.Rows)
            {
                if (row.Cells.Count > 10)
                {
                    // Saldo pendiente (en la columna 7 - Saldo Pendiente a Capital)
                    string saldoText = row.Cells[7].Text.Replace("$", "").Replace(",", "");
                    decimal saldo;
                    if (decimal.TryParse(saldoText, out saldo))
                    {
                        if (saldo > 0)
                            beneficiariosConSaldo++;
                        else
                            beneficiariosSinSaldo++;
                            
                        totalSaldoPendiente += saldo;
                    }
                    
                    // Total pagado (en la columna 6 - Total Pagado)
                    string pagadoText = row.Cells[6].Text.Replace("$", "").Replace(",", "");
                    decimal pagado;
                    if (decimal.TryParse(pagadoText, out pagado))
                    {
                        totalMontoPagado += pagado;
                    }
                }
            }
            
            // Porcentajes para análisis
            double porcentajeConSaldo = totalBeneficiarios > 0 ? Math.Round((double)beneficiariosConSaldo / totalBeneficiarios * 100, 1) : 0;
            double porcentajeSinSaldo = totalBeneficiarios > 0 ? Math.Round((double)beneficiariosSinSaldo / totalBeneficiarios * 100, 1) : 0;
            
            // Primera columna: Estadísticas de beneficiarios
            PdfPCell cellEstadisticas = new PdfPCell();
            cellEstadisticas.Border = Rectangle.BOX;
            cellEstadisticas.BorderColor = grayColor;
            cellEstadisticas.Padding = 10f;
            
            Paragraph estadisticasTitle = new Paragraph("ESTADÍSTICAS DE BENEFICIARIOS", smallBoldFont);
            estadisticasTitle.Alignment = Element.ALIGN_CENTER;
            estadisticasTitle.SpacingAfter = 10f;
            cellEstadisticas.AddElement(estadisticasTitle);
            
            PdfPTable estadisticasTable = new PdfPTable(2);
            estadisticasTable.WidthPercentage = 100;
            estadisticasTable.SetWidths(new float[] { 2, 1 });
            
            // Agregar filas de estadísticas con formato mejorado
            AddStatRow(estadisticasTable, "Total de beneficiarios:", totalBeneficiarios.ToString(), smallFont, smallBoldFont, lightGrayColor);
            AddStatRow(estadisticasTable, "Con saldo pendiente:", beneficiariosConSaldo.ToString() + " (" + porcentajeConSaldo + "%)", smallFont, smallBoldFont, lightRedColor);
            AddStatRow(estadisticasTable, "Sin saldo pendiente:", beneficiariosSinSaldo.ToString() + " (" + porcentajeSinSaldo + "%)", smallFont, smallBoldFont, lightGreenColor);
            
            cellEstadisticas.AddElement(estadisticasTable);
            beneficiariosInfoTable.AddCell(cellEstadisticas);
            
            // Segunda columna: Gráfico visual de distribución
            PdfPCell cellGrafico = new PdfPCell();
            cellGrafico.Border = Rectangle.BOX;
            cellGrafico.BorderColor = grayColor;
            cellGrafico.Padding = 10f;
            
            Paragraph graficoTitle = new Paragraph("DISTRIBUCIÓN DE SALDOS", smallBoldFont);
            graficoTitle.Alignment = Element.ALIGN_CENTER;
            graficoTitle.SpacingAfter = 10f;
            cellGrafico.AddElement(graficoTitle);
            
            // Crear simulación visual de distribución con bloques de color
            Paragraph pagadoCapitalItem = new Paragraph();
            pagadoCapitalItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, greenColor)));
            pagadoCapitalItem.Add(new Chunk("Capital: " + lblMontoPagadoCapital.Text, smallFont));
            pagadoCapitalItem.SpacingAfter = 5f;
            cellGrafico.AddElement(pagadoCapitalItem);
            
            Paragraph pagadoInteresesItem = new Paragraph();
            pagadoInteresesItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, blueColor)));
            pagadoInteresesItem.Add(new Chunk("Intereses: " + lblMontoPagadoIntereses.Text, smallFont));
            pagadoInteresesItem.SpacingAfter = 5f;
            cellGrafico.AddElement(pagadoInteresesItem);
            
            Paragraph otrosGastosItem = new Paragraph();
            otrosGastosItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, grayColor)));
            otrosGastosItem.Add(new Chunk("Otros gastos: " + lblOtrosGastos.Text, smallFont));
            otrosGastosItem.SpacingAfter = 5f;
            cellGrafico.AddElement(otrosGastosItem);
            
            Paragraph pendienteCapitalItem = new Paragraph();
            pendienteCapitalItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, redColor)));
            pendienteCapitalItem.Add(new Chunk("Pendiente Capital: " + lblSaldoPendienteCapital.Text, smallFont));
            pendienteCapitalItem.SpacingAfter = 5f;
            cellGrafico.AddElement(pendienteCapitalItem);
            
            Paragraph pendienteInteresRealItem = new Paragraph();
            pendienteInteresRealItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, darkRedColor)));
            pendienteInteresRealItem.Add(new Chunk("Pendiente Interés Real: " + lblSaldoPendienteInteresReal.Text, smallFont));
            pendienteInteresRealItem.SpacingAfter = 5f;
            cellGrafico.AddElement(pendienteInteresRealItem);
            
            Paragraph pendienteMoratoriosItem = new Paragraph();
            pendienteMoratoriosItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, new BaseColor(139, 0, 0))));
            pendienteMoratoriosItem.Add(new Chunk("Pendiente Moratorios: " + lblSaldoPendienteInteresesMoratorios.Text, smallFont));
            pendienteMoratoriosItem.SpacingAfter = 10f;
            cellGrafico.AddElement(pendienteMoratoriosItem);
            
            // Agregar leyenda de progreso general
            Paragraph progresoTitle = new Paragraph("Progreso general de recuperación:", smallBoldFont);
            progresoTitle.SpacingAfter = 5f;
            cellGrafico.AddElement(progresoTitle);
            
            // Calcular porcentaje de recuperación
            decimal montoAsignado = 0;
            decimal.TryParse(lblTotalAsignado.Text.Replace("$", "").Replace(",", ""), out montoAsignado);
            
            double porcentajeRecuperado = montoAsignado > 0 ? Math.Round((double)totalMontoPagado / (double)montoAsignado * 100, 1) : 0;
            
            // Crear barra de progreso visual
            PdfPTable progressTable = new PdfPTable(1);
            progressTable.WidthPercentage = 100;
            PdfPCell progressCell = new PdfPCell(new Phrase(porcentajeRecuperado + "% recuperado", smallFont));
            progressCell.BackgroundColor = greenColor;
            progressCell.BorderColor = grayColor;
            progressCell.Border = Rectangle.BOX;
            progressCell.HorizontalAlignment = Element.ALIGN_CENTER;
            progressCell.PaddingTop = 5f;
            progressCell.PaddingBottom = 5f;
            
            // Ajustar ancho según porcentaje (máximo 100%)
            if (porcentajeRecuperado < 100)
            {
                progressTable.WidthPercentage = (float)porcentajeRecuperado;
            }
            
            progressTable.AddCell(progressCell);
            cellGrafico.AddElement(progressTable);
            
            beneficiariosInfoTable.AddCell(cellGrafico);
            pdfDoc.Add(beneficiariosInfoTable);
            
            // === SECCIÓN 2: DETALLE DE TODOS LOS BENEFICIARIOS ===
            Paragraph detalleTitle = new Paragraph("DETALLE COMPLETO DE BENEFICIARIOS", sectionFont);
            detalleTitle.Alignment = Element.ALIGN_CENTER;
            detalleTitle.SpacingBefore = 10f;
            detalleTitle.SpacingAfter = 10f;
            pdfDoc.Add(detalleTitle);
            
            // Tabla de todos los beneficiarios con colores mejorados y formato optimizado
            if (gridView.Rows.Count > 0)
            {
                // 2.1 Agregar leyenda de colores
                PdfPTable leyendaTable = new PdfPTable(4);
                leyendaTable.WidthPercentage = 100;
                leyendaTable.SpacingAfter = 10f;
                
                PdfPCell leyendaCell1 = new PdfPCell(new Phrase("■ INFORMACIÓN GENERAL", new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, redColor)));
                PdfPCell leyendaCell2 = new PdfPCell(new Phrase("■ MONTOS ASIGNADOS", new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, grayColor)));
                PdfPCell leyendaCell3 = new PdfPCell(new Phrase("■ PAGOS REALIZADOS", new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, greenColor)));
                PdfPCell leyendaCell4 = new PdfPCell(new Phrase("■ SALDOS PENDIENTES", new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, darkRedColor)));
                
                leyendaCell1.Border = Rectangle.NO_BORDER;
                leyendaCell2.Border = Rectangle.NO_BORDER;
                leyendaCell3.Border = Rectangle.NO_BORDER;
                leyendaCell4.Border = Rectangle.NO_BORDER;
                
                leyendaTable.AddCell(leyendaCell1);
                leyendaTable.AddCell(leyendaCell2);
                leyendaTable.AddCell(leyendaCell3);
                leyendaTable.AddCell(leyendaCell4);
                
                pdfDoc.Add(leyendaTable);
                
                // 2.2 Tabla principal de beneficiarios
                PdfPTable dataTable = new PdfPTable(gridView.Columns.Count - 1); // -1 para excluir la columna de acciones
                dataTable.WidthPercentage = 100;
                dataTable.SetWidths(new float[] { 0.5f, 2f, 1.1f, 1.1f, 1.1f, 0.9f, 1.1f, 1.1f, 1.1f, 1.1f, 1.1f, 1f, 1f });
                
                // Definir colores para las secciones
                BaseColor headerRedColor = redColor;
                BaseColor headerGrayColor = grayColor;
                BaseColor headerGreenColor = greenColor;
                BaseColor headerDarkRedColor = darkRedColor;
                BaseColor headerBlueColor = blueColor;
                
                // Colores de fondo para las celdas de datos
                BaseColor dataRedLightColor = new BaseColor(255, 235, 238);
                BaseColor dataGrayLightColor = new BaseColor(245, 245, 245);
                BaseColor dataGreenLightColor = new BaseColor(232, 245, 233);
                BaseColor dataGreenMedColor = new BaseColor(200, 230, 201);
                BaseColor dataBlueLightColor = new BaseColor(227, 242, 253);
                
                // Encabezados de la tabla con colores por sección
                Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.WHITE);
                
                for (int i = 0; i < gridView.Columns.Count - 1; i++)
                {
                    string headerText = gridView.Columns[i].HeaderText;
                    
                    // Ajustes de nombres para mejor visualización
                    if (headerText == "Saldo pendiente Interes Real" || headerText == "Saldo Pendiente a Intereses")
                        headerText = "Saldo Pend. Interés Real";
                    if (headerText == "Saldo pendiente intereses moratorios")
                        headerText = "Saldo Pend. Int. Moratorios";
                    if (headerText == "Monto Pagado a Capital")
                        headerText = "Pagado a Capital";
                    if (headerText == "Monto Pagado a Intereses") 
                        headerText = "Pagado a Intereses";
                    if (headerText == "Saldo Pendiente a Capital")
                        headerText = "Saldo Pend. Capital";
                    if (headerText == "Saldo para Liquidar")
                        headerText = "Saldo Liquidación";
                    if (headerText == "Fecha de Contratación")
                        headerText = "Fecha Contrato";
                    if (headerText == "Fecha Último Pago")
                        headerText = "Último Pago";
                        
                    PdfPCell headerCell = new PdfPCell(new Phrase(headerText, tableHeaderFont));
                    
                    // Asignar colores según el tipo de columna
                    if (i <= 1) // ID y Beneficiario
                        headerCell.BackgroundColor = headerRedColor;
                    else if (i == 2) // Monto Asignado
                        headerCell.BackgroundColor = headerGrayColor;
                    else if (i >= 3 && i <= 6) // Montos Pagados
                        headerCell.BackgroundColor = headerGreenColor;
                    else if (i >= 7 && i <= 10) // Saldos
                        headerCell.BackgroundColor = headerDarkRedColor;
                    else // Fechas
                        headerCell.BackgroundColor = headerBlueColor;
                    
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    headerCell.Padding = 4f;
                    dataTable.AddCell(headerCell);
                }
                
                // Agregar datos de todos los beneficiarios
                Font tableDataFont = FontFactory.GetFont(FontFactory.HELVETICA, 6, BaseColor.BLACK);
                Font tableDataFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, BaseColor.BLACK);
                
                foreach (GridViewRow row in gridView.Rows)
                {
                    for (int i = 0; i < row.Cells.Count - 1; i++) // -1 para excluir la columna de acciones
                    {
                        // Determinar fuente y color de fondo según el tipo de dato
                        Font cellFont = tableDataFont;
                        BaseColor cellBackgroundColor = BaseColor.WHITE;
                        
                        if (i <= 1) // ID y Beneficiario
                            cellBackgroundColor = BaseColor.WHITE;
                        else if (i == 2) // Monto Asignado
                            cellBackgroundColor = dataGrayLightColor;
                        else if (i >= 3 && i <= 5) // Pagos individuales
                            cellBackgroundColor = dataGreenLightColor;
                        else if (i == 6) // Total Pagado
                        {
                            cellBackgroundColor = dataGreenMedColor;
                            cellFont = tableDataFontBold;
                        }
                        else if (i >= 7 && i <= 10) // Saldos
                            cellBackgroundColor = dataRedLightColor;
                        else // Fechas
                            cellBackgroundColor = dataBlueLightColor;
                        
                        // Alternar colores para filas pares/impares
                        if (row.RowIndex % 2 == 1)
                        {
                            cellBackgroundColor = new BaseColor(
                                Math.Max(0, cellBackgroundColor.R - 10),
                                Math.Max(0, cellBackgroundColor.G - 10),
                                Math.Max(0, cellBackgroundColor.B - 10)
                            );
                        }
                        
                        // Obtener y limpiar el texto de la celda
                        string cellText = HttpUtility.HtmlDecode(row.Cells[i].Text);
                        if (cellText == "&nbsp;")
                            cellText = "";
                        
                        PdfPCell dataCell = new PdfPCell(new Phrase(cellText, cellFont));
                        dataCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        dataCell.BackgroundColor = cellBackgroundColor;
                        dataCell.PaddingTop = 3f;
                        dataCell.PaddingBottom = 3f;
                        dataCell.MinimumHeight = 20f;
                        
                        dataTable.AddCell(dataCell);
                    }
                }
                
                // Agregar la tabla al documento
                pdfDoc.Add(dataTable);
                
                // Agregar nota explicativa
                Paragraph nota = new Paragraph("Nota: Los saldos pendientes incluyen el capital, intereses reales e intereses moratorios. " +
                    "El saldo para liquidar representa el monto total requerido para saldar la deuda del beneficiario.", 
                    FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC, BaseColor.GRAY));
                nota.Alignment = Element.ALIGN_LEFT;
                nota.SpacingBefore = 10f;
                pdfDoc.Add(nota);
            }
            else
            {
                // Mensaje cuando no hay beneficiarios
                Paragraph noData = new Paragraph("No hay beneficiarios registrados en este programa.", 
                    FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.ITALIC, BaseColor.GRAY));
                noData.Alignment = Element.ALIGN_CENTER;
                noData.SpacingBefore = 20f;
                noData.SpacingAfter = 20f;
                pdfDoc.Add(noData);
            }
            
            pdfDoc.Close();
            
            // Enviar el PDF al navegador
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=ReporteCompleto_" + 
                programaSeleccionado.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
            Response.BinaryWrite(ms.ToArray());
            Response.End();
        }
    }
    
    // Método auxiliar para agregar filas de estadísticas
    private static void AddStatRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont, BaseColor backgroundColor)
    {
        PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
        labelCell.Border = Rectangle.NO_BORDER;
        labelCell.PaddingBottom = 5f;
        table.AddCell(labelCell);
        
        PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
        valueCell.Border = Rectangle.BOX;
        valueCell.BorderColor = backgroundColor;
        valueCell.BackgroundColor = backgroundColor;
        valueCell.HorizontalAlignment = Element.ALIGN_CENTER;
        valueCell.PaddingTop = 5f;
        valueCell.PaddingBottom = 5f;
        table.AddCell(valueCell);
    }
}

public partial class DetalleSaldos : System.Web.UI.Page
{
    // Cadena de conexión a la base de datos
    private string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";
    private string programaSeleccionado = "";
    
    protected void Page_Load(object sender, EventArgs e)
    {
        // Recupera el programa de la QueryString siempre
        if (Request.QueryString["Programa"] != null)
        {
            programaSeleccionado = Request.QueryString["Programa"].ToString();
            lblPrograma.Text = programaSeleccionado;
        }
        
        if (!IsPostBack)
        {
            // Mostrar indicador de carga
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", 
                "document.getElementById('loadingMessage').style.display = 'block';", true);
            
            if (!string.IsNullOrEmpty(programaSeleccionado))
            {
                // Cargar municipios primero
                CargarMunicipios();
                
                // Carga los datos del programa y los beneficiarios
                CargarResumenPrograma();
                CargarDetalleBeneficiarios();
                
                // Ocultar indicador de carga cuando todo esté listo
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", 
                    "document.getElementById('loadingMessage').style.display = 'none';", true);
            }
            else
            {
                // Redirige a la página principal si no hay programa seleccionado
                Response.Redirect("VerDatos.aspx");
            }
        }
    }

    // Carga el resumen del programa (totales)
    private void CargarResumenPrograma()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"SELECT 
                                ISNULL(SUM(MontoAsignado), 0) AS TotalAsignado, 
                                ISNULL(SUM(AmortizacionPag), 0) AS MontoPagadoCapital,
                                ISNULL(SUM(InteresRealPag + InteresFijoPag + MoratorioPag - DescuentoPag), 0) AS MontoPagadoIntereses,
                                ISNULL(SUM(GastosAdmonPag + SeguroPag + OtroSeguroPag - SubsidioPag), 0) AS OtrosGastos,
                                ISNULL(SUM(TotalPag), 0) AS TotalPagado,
                                ISNULL(SUM(MontoAsignado) - SUM(AmortizacionPag), 0) AS SaldoPendienteCapital,
                                ISNULL(SUM(fiLiquidacionOrd) - SUM(fiInteresMoratorio) - (SUM(MontoAsignado) - SUM(AmortizacionPag)), 0) AS SaldoPendienteInteresReal,
                                ISNULL(SUM(fiInteresMoratorio), 0) AS SaldoPendienteInteresesMoratorios,
                                0 AS SaldoPendiente, -- Lo vamos a calcular por separado
                                ISNULL(COUNT(DISTINCT fiBeneficiario), 0) AS TotalBeneficiarios
                            FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] 
                            WHERE Programa = @Programa AND Estatus = 'Aceptado por Cobranza'";

            // Agregar filtro de municipio si está seleccionado
            if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
            {
                consulta += " AND LTRIM(RTRIM(MunicipioContrato)) = @Municipio";
            }

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@Programa", programaSeleccionado);
            
            if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
            {
                cmd.Parameters.AddWithValue("@Municipio", ddlMunicipio.SelectedValue.Trim());
            }
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                decimal totalAsignado = reader["TotalAsignado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAsignado"]) : 0;
                decimal montoPagadoCapital = reader["MontoPagadoCapital"] != DBNull.Value ? Convert.ToDecimal(reader["MontoPagadoCapital"]) : 0;
                decimal montoPagadoIntereses = reader["MontoPagadoIntereses"] != DBNull.Value ? Convert.ToDecimal(reader["MontoPagadoIntereses"]) : 0;
                decimal otrosGastos = reader["OtrosGastos"] != DBNull.Value ? Convert.ToDecimal(reader["OtrosGastos"]) : 0;
                decimal totalPagado = reader["TotalPagado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPagado"]) : 0;
                decimal saldoPendienteCapital = reader["SaldoPendienteCapital"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteCapital"]) : 0;
                decimal saldoPendienteInteresReal = reader["SaldoPendienteInteresReal"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteInteresReal"]) : 0;
                decimal saldoPendienteInteresesMoratorios = reader["SaldoPendienteInteresesMoratorios"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendienteInteresesMoratorios"]) : 0;
                decimal saldoPendiente = reader["SaldoPendiente"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoPendiente"]) : 0;
                int totalBeneficiarios = reader["TotalBeneficiarios"] != DBNull.Value ? Convert.ToInt32(reader["TotalBeneficiarios"]) : 0;

                // Asigna los valores a los labels
                lblTotalAsignado.Text = "$" + totalAsignado.ToString("N2");
                lblMontoPagadoCapital.Text = "$" + montoPagadoCapital.ToString("N2");
                lblMontoPagadoIntereses.Text = "$" + montoPagadoIntereses.ToString("N2");
                lblOtrosGastos.Text = "$" + otrosGastos.ToString("N2");
                lblTotalPagado.Text = "$" + totalPagado.ToString("N2");
                lblSaldoPendienteCapital.Text = "$" + saldoPendienteCapital.ToString("N2");
                lblSaldoPendienteInteresReal.Text = "$" + saldoPendienteInteresReal.ToString("N2");
                lblSaldoPendienteInteresesMoratorios.Text = "$" + saldoPendienteInteresesMoratorios.ToString("N2");
                // Calcular el total de saldo para liquidar sumando el saldo individual de cada beneficiario
                decimal totalSaldoParaLiquidar = CalcularTotalSaldoParaLiquidar();
                lblSaldoPendiente.Text = "$" + totalSaldoParaLiquidar.ToString("N2");
                lblTotalBeneficiarios.Text = totalBeneficiarios.ToString();
            }
            
            reader.Close();
        }
    }

    // Carga la lista de municipios disponibles en el programa
    private void CargarMunicipios()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"SELECT DISTINCT MunicipioContrato 
                               FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] 
                               WHERE Programa = @Programa AND Estatus = 'Aceptado por Cobranza' 
                               AND MunicipioContrato IS NOT NULL AND MunicipioContrato <> ''
                               ORDER BY MunicipioContrato";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.Parameters.AddWithValue("@Programa", programaSeleccionado);
            
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            ddlMunicipio.Items.Clear();
            ddlMunicipio.Items.Add(new System.Web.UI.WebControls.ListItem("-- Todos los Municipios --", ""));

            while (reader.Read())
            {
                string municipio = reader["MunicipioContrato"].ToString().Trim();
                if (!string.IsNullOrEmpty(municipio))
                {
                    ddlMunicipio.Items.Add(new System.Web.UI.WebControls.ListItem(municipio, municipio));
                }
            }

            reader.Close();
        }
    }

    // Carga el detalle de beneficiarios usando datos precalculados
    private void CargarDetalleBeneficiarios()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            con.Open();

            // Crear tabla temporal para almacenar saldos precalculados (solo SaldoPendienteInteresesMoratorios)
            SqlCommand cmdTemp = new SqlCommand(@"
                IF OBJECT_ID('tempdb..#TempSaldosPrograma') IS NOT NULL DROP TABLE #TempSaldosPrograma;
                CREATE TABLE #TempSaldosPrograma (
                    IdBeneficiario INT,
                    Beneficiario NVARCHAR(255),
                    MunicipioContrato NVARCHAR(255),
                    MontoAsignado DECIMAL(18,2),
                    MontoPagadoCapital DECIMAL(18,2),
                    MontoPagadoIntereses DECIMAL(18,2),
                    OtrosGastos DECIMAL(18,2),
                    TotalPagado DECIMAL(18,2),
                    SaldoPendienteCapital DECIMAL(18,2),
                    SaldoPendienteIntereses DECIMAL(18,2),
                    SaldoPendienteInteresesMoratorios DECIMAL(18,2),
                    IdSubprograma INT,
                    FechaUltimoPago VARCHAR(10),
                    SaldoParaLiquidar DECIMAL(18,2),
                    FecContratacion VARCHAR(10)
                )", con);
            cmdTemp.ExecuteNonQuery();

            // Insertar datos básicos usando la vista SIVvwResumenBeneficiario (agrega FecContratacion y SaldoPendienteInteresesMoratorios)
            string consultaBase = @"
                INSERT INTO #TempSaldosPrograma (IdBeneficiario, Beneficiario, MunicipioContrato, MontoAsignado, MontoPagadoCapital, 
                                                MontoPagadoIntereses, OtrosGastos, TotalPagado, SaldoPendienteCapital, SaldoPendienteIntereses, SaldoPendienteInteresesMoratorios, IdSubprograma, FechaUltimoPago, SaldoParaLiquidar, FecContratacion)
                SELECT 
                    ISNULL(r.fiBeneficiario, 0),
                    (ISNULL(r.Nombre, '') + ' ' + ISNULL(r.Apaterno, '') + ' ' + ISNULL(r.Amaterno, '')),
                    ISNULL(r.MunicipioContrato, ''),
                    ISNULL(r.MontoAsignado, 0),
                    ISNULL(r.AmortizacionPag, 0),
                    (ISNULL(r.InteresRealPag, 0) + ISNULL(r.InteresFijoPag, 0) + ISNULL(r.MoratorioPag, 0) - ISNULL(r.DescuentoPag, 0)),
                    (ISNULL(r.GastosAdmonPag, 0) + ISNULL(r.SeguroPag, 0) + ISNULL(r.OtroSeguroPag, 0) - ISNULL(r.SubsidioPag, 0)),
                    ISNULL(r.TotalPag, 0),
                    (ISNULL(r.MontoAsignado, 0) - ISNULL(r.AmortizacionPag, 0)),
                    (ISNULL(r.fiLiquidacionOrd, 0) - ISNULL(r.fiInteresMoratorio, 0) - (ISNULL(r.MontoAsignado, 0) - ISNULL(r.AmortizacionPag, 0))),
                    ISNULL(r.fiInteresMoratorio, 0),
                    ISNULL(r.fiSubprograma, 0),
                    (SELECT ISNULL(CONVERT(varchar, MAX(fecAlta), 103), 'N/A') 
                     FROM [SIAC].[dbo].[SIVaSolicitudCaja] 
                     WHERE fiBeneficiario = r.fiBeneficiario AND fiSubprograma = r.fiSubprograma AND fiTipoPago <> 3) AS FechaUltimoPago,
                    ISNULL(r.fiLiquidacionOrd, 0),
                    ISNULL(CONVERT(varchar, r.FecContratacion, 103), 'N/A')
                FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] r
                WHERE r.Programa = @Programa AND r.Estatus = 'Aceptado por Cobranza'";

            // Agregar filtro de municipio si está seleccionado
            if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
            {
                consultaBase += " AND LTRIM(RTRIM(r.MunicipioContrato)) = @Municipio";
            }

            if (!string.IsNullOrEmpty(txtBuscar.Text))
            {
                consultaBase += " AND (r.Nombre LIKE @Buscar OR r.Apaterno LIKE @Buscar OR r.Amaterno LIKE @Buscar)";
            }

            SqlCommand cmdBase = new SqlCommand(consultaBase, con);
            cmdBase.Parameters.AddWithValue("@Programa", programaSeleccionado);

            if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
            {
                cmdBase.Parameters.AddWithValue("@Municipio", ddlMunicipio.SelectedValue.Trim());
            }

            if (!string.IsNullOrEmpty(txtBuscar.Text))
            {
                cmdBase.Parameters.AddWithValue("@Buscar", "%" + txtBuscar.Text + "%");
            }

            cmdBase.ExecuteNonQuery();

            // Obtener los datos básicos que ya incluyen el saldo para liquidar y FecContratacion
            string consultaTemp = @"
                SELECT IdBeneficiario, Beneficiario, MunicipioContrato, MontoAsignado, MontoPagadoCapital, 
                       MontoPagadoIntereses, OtrosGastos, TotalPagado, SaldoPendienteCapital, 
                       SaldoPendienteIntereses, SaldoPendienteInteresesMoratorios, IdSubprograma, FechaUltimoPago, SaldoParaLiquidar, FecContratacion
                FROM #TempSaldosPrograma
                ORDER BY Beneficiario";

            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(consultaTemp, con))
            {
                da.Fill(dt);
            }

            // Limpiar tabla temporal
            SqlCommand cmdDrop = new SqlCommand("DROP TABLE #TempSaldosPrograma", con);
            cmdDrop.ExecuteNonQuery();

            GridViewDetalle.DataSource = dt;
            GridViewDetalle.DataBind();
        }
    }

    // Formato de celdas en GridView
    protected void GridViewDetalle_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Formatea las columnas de moneda (índices 3-11, ahora que agregamos Municipio en índice 2)
            for (int i = 3; i <= 11; i++)
            {
                if (i < e.Row.Cells.Count)
                {
                    decimal valor;
                    if (decimal.TryParse(e.Row.Cells[i].Text, out valor))
                    {
                        e.Row.Cells[i].Text = "$" + valor.ToString("N2");
                    }
                }
            }

            // === FORMATO PARA LA COLUMNA MUNICIPIO (ÍNDICE 2) ===
            if (e.Row.Cells.Count > 2)
            {
                e.Row.Cells[2].BackColor = System.Drawing.Color.FromArgb(230, 230, 250); // Lavanda claro
                e.Row.Cells[2].ForeColor = System.Drawing.Color.FromArgb(75, 0, 130); // Indigo
                e.Row.Cells[2].Font.Bold = true;
            }

            // === SECCIÓN DE MONTOS PAGADOS (FONDO VERDE CLARO) ===
            // Índice 3: Monto Asignado - Color neutro y negrita
            if (e.Row.Cells.Count > 3)
            {
                e.Row.Cells[3].BackColor = System.Drawing.Color.FromArgb(245, 245, 245); // Gris muy claro
                e.Row.Cells[3].ForeColor = System.Drawing.Color.FromArgb(33, 37, 41); // Gris oscuro
                e.Row.Cells[3].Font.Bold = true;
            }
            // Índice 4: Monto Pagado a Capital - Verde claro
            if (e.Row.Cells.Count > 4)
            {
                e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(200, 230, 201); // Verde claro
                e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(27, 94, 32); // Verde oscuro
            }
            // Índice 5: Monto Pagado a Intereses - Verde claro
            if (e.Row.Cells.Count > 5)
            {
                e.Row.Cells[5].BackColor = System.Drawing.Color.FromArgb(200, 230, 201); // Verde claro
                e.Row.Cells[5].ForeColor = System.Drawing.Color.FromArgb(27, 94, 32); // Verde oscuro
            }
            // Índice 6: Otros Gastos - Verde claro
            if (e.Row.Cells.Count > 6)
            {
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(200, 230, 201); // Verde claro
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(27, 94, 32); // Verde oscuro
            }
            // Índice 7: Total Pagado - Verde más intenso y negrita
            if (e.Row.Cells.Count > 7)
            {
                e.Row.Cells[7].BackColor = System.Drawing.Color.FromArgb(165, 214, 167); // Verde medio
                e.Row.Cells[7].ForeColor = System.Drawing.Color.FromArgb(27, 94, 32); // Verde oscuro
                e.Row.Cells[7].Font.Bold = true;
            }
            // === SECCIÓN DE SALDOS PENDIENTES (COLOR ROJO UNIFICADO) ===
            // Índice 8: Saldo Pendiente a Capital
            if (e.Row.Cells.Count > 8)
            {
                e.Row.Cells[8].BackColor = System.Drawing.Color.FromArgb(255, 205, 210); // Rojo claro
                e.Row.Cells[8].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28); // Rojo oscuro
            }
            // Índice 9: Total Saldo Pendiente - Rojo claro y negrita
            if (e.Row.Cells.Count > 9)
            {
                e.Row.Cells[9].BackColor = System.Drawing.Color.FromArgb(255, 205, 210); // Rojo claro
                e.Row.Cells[9].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28); // Rojo oscuro
                e.Row.Cells[9].Font.Bold = false;
            }
            // Índice 10: Saldo pendiente intereses moratorios (sin negrita)
            if (e.Row.Cells.Count > 10)
            {
                e.Row.Cells[10].BackColor = System.Drawing.Color.FromArgb(255, 205, 210); // Rojo claro
                e.Row.Cells[10].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28); // Rojo oscuro
                e.Row.Cells[10].Font.Bold = false;
            }
            // Índice 11: Saldo Para Liquidar - Rojo claro y negrita
            if (e.Row.Cells.Count > 11)
            {
                e.Row.Cells[11].BackColor = System.Drawing.Color.FromArgb(255, 205, 210); // Rojo claro
                e.Row.Cells[11].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28); // Rojo oscuro
                e.Row.Cells[11].Font.Bold = true;
            }
            // Índice 12: Fecha de Contratación - Azul claro
            if (e.Row.Cells.Count > 12)
            {
                e.Row.Cells[12].BackColor = System.Drawing.Color.FromArgb(227, 242, 253); // Azul muy claro
                e.Row.Cells[12].ForeColor = System.Drawing.Color.FromArgb(13, 71, 161); // Azul oscuro
                e.Row.Cells[12].Font.Bold = true;
            }
            // Índice 13: Fecha Último Pago - Azul claro
            if (e.Row.Cells.Count > 13)
            {
                e.Row.Cells[13].BackColor = System.Drawing.Color.FromArgb(227, 242, 253); // Azul muy claro
                e.Row.Cells[13].ForeColor = System.Drawing.Color.FromArgb(13, 71, 161); // Azul oscuro
                e.Row.Cells[13].Font.Bold = true;
            }
        }
        
        // === ESTILO PARA ENCABEZADOS ===
        else if (e.Row.RowType == DataControlRowType.Header)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                if (i >= 4 && i <= 7) // Columnas de "MONTO PAGADO" (ahora índices 4-7)
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(76, 175, 80); // Verde
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else if (i >= 8 && i <= 11) // Las cuatro columnas de saldos (ahora índices 8-11)
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(244, 67, 54); // Rojo fuerte
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else if (i == 2) // Municipio
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(156, 39, 176); // Púrpura
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else if (i == 3) // Monto Asignado
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(96, 125, 139); // Gris azulado
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else if (i == 12) // Fecha de Contratación
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(63, 81, 181); // Azul índigo
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else if (i == 13) // Fecha Último Pago
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(63, 81, 181); // Azul índigo
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
                else
                {
                    e.Row.Cells[i].BackColor = System.Drawing.Color.FromArgb(216, 38, 40); // Rojo original
                    e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
                    e.Row.Cells[i].Font.Bold = true;
                }
            }
        }
    }

    // Comando para ver el detalle de pagos de un beneficiario
    protected void GridViewDetalle_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "VerDetalleBeneficiario")
        {
            // Obtener el índice de la fila seleccionada
            int index = Convert.ToInt32(e.CommandArgument);
            
            // Obtener el valor del IdBeneficiario y IdSubprograma de la fila
            string idBeneficiario = GridViewDetalle.DataKeys[index]["IdBeneficiario"].ToString();
            string idSubprograma = GridViewDetalle.DataKeys[index]["IdSubprograma"].ToString();
            
            string programa = Request.QueryString["Programa"];
            Response.Redirect(string.Format("DetallePagosBeneficiario.aspx?IdBeneficiario={0}&IdSubprograma={1}&Programa={2}", idBeneficiario, idSubprograma, programa));
        }
    }

    // Botón para volver a la página anterior
    protected void btnVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("VerDatos.aspx");
    }
    
    // Evento para el cambio de selección del municipio
    protected void ddlMunicipio_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Recargar resumen y detalle con el filtro de municipio
        CargarResumenPrograma();
        CargarDetalleBeneficiarios();
    }
    
    // Botón para buscar beneficiarios
    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        // Mostrar indicador de carga
        ScriptManager.RegisterStartupScript(this, GetType(), "showLoadingSearch", 
            "document.getElementById('loadingMessage').style.display = 'block';", true);
        
        CargarDetalleBeneficiarios();
        
        // Ocultar indicador de carga cuando termina
        ScriptManager.RegisterStartupScript(this, GetType(), "hideLoadingSearch", 
            "document.getElementById('loadingMessage').style.display = 'none';", true);
    }

    // Exporta los datos a PDF con formato mejorado usando la clase DetalleSaldosPDFExporter
    protected void btnExportarPDF_Click(object sender, EventArgs e)
    {
        try
        {
            // Asegura que el GridView tenga los datos más recientes
            GridViewDetalle.AllowPaging = false;
            CargarDetalleBeneficiarios();
            string programaSeleccionado = Request.QueryString["Programa"] != null ? Request.QueryString["Programa"].ToString() : "Todos";

            // Llama al nuevo método para generar el PDF profesional
            DetalleSaldosPDFExporter.GenerarPDFCompleto(
                GridViewDetalle,
                lblPrograma,
                lblTotalAsignado,
                lblMontoPagadoCapital,
                lblMontoPagadoIntereses,
                lblTotalPagado,
                lblSaldoPendienteCapital,
                lblSaldoPendienteInteresReal,
                lblSaldoPendienteInteresesMoratorios,
                lblOtrosGastos,
                lblSaldoPendiente,
                lblTotalBeneficiarios,
                programaSeleccionado,
                Response,
                Server
            );
        }
        catch (Exception ex)
        {
            // Muestra un mensaje de error amigable
            string script = string.Format("alert('Ocurrió un error al generar el PDF: {0}');", ex.Message.Replace("'", "\\'"));
            ScriptManager.RegisterStartupScript(this, GetType(), "ErrorPDF", script, true);
        }
    }

    // Exporta los datos a Excel
    protected void btnExportarExcel_Click(object sender, EventArgs e)
    {
        GridViewDetalle.AllowPaging = false;
        CargarDetalleBeneficiarios();

        // Oculta la columna de acciones antes de exportar (ahora es la columna 14)
        GridViewDetalle.Columns[14].Visible = false;

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=DetalleSaldos_" + programaSeleccionado.Replace(" ", "_") + ".xls");
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                // Escribir encabezado HTML para Excel
                hw.Write("<html><head><meta charset='UTF-8'></head><body>");
                hw.Write("<table border='1' cellpadding='3' cellspacing='0'>");
                
                // Escribir encabezados
                hw.Write("<tr>");
                for (int i = 0; i < GridViewDetalle.Columns.Count; i++)
                {
                    if (GridViewDetalle.Columns[i].Visible)
                    {
                        string headerText = GridViewDetalle.Columns[i].HeaderText;
                        // Cambiar el nombre solo para el Excel si es la columna de intereses
                        if (headerText == "Saldo pendiente Interes Real" || headerText == "Saldo Pendiente a Intereses")
                            headerText = "Saldo pendiente Interes Real";
                        if (headerText == "Saldo pendiente intereses moratorios")
                            headerText = "Saldo pendiente intereses moratorios";
                        hw.Write("<th style='background-color: #D82628; color: white; font-weight: bold;'>");
                        hw.Write(headerText);
                        hw.Write("</th>");
                    }
                }
                hw.Write("</tr>");
                
                // Escribir datos - verificar que hay filas
                if (GridViewDetalle.Rows.Count > 0)
                {
                    foreach (GridViewRow row in GridViewDetalle.Rows)
                    {
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            hw.Write("<tr>");
                            for (int i = 0; i < row.Cells.Count; i++)
                            {
                                if (i < GridViewDetalle.Columns.Count && GridViewDetalle.Columns[i].Visible)
                                {
                                    hw.Write("<td>");
                                    // Limpiar el texto para Excel
                                    string cellText = row.Cells[i].Text;
                                    if (cellText == "&nbsp;")
                                        cellText = "";
                                    hw.Write(cellText);
                                    hw.Write("</td>");
                                }
                            }
                            hw.Write("</tr>");
                        }
                    }
                }
                else
                {
                    // Si no hay datos, mostrar mensaje
                    hw.Write("<tr><td colspan='" + GridViewDetalle.Columns.Count + "'>No hay datos para mostrar</td></tr>");
                }
                
                hw.Write("</table></body></html>");
                
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

        // Vuelve a mostrar la columna después de exportar
        GridViewDetalle.Columns[14].Visible = true;
    }

    // Requerido por ASP.NET para exportar controles de servidor
    public override void VerifyRenderingInServerForm(Control control)
    {
        // Método vacío para evitar errores de renderizado al exportar
    }

    // Calcula el saldo para liquidar para un beneficiario específico
    private decimal CalcularSaldoParaLiquidar(int idBeneficiario, int idSubprograma)
    {
        decimal saldoLiquidar = 0;
        
        try
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string consulta = @"
                    SELECT ISNULL(fiLiquidacionOrd, 0) AS SaldoLiquidar
                    FROM [SIAC].[dbo].[SIVvwResumenBeneficiario]
                    WHERE fiBeneficiario = @IdBeneficiario AND fiSubprograma = @IdSubprograma AND Estatus = 'Aceptado por Cobranza'";
                
                SqlCommand cmd = new SqlCommand(consulta, con);
                cmd.Parameters.AddWithValue("@IdBeneficiario", idBeneficiario);
                cmd.Parameters.AddWithValue("@IdSubprograma", idSubprograma);
                
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    saldoLiquidar = Convert.ToDecimal(result);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error pero continúa con el valor predeterminado
            System.Diagnostics.Debug.WriteLine(string.Format("Error al calcular saldo para liquidar: {0}", ex.Message));
        }
        
        return saldoLiquidar;
    }

    // Calcula el total de saldo para liquidar de todos los beneficiarios del programa actual
    private decimal CalcularTotalSaldoParaLiquidar()
    {
        decimal totalSaldoLiquidar = 0;
        
        try
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                
                // Consulta directa a la vista SIVvwResumenBeneficiario
                string consultaTotal = @"
                    SELECT ISNULL(SUM(ISNULL(r.fiLiquidacionOrd, 0)), 0) AS TotalSaldoLiquidar
                    FROM [SIAC].[dbo].[SIVvwResumenBeneficiario] r
                    WHERE r.Programa = @Programa AND r.Estatus = 'Aceptado por Cobranza'";
                
                // Agregar filtro de municipio si está seleccionado
                if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
                {
                    consultaTotal += " AND LTRIM(RTRIM(r.MunicipioContrato)) = @Municipio";
                }
                
                SqlCommand cmd = new SqlCommand(consultaTotal, con);
                cmd.Parameters.AddWithValue("@Programa", programaSeleccionado);
                
                if (ddlMunicipio != null && !string.IsNullOrEmpty(ddlMunicipio.SelectedValue))
                {
                    cmd.Parameters.AddWithValue("@Municipio", ddlMunicipio.SelectedValue.Trim());
                }
                
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalSaldoLiquidar = Convert.ToDecimal(result);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error pero continúa
            System.Diagnostics.Debug.WriteLine(string.Format("Error al calcular total de saldo para liquidar: {0}", ex.Message));
        }
        
        return totalSaldoLiquidar;
    }
}
