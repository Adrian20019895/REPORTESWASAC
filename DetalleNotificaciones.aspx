<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DetalleNotificaciones.aspx.cs" Inherits="DetalleNotificaciones" ResponseEncoding="UTF-8" Debug="true" EnableEventValidation="false" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>DETALLE NOTIFICACIONES - CODESVI</title>
    <style>
        /* Estilos generales para la pagina */
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
            border-collapse: collapse;
            margin-top: 20px;
            font-size: 0.9em;
        }
        .gridview th, .gridview td {
            padding: 10px 8px;
            border-bottom: 1px solid #e0e6ed;
            text-align: left;
        }
        .gridview th {
            background: #3F51B5;
            color: #fff;
            font-weight: 600;
            font-size: 0.95em;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        .gridview tr:nth-child(even) {
            background: #f7faff;
        }
        .gridview tr:hover {
            background: #e6f0ff;
            transform: scale(1.005);
            transition: all 0.2s ease;
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
            border-top: 6px solid #3F51B5;
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
        /* Botones */
        .btn-volver {
            background: linear-gradient(90deg, #607D8B 0%, #455A64 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            padding: 12px 32px;
            font-size: 1.1em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(96, 125, 139, 0.15);
            transition: background 0.3s, transform 0.2s;
            margin-bottom: 24px;
            margin-top: 8px;
            letter-spacing: 1px;
            text-decoration: none;
            display: inline-block;
            margin-right: 15px;
        }
        .btn-volver:hover {
            background: linear-gradient(90deg, #455A64 0%, #607D8B 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(96, 125, 139, 0.25);
        }
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
            margin-top: 120px;
        }
        .logo-institucion {
            height: 138px;  
            border-radius: 8px;
            padding: 4px 8px;
            margin: 0 8px;
        }
        .info-programa {
            background: linear-gradient(135deg, #3F51B5 0%, #303F9F 100%);
            color: white;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 30px;
        }
        .info-programa h2 {
            margin: 0 0 15px 0;
            font-size: 1.6em;
        }
        .resumen-stats {
            display: flex;
            justify-content: space-around;
            flex-wrap: wrap;
            gap: 15px;
            margin-top: 20px;
        }
        .stat-item {
            background: rgba(255, 255, 255, 0.15);
            padding: 15px;
            border-radius: 8px;
            text-align: center;
            flex: 1;
            min-width: 150px;
        }
        .stat-number {
            font-size: 2em;
            font-weight: bold;
            display: block;
        }
        .stat-label {
            font-size: 0.9em;
            opacity: 0.9;
        }
        .filtros-container {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 15px;
            flex-wrap: wrap;
        }
        .filtros-container label {
            font-weight: bold;
            color: #2d3e50;
        }
        .filtro-select {
            padding: 8px 12px;
            border: 2px solid #3F51B5;
            border-radius: 5px;
            font-size: 1em;
        }
        .mensualidades-criticas {
            background: #FFEBEE !important;
            border-left: 4px solid #F44336 !important;
        }
        .mensualidades-altas {
            background: #FFF3E0 !important;
            border-left: 4px solid #FF9800 !important;
        }
        .mensualidades-moderadas {
            background: #E8F5E8 !important;
            border-left: 4px solid #4CAF50 !important;
        }
        .monto-alto {
            color: #C62828 !important;
            font-weight: bold !important;
        }
        .beneficiario-nombre {
            font-weight: bold;
            color: #2d3e50;
        }
        /* Estilos para las columnas de saldos */
        .monto-asignado {
            color: #455A64 !important;
            font-weight: 500;
        }
        .monto-pagado {
            color: #2E7D32 !important;
            font-weight: 500;
        }
        .saldo-pendiente {
            color: #C62828 !important;
            font-weight: 500;
        }
        .saldo-liquidar {
            color: #1976D2 !important;
            font-weight: bold;
            background: #E3F2FD !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo 1" class="logo-institucion" />
            <div>
                <a href="Notificaciones.aspx" class="btn-volver">← Volver a Notificaciones</a>
                <asp:Button ID="btnExportarExcel" runat="server" Text="📊 Exportar a Excel" OnClick="btnExportarExcel_Click" CssClass="btn-excel" />
            </div>
        </div>
        
        <div class="container">
            <div class="info-programa">
                <h2>🔔 <asp:Label ID="lblProgramaSeleccionado" runat="server" Text="Programa"></asp:Label></h2>
                <p>Beneficiarios con mensualidades atrasadas que requieren gestión de cobranza</p>
                
                <div class="resumen-stats">
                    <div class="stat-item">
                        <span class="stat-number"><asp:Label ID="lblTotalBeneficiarios" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Beneficiarios con Atrasos</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-number"><asp:Label ID="lblPromedioMensualidades" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Promedio Mensualidades Atrasadas</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-number"><asp:Label ID="lblMaximoMensualidades" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Máximo Mensualidades Atrasadas</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-number"><asp:Label ID="lblMontoTotalAtrasado" runat="server" Text="$0"></asp:Label></span>
                        <span class="stat-label">Monto Total Pendiente</span>
                    </div>
                </div>
            </div>

            <div class="filtros-container">
                <label>Filtrar por mensualidades atrasadas:</label>
                <asp:DropDownList ID="ddlFiltroMensualidades" runat="server" CssClass="filtro-select" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroMensualidades_SelectedIndexChanged">
                    <asp:ListItem Value="1" Text="1+ mensualidades" Selected="true"></asp:ListItem>
                    <asp:ListItem Value="3" Text="3+ mensualidades"></asp:ListItem>
                    <asp:ListItem Value="5" Text="5+ mensualidades"></asp:ListItem>
                    <asp:ListItem Value="10" Text="10+ mensualidades"></asp:ListItem>
                    <asp:ListItem Value="0" Text="Todos (incluso sin atrasos)"></asp:ListItem>
                </asp:DropDownList>
                
                <label>Filtrar por contacto:</label>
                <asp:DropDownList ID="ddlFiltroContacto" runat="server" CssClass="filtro-select" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroContacto_SelectedIndexChanged">
                    <asp:ListItem Value="todos" Text="Todos"></asp:ListItem>
                    <asp:ListItem Value="sin_contacto" Text="Sin teléfono ni email"></asp:ListItem>
                    <asp:ListItem Value="con_telefono" Text="Con teléfono"></asp:ListItem>
                    <asp:ListItem Value="con_email" Text="Con email"></asp:ListItem>
                </asp:DropDownList>
                
                <label>Ordenar por:</label>
                <asp:DropDownList ID="ddlOrdenamiento" runat="server" CssClass="filtro-select" AutoPostBack="true" OnSelectedIndexChanged="ddlOrdenamiento_SelectedIndexChanged">
                    <asp:ListItem Value="mensualidades_desc" Text="Más mensualidades atrasadas"></asp:ListItem>
                    <asp:ListItem Value="ultimo_pago" Text="Último pago más antiguo"></asp:ListItem>
                    <asp:ListItem Value="monto_desc" Text="Mayor saldo para liquidar"></asp:ListItem>
                    <asp:ListItem Value="nombre_asc" Text="Nombre A-Z"></asp:ListItem>
                    <asp:ListItem Value="mensualidades_asc" Text="Menos mensualidades atrasadas"></asp:ListItem>
                    <asp:ListItem Value="monto_asc" Text="Menor saldo para liquidar"></asp:ListItem>
                </asp:DropDownList>
            </div>
            
            <div class="table-responsive">
                <asp:GridView ID="GridViewBeneficiarios" runat="server" AutoGenerateColumns="False" CssClass="gridview" OnRowDataBound="GridViewBeneficiarios_RowDataBound" AllowPaging="true" PageSize="50" OnPageIndexChanging="GridViewBeneficiarios_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="fiBeneficiario" HeaderText="ID" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                        <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre Completo" ItemStyle-CssClass="beneficiario-nombre" ItemStyle-Width="200px" />
                        <asp:BoundField DataField="CURP" HeaderText="CURP" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="MunicipioContrato" HeaderText="Municipio" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="fiMenAtrasadas" HeaderText="Mens. Atrasadas" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px" />
                        <asp:BoundField DataField="MontoAsignado" HeaderText="Monto Asignado" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="AmortizacionPag" HeaderText="Pagado a Capital" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="SaldoPendienteCapital" HeaderText="Saldo Pendiente Capital" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="130px" />
                        <asp:BoundField DataField="SaldoPendienteInteresReal" HeaderText="Saldo Pendiente Interés Real" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="140px" />
                        <asp:BoundField DataField="SaldoPendienteInteresesMoratorios" HeaderText="Saldo Pendiente Intereses Moratorios" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="160px" />
                        <asp:BoundField DataField="SaldoParaLiquidar" HeaderText="Saldo para Liquidar" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" ItemStyle-Width="130px" />
                        <asp:BoundField DataField="fdUltimoPago" HeaderText="Último Pago" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="Telefono" HeaderText="Teléfono" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-Width="150px" />
                    </Columns>
                    <PagerStyle HorizontalAlign="Center" CssClass="pager" />
                </asp:GridView>
            </div>
        </div>
    </form>
    
    <!-- Indicador de carga -->
    <div id="loadingMessage">
        <div class="loading-content">
            <div class="spinner"></div>
            <div>Cargando beneficiarios...</div>
            <div><small>Procesando datos de mensualidades atrasadas</small></div>
        </div>
    </div>
</body>
</html>
