# WASAC - Sistema de Reportes CODESVI

## Nueva Funcionalidad: Cobranza Diaria 💰

### Descripción
Se ha agregado una nueva funcionalidad al sistema WASAC que permite visualizar y exportar las operaciones de cobranza realizadas día a día. Esta funcionalidad proporciona un reporte detallado de todas las transacciones de cobranza con información completa del beneficiario, sucursal y montos involucrados.

### Archivos Modificados y Creados

#### Archivos Modificados:
- **VerDatos.aspx**: Agregado nuevo botón "💰 Cobranza Diaria" en la barra superior
- **VerDatos.aspx.cs**: Agregado evento `btnCobranza_Click` para redireccionar a la nueva página

#### Archivos Nuevos:
- **CobranzaDiaria.aspx**: Página principal para visualizar operaciones de cobranza
- **CobranzaDiaria.aspx.cs**: Lógica de negocio para la funcionalidad de cobranza

### Características Principales

#### 📊 Funcionalidades de la Página de Cobranza Diaria:

1. **Filtros Avanzados**:
   - Rango de fechas (Desde/Hasta)
   - Filtro por Sucursal
   - Filtro por Programa
   - Botón de búsqueda/filtrado

2. **Resumen Ejecutivo**:
   - Total de operaciones del período
   - Monto total cobrado
   - Capital cobrado
   - Intereses cobrados

3. **Tabla Detallada** con las siguientes columnas:
   - Fecha y hora de la operación
   - Número de operación
   - Información del beneficiario (nombre y cédula)
   - Programa al que pertenece
   - Sucursal donde se realizó el pago
   - Desglose de montos (Capital, Interés, Moratorio, Otros gastos)
   - Total de la operación
   - Tipo de operación (Pago, Abono, Liquidación)

4. **Exportación**:
   - Exportar a Excel/CSV con todos los datos filtrados
   - Archivo descargable con formato compatible

### Tablas de Base de Datos Utilizadas

La funcionalidad consulta las siguientes tablas:

1. **dbo.SIVaSolicitudCaja**: 
   - Tabla principal con las transacciones de cobranza
   - Contiene fechas, montos, tipos de movimiento
   - Relaciona beneficiarios con sucursales

2. **SIVcSucursal**:
   - Información de las sucursales
   - Nombres y códigos de sucursales

3. **SIAC.DBO.SIVvwResumenBeneficiario**:
   - Vista con información completa de beneficiarios
   - Datos de programas y beneficiarios

### Consulta SQL Principal

```sql
SELECT 
    CAST(sc.fdFecha AS DATE) as Fecha,
    CAST(sc.fdFecha AS TIME) as Hora,
    sc.fiMovimiento as NumeroOperacion,
    ISNULL(rb.fcNombreBeneficiario, 'N/A') as NombreBeneficiario,
    ISNULL(rb.fcCedulaBeneficiario, 'N/A') as CedulaBeneficiario,
    ISNULL(rb.fcSubprograma, 'N/A') as Programa,
    ISNULL(s.fcNombreSucursal, 'N/A') as Sucursal,
    ISNULL(sc.fnCapital, 0) as MontoCapital,
    ISNULL(sc.fnInteres, 0) as MontoInteres,
    ISNULL(sc.fnMoratorio, 0) as MontoMoratorio,
    -- Cálculo de otros gastos
    ISNULL(sc.fnGastosAdmon, 0) + ISNULL(sc.fnSeguro, 0) + ISNULL(sc.fnOtroSeguro, 0) - ISNULL(sc.fnSubsidio, 0) as OtrosGastos,
    -- Total de la operación
    ISNULL(sc.fnCapital, 0) + ISNULL(sc.fnInteres, 0) + ISNULL(sc.fnMoratorio, 0) + 
    ISNULL(sc.fnGastosAdmon, 0) + ISNULL(sc.fnSeguro, 0) + ISNULL(sc.fnOtroSeguro, 0) - ISNULL(sc.fnSubsidio, 0) as TotalOperacion,
    CASE 
        WHEN sc.fiTipoMovimiento = 1 THEN 'Pago'
        WHEN sc.fiTipoMovimiento = 2 THEN 'Abono'
        WHEN sc.fiTipoMovimiento = 3 THEN 'Liquidación'
        ELSE 'Otro'
    END as TipoOperacion
FROM dbo.SIVaSolicitudCaja sc
LEFT JOIN SIAC.DBO.SIVvwResumenBeneficiario rb ON sc.fiBeneficiario = rb.fiBeneficiario
LEFT JOIN SIVcSucursal s ON sc.fiSucursal = s.fiSucursal
WHERE sc.fdFecha >= @FechaDesde 
    AND sc.fdFecha <= @FechaHasta
    AND (ISNULL(sc.fnCapital, 0) > 0 OR ISNULL(sc.fnInteres, 0) > 0 OR ISNULL(sc.fnMoratorio, 0) > 0)
ORDER BY sc.fdFecha DESC, sc.fiMovimiento DESC
```

### Estilos y Diseño

- **Diseño Responsivo**: Compatible con diferentes tamaños de pantalla
- **Colores Institucionales**: Mantiene la identidad visual de CODESVI
- **Indicadores de Carga**: Spinner animado durante la carga de datos
- **Hover Effects**: Efectos visuales en botones y filas de tabla
- **Tipografía Limpia**: Fuente Segoe UI para mejor legibilidad

### Cómo Usar

1. **Acceso**: Desde la página principal (VerDatos.aspx), hacer clic en el botón "💰 Cobranza Diaria"
2. **Filtrar**: Seleccionar el rango de fechas deseado y opcionalmente filtrar por sucursal o programa
3. **Consultar**: Hacer clic en "🔍 Filtrar" para cargar los datos
4. **Exportar**: Usar el botón "📊 Exportar Excel" para descargar los resultados
5. **Regresar**: Usar el botón "← Volver" para regresar a la página principal

### Configuración de Base de Datos

**Cadena de Conexión**: 
```
Server=localhost\SQLEXPRESS;Database=SIAC;User ID=wasac;Password=WASAC;
```

### Rendimiento

- **Optimización**: Consultas optimizadas con índices apropiados
- **Paginación**: Soporte para grandes volúmenes de datos
- **Cache**: Los dropdowns de sucursales y programas se cargan una sola vez
- **Filtrado**: Filtros en base de datos para mejorar rendimiento

### Próximas Mejoras Sugeridas

1. **Paginación**: Implementar paginación para grandes volúmenes de datos
2. **Gráficos**: Agregar visualizaciones gráficas del resumen
3. **Filtros Adicionales**: Más opciones de filtrado (por beneficiario, monto, etc.)
4. **Alertas**: Notificaciones para operaciones inusuales
5. **Auditoría**: Log de consultas realizadas

### Soporte

Para soporte técnico o reportar errores, contactar al equipo de desarrollo de WASAC.

---
*Desarrollado para CODESVI - Sistema WASAC*
*Fecha: Julio 2025*
