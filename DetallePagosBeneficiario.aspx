<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DetallePagosBeneficiario.aspx.cs" Inherits="DetallePagosBeneficiario" ResponseEncoding="UTF-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Detalle de Pagos del Beneficiario</title>
    <style>
        /* Estilos generales para la página */
        body {
            background: #f4f6fb;
            font-family: 'Segoe UI', Arial, sans-serif;
            margin: 0;
            padding: 0;
        }
        h1 {
            text-align: center;
            color: #2d3e50;
            margin-bottom: 32px;
        }
        .table-responsive {
            width: 100%;
            overflow-x: auto;
        }
        /* Estilos para el GridView */
        .gridview {
            width: 100%;
            min-width: 900px;
            border-collapse: collapse;
        }
        .gridview th, .gridview td {
            padding: 10px 12px;
            border-bottom: 1px solid #e0e6ed;
        }
        .gridview th {
            background: #D82628;
            color: #fff;
            font-weight: 600;
        }
        .gridview tr:nth-child(even) {
            background: #f7faff;
        }
        .gridview tr:hover {
            background: #e6f0ff;
        }
        /* Botón para PDF */
        .btn-pdf {
            background: linear-gradient(90deg, #ff5858 0%, #D82628 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(255, 88, 88, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
        }
        .btn-pdf:hover {
            background: linear-gradient(90deg, #D82628 100%, #ff5858 0%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(255, 88, 88, 0.25);
        }
        /* Botón para Excel */
        .btn-excel {
            background: linear-gradient(90deg, #43a047 0%, #388e3c 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(67, 160, 71, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
        }
        .btn-excel:hover {
            background: linear-gradient(90deg, #388e3c 0%, #43a047 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(67, 160, 71, 0.25);
        }
        /* Botón para Consulta Saldos */
        .btn-consulta {
            background: linear-gradient(90deg, #007bff 0%, #0056b3 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0, 123, 255, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
        }
        .btn-consulta:hover {
            background: linear-gradient(90deg, #0056b3 0%, #007bff 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(0, 123, 255, 0.25);
        }
        .barra-institucional {
            width: 100%;
            background: #BB4349;
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 8px 32px;
            box-sizing: border-box;
            min-height: 60px;
            position: fixed;
            top: 0;
            left: 0;
            z-index: 1000;
        }
        .container {
            max-width: 95vw;
            margin: 40px auto;
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 4px 24px rgba(0,0,0,0.08);
            padding: 32px 24px;
            margin-top: 170px;
        }
        .logo-institucion {
            height: 138px;  
            border-radius: 8px;
            padding: 4px 8px;
            margin: 0 8px;
        }
        .barra-botones-exportar {
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .info-beneficiario {
            background-color: #f7faff;
            border-radius: 8px;
            padding: 16px;
            margin-bottom: 24px;
            border-left: 5px solid #D82628;
        }
        .info-beneficiario h2 {
            margin-top: 0;
            color: #D82628;
        }
        .resumen-pagos {
            display: flex;
            gap: 16px;
            margin-top: 16px;
            flex-wrap: wrap;
        }
        .resumen-item {
            flex: 1;
            padding: 16px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
            text-align: center;
        }
        .resumen-item h3 {
            margin-top: 0;
            color: #2d3e50;
            font-size: 1em;
        }
        .resumen-item .valor {
            font-size: 1.6em;
            font-weight: bold;
            color: #D82628;
        }
        .btn-volver {
            background: linear-gradient(90deg, #2d3e50 0%, #34495e 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(45, 62, 80, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
        }
        .btn-volver:hover {
            background: linear-gradient(90deg, #34495e 0%, #2d3e50 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(45, 62, 80, 0.25);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo 1" class="logo-institucion" />
            <div class="barra-botones-exportar">
                <asp:Button ID="btnExportarPDF" runat="server" Text="Exportar a PDF" OnClick="btnExportarPDF_Click" CssClass="btn-pdf" />
                <asp:Button ID="btnExportarExcel" runat="server" Text="Exportar a Excel" OnClick="btnExportarExcel_Click" CssClass="btn-excel" />
                <asp:Button ID="btnConsultaSaldos" runat="server" Text="Consulta Saldos" OnClick="btnConsultaSaldos_Click" CssClass="btn-consulta" />
                <asp:Button ID="btnVolver" runat="server" Text="Volver" OnClick="btnVolver_Click" CssClass="btn-volver" />
            </div>
        </div>
        <div class="container">
            <div class="info-beneficiario">
                <h2>Historial de Pagos - <asp:Label ID="lblNombreBeneficiario" runat="server"></asp:Label></h2>
                <p><strong>Programa:</strong> <asp:Label ID="lblPrograma" runat="server"></asp:Label></p>
                <div class="resumen-pagos">
                    <div class="resumen-item">
                        <h3>Total de Pagos</h3>
                        <div class="valor"><asp:Label ID="lblTotalPagos" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Monto Total Pagado</h3>
                        <div class="valor"><asp:Label ID="lblMontoTotalPagado" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Ultimo Pago</h3>
                        <div class="valor"><asp:Label ID="lblUltimoPago" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Monto Asignado</h3>
                        <div class="valor"><asp:Label ID="lblMontoAsignado" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Saldo para Liquidar</h3>
                        <div class="valor"><asp:Label ID="lblSaldoLiquidar" runat="server"></asp:Label></div>
                    </div>
                </div>
            </div>
            
            <h1>Detalle de Pagos Realizados</h1>

            <div class="table-responsive">
                <asp:GridView ID="GridViewPagos" runat="server" AutoGenerateColumns="False" CssClass="gridview" OnRowDataBound="GridViewPagos_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="fiPago" HeaderText="Numero de Pago" />
                        <asp:BoundField DataField="fcFecha" HeaderText="Fecha de Pago" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="fiMonto" HeaderText="Monto Pagado" />
                        <asp:BoundField DataField="fiAmortizacion" HeaderText="Capital" />
                        <asp:BoundField DataField="fiInteresReal" HeaderText="Interes Real" />
                        <asp:BoundField DataField="fiMoratorio" HeaderText="Moratorio" />
                        <asp:BoundField DataField="fiDescuento" HeaderText="Descuento" />
                        <asp:BoundField DataField="fiSaldo" HeaderText="Saldo Despues del Pago" />
                        <asp:BoundField DataField="fcCobratorio" HeaderText="Cobrador" />
                        <asp:BoundField DataField="fcObservaciones" HeaderText="Observaciones" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
