# DWOpiniones – Sistema ETL para Análisis de Opiniones de Clientes

## 1. Descripción general

DWOpiniones es un proyecto ETL desarrollado como **Worker Service en .NET**, cuyo propósito es extraer, transformar y cargar datos de opiniones de clientes provenientes de distintas fuentes hacia un **Data Warehouse en PostgreSQL**, utilizando un **modelo dimensional orientado a análisis**.

El sistema integra información desde encuestas, reseñas web y redes sociales, permitiendo análisis temporales, comparativos y de satisfacción del cliente.

---

## 2. Arquitectura del proyecto

El proyecto está estructurado por capas funcionales, separando responsabilidades para facilitar el mantenimiento, la escalabilidad y la reutilización del código.


Cada extractor se encarga de una fuente específica y cada loader gestiona la persistencia de datos en tablas concretas.

---

## 3. Fuentes de datos

### 3.1 Archivos CSV de staging

| Archivo | Tabla destino |
|------|-------------|
| surveys_part1.csv | staging.surveys_part1 |
| web_reviews.csv | staging.web_reviews |
| social_comments.csv | staging.social_comments |

Estas tablas almacenan los datos crudos, sin transformación dimensional.

---

## 4. Esquemas de base de datos

La base de datos está organizada en **tres esquemas**, siguiendo buenas prácticas de Data Warehousing:

### 4.1 Esquema `staging`
Contiene los datos originales cargados desde las fuentes externas.

- staging.surveys_part1  
- staging.web_reviews  
- staging.social_comments  

### 4.2 Esquema `dimension`
Almacena información descriptiva normalizada.

- dim_clientes  
- dim_products  
- dim_fuente_datos  
- dim_time  
- dim_clasificacion_opinion  
- dim_red_social  

### 4.3 Esquema `fact`
Contiene los hechos analíticos.

- fact_opiniones  

---

## 5. Uso de Tablespaces

El proyecto utiliza **tablespaces personalizados** para una mejor organización física y rendimiento del Data Warehouse.

### 5.1 `ts_primary`
Se utiliza para:
- Tablas de dimensiones
- Tabla de hechos
- Índices principales

Justificación:
- Centraliza los datos analíticos.
- Permite optimizar almacenamiento y rendimiento.
- Facilita administración y escalabilidad del sistema.

### 5.2 `ts_index`
Se utiliza para:
- Índices secundarios y únicos

Justificación:
- Reduce la contención de E/S.
- Mejora el rendimiento de consultas analíticas.
- Permite separar carga de datos e índices.

---

## 6. Dimensiones

### 6.1 Dimensión Clientes (`dim_clientes`)
Almacena información descriptiva de los clientes y utiliza una **clave sustituta** (`cliente_key`) y una **clave natural** (`nk_cliente`).

### 6.2 Dimensión Productos (`dim_products`)
Describe los productos ofrecidos, incluyendo nombre y categoría.

### 6.3 Dimensión Fuente de Datos (`dim_fuente_datos`)
Registra el origen de la información (CSV, Web, Red Social), permitiendo trazabilidad del dato.

### 6.4 Dimensión Tiempo (`dim_time`)
Se genera automáticamente desde el programa.
Incluye:
- Clave YYYYMMDD
- Año, trimestre, mes
- Nombre del mes y día
- Semana del año
- Indicador de fin de semana

Esta dimensión permite análisis temporales detallados.

### 6.5 Dimensión Clasificación de Opinión
Contiene los valores:
- Positiva
- Neutra
- Negativa

Se genera de forma controlada para garantizar consistencia.

### 6.6 Dimensión Red Social
Relaciona redes sociales con su fuente de datos, manteniendo integridad referencial.

---

## 7. Tabla de Hechos (`fact_opiniones`)

La tabla de hechos consolida todas las opiniones y se relaciona con las dimensiones mediante claves foráneas.

Contiene métricas como:
- Puntuación
- Longitud del texto
- Indicador de texto presente

Y atributos descriptivos como:
- Comentario
- Origen
- Identificador del origen

---

## 8. Flujo ETL

1. Extracción de datos desde CSV y generadores internos.
2. Carga de datos crudos en staging.
3. Inserción o actualización de dimensiones.
4. Generación de dimensión tiempo.
5. Preparación para la carga de la tabla de hechos.

El proceso es **repetible, controlado e idempotente** para evitar duplicados.

---

## 9. Tecnologías utilizadas

- .NET 8 – Worker Service
- PostgreSQL
- Npgsql
- CsvHelper
- Dependency Injection
- Logging con ILogger

---

## 10. Ejecución del proyecto

1. Configurar la cadena de conexión en `appsettings.json`.
2. Ejecutar el proyecto con: `dotnet run`
3. El Worker ejecuta automáticamente todos los extractores y generadores registrados.

---

## 11. Objetivo del sistema

El Data Warehouse permite:
- Analizar satisfacción del cliente.
- Evaluar productos y servicios.
- Comparar fuentes de datos.
- Realizar análisis temporal.
- Facilitar reportes en herramientas de BI.

---

## 12. Autora

Yeliana Díaz  
Desarrollo de Software – ITLA  
Proyecto académico de Data Warehouse y ETL

