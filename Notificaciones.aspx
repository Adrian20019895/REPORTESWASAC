<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Notificaciones.aspx.cs" Inherits="Notificaciones" ResponseEncoding="UTF-8" Debug="true" EnableEventValidation="false" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>NOTIFICACIONES - CODESVI</title>
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
        }
        .gridview th, .gridview td {
            padding: 15px 20px;
            border-bottom: 1px solid #e0e6ed;
            text-align: left;
        }
        .gridview th {
            background: #3F51B5;
            color: #fff;
            font-weight: 600;
            font-size: 1.1em;
        }
        .gridview tr:nth-child(even) {
            background: #f7faff;
        }
        .gridview tr:hover {
            background: #e6f0ff;
            transform: scale(1.01);
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
        /* Botón para volver */
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
        }
        .btn-volver:hover {
            background: linear-gradient(90deg, #455A64 0%, #607D8B 100%);
            transform: translateY(-2px) scale(1.04);
            box-shadow: 0 4px 16px rgba(96, 125, 139, 0.25);
        }
        /* Botón para ver detalle */
        .btn-detalle {
            background: linear-gradient(90deg, #4CAF50 0%, #388E3C 100%);
            color: #fff;
            border: none;
            border-radius: 20px;
            padding: 8px 20px;
            font-size: 0.9em;
            font-weight: bold;
            cursor: pointer;
            box-shadow: 0 2px 6px rgba(76, 175, 80, 0.15);
            transition: all 0.2s;
        }
        .btn-detalle:hover {
            background: linear-gradient(90deg, #388E3C 0%, #4CAF50 100%);
            transform: translateY(-2px) scale(1.05);
            box-shadow: 0 4px 12px rgba(76, 175, 80, 0.25);
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
            margin-top: 120px;
        }
        .logo-institucion {
            height: 138px;  
            border-radius: 8px;
            padding: 4px 8px;
            margin: 0 8px;
        }
        .info-resumen {
            background: linear-gradient(135deg, #3F51B5 0%, #303F9F 100%);
            color: white;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 30px;
            text-align: center;
        }
        .info-resumen h2 {
            margin: 0 0 10px 0;
            font-size: 1.5em;
        }
        .info-resumen p {
            margin: 5px 0;
            font-size: 1.1em;
        }
        .alerta-mensualidades {
            color: #FF5722;
            font-weight: bold;
            font-size: 1.2em;
        }
        .programa-titulo {
            font-size: 1.1em;
            font-weight: bold;
            color: #2d3e50;
        }
        .contador-beneficiarios {
            background: #E3F2FD;
            color: #1976D2;
            padding: 5px 12px;
            border-radius: 15px;
            font-weight: bold;
            font-size: 0.9em;
        }
        .mensualidades-promedio {
            background: #FFEBEE;
            color: #C62828;
            padding: 5px 12px;
            border-radius: 15px;
            font-weight: bold;
            font-size: 0.9em;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="barra-institucional">
            <img src="/WASAC/img/logocodesvi.png" alt="Logo 1" class="logo-institucion" />
            <a href="VerDatos.aspx" class="btn-volver">← Volver al Dashboard</a>
        </div>
        
        <div class="container">
            <div class="info-resumen">
                <h2>🔔 Centro de Notificaciones</h2>
                <p>Programas con beneficiarios en estado de mensualidades atrasadas</p>
                <p class="alerta-mensualidades">⚠️ Requieren atención prioritaria para gestión de cobranza</p>
            </div>
            
            <h1>Programas con Mensualidades Atrasadas</h1>
            <p style="text-align: center; color: #666; margin-bottom: 30px;">
                Seleccione un programa para ver el detalle de beneficiarios con mensualidades pendientes
            </p>
            
            <div class="table-responsive">
                <asp:GridView ID="GridViewProgramas" runat="server" AutoGenerateColumns="False" CssClass="gridview" OnRowDataBound="GridViewProgramas_RowDataBound" OnRowCommand="GridViewProgramas_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Programa" HeaderText="Programa" ItemStyle-CssClass="programa-titulo" />
                        <asp:BoundField DataField="TotalBeneficiarios" HeaderText="Beneficiarios con Atrasos" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="PromedioMensualidades" HeaderText="Promedio Mensualidades Atrasadas" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="MaximoMensualidades" HeaderText="Máximo Mensualidades Atrasadas" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="TotalMontoAtrasado" HeaderText="Monto Total Atrasado" ItemStyle-HorizontalAlign="Right" />
                        <asp:TemplateField HeaderText="Acciones" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Button ID="btnVerDetalle" runat="server" Text="Ver Beneficiarios" 
                                           CommandName="VerDetalle" CommandArgument='<%# Eval("Programa") %>' 
                                           CssClass="btn-detalle" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
    
    <!-- Indicador de carga -->
    <div id="loadingMessage">
        <div class="loading-content">
            <div class="spinner"></div>
            <div>Cargando datos de notificaciones...</div>
            <div><small>Analizando mensualidades atrasadas por programa</small></div>
        </div>
    </div>
</body>
</html>
