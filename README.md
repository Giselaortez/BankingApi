# Mi Proyecto de API Bancaria (Banking API)

Este es mi proyecto de API bancaria básica desarrollado con ASP.NET Core. La API está diseñada para gestionar clientes y sus cuentas bancarias, permitiendo operaciones como la creación de clientes y cuentas, la consulta de saldos y la realización de depósitos.

## Características Implementadas


-   **Gestión de Clientes:**
    -   `POST /api/Clients`: Para crear un nuevo cliente.
-   **Gestión de Cuentas:**
    -   `POST /api/Accounts`: Para crear una nueva cuenta y asociarla a un cliente existente.
    -   `GET /api/Accounts/{accountNumber}/balance`: Para consultar el saldo de una cuenta específica utilizando su número de cuenta.
    -   `POST /api/Accounts/{accountNumber}/deposit`: Para realizar un depósito en una cuenta específica.

## Tecnologías Utilizadas

Para el desarrollo de esta API, he utilizado las siguientes tecnologías:

-   **ASP.NET Core (mi versión es .NET 8):** Como el framework principal para construir la API RESTful.
-   **Entity Framework Core (la versión que estoy usando es 8.0.6):** Para manejar la persistencia de datos y la interacción con la base de datos.
-   **SQLite:** Una base de datos ligera que he usado para el desarrollo y las pruebas locales del proyecto.
-   **Swagger/OpenAPI:** Para generar la documentación interactiva de la API, lo cual facilita mucho las pruebas directas desde el navegador.

## Requisitos Previos

Para poder ejecutar y probar este proyecto en su entorno, necesitará tener instalado:

-   **Visual Studio 2022 (recomendado):** Es el IDE que utilicé y donde el proyecto funciona óptimamente.
-   **SDK de .NET Core (mi versión es .NET 8 SDK):** Necesario para compilar y ejecutar aplicaciones .NET Core.
-   **Git:** Para clonar el repositorio desde GitHub.

## Configuración y Ejecución del Entorno de Desarrollo

Siga estos pasos para configurar y ejecutar el proyecto:

1.  **Clonar el repositorio:**
    Abra una terminal o Git Bash y ejecute:
    ```bash
    git clone [https://github.com/Giselaortez/BankingApi.git](https://github.com/Giselaortez/BankingApi.git)
    ```
2.  **Navegar al directorio del proyecto:**
    Diríjase a la carpeta donde se encuentra el archivo de la solución (`BankingApi.sln`).
    ```bash
    cd BankingApi # O la ruta específica si la solución no está en la raíz del clon
    ```
3.  **Restaurar paquetes NuGet:**
    Puede ejecutar `dotnet restore` en la terminal, aunque Visual Studio suele hacerlo automáticamente al abrir la solución.
    ```bash
    dotnet restore
    ```
4.  **Abrir en Visual Studio:**
    Abra el archivo `BankingApi.sln` con Visual Studio 2022.

### Ejecutar el Proyecto

1.  **Establecer Proyecto de Inicio:** En el "Explorador de Soluciones" de Visual Studio, haga clic derecho en el proyecto `BankingApi` y seleccione la opción "Establecer como proyecto de inicio".
2.  **Iniciar Depuración:** Presione `F5` (o el botón verde de "Reproducir") en Visual Studio. Esto iniciará la API usando IIS Express y automáticamente abrirá la interfaz de Swagger UI en su navegador predeterminado.
3.  **URL de Swagger UI:** La API estará accesible en `https://localhost:7193/swagger` (el puerto puede variar dependiendo de su configuración, pero Swagger UI le mostrará la URL correcta).

### Ejecutar las Migraciones de la Base de Datos

Si es la primera vez que ejecuta el proyecto o si he realizado cambios en los modelos de datos que requieren actualizar la base de datos (SQLite en este caso):

1.  Abra la "Consola del Administrador de Paquetes" en Visual Studio (accesible desde el menú: Herramientas -> Administrador de paquetes NuGet -> Consola del Administrador de Paquetes).
2.  Asegúrese de que "Proyecto predeterminado" esté configurado en `BankingApi` (este es el proyecto que contiene el `DbContext`).
3.  Ejecute el comando para aplicar las migraciones y crear/actualizar la base de datos:
    ```powershell
    Update-Database
    ```

### Ejecutar las Pruebas

Para ejecutar las pruebas unitarias que he incluido en el proyecto (`BankingApi.Tests`):

1.  Abra el "Explorador de Pruebas" en Visual Studio (menú: Pruebas -> Explorador de Pruebas).
2.  Haga clic en el botón "Ejecutar todas las pruebas" (el ícono de reproducción verde que se encuentra en la parte superior del Explorador de Pruebas).

## Solución de Problemas Comunes y Notas de Depuración

Durante el desarrollo, me encontré con algunos errores comunes que he logrado resolver. Los incluyo aquí para facilitar la depuración si le surgen los mismos:

### 1. Error 500 "A possible object cycle was detected" (Ciclo de referencia en JSON)

* **Síntoma:** Al intentar obtener datos de clientes o cuentas que tienen referencias mutuas (ej., un cliente con sus cuentas y cada cuenta referenciando de nuevo a su cliente), la API devuelve un `500 Internal Server Error` con un mensaje indicando un "object cycle" (ciclo de objeto).
* **Causa:** Este error ocurre cuando el serializador JSON (System.Text.Json) se encuentra con referencias circulares entre los objetos del modelo y no sabe cómo serializarlas sin caer en un bucle infinito.
* **Solución:** He configurado el serializador JSON para ignorar estos ciclos. Verifique que mi archivo `Program.cs` contenga la siguiente configuración al agregar los controladores:
    ```csharp
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
    ```
   

### 2. Errores 400 "The Accounts field is required." o "The Client field is required."

* **Síntoma:** Al intentar crear un nuevo cliente (`POST /api/Clients`), recibía un `400 Bad Request` con el mensaje "The Accounts field is required." De manera similar, al crear una cuenta, podía recibir "The Client field is required."
* **Causa:** Esto indicaba que las propiedades de navegación (`Accounts` en `Client`, o `Client` en `Account`) estaban marcadas como requeridas en el modelo o DTO, cuando no deberían serlo al crear la entidad principal.
* **Solución:**
    * **Para "The Accounts field is required.":** Eliminé el atributo `[Required]` de la propiedad `Accounts` en mi modelo `Client` (o el DTO que utilizo para la creación). Para el `Request body` al crear un cliente, ahora es correcto enviar un arreglo vacío para la propiedad `accounts`:
        ```json
        {
          "id": 0,
          "name": "Nuevo Cliente de Prueba",
          "dateOfBirth": "1988-03-20T00:00:00Z",
          "gender": "Female",
          "income": 80000.00,
          "accounts": []  // Asegúrese de incluir esto, incluso si está vacío
        }
        ```
       
    * **Para "The Client field is required.":** Eliminé el atributo `[Required]` de la propiedad `Client` en mi modelo `Account` (o el DTO de creación de cuenta). La relación se establece a través del `ClientId`.

### 3. Errores de Formato de Fecha/Hora (ej. "'-' is an invalid end of a number.")

* **Síntoma:** Al intentar crear un cliente, recibía un `400 Bad Request` con un error de validación sobre `dateOfBirth`, indicando un formato incorrecto o un carácter inesperado.
* **Causa:** El formato de fecha y hora que enviaba en el `Request body` en Swagger UI no coincidía con el formato esperado por el modelo en C#.
* **Solución:** Siempre me aseguro de enviar las fechas en un formato ISO 8601 válido y completo, como por ejemplo: `"1988-03-20T00:00:00Z"`. La `Z` al final es importante para indicar que es una hora UTC.


---

Espero que este `README.md` detallado sea de gran ayuda para su revisión 
