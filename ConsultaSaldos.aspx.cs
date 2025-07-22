using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

public partial class Cobranza_ConsultaSaldos : System.Web.UI.Page
{
    // Cadena de conexión - ajusta según tu configuración
    private string connectionString = "Server=localhost\\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Verificar si se recibieron parámetros de otro módulo
            if (Request.QueryString["IdBeneficiario"] != null && Request.QueryString["IdSubprograma"] != null)
            {
                // Auto-llenar los campos con los parámetros recibidos
                txtBeneficiario.Text = Request.QueryString["IdBeneficiario"];
                txtSubprograma.Text = Request.QueryString["IdSubprograma"];
                
                // Ejecutar automáticamente la consulta
                try
                {
                    int beneficiario = Convert.ToInt32(Request.QueryString["IdBeneficiario"]);
                    int subprograma = Convert.ToInt32(Request.QueryString["IdSubprograma"]);
                    ConsultarSaldos(beneficiario, subprograma);
                }
                catch (Exception ex)
                {
                    MostrarMensaje(string.Format("Error al cargar los datos: {0}", ex.Message), true);
                }
            }
            else
            {
                // Limpiar la página al cargar por primera vez sin parámetros
                LimpiarFormulario();
            }
        }
    }

    protected void btnConsultar_Click(object sender, EventArgs e)
    {
        try
        {
            // Validar que los campos sean numéricos
            int beneficiario, subprograma;
            
            if (!int.TryParse(txtBeneficiario.Text.Trim(), out beneficiario))
            {
                MostrarMensaje("El ID del beneficiario debe ser un número válido.", true);
                return;
            }
            
            if (!int.TryParse(txtSubprograma.Text.Trim(), out subprograma))
            {
                MostrarMensaje("El ID del subprograma debe ser un número válido.", true);
                return;
            }

            // Consultar los datos
            ConsultarSaldos(beneficiario, subprograma);
        }
        catch (Exception ex)
        {
            MostrarMensaje(string.Format("Error al consultar los datos: {0}", ex.Message), true);
        }
    }

    protected void btnLimpiar_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void ConsultarSaldos(int beneficiario, int subprograma)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                using (SqlCommand command = new SqlCommand("spSIVCnListaSaldoActBeneficiario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@pfiSubprograma", subprograma);
                    command.Parameters.AddWithValue("@pfiBeneficiario", beneficiario);
                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Llenar los controles con los datos obtenidos
                            LlenarDatos(reader);
                            pnlResultados.Visible = true;
                            MostrarMensaje("Consulta realizada exitosamente.", false);
                        }
                        else
                        {
                            pnlResultados.Visible = false;
                            MostrarMensaje("No se encontraron datos para el beneficiario y subprograma especificados.", true);
                        }
                    }
                }
            }
        }
        catch (SqlException sqlEx)
        {
            pnlResultados.Visible = false;
            MostrarMensaje(string.Format("Error de base de datos: {0}", sqlEx.Message), true);
        }
        catch (Exception ex)
        {
            pnlResultados.Visible = false;
            MostrarMensaje(string.Format("Error inesperado: {0}", ex.Message), true);
        }
    }

    private void LlenarDatos(SqlDataReader reader)
    {
        // Información del beneficiario
        lblNombreCompleto.Text = GetSafeString(reader, "fcNombre");
        lblPrograma.Text = GetSafeString(reader, "fcPrograma");
        lblSubprograma.Text = GetSafeString(reader, "fcSubprograma");
        lblContrato.Text = GetSafeString(reader, "fcContrato");
        
        // Formatear fecha de contratación
        DateTime fechaContratacion = GetSafeDateTime(reader, "fdFecContratacion");
        lblFechaContratacion.Text = fechaContratacion != DateTime.MinValue 
            ? fechaContratacion.ToString("dd/MM/yyyy") 
            : "No disponible";

        // Formatear monto asignado
        decimal montoAsignado = GetSafeDecimal(reader, "fiMontoAsignado");
        lblMontoAsignado.Text = montoAsignado.ToString("C");

        lblDuracion.Text = GetSafeInt(reader, "fiDuracion").ToString();

        // Estado de mensualidades
        int menAtrasadas = GetSafeInt(reader, "fiMenAtrasadas");
        int menPagadas = GetSafeInt(reader, "fiMenPagadas");
        int menFaltantes = GetSafeInt(reader, "fiMenFaltantes");

        lblMenAtrasadas.Text = menAtrasadas.ToString();
        lblMenPagadas.Text = menPagadas.ToString();
        lblMenFaltantes.Text = menFaltantes.ToString();

        // Cálculos financieros
        decimal amortizacion = GetSafeDecimal(reader, "fiAmortizacion");
        decimal interesReal = GetSafeDecimal(reader, "fiInteresReal");
        decimal interesMoratorio = GetSafeDecimal(reader, "fiInteresMoratorio");
        decimal liquidacionOrd = GetSafeDecimal(reader, "fiLiquidacionOrd");
        decimal liquidacionIme = GetSafeDecimal(reader, "fiLiquidacionIme");

        lblAmortizacion.Text = amortizacion.ToString("C");
        lblInteresReal.Text = interesReal.ToString("C");
        lblInteresMoratorio.Text = interesMoratorio.ToString("C");
        lblLiquidacionOrd.Text = liquidacionOrd.ToString("C");
        lblLiquidacionIme.Text = liquidacionIme.ToString("C");

        // Información adicional
        DateTime ultimoPago = GetSafeDateTime(reader, "fdUltimoPago");
        lblUltimoPago.Text = ultimoPago != DateTime.MinValue 
            ? ultimoPago.ToString("dd/MM/yyyy") 
            : "Sin pagos registrados";

        lblDireccion.Text = GetSafeString(reader, "fcDireccion");
    }

    private void LimpiarFormulario()
    {
        txtBeneficiario.Text = "";
        txtSubprograma.Text = "";
        pnlResultados.Visible = false;
        lblMensaje.Visible = false;
    }

    private void MostrarMensaje(string mensaje, bool esError)
    {
        lblMensaje.Text = mensaje;
        lblMensaje.CssClass = esError ? "error-message" : "success-message";
        lblMensaje.Visible = true;
    }

    // Métodos auxiliares para manejo seguro de datos del DataReader
    private string GetSafeString(SqlDataReader reader, string columnName)
    {
        try
        {
            return reader[columnName] != DBNull.Value ? reader[columnName].ToString() : "";
        }
        catch
        {
            return "";
        }
    }

    private int GetSafeInt(SqlDataReader reader, string columnName)
    {
        try
        {
            return reader[columnName] != DBNull.Value ? Convert.ToInt32(reader[columnName]) : 0;
        }
        catch
        {
            return 0;
        }
    }

    private decimal GetSafeDecimal(SqlDataReader reader, string columnName)
    {
        try
        {
            return reader[columnName] != DBNull.Value ? Convert.ToDecimal(reader[columnName]) : 0;
        }
        catch
        {
            return 0;
        }
    }

    private DateTime GetSafeDateTime(SqlDataReader reader, string columnName)
    {
        try
        {
            return reader[columnName] != DBNull.Value ? Convert.ToDateTime(reader[columnName]) : DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}
