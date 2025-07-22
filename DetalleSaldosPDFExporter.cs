using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

public partial class DetalleSaldosPDFExporter
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
        Label lblSaldoPendienteCapital, Label lblOtrosGastos, Label lblSaldoPendiente, 
        Label lblTotalBeneficiarios, string programaSeleccionado, HttpResponse Response, HttpServerUtility Server)
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
            
            // 1.1 Tabla de montos principales
            PdfPTable mainSummaryTable = new PdfPTable(5);
            mainSummaryTable.WidthPercentage = 100;
            mainSummaryTable.SpacingAfter = 15f;
            
            // Encabezados de la tabla principal
            PdfPCell[] mainHeaders = {
                new PdfPCell(new Phrase("TOTAL ASIGNADO", whiteFont)),
                new PdfPCell(new Phrase("PAGADO A CAPITAL", whiteFont)),
                new PdfPCell(new Phrase("PAGADO A INTERESES", whiteFont)),
                new PdfPCell(new Phrase("TOTAL PAGADO", whiteFont)),
                new PdfPCell(new Phrase("SALDO PENDIENTE", whiteFont))
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
                new PdfPCell(new Phrase(lblSaldoPendienteCapital.Text, boldFont))
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
            
            Paragraph graficoTitle = new Paragraph("DISTRIBUCIÓN DE PAGOS", smallBoldFont);
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
            
            Paragraph pendienteItem = new Paragraph();
            pendienteItem.Add(new Chunk("■ ", new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL, redColor)));
            pendienteItem.Add(new Chunk("Pendiente: " + lblSaldoPendienteCapital.Text, smallFont));
            pendienteItem.SpacingAfter = 10f;
            cellGrafico.AddElement(pendienteItem);
            
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
                    FontFactory.GetFont(FontFactory.HELVETICA_ITALIC, 8, BaseColor.GRAY));
                nota.Alignment = Element.ALIGN_LEFT;
                nota.SpacingBefore = 10f;
                pdfDoc.Add(nota);
            }
            else
            {
                // Mensaje cuando no hay beneficiarios
                Paragraph noData = new Paragraph("No hay beneficiarios registrados en este programa.", 
                    FontFactory.GetFont(FontFactory.HELVETICA_ITALIC, 10, BaseColor.GRAY));
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
