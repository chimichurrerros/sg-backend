# Payroll Frontend Contract (Estado actual del backend)

Fecha: 2026-05-30
Repositorio: sg-backend

## 1) Estado real de la API

El backend NO tiene CRUD completo de planillas (payroll process) en este momento.

No existen estos endpoints:
- GET /api/payroll-processes
- POST /api/payroll-processes
- GET /api/payroll-processes/{id}
- PUT/PATCH /api/payroll-processes/{id} (edicion de nombre/fechas)
- DELETE /api/payroll-processes/{id}

Si el frontend necesita esas operaciones para la pantalla de Planillas, actualmente son GAP de backend.

## 2) Endpoints disponibles para planillas

### 2.1 Cambiar estado de planilla
- Metodo: PATCH
- URL: /api/payroll-processes/{processId}/status
- Body:
  {
    "payrollStatusId": 1
  }
- Success: 204 No Content
- Errores:
  - 404 si no existe planilla o estado
  - 400 si request invalido
  - 409 en conflicto de negocio
  - 500 error inesperado

### 2.2 Agregar/actualizar concepto manual por empleado (upsert)
- Metodo: POST
- URL: /api/payroll-processes/{processId}/manual-details
- Body:
  {
    "employeeId": 15,
    "payrollUpdateId": 8,
    "amount": 150000
  }
- Success: 200 OK
- Response:
  {
    "id": 41,
    "employeeId": 15,
    "employeeFullName": "ANA PEREZ",
    "conceptName": "HORAS EXTRA",
    "payrollTypeName": "Haber",
    "amount": 150000
  }
- Errores:
  - 404 empleado/planilla/concepto no encontrado
  - 400 validacion
  - 409 si planilla no esta abierta
  - 500 error inesperado

### 2.3 Listar conceptos manuales de planilla
- Metodo: GET
- URL: /api/payroll-processes/{processId}/manual-details
- Success: 200 OK
- Response:
  [
    {
      "id": 41,
      "employeeId": 15,
      "employeeFullName": "ANA PEREZ",
      "conceptName": "HORAS EXTRA",
      "payrollTypeName": "Haber",
      "amount": 150000
    }
  ]
- Errores:
  - 404 si planilla no existe
  - 500 error inesperado

### 2.4 Eliminar concepto manual
- Metodo: DELETE
- URL: /api/payroll-processes/manual-details/{id}
- Success: 204 No Content
- Errores:
  - 404 detalle no encontrado
  - 400 si el concepto no es de tipo fijo
  - 409 si planilla no esta abierta
  - 500 error inesperado

### 2.5 Calcular planilla
- Metodo: POST
- URL: /api/payroll-processes/{id}/calculate
- Success: 200 OK
- Response:
  {
    "payrollProcessId": 10,
    "payrollProcessName": "PLANILLA - ENERO 2026",
    "employeesProcessed": 3,
    "totalHaberes": 9311478,
    "totalDescuentos": 782745,
    "totalNeto": 8528733,
    "employees": [
      {
        "employeeId": 1,
        "employeeName": "HEBER ARANDA",
        "salarioBase": 3000000,
        "jornalDiario": 100000,
        "diasTrabajados": 30,
        "cantidadHijos": 0,
        "totalDeducibleIPS": 3000000,
        "totalHaberes": 3000000,
        "totalDescuentos": 260915,
        "totalNeto": 2739085,
        "details": [
          {
            "payrollUpdateId": 1,
            "payrollUpdateName": "SALARIO BASICO",
            "payrollTypeId": 1,
            "formulaTypeId": 2,
            "amount": 3000000
          }
        ]
      }
    ]
  }
- Errores:
  - 404 planilla no encontrada
  - 400 validacion (formula invalida, variable invalida, etc.)
  - 409 si no esta abierta o falta monto manual para concepto fijo
  - 500 error inesperado

## 3) Endpoints disponibles de incidencias manuales (pre-planilla)

### 3.1 Crear incidencia manual pendiente
- Metodo: POST
- URL: /api/manual-concepts
- Body:
  {
    "employeeId": 15,
    "payrollUpdateId": 8,
    "amount": 150000,
    "occurrenceDate": "2026-05-30"
  }
- Success: 201 Created
- Response:
  {
    "id": 12,
    "employeeId": 15,
    "employeeFullName": "ANA PEREZ",
    "payrollUpdateId": 8,
    "conceptName": "HORAS EXTRA",
    "payrollTypeName": "Haber",
    "amount": 150000,
    "occurrenceDate": "2026-05-30",
    "statusName": "Pending",
    "payrollProcessId": null
  }
- Errores:
  - 404 empleado o concepto no encontrado
  - 400 validacion (solo concepto fijo)
  - 500 error inesperado

### 3.2 Listar incidencias pendientes
- Metodo: GET
- URL: /api/manual-concepts/pending
- Success: 200 OK
- Response: lista de ManualConceptIncidentResponseDto
- Errores: 500 error inesperado

## 4) Endpoints disponibles de catalogo de conceptos

### 4.1 Listar conceptos de nomina
- Metodo: GET
- URL: /api/payroll-updates
- Success: 200 OK
- Response:
  [
    {
      "id": 8,
      "payrollTypeId": 1,
      "payrollTypeName": "Haberes",
      "formulaTypeId": 1,
      "formulaTypeName": "Fijo",
      "name": "HORAS EXTRA",
      "formula": null,
      "ipsDeductible": true
    }
  ]

### 4.2 Crear concepto de nomina
- Metodo: POST
- URL: /api/payroll-updates
- Body:
  {
    "name": "BONO PRODUCTIVIDAD",
    "payrollTypeId": 1,
    "formulaTypeId": 1,
    "formula": null,
    "ipsDeductible": false
  }
- Success: 201 Created
- Errores: 400 validacion, 500 error inesperado

## 5) Reglas de negocio actuales

1. Una planilla debe estar en estado "Abierto" para:
   - agregar/actualizar detalle manual
   - eliminar detalle manual
   - calcular planilla

2. Solo conceptos de tipo FormulaType = Fixed pueden ser manuales.

3. En manual-details existe upsert por (payrollProcessId + employeeId + payrollUpdateId).
   - no crea duplicado para ese triplete
   - actualiza amount si ya existe

4. En manual-concepts/pending no hay bloqueo de duplicados exactos.

5. Al calcular:
   - cambia estado de planilla a "Procesado"
   - vincula incidencias pendientes a esa planilla

6. Al cambiar estado de planilla a estado final ("Cerrado" o "Pagado"), incidencias pendientes asignadas pasan a Assigned.

## 6) Estructuras de datos

### 6.1 Entidad PayrollProcess
Campos:
- id
- payrollStatusId
- processTypeId
- name
- year
- month
- startDate
- payDate (nullable)
- payrollProcessDetails (navegacion)
- payrollStatus (navegacion)

Observaciones:
- No existe isOpen en el contrato actual.
- No existe statusName en la entidad. El frontend debe resolverlo desde catalogo/relacion.

### 6.2 Entidad/DTO de detalle manual usado por frontend
PayrollManualDetailResponseDto:
- id
- employeeId
- employeeFullName
- conceptName
- payrollTypeName
- amount

Observaciones:
- No retorna payrollUpdateId en este DTO.

### 6.3 Incidencias manuales
ManualConceptIncidentResponseDto:
- id
- employeeId
- employeeFullName
- payrollUpdateId
- conceptName
- payrollTypeName
- amount
- occurrenceDate
- statusName
- payrollProcessId (nullable)

## 7) Autenticacion y permisos

- Todos los controladores de payroll tienen [Authorize].
- No se definieron Roles especificos en estos controladores.
- JWT se toma de cookie current_user.
- Headers recomendados desde frontend:
  - Content-Type: application/json
  - withCredentials: true en axios/fetch para enviar cookie

## 8) Paginacion, filtros y ordenamiento

No existen parametros de paginacion/filtro/orden en los endpoints de planilla manual/calculo actuales.

## 9) Formato de errores

### 9.1 Validacion (400)
ValidationProblemDetails (diccionario por campo), ejemplo:
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Formula": [
      "The payroll concept formula is required for calculated concepts"
    ]
  }
}

### 9.2 NotFound (404)
ProblemDetails, ejemplo:
{
  "title": "Not Found",
  "status": 404,
  "detail": "The requested payroll process was not found"
}

### 9.3 Conflict (409)
Actualmente se devuelve string plano como body, ejemplo:
"The payroll process must be open before it can be calculated"

## 10) GAP list para igualar la UI mostrada

Para cubrir completamente las pantallas de Planillas (listado + alta + edicion + eliminacion) faltan endpoints:

1. GET /api/payroll-processes (con filtros/paginacion)
2. POST /api/payroll-processes
3. GET /api/payroll-processes/{id}
4. PUT/PATCH /api/payroll-processes/{id}
5. DELETE /api/payroll-processes/{id}
6. Opcional: endpoint para empleados elegibles de una planilla con filtros de sucursal/area/cargo

Sin esos endpoints, el frontend puede integrar calculo y detalles manuales, pero no puede gestionar ciclo completo de planillas desde API.
