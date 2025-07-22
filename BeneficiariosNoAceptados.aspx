<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BeneficiariosNoAceptados.aspx.cs" Inherits="BeneficiariosNoAceptados" ResponseEncoding="UTF-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Beneficiarios No Aceptados por Cobranza</title>
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
            border: 1px solid #ddd;
        }
        .gridview th, .gridview td {
            padding: 10px 12px;
            border: 1px solid #e0e6ed;
            text-align: center;
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
        /* Indicador de carga */
        #loadingMessage {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.8);
            z-index: 9999;
            text-align: center;
            padding-top: 20%;
        }
        .loading-content {
            display: inline-block;
            padding: 20px 40px;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }
        .spinner {
            border: 6px solid #f3f3f3;
            border-top: 6px solid #D82628;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 2s linear infinite;
            margin: 0 auto 15px auto;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
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
        /* Botón para Volver */
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
        /* Estilos para el DropDownList */
        .barra-seleccion {
            padding: 10px 16px;
            border-radius: 25px;
            border: 2px solid #D82628;
            font-size: 1.1em;
            background: #fff;
            color: #D82628;
            font-weight: bold;
            outline: none;
            margin-right: 16px;
            margin-bottom: 16px;
            box-shadow: 0 2px 8px rgba(216, 38, 40, 0.08);
            transition: border 0.3s, box-shadow 0.3s;
            appearance: none;
            -webkit-appearance: none;
            -moz-appearance: none;
            cursor: pointer;
            background-image: url('data:image/svg+xml;utf8,<svg fill="D82628" height="20" viewBox="0 0 24 24" width="20" xmlns="http://www.w3.org/2000/svg"><path d="M7 10l5 5 5-5z"/></svg>');
            background-repeat: no-repeat;
            background-position: right 12px center;
            background-size: 20px 20px;
            padding-right: 40px;
        }
        .barra-seleccion:focus {
            border: 2px solid #a81c1c;
            box-shadow: 0 0 0 2px #f7bcbc;
        }
        /* Barra superior con filtros y botones */
        .barra-superior {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 24px;
            flex-wrap: wrap;
        }
        .barra-superior .izquierda {
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .barra-superior .derecha {
            display: flex;
            align-items: center;
            gap: 12px;
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
        .info-resumen {
            background-color: #f7faff;
            border-radius: 12px;
            padding: 16px;
            margin-bottom: 24px;
            border-left: 5px solid #D82628;
            overflow: hidden;
        }
        .info-resumen h2 {
            margin-top: 0;
            color: #D82628;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 1px solid #e0e6ed;
        }
        .resumen-stats {
            display: flex;
            flex-wrap: wrap;
            gap: 16px;
            margin-top: 16px;
        }
        .stat-item {
            flex: 1;
            min-width: 200px;
            padding: 16px;
            border-radius: 8px;
            text-align: center;
            transition: transform 0.2s;
            background-color: #fff3cd;
            border-bottom: 3px solid #FFA726;
        }
        .stat-item:hover {
            transform: translateY(-5px);
        }
        .stat-item h3 {
            margin-top: 0;
            margin-bottom: 10px;
            color: #2d3e50;
            font-size: 0.9em;
        }
        .stat-item .valor {
            font-size: 1.4em;
            font-weight: bold;
            color: #FF8F00;
        }
        .alert {
            padding: 12px 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-weight: 500;
        }
        .alert-warning {
            background-color: #fff3cd;
            border: 1px solid #ffecb5;
            color: #856404;
        }
        .alert-info {
            background-color: #d1ecf1;
            border: 1px solid #bee5eb;
            color: #0c5460;
        }
    </style>
</head>
<body>
    <!-- Indicador de carga -->
    <div id="loadingMessage">
        <div class="loading-content">
            <div class="spinner"></div>
            <div>Cargando datos, por favor espere...</div>
            <div><small>Esta operación puede tardar unos momentos</small></div>
        </div>
    </div>

    <!-- Barra superior institucional con logo y botones de exportación -->
    <form id="form1" runat="server">
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo 1" class="logo-institucion" />
            <div class="barra-botones-exportar">
                <asp:Button ID="btnExportarPDF" runat="server" Text="Exportar a PDF" OnClick="btnExportarPDF_Click" CssClass="btn-pdf" />
                <asp:Button ID="btnExportarExcel" runat="server" Text="Exportar a Excel" OnClick="btnExportarExcel_Click" CssClass="btn-excel" />
                <asp:Button ID="btnVolver" runat="server" Text="Volver" OnClick="btnVolver_Click" CssClass="btn-volver" />
            </div>
        </div>
        <div class="container">
            <div class="info-resumen">
                <h2>Beneficiarios No Aceptados por Cobranza</h2>
                <div class="alert alert-warning">
                    <strong>Información:</strong> Esta página muestra todos los beneficiarios que NO tienen el estatus "Aceptado por Cobranza". 
                    Estos registros requieren revisión o están en proceso de validación.
                </div>
                <div class="resumen-stats">
                    <div class="stat-item">
                        <h3>Total de Beneficiarios</h3>
                        <div class="valor"><asp:Label ID="lblTotalBeneficiarios" runat="server" Text="0"></asp:Label></div>
                    </div>
                    <div class="stat-item">
                        <h3>Programas Afectados</h3>
                        <div class="valor"><asp:Label ID="lblTotalProgramas" runat="server" Text="0"></asp:Label></div>
                    </div>
                    <div class="stat-item">
                        <h3>Monto Total Asignado</h3>
                        <div class="valor"><asp:Label ID="lblMontoTotal" runat="server" Text="$0.00"></asp:Label></div>
                    </div>
                </div>
            </div>
            
            <h1>Listado de Beneficiarios No Aceptados</h1>

            <div class="table-responsive">
                <div class="barra-superior">
                    <div class="izquierda">
                        <asp:DropDownList ID="ddlEstatus" runat="server" CssClass="barra-seleccion">
                            <asp:ListItem Text="Todos los estatus (excepto Aceptado)" Value="" Selected="True"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:TextBox ID="txtBuscar" runat="server" placeholder="Buscar beneficiario..." CssClass="barra-seleccion"></asp:TextBox>
                        <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn-pdf" OnClick="btnBuscar_Click" />
                    </div>
                </div>
                <br />
                <asp:GridView ID="GridViewBeneficiarios" runat="server" AutoGenerateColumns="False" CssClass="gridview" OnRowDataBound="GridViewBeneficiarios_RowDataBound" DataKeyNames="fiBeneficiario,fiSubprograma">
                    <Columns>
                        <asp:BoundField DataField="fiBeneficiario" HeaderText="ID Beneficiario" />
                        <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre Completo" />
                        <asp:BoundField DataField="Programa" HeaderText="Programa" />
                        <asp:BoundField DataField="Subprograma" HeaderText="Subprograma" />
                        <asp:BoundField DataField="Estatus" HeaderText="Estatus" />
                        <asp:BoundField DataField="MontoAsignado" HeaderText="Monto Asignado" />
                        <asp:BoundField DataField="FechaRegistro" HeaderText="Fecha Registro" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>