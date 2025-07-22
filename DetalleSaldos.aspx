<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DetalleSaldos.aspx.cs" Inherits="DetalleSaldos" ResponseEncoding="UTF-8" EnableEventValidation="false" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Detalle de Saldos por Persona</title>
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
            color: #fff !important;
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
            text-decoration: none;
            display: inline-block;
        }
        .btn-volver:hover {
            background: linear-gradient(90deg, #34495e 0%, #2d3e50 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(45, 62, 80, 0.25);
            color: #fff !important;
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
        .nombre-institucion {
            color: #fff;
            font-size: 1.3em;
            font-weight: bold;
            letter-spacing: 1px;
            text-align: center;
            flex: 1;
        }
        .barra-botones-exportar {
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .info-programa {
            background-color: #f7faff;
            border-radius: 12px;
            padding: 16px;
            margin-bottom: 24px;
            border-left: 5px solid #D82628;
            overflow: hidden;
        }
        .info-programa h2 {
            margin-top: 0;
            color: #D82628;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 1px solid #e0e6ed;
        }
        .resumen-saldos {
            display: flex;
            flex-wrap: wrap;
            gap: 16px;
            margin-top: 16px;
        }
        .resumen-section {
            flex: 1;
            min-width: 300px;
            margin-bottom: 16px;
            border-radius: 8px;
            padding: 15px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
        }
        .section-pagos {
            background-color: #f1f8e9; /* Fondo verde claro */
            border-left: 5px solid #4CAF50;
        }
        .section-saldos {
            background-color: #ffebee; /* Fondo rojo claro */
            border-left: 5px solid #F44336;
        }
        .section-otros {
            background-color: #f5f5f5; /* Fondo gris claro */
            border-left: 5px solid #607D8B;
        }
        .section-title {
            font-size: 16px;
            font-weight: bold;
            margin-bottom: 15px;
            color: #2d3e50;
            text-align: center;
            padding-bottom: 8px;
            border-bottom: 1px dashed #d1d1d1;
        }
        .resumen-items {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
        }
        .resumen-item {
            flex: 1;
            min-width: 140px;
            padding: 16px;
            border-radius: 8px;
            text-align: center;
            transition: transform 0.2s;
        }
        .resumen-item:hover {
            transform: translateY(-5px);
        }
        .resumen-item.monto-asignado {
            background-color: #eeeeee;
            border-bottom: 3px solid #607D8B;
        }
        .resumen-item.monto-pagado {
            background-color: #e8f5e8;
            border-bottom: 3px solid #4CAF50;
        }
        .resumen-item.saldo-pendiente {
            background-color: #ffebee;
            border-bottom: 3px solid #F44336;
        }
        .resumen-item h3 {
            margin-top: 0;
            margin-bottom: 10px;
            color: #2d3e50;
            font-size: 0.9em;
        }
        .resumen-item .valor {
            font-size: 1.4em;
            font-weight: bold;
        }
        .resumen-item .valor.monto-asignado {
            color: #455A64;
        }
        .resumen-item .valor.monto-pagado {
            color: #2E7D32;
        }
        .resumen-item .valor.saldo-pendiente {
            color: #C62828;
        }
        .resumen-item .valor.otros {
            color: #455A64;
        }
        .resumen-item .valor.beneficiarios {
            color: #0277BD;
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
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo 1" class="logo-institucion" />
            <div class="barra-botones-exportar">
                <asp:Button ID="Button1" runat="server" Text="Exportar a PDF" OnClick="btnExportarPDF_Click" CssClass="btn-pdf" />
                <asp:Button ID="Button2" runat="server" Text="Exportar a Excel" OnClick="btnExportarExcel_Click" CssClass="btn-excel" />
                <asp:Button ID="btnVolver" runat="server" Text="Volver" OnClick="btnVolver_Click" 
                    style="background: linear-gradient(90deg, #2d3e50 0%, #34495e 100%); color: #fff; border: none; border-radius: 25px; padding: 12px 32px; font-size: 1.1em; font-weight: bold; cursor: pointer; box-shadow: 0 2px 8px rgba(45, 62, 80, 0.15); margin: 8px 0 24px; letter-spacing: 1px;" />
            </div>
        </div>
        <div class="container">
            <div class="info-programa">
                <h2>Programa: <asp:Label ID="lblPrograma" runat="server"></asp:Label></h2>
                
                <div class="resumen-saldos">
                    <!-- SECCIÓN DE MONTOS ASIGNADOS -->
                    <div class="resumen-section section-otros">
                        <div class="section-title">MONTO ASIGNADO</div>
                        <div class="resumen-items">
                            <div class="resumen-item monto-asignado">
                                <h3>Total Asignado</h3>
                                <div class="valor monto-asignado"><asp:Label ID="lblTotalAsignado" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item">
                                <h3>Total Beneficiarios</h3>
                                <div class="valor beneficiarios"><asp:Label ID="lblTotalBeneficiarios" runat="server"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- SECCIÓN DE MONTOS PAGADOS -->
                    <div class="resumen-section section-pagos">
                        <div class="section-title">MONTOS PAGADOS</div>
                        <div class="resumen-items">
                            <div class="resumen-item monto-pagado">
                                <h3>Monto Pagado a Capital</h3>
                                <div class="valor monto-pagado"><asp:Label ID="lblMontoPagadoCapital" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item monto-pagado">
                                <h3>Monto Pagado a Intereses</h3>
                                <div class="valor monto-pagado"><asp:Label ID="lblMontoPagadoIntereses" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item monto-pagado">
                                <h3>Otros Gastos</h3>
                                <div class="valor monto-pagado"><asp:Label ID="lblOtrosGastos" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item monto-pagado">
                                <h3>Total Pagado</h3>
                                <div class="valor monto-pagado" style="font-size: 1.6em;"><asp:Label ID="lblTotalPagado" runat="server"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- SECCIÓN DE SALDOS PENDIENTES -->
                    <div class="resumen-section section-saldos">
                        <div class="section-title">SALDOS PENDIENTES</div>
                        <div class="resumen-items">
                            <div class="resumen-item saldo-pendiente">
                                <h3>Saldo Pendiente a Capital</h3>
                                <div class="valor saldo-pendiente"><asp:Label ID="lblSaldoPendienteCapital" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item saldo-pendiente">
                                <h3>Saldo Pendiente Interés Real</h3>
                                <div class="valor saldo-pendiente"><asp:Label ID="lblSaldoPendienteInteresReal" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item saldo-pendiente">
                                <h3>Saldo Pendiente Intereses Moratorios</h3>
                                <div class="valor saldo-pendiente"><asp:Label ID="lblSaldoPendienteInteresesMoratorios" runat="server"></asp:Label></div>
                            </div>
                            <div class="resumen-item saldo-pendiente">
                                <h3>Total Saldo Pendiente</h3>
                                <div class="valor saldo-pendiente" style="font-size: 1.6em;"><asp:Label ID="lblSaldoPendiente" runat="server"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <h1>Detalle de Saldos por Persona</h1>
            
            <!-- Leyenda de colores -->
            <div class="leyenda-colores" style="display: flex; justify-content: center; margin-bottom: 20px; flex-wrap: wrap; gap: 15px;">
                <div style="display: flex; align-items: center; padding: 8px 16px; background: #f5f5f5; border-radius: 20px; border-left: 4px solid #607D8B;">
                    <span style="width: 16px; height: 16px; background: #607D8B; border-radius: 50%; margin-right: 8px;"></span>
                    <span style="font-size: 14px; font-weight: 500; color: #607D8B;">Monto Asignado</span>
                </div>
                <div style="display: flex; align-items: center; padding: 8px 16px; background: #f3e5f5; border-radius: 20px; border-left: 4px solid #9C27B0;">
                    <span style="width: 16px; height: 16px; background: #9C27B0; border-radius: 50%; margin-right: 8px;"></span>
                    <span style="font-size: 14px; font-weight: 500; color: #9C27B0;">Municipio</span>
                </div>
                <div style="display: flex; align-items: center; padding: 8px 16px; background: #e8f5e8; border-radius: 20px; border-left: 4px solid #4CAF50;">
                    <span style="width: 16px; height: 16px; background: #4CAF50; border-radius: 50%; margin-right: 8px;"></span>
                    <span style="font-size: 14px; font-weight: 500; color: #4CAF50;">Montos Pagados</span>
                </div>
                <div style="display: flex; align-items: center; padding: 8px 16px; background: #ffebee; border-radius: 20px; border-left: 4px solid #F44336;">
                    <span style="width: 16px; height: 16px; background: #F44336; border-radius: 50%; margin-right: 8px;"></span>
                    <span style="font-size: 14px; font-weight: 500; color: #F44336;">Saldos Pendientes (Capital, Intereses, Liquidar)</span>
                </div>
            </div>

            <div class="table-responsive">
                <div class="barra-superior">
                    <div class="izquierda">
                        <asp:DropDownList ID="ddlMunicipio" runat="server" CssClass="barra-seleccion" AutoPostBack="true" OnSelectedIndexChanged="ddlMunicipio_SelectedIndexChanged">
                        </asp:DropDownList>
                        <asp:TextBox ID="txtBuscar" runat="server" placeholder="Buscar beneficiario..." CssClass="barra-seleccion"></asp:TextBox>
                        <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn-pdf" OnClick="btnBuscar_Click" />
                    </div>
                </div>
                <br />
                <asp:GridView ID="GridViewDetalle" runat="server" AutoGenerateColumns="False" CssClass="gridview" OnRowDataBound="GridViewDetalle_RowDataBound" OnRowCommand="GridViewDetalle_RowCommand" DataKeyNames="IdBeneficiario,IdSubprograma">
                    <Columns>
                        <asp:BoundField DataField="IdBeneficiario" HeaderText="ID" />
                        <asp:BoundField DataField="Beneficiario" HeaderText="Beneficiario" />
                        <asp:BoundField DataField="MunicipioContrato" HeaderText="Municipio" />
                        <asp:BoundField DataField="MontoAsignado" HeaderText="Total Asignado" />
                        <asp:BoundField DataField="MontoPagadoCapital" HeaderText="Monto Pagado a Capital" />
                        <asp:BoundField DataField="MontoPagadoIntereses" HeaderText="Monto Pagado a Intereses" />
                        <asp:BoundField DataField="OtrosGastos" HeaderText="Otros Gastos" />
                        <asp:BoundField DataField="TotalPagado" HeaderText="Total Pagado" />
                        <asp:BoundField DataField="SaldoPendienteCapital" HeaderText="Saldo Pendiente a Capital" />
                        <asp:BoundField DataField="SaldoPendienteIntereses" HeaderText="Saldo pendiente Interes Real" />
                        <asp:BoundField DataField="SaldoPendienteInteresesMoratorios" HeaderText="Saldo pendiente intereses moratorios" />
                        <asp:BoundField DataField="SaldoParaLiquidar" HeaderText="Saldo para Liquidar" />
                        <asp:BoundField DataField="FecContratacion" HeaderText="Fecha de Contratación" HeaderStyle-BackColor="#1976D2" HeaderStyle-ForeColor="White" ItemStyle-BackColor="#E3F2FD" ItemStyle-ForeColor="#1976D2" />
                        <asp:BoundField DataField="FechaUltimoPago" HeaderText="Fecha Último Pago" />
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnVerDetalleBeneficiario" runat="server" Text="Ver Pagos" 
                                    CommandName="VerDetalleBeneficiario" 
                                    CommandArgument='<%# Container.DataItemIndex %>'
                                    CssClass="btn-pdf" style="padding: 8px 16px; font-size: 0.9em;" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
