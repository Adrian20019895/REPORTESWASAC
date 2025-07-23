using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Notificaciones : System.Web.UI.Page
{
    private string conexion = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "document.getElementById('loadingMessage').style.display = 'block';", true);
            CargarProgramasConAtrasos();
            ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "document.getElementById('loadingMessage').style.display = 'none';", true);
        }
    }

    private void CargarProgramasConAtrasos()
    {
        using (SqlConnection con = new SqlConnection(conexion))
        {
            string consulta = @"
                SELECT 
                    r.Programa,
                    COUNT(DISTINCT r.fiBeneficiario) AS TotalBeneficiarios,
                    CAST(AVG(CAST(r.fiMenAtrasadas AS FLOAT)) AS DECIMAL(10,2)) AS PromedioMensualidades,
                    MAX(r.fiMenAtrasadas) AS MaximoMensualidades,
                    SUM(r.MontoAsignado - r.AmortizacionPag) AS TotalMontoAtrasado
                FROM 
                    [SIAC].[dbo].[SIVvwResumenBeneficiario] r WITH (NOLOCK)
                WHERE 
                    r.Estatus = 'Aceptado por Cobranza'
                    AND r.fiMenAtrasadas > 0
                GROUP BY 
                    r.Programa
                ORDER BY 
                    AVG(CAST(r.fiMenAtrasadas AS FLOAT)) DESC, 
                    COUNT(DISTINCT r.fiBeneficiario) DESC";

            SqlCommand cmd = new SqlCommand(consulta, con);
            cmd.CommandTimeout = 180;
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            
            con.Open();
            da.Fill(dt);
            con.Close();

            GridViewProgramas.DataSource = dt;
            GridViewProgramas.DataBind();
        }
    }

    protected void GridViewProgramas_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Formatear el monto total atrasado
            if (e.Row.Cells.Count > 4)
            {
                decimal montoAtrasado;
                if (decimal.TryParse(e.Row.Cells[4].Text, out montoAtrasado))
                {
                    e.Row.Cells[4].Text = "$" + montoAtrasado.ToString("N2");
                    e.Row.Cells[4].ForeColor = System.Drawing.Color.FromArgb(198, 40, 40);
                    e.Row.Cells[4].Font.Bold = true;
                }
            }

            // Formatear contador de beneficiarios
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].CssClass += " contador-beneficiarios";
            }

            // Formatear promedio de mensualidades
            if (e.Row.Cells.Count > 2)
            {
                decimal promedio;
                if (decimal.TryParse(e.Row.Cells[2].Text, out promedio))
                {
                    if (promedio >= 3)
                    {
                        e.Row.Cells[2].CssClass += " mensualidades-promedio";
                        e.Row.Cells[2].Font.Bold = true;
                    }
                }
            }

            // Formatear máximo de mensualidades
            if (e.Row.Cells.Count > 3)
            {
                int maximo;
                if (int.TryParse(e.Row.Cells[3].Text, out maximo))
                {
                    if (maximo >= 5)
                    {
                        e.Row.Cells[3].BackColor = System.Drawing.Color.FromArgb(255, 205, 210);
                        e.Row.Cells[3].ForeColor = System.Drawing.Color.FromArgb(183, 28, 28);
                        e.Row.Cells[3].Font.Bold = true;
                    }
                    else if (maximo >= 3)
                    {
                        e.Row.Cells[3].BackColor = System.Drawing.Color.FromArgb(255, 243, 224);
                        e.Row.Cells[3].ForeColor = System.Drawing.Color.FromArgb(245, 124, 0);
                        e.Row.Cells[3].Font.Bold = true;
                    }
                }
            }
        }
    }

    protected void GridViewProgramas_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "VerDetalle")
        {
            string programa = e.CommandArgument.ToString();
            Response.Redirect("DetalleNotificaciones.aspx?Programa=" + Server.UrlEncode(programa));
        }
    }
}
