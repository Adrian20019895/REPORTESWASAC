<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ConsultaSaldos.aspx.cs" Inherits="Cobranza_ConsultaSaldos" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Consulta de Saldos</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <style type="text/css">
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1000px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .header {
            background-color: #004890;
            color: white;
            padding: 15px;
            text-align: center;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .form-group {
            margin: 15px 0;
        }
        .form-group label {
            display: inline-block;
            width: 150px;
            font-weight: bold;
            color: #333;
        }
        .form-group input[type="text"] {
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            width: 200px;
        }
        .btn {
            background-color: #004890;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin: 5px;
        }
        .btn:hover {
            background-color: #003670;
        }
        .btn-clear {
            background-color: #6c757d;
        }
        .btn-clear:hover {
            background-color: #545b62;
        }
        .result-container {
            margin-top: 20px;
            padding: 15px;
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 5px;
        }
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin: 10px 0;
        }
        .info-table th, .info-table td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }
        .info-table th {
            background-color: #004890;
            color: white;
        }
        .info-table tr:nth-child(even) {
            background-color: #f2f2f2;
        }
        .error-message {
            color: red;
            font-weight: bold;
            margin: 10px 0;
        }
        .success-message {
            color: green;
            font-weight: bold;
            margin: 10px 0;
        }
        .section-title {
            background-color: #e9ecef;
            padding: 10px;
            margin: 15px 0 10px 0;
            border-left: 4px solid #004890;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>Consulta de Saldos de Beneficiario</h1>
                <p>Sistema de Cobranza WASAC</p>
            </div>
            
            <div class="form-group">
                <label for="txtBeneficiario">ID Beneficiario:</label>
                <asp:TextBox ID="txtBeneficiario" runat="server" CssClass="form-control"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvBeneficiario" runat="server" 
                    ControlToValidate="txtBeneficiario" 
                    ErrorMessage="El ID del beneficiario es requerido" 
                    ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>
            
            <div class="form-group">
                <label for="txtSubprograma">ID Subprograma:</label>
                <asp:TextBox ID="txtSubprograma" runat="server" CssClass="form-control"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvSubprograma" runat="server" 
                    ControlToValidate="txtSubprograma" 
                    ErrorMessage="El ID del subprograma es requerido" 
                    ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>
            
            <div class="form-group">
                <asp:Button ID="btnConsultar" runat="server" Text="Consultar Saldos" 
                    CssClass="btn" OnClick="btnConsultar_Click" />
                <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" 
                    CssClass="btn btn-clear" OnClick="btnLimpiar_Click" CausesValidation="false" />
            </div>
            
            <asp:Label ID="lblMensaje" runat="server" CssClass="error-message" Visible="false"></asp:Label>
            
            <asp:Panel ID="pnlResultados" runat="server" Visible="false" CssClass="result-container">
                
                <div class="section-title">Información del Beneficiario</div>
                <table class="info-table">
                    <tr>
                        <th>Campo</th>
                        <th>Valor</th>
                    </tr>
                    <tr>
                        <td><strong>Nombre Completo</strong></td>
                        <td><asp:Label ID="lblNombreCompleto" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Programa</strong></td>
                        <td><asp:Label ID="lblPrograma" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Subprograma</strong></td>
                        <td><asp:Label ID="lblSubprograma" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Número de Contrato</strong></td>
                        <td><asp:Label ID="lblContrato" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Fecha de Contratación</strong></td>
                        <td><asp:Label ID="lblFechaContratacion" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Monto Asignado</strong></td>
                        <td><asp:Label ID="lblMontoAsignado" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Duración (meses)</strong></td>
                        <td><asp:Label ID="lblDuracion" runat="server"></asp:Label></td>
                    </tr>
                </table>

                <div class="section-title">Estado de Mensualidades</div>
                <table class="info-table">
                    <tr>
                        <th>Concepto</th>
                        <th>Cantidad</th>
                    </tr>
                    <tr>
                        <td><strong>Mensualidades Atrasadas</strong></td>
                        <td><asp:Label ID="lblMenAtrasadas" runat="server" ForeColor="Red"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Mensualidades Pagadas</strong></td>
                        <td><asp:Label ID="lblMenPagadas" runat="server" ForeColor="Green"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Mensualidades Faltantes</strong></td>
                        <td><asp:Label ID="lblMenFaltantes" runat="server"></asp:Label></td>
                    </tr>
                </table>

                <div class="section-title">Cálculos Financieros</div>
                <table class="info-table">
                    <tr>
                        <th>Concepto</th>
                        <th>Monto</th>
                    </tr>
                    <tr>
                        <td><strong>Amortización (Capital)</strong></td>
                        <td><asp:Label ID="lblAmortizacion" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Interés Real</strong></td>
                        <td><asp:Label ID="lblInteresReal" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Interés Moratorio</strong></td>
                        <td><asp:Label ID="lblInteresMoratorio" runat="server" ForeColor="Red"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Liquidación Ordinaria</strong></td>
                        <td><asp:Label ID="lblLiquidacionOrd" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Liquidación Inmediata</strong></td>
                        <td><asp:Label ID="lblLiquidacionIme" runat="server" ForeColor="Blue"></asp:Label></td>
                    </tr>
                </table>

                <div class="section-title">Información Adicional</div>
                <table class="info-table">
                    <tr>
                        <th>Campo</th>
                        <th>Valor</th>
                    </tr>
                    <tr>
                        <td><strong>Último Pago</strong></td>
                        <td><asp:Label ID="lblUltimoPago" runat="server"></asp:Label></td>
                    </tr>
                    <tr>
                        <td><strong>Dirección</strong></td>
                        <td><asp:Label ID="lblDireccion" runat="server"></asp:Label></td>
                    </tr>
                </table>
                
            </asp:Panel>
        </div>
    </form>
</body>
</html>
