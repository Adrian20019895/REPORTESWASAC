using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

// Clase auxiliar para paginación en PDF
public class PdfPaginationHelper : PdfPageEventHelper
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
