<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CobranzaDiaria.aspx.cs" Inherits="CobranzaDiaria" ResponseEncoding="UTF-8" Debug="true" EnableEventValidation="false" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>COBRANZA DIARIA - REPORTES CODESVI</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            min-height: 100vh;
        }
        h1 {
            color: #D82628;
            text-align: center;
            margin-bottom: 20px;
            font-size: 2em;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .table-responsive {
            overflow-x: auto;
        }
        /* Estilos para el GridView */
        .gridview {
            width: 100%;
            border-collapse: collapse;
            background: #fff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 24px rgba(0,0,0,0.08);
        }
        .gridview th, .gridview td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #e0e6ed;
            font-size: 0.9em;
        }
        .gridview th {
            background: linear-gradient(90deg, #D82628 0%, #a81c1c 100%);
            color: white;
            font-weight: bold;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        .gridview tr:nth-child(even) {
            background-color: #f7faff;
        }
        .gridview tr:hover {
            background-color: #e3f2fd;
            transform: scale(1.01);
            transition: all 0.2s ease;
        }
        /* Indicador de carga */
        #loadingMessage {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.95);
            display: none;
            z-index: 10000;
            justify-content: center;
            align-items: center;
        }
        .loading-content {
            text-align: center;
            color: #D82628;
            font-size: 1.2em;
        }
        .spinner {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #D82628;
            border-radius: 50%;
            width: 50px;
            height: 50px;
            animation: spin 1s linear infinite;
            margin: 0 auto 20px;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        /* Botones */
        .btn {
            background: linear-gradient(90deg, #D82628 0%, #a81c1c 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(216, 38, 40, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
        }
        .btn:hover {
            background: linear-gradient(90deg, #a81c1c 0%, #D82628 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(216, 38, 40, 0.25);
        }
        .btn-excel {
            background: linear-gradient(90deg, #43a047 0%, #388e3c 100%);
        }
        .btn-excel:hover {
            background: linear-gradient(90deg, #388e3c 0%, #43a047 100%);
        }
        .btn-back {
            background: linear-gradient(90deg, #607D8B 0%, #455A64 100%);
        }
        .btn-back:hover {
            background: linear-gradient(90deg, #455A64 0%, #607D8B 100%);
        }
        /* Estilos para controles de filtro */
        .filtros {
            background: #fff;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 4px 24px rgba(0,0,0,0.08);
        }
        .filtros-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: end;
            margin-bottom: 20px;
        }
        .filtro-group {
            display: flex;
            flex-direction: column;
            min-width: 200px;
        }
        .filtro-label {
            font-weight: bold;
            color: #2d3e50;
            margin-bottom: 5px;
            font-size: 0.9em;
        }
        .filtro-control {
            padding: 10px 16px;
            border-radius: 8px;
            border: 2px solid #e0e6ed;
            font-size: 1em;
            background: #fff;
            color: #2d3e50;
            outline: none;
            transition: border 0.3s, box-shadow 0.3s;
        }
        .filtro-control:focus {
            border: 2px solid #D82628;
            box-shadow: 0 0 0 2px rgba(216, 38, 40, 0.1);
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
            max-width: 98vw;
            margin: 40px auto;
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 4px 24px rgba(0,0,0,0.08);
            padding: 32px 24px;
            margin-top: 100px;
        }
        .logo-institucion {
            height: 50px;
            border-radius: 8px;
            padding: 4px 8px;
            margin: 0 8px;
        }
        .nombre-institucion {
            color: #fff;
            font-size: 1.3em;
            font-weight: bold;
            letter-spacing: 1px;
            text-align: center;
            flex: 1;
        }
        .barra-botones {
            display: flex;
            align-items: center;
            gap: 12px;
        }
        /* Resumen de totales */
        .resumen-totales {
            background: #f7faff;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 20px;
            border-left: 5px solid #D82628;
        }
        .resumen-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
        }
        .resumen-item {
            background: #fff;
            padding: 15px;
            border-radius: 8px;
            text-align: center;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }
        .resumen-item h3 {
            margin: 0 0 10px 0;
            color: #2d3e50;
            font-size: 0.9em;
        }
        .resumen-item .valor {
            font-size: 1.3em;
            font-weight: bold;
            color: #D82628;
        }
        .fecha-column {
            background-color: #f8f9fa !important;
            font-weight: bold;
            color: #2d3e50;
        }
        .monto-column {
            text-align: right;
            font-weight: bold;
        }
        .sucursal-column {
            color: #007bff;
            font-weight: 500;
        }
        .beneficiario-column {
            color: #28a745;
            font-weight: 500;
        }
        
        /* Estilos para registros inactivos */
        .registro-inactivo {
            background-color: #d32f2f !important;
            color: #ffffff !important;
            font-weight: bold;
        }
        
        .registro-inactivo:hover {
            background-color: #b71c1c !important;
            transform: none;
        }
        
        .registro-inactivo .monto-column {
            color: #ffffff !important;
            font-weight: bold;
        }
        
        /* Estilo para columna de observaciones */
        .observaciones-column {
            max-width: 200px;
            word-wrap: break-word;
            word-break: break-word;
            font-size: 0.85em;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        
        <!-- Barra superior institucional -->
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo CODESVI" class="logo-institucion" />
            <div class="nombre-institucion">COBRANZA DIARIA - SISTEMA DE REPORTES</div>
            <div class="barra-botones">
                <asp:Button ID="btnVolver" runat="server" Text="Volver" OnClick="btnVolver_Click" CssClass="btn btn-back" />
                <asp:Button ID="btnExportarExcel" runat="server" Text="Exportar Excel" OnClick="btnExportarExcel_Click" CssClass="btn btn-excel" />
            </div>
        </div>

        <div class="container">
            <h1>Operaciones de Cobranza por Dia</h1>
            
            <!-- Filtros -->
            <div class="filtros">
                <div class="filtros-row">
                    <div class="filtro-group">
                        <label class="filtro-label">Fecha Desde:</label>
                        <asp:TextBox ID="txtFechaDesde" runat="server" TextMode="Date" CssClass="filtro-control"></asp:TextBox>
                    </div>
                    <div class="filtro-group">
                        <label class="filtro-label">Fecha Hasta:</label>
                        <asp:TextBox ID="txtFechaHasta" runat="server" TextMode="Date" CssClass="filtro-control"></asp:TextBox>
                    </div>
                    <div class="filtro-group">
                        <label class="filtro-label">Sucursal:</label>
                        <asp:DropDownList ID="ddlSucursal" runat="server" CssClass="filtro-control">
                        </asp:DropDownList>
                    </div>
                    <div class="filtro-group">
                        <label class="filtro-label">Programa:</label>
                        <asp:DropDownList ID="ddlPrograma" runat="server" CssClass="filtro-control">
                        </asp:DropDownList>
                    </div>
                    <div class="filtro-group">
                        <label class="filtro-label">&nbsp;</label>
                        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" OnClick="btnFiltrar_Click" CssClass="btn" />
                    </div>
                </div>
            </div>

            <!-- Resumen de totales -->
            <asp:Panel ID="pnlResumen" runat="server" CssClass="resumen-totales" Visible="false">
                <h2 style="margin-top: 0; color: #D82628;">Resumen del Periodo</h2>
                <div class="resumen-grid">
                    <div class="resumen-item">
                        <h3>Total Operaciones</h3>
                        <div class="valor"><asp:Label ID="lblTotalOperaciones" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Total Cobrado</h3>
                        <div class="valor"><asp:Label ID="lblTotalCobrado" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Capital Cobrado</h3>
                        <div class="valor"><asp:Label ID="lblCapitalCobrado" runat="server"></asp:Label></div>
                    </div>
                    <div class="resumen-item">
                        <h3>Intereses Cobrados</h3>
                        <div class="valor"><asp:Label ID="lblInteresesCobrados" runat="server"></asp:Label></div>
                    </div>
                </div>
            </asp:Panel>

            <!-- Tabla de operaciones de cobranza -->
            <div class="table-responsive">
                <asp:GridView ID="gvCobranza" runat="server" AutoGenerateColumns="False" CssClass="gridview" 
                    OnRowDataBound="gvCobranza_RowDataBound" EmptyDataText="No se encontraron operaciones de cobranza para el periodo seleccionado."
                    EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Hora" HeaderText="Hora" DataFormatString="{0:HH:mm:ss}" />
                        <asp:BoundField DataField="NumeroOperacion" HeaderText="Numero Operacion" />
                        <asp:BoundField DataField="NombreBeneficiario" HeaderText="Beneficiario" />
                        <asp:BoundField DataField="Cobratario" HeaderText="Cobratario" />
                        <asp:BoundField DataField="Programa" HeaderText="Programa" />
                        <asp:BoundField DataField="Sucursal" HeaderText="Sucursal" />
                        <asp:BoundField DataField="MontoCapital" HeaderText="Capital" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="MontoInteres" HeaderText="Interes" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="MontoMoratorio" HeaderText="Moratorio" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="OtrosGastos" HeaderText="Otros Gastos" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="Descuentos" HeaderText="Descuentos" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="TotalOperacion" HeaderText="Total" DataFormatString="{0:C2}" />
                        <asp:BoundField DataField="TipoOperacion" HeaderText="Tipo" />
                        <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>

    <!-- Indicador de carga -->
    <div id="loadingMessage">
        <div class="loading-content">
            <div class="spinner"></div>
            <div>Cargando operaciones de cobranza...</div>
            <div><small>Esta operación puede tardar unos momentos</small></div>
        </div>
    </div>

    <script type="text/javascript">
        function showLoading() {
            document.getElementById('loadingMessage').style.display = 'flex';
        }
        
        function hideLoading() {
            document.getElementById('loadingMessage').style.display = 'none';
        }
    </script>
</body>
</html>
