using System;
using System.IO;
using System.Text;

class Program
{
    // Arreglo que almacena hasta 1000 expedientes.
    static string[] expedientes = new string[1000];
    static int cantidad = 0;
    const int MAX_VACANTES = 2 ;

    static string archivo = "expedientes.txt";
    static string archivoContador = "contador_alumnos.txt";
    static int ultimoCodigo = 0;

    static void Main()
{
    Console.OutputEncoding = Encoding.UTF8;
    CargarExpedientes();
    if (!AsignarCodigosAnteriores())
{
    Console.WriteLine(
        "No se completó la asignación de códigos. Revise los archivos."
    );
    return;
    }

    if (!NormalizarEstadosMatricula())
{
    Console.WriteLine(
        "No se pudieron actualizar los estados de matrícula."
    );
    return;

}

    int opcion;

    do
    {
        Console.WriteLine("\n===== SISTEMA DE MATRÍCULA =====");
        Console.WriteLine("1. Registro de alumno");
        Console.WriteLine("2. Búsqueda de alumno");
        Console.WriteLine("3. Orden de alumnos alfabéticamente");
        Console.WriteLine("4. Validación de alumno");
        Console.WriteLine("5. Reporte de alumnos");
        Console.WriteLine("6. Confirmar matrícula");
        Console.WriteLine("7. Cancelar matrícula");
        Console.WriteLine("8. Buscar alumnos por nivel educativo");
        Console.WriteLine("9. Salir");


        opcion = LeerEntero("Seleccione una opción: ");

        switch (opcion)
{
    case 1:
        RegistrarSolicitud();
        break;

    case 2:
        BuscarExpediente();
        break;

    case 3:
        OrdenarExpedientes();
        break;

    case 4:
        ValidarAlumno();
        break;

    case 5:
        ReporteAlumnos();
        break;

    case 6:
        ConfirmarMatricula();
        break;

    case 7:
        CancelarMatricula();
        break;

    case 8:
    BuscarPorNivel();
    break;

    case 9:
        Console.WriteLine("Programa finalizado.");
        break;

    default:
        Console.WriteLine("Seleccione una opción del 1 al 9.");
        break;
}

   } while (opcion != 9);
}

    // MÓDULO DE ENTRADA DE DATOS

    static string LeerTexto(string mensaje)
    {
        Console.Write(mensaje);

        // Evita introducir el separador usado en el archivo.
       return (Console.ReadLine() ?? "").Trim();
    }

    static int LeerEntero(string mensaje)
    {
        int numero;

        while (true)
        {
            Console.Write(mensaje);

            if (int.TryParse(Console.ReadLine(), out numero))
            {
                return numero;
            }

            Console.WriteLine("Error: ingrese un número entero.");
        }
    }

    static bool LeerLogico(string mensaje)
    {
        while (true)
        {
            string respuesta = LeerTexto(
                mensaje + " (Verdadero/Falso): "
            ).ToLowerInvariant();

            if (respuesta == "verdadero")
            {
                return true;
            }

            if (respuesta == "falso")
            {
                return false;
            }

            Console.WriteLine("Escriba Verdadero o Falso.");
        }
    }

    // MÓDULO DE VALIDACIÓN

    static bool ValidarDatos(
        string dniEstudiante,
        string dniApoderado,
        string nombre,
        int grado,
        int turno)
    {
        bool datosCorrectos = true;

       if (!ValidarDni(dniEstudiante))
{
    Console.WriteLine(
        "DNI del estudiante inválido: debe tener 8 dígitos " +
        "y no comenzar con 0."
    );
    datosCorrectos = false;
}

if (!ValidarDni(dniApoderado))
{
    Console.WriteLine(
        "DNI del apoderado inválido: debe tener 8 dígitos " +
        "y no comenzar con 0."
    );
    datosCorrectos = false;
}
       if (string.IsNullOrWhiteSpace(nombre))
{
    Console.WriteLine("El nombre no puede estar vacío.");
    datosCorrectos = false;
}
else
{
    foreach (char letra in nombre)
    {
        if (!char.IsLetter(letra) && letra != ' ')
        {
            Console.WriteLine(
                "El nombre solo puede contener letras y espacios."
            );
            datosCorrectos = false;
            break;
        }
    }
}

        if (grado < 1 || grado > 6)
            datosCorrectos = false;
    if (turno != 1 && turno != 2)
            datosCorrectos = false;

        return datosCorrectos;
    }

    
static bool VerificarVacante(int nivel, int grado, int turno)
{
    int ocupadas = 0;

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos.Length < 16)
            continue;

        bool mismoNivel = datos[11] == nivel.ToString();
        bool mismoGrado = datos[3] == grado.ToString();
        bool mismoTurno = datos[4] == turno.ToString();

        bool matriculaActiva =
            datos[15] == "PENDIENTE" ||
            datos[15] == "CONFIRMADA";

        if (mismoNivel && mismoGrado &&
            mismoTurno && matriculaActiva)
        {
            ocupadas++;
        }
    }

    Console.WriteLine("\n===== DISPONIBILIDAD DE VACANTES =====");
    Console.WriteLine("Capacidad: " + MAX_VACANTES);
    Console.WriteLine("Vacantes ocupadas: " + ocupadas);
    Console.WriteLine("Vacantes disponibles: " +
        (MAX_VACANTES - ocupadas));

    if (ocupadas >= MAX_VACANTES)
    {
        Console.WriteLine("NO HAY VACANTES DISPONIBLES.");
        return false;
    }

    Console.WriteLine("Vacante disponible.");
    return true;
}


    static string DeterminarEstado(
        bool tieneDocumentos,
        bool tieneSalud,
        bool tienePsicologia)
    {
        if (tieneDocumentos && tieneSalud && tienePsicologia)
        {
            return "REGISTRADO";
        }

        return "OBSERVADO";
    }

    // MÓDULO DE REGISTRO

    static void RegistrarSolicitud()
    {
        if (cantidad >= expedientes.Length)
        {
            Console.WriteLine("Se alcanzó el límite de expedientes.");
            return;
        }

        Console.WriteLine("\n===== REGISTRAR SOLICITUD =====");

        string dniEstudiante = LeerTexto(
            "Ingrese DNI del estudiante: "
        );
        if (ExisteDni(dniEstudiante))
{
    Console.WriteLine(
        "Ya existe un alumno registrado con ese DNI."
    );
    return;
}

        string dniApoderado = LeerTexto(
            "Ingrese DNI del apoderado: "
        );

        string nombre = LeerTexto(
            "Ingrese nombres completos del estudiante: "
        );
string discapacidad;

do
{
    discapacidad = LeerTexto(
        "¿El alumno presenta discapacidad? (S/N): "
    ).ToUpperInvariant();

    if (discapacidad != "S" && discapacidad != "N")
    {
        Console.WriteLine(
            "Respuesta inválida. Ingrese S para Sí o N para No."
        );
    }

} while (discapacidad != "S" && discapacidad != "N");
       int nivel;

do
{
    Console.WriteLine("\nSeleccione el nivel educativo:");
    Console.WriteLine("1. Inicial");
    Console.WriteLine("2. Primaria");
    Console.WriteLine("3. Secundaria");

    nivel = LeerEntero("Nivel: ");

    if (nivel < 1 || nivel > 3)
    {
        Console.WriteLine("Seleccione una opción del 1 al 3.");
    }

} while (nivel < 1 || nivel > 3);

int minimo;
int maximo;
string mensajeGrado;

if (nivel == 1)
{
    minimo = 3;
    maximo = 5;
    mensajeGrado = "Ingrese edad de inicial (3 a 5 años): ";
}
else if (nivel == 2)
{
    minimo = 1;
    maximo = 6;
    mensajeGrado = "Ingrese grado de primaria (1 a 6): ";
}
else
{
    minimo = 1;
    maximo = 5;
    mensajeGrado = "Ingrese grado de secundaria (1 a 5): ";
}

int grado;

do
{
    grado = LeerEntero(mensajeGrado);

    if (grado < minimo || grado > maximo)
    {
        Console.WriteLine(
            "Ingrese un valor entre " + minimo + " y " + maximo + "."
        );
    }

} while (grado < minimo || grado > maximo);
        string seccion = "No aplica";

if (nivel == 2 || nivel == 3)
{
    do
    {
        seccion = LeerTexto(
            "Ingrese sección (A o B): "
        ).ToUpperInvariant();

        if (seccion != "A" && seccion != "B")
        {
            Console.WriteLine(
                "Sección inválida. Ingrese A o B."
            );
        }

    } while (seccion != "A" && seccion != "B");
}

        
int turno;

do
{
    Console.WriteLine("\nSeleccione el turno:");
    Console.WriteLine("1. Mañana");
    Console.WriteLine("2. Tarde");

    turno = LeerEntero("Turno: ");

    if (turno != 1 && turno != 2)
    {
        Console.WriteLine("Opción inválida. Seleccione 1 o 2.");
    }

} while (turno != 1 && turno != 2);


        string telefono;
bool telefonoValido;

do
{
    telefono = LeerTexto("Ingrese teléfono de contacto: ");
    telefonoValido = telefono.Length == 9;

    foreach (char digito in telefono)
    {
        if (digito < '0' || digito > '9')
        {
            telefonoValido = false;
            break;
        }
    }

    if (!telefonoValido)
    {
        Console.WriteLine(
            "Teléfono inválido: debe contener exactamente 9 dígitos."
        );
    }

} while (!telefonoValido);
        

        string colegioProcedencia = LeerTexto(
            "Ingrese colegio de procedencia: "
            
        );
        colegioProcedencia = colegioProcedencia.Replace("|", " ");

        bool tieneDocumentos = LeerLogico(
            "¿Presenta los documentos obligatorios?"
        );

        bool tieneSalud = LeerLogico(
            "¿Presenta certificado de salud?"
        );

        bool tienePsicologia = LeerLogico(
            "¿Presenta certificado de examen psicológico?"
        );

        bool datosCorrectos = ValidarDatos(
            dniEstudiante, dniApoderado, nombre, grado, turno
        );

        Console.WriteLine("\n===== RESULTADO DE MATRÍCULA =====");

        if (!datosCorrectos)
        {
            Console.WriteLine("Solicitud rechazada.");
            Console.WriteLine(
                "Existen datos incorrectos o incompletos."
            );
            return;
        }

        bool hayVacante = VerificarVacante(nivel, grado, turno);
        if (!hayVacante)
        {
            Console.WriteLine(
                "No hay vacantes disponibles para el grado " +
                "y turno seleccionado."
            );
            return;
        }

        Console.WriteLine("Vacante disponible.");
        Console.WriteLine("Se reserva temporalmente por 7 días.");

        string estado = DeterminarEstado(
            tieneDocumentos, tieneSalud, tienePsicologia
            
        );
        
DateTime fechaRegistro = DateTime.Today;
DateTime fechaReserva = fechaRegistro.AddDays(7);

string fechaSubsanacion = estado == "OBSERVADO"
    ? fechaRegistro.AddDays(7).ToString("yyyy-MM-dd")
    : "NO APLICA";


        // Los campos se separan con | para guardarlos en texto.
        string codigoAlumno = GenerarCodigoAlumno();

if (codigoAlumno == "")
{
    Console.WriteLine(
        "No se pudo generar el código. La solicitud no se guardó."
    );
    return;
}
        string registro = string.Join("|",
            dniEstudiante,
            dniApoderado,
            nombre,
            grado.ToString(),
            turno.ToString(),
            telefono,
            colegioProcedencia,
            tieneDocumentos.ToString(),
            tieneSalud.ToString(),
            tienePsicologia.ToString(),
            estado,
            nivel.ToString(),
            codigoAlumno,
            seccion,
            discapacidad,
            "PENDIENTE"
        );

        expedientes[cantidad] = registro;
        cantidad++;

        // Si falla el guardado, se revierte el registro en memoria.
        if (!GuardarExpedientes())
        {
            cantidad--;
            expedientes[cantidad] = "";
            Console.WriteLine(
                "La solicitud no se guardó. Intente nuevamente."
            );
            return;
        }

        MostrarExpediente(registro);

        if (estado == "REGISTRADO")
        {
            Console.WriteLine("Matrícula registrada correctamente.");
            Console.WriteLine("Vacante reservada.");
            Console.WriteLine("Generando constancia de matrícula...");
        }
        else
        {
            Console.WriteLine("Expediente observado.");
            Console.WriteLine(
                "Debe subsanar los documentos faltantes."
            );
            Console.WriteLine("Plazo para subsanar: 7 días.");
        }
    }

    // MÓDULO DE CONSULTA

    static void MostrarExpediente(string registro)
    {
        string[] datos = registro.Split('|');

        Console.WriteLine("------------------------------");
        if (datos.Length >= 13)
{
    Console.WriteLine("Código del alumno: " + datos[12]);
}
else
{
    Console.WriteLine("Código del alumno: Pendiente de asignación");
}
        Console.WriteLine("Estudiante: " + datos[2]);
        Console.WriteLine("DNI: " + datos[0]);
        Console.WriteLine("DNI del apoderado: " + datos[1]);
        string nivelGuardado = datos.Length >= 12 ? datos[11] : "";
        if (nivelGuardado == "1")
{
    Console.WriteLine("Nivel: Inicial");
    Console.WriteLine("Edad de inicial: " + datos[3] + " años");
}
else if (nivelGuardado == "2")
{
    Console.WriteLine("Nivel: Primaria");
    Console.WriteLine("Grado: " + datos[3]);
}
else if (nivelGuardado == "3")
{
    Console.WriteLine("Nivel: Secundaria");
    Console.WriteLine("Grado: " + datos[3]);
}
else
{
    Console.WriteLine("Nivel: No especificado");
    Console.WriteLine("Grado: " + datos[3]);
}

        string turno = datos[4] == "1" ? "Mañana" : "Tarde";

        Console.WriteLine("Turno: " + turno);
        if (datos.Length >= 15)
{
    Console.WriteLine("Sección: " + datos[13]);

    string discapacidadTexto = datos[14] == "S" ? "Sí" : "No";

    Console.WriteLine(
        "Presenta discapacidad: " + discapacidadTexto
    );
}
else
{
    Console.WriteLine("Sección: No registrada");
    Console.WriteLine("Discapacidad: No registrada");
}
        Console.WriteLine("Teléfono: " + datos[5]);
        Console.WriteLine("Colegio de procedencia: " + datos[6]);
        Console.WriteLine("Estado del expediente: " + datos[10]);
       if (datos.Length >= 16)
{
    Console.WriteLine(
        "Estado de matrícula: " + datos[15]
    );
}
else
{
    Console.WriteLine(
        "Estado de matrícula: PENDIENTE DE ACTUALIZACIÓN"
    );

}

if (datos.Length >= 19)
{
    Console.WriteLine("Fecha de registro: " + datos[16]);
    Console.WriteLine("Vencimiento de reserva: " + datos[17]);
    Console.WriteLine("Límite de subsanación: " + datos[18]);

    if (DateTime.TryParseExact(
        datos[17], "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None,
        out DateTime vencimiento))
    {
        if (datos[15] == "PENDIENTE" &&
            DateTime.Today > vencimiento)
        {
            Console.WriteLine("AVISO: La reserva está vencida.");
        }
    }
}

    }

    
static void BuscarExpediente()
{
    Console.WriteLine("\n===== BÚSQUEDA DE EXPEDIENTE =====");
    Console.WriteLine("1. Buscar por DNI");
    Console.WriteLine("2. Buscar por código de expediente");
    Console.WriteLine("0. Regresar");

    int opcionBusqueda = LeerEntero("Seleccione una opción: ");

    if (opcionBusqueda == 0)
        return;

    if (opcionBusqueda != 1 && opcionBusqueda != 2)
    {
        Console.WriteLine("Opción inválida.");
        return;
    }

    string valor = LeerTexto(
        opcionBusqueda == 1
        ? "Ingrese DNI del estudiante: "
        : "Ingrese código de expediente (ejemplo ALU001): "
    ).ToUpperInvariant();

    bool encontrado = false;

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        bool coincide = opcionBusqueda == 1
            ? datos[0] == valor
            : datos.Length >= 13 &&
              datos[12].Equals(valor, StringComparison.OrdinalIgnoreCase);

        if (coincide)
        {
            MostrarExpediente(expedientes[i]);
            encontrado = true;
            break;
        }
    }

    if (!encontrado)
    {
        Console.WriteLine("No se encontró el expediente.");
    }
}


    static void ListarExpedientes()
    {
        if (cantidad == 0)
        {
            Console.WriteLine("No hay expedientes registrados.");
            return;
        }

        for (int i = 0; i < cantidad; i++)
        {
            MostrarExpediente(expedientes[i]);
        }
    }

   static void OrdenarPorBurbuja()
{
    for (int i = 0; i < cantidad - 1; i++)
    {
        for (int j = 0; j < cantidad - i - 1; j++)
        {
            string nombre1 = expedientes[j].Split('|')[2];
            string nombre2 = expedientes[j + 1].Split('|')[2];

            // Compara alfabéticamente, ignorando mayúsculas.
            int comparacion = string.Compare(
                nombre1,
                nombre2,
                StringComparison.CurrentCultureIgnoreCase
            );

            if (comparacion > 0)
            {
                // Intercambia el expediente completo.
                string auxiliar = expedientes[j];
                expedientes[j] = expedientes[j + 1];
                expedientes[j + 1] = auxiliar;
            }
         }
    }
    }
    
static void OrdenarPorCodigo()
{
    for (int i = 0; i < cantidad - 1; i++)
    {
        for (int j = 0; j < cantidad - i - 1; j++)
        {
            string[] datos1 = expedientes[j].Split('|');
            string[] datos2 = expedientes[j + 1].Split('|');

            string codigo1 = datos1.Length >= 13 ? datos1[12] : "";
            string codigo2 = datos2.Length >= 13 ? datos2[12] : "";

            int comparacion = string.Compare(
                codigo1,
                codigo2,
                StringComparison.OrdinalIgnoreCase
            );

            if (comparacion > 0)
            {
                string auxiliar = expedientes[j];
                expedientes[j] = expedientes[j + 1];
                expedientes[j + 1] = auxiliar;
            }
        }
    }
}




static void OrdenarExpedientes()
{
    if (cantidad == 0)
    {
        Console.WriteLine("No hay alumnos para ordenar.");
        return;
    }

    Console.WriteLine("\n===== ORDENAR EXPEDIENTES =====");
    Console.WriteLine("1. Ordenar alfabéticamente");
    Console.WriteLine("2. Ordenar por código de expediente");
    Console.WriteLine("0. Regresar");

    int opcionOrden = LeerEntero("Seleccione una opción: ");

    if (opcionOrden == 0)
        return;

    if (opcionOrden == 1)
    {
        OrdenarPorBurbuja();
    }
    else if (opcionOrden == 2)
    {
        OrdenarPorCodigo();
    }
    else
    {
        Console.WriteLine("Opción inválida.");
        return;
    }

    if (!GuardarExpedientes())
    {
        Console.WriteLine("No se pudo guardar el orden.");
        return;
    }

    Console.WriteLine("\n===== EXPEDIENTES ORDENADOS =====");

    for (int i = 0; i < cantidad; i++)
    {
        Console.WriteLine("\nAlumno N° " + (i + 1));
        MostrarExpediente(expedientes[i]);
    }

    Console.WriteLine("\nTotal de alumnos: " + cantidad);
}


    // MÓDULO DE ARCHIVOS

    static bool GuardarExpedientes()
    {
        try
        {
            string[] registros = new string[cantidad];

            Array.Copy(expedientes, registros, cantidad);
            File.WriteAllLines(archivo, registros, Encoding.UTF8);

            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error de archivo: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine("Error de permisos: " + ex.Message);
        }

        return false;
    }

    static void CargarExpedientes() 
    {
        try
        {
            if (!File.Exists(archivo))
                return;

            string[] registros = File.ReadAllLines(
                archivo, Encoding.UTF8
            );

            foreach (string registro in registros)
            {
                string[] datos = registro.Split('|');

                // Comprueba la estructura antes de cargar el registro.
               
                    if ((datos.Length != 11 && datos.Length != 12 &&
                      datos.Length != 13 && datos.Length != 15 &&
                      datos.Length != 16 && datos.Length != 19) ||

                    !int.TryParse(datos[3], out int grado) ||
                    grado < 1 || grado > 6 ||
                    (datos[4] != "1" && datos[4] != "2") ||
                    (datos[10] != "REGISTRADO" &&
                    datos[10] != "OBSERVADO"))
                {
                    Console.WriteLine(
                        "Se omitió un registro con formato incorrecto."
                    );
                    continue;
                }
                // Valida el nivel y su rango en los registros nuevos.
if (datos.Length >= 12 && datos[11] != "")
{
    string nivelGuardado = datos[11];

    bool nivelYGradoValidos =
        (nivelGuardado == "1" && grado >= 3 && grado <= 5) ||
        (nivelGuardado == "2" && grado >= 1 && grado <= 6) ||
        (nivelGuardado == "3" && grado >= 1 && grado <= 5);

    if (!nivelYGradoValidos)
    {
        Console.WriteLine(
            "Se omitió un registro con nivel o grado incorrecto."
        );
        continue;
    }
}

                if (cantidad >= expedientes.Length)
                {
                    Console.WriteLine(
                        "Se alcanzó el límite de carga de expedientes."
                    );
                    break;
                }

                expedientes[cantidad] = registro;
                cantidad++;
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error al cargar: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine("Error de permisos: " + ex.Message);
        }
    }
    static void ValidarAlumno()
{
    string documento = LeerTexto(
        "\nIngrese dni del alumno: "
    );

    bool encontrado = false;

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos[0].Equals(
            documento, StringComparison.OrdinalIgnoreCase))
        {
            encontrado = true;

            Console.WriteLine("\n===== VALIDACIÓN DEL ALUMNO =====");
            Console.WriteLine("Alumno: " + datos[2]);

            bool numerosValidos =
                int.TryParse(datos[3], out int grado) &
                int.TryParse(datos[4], out int turno);

            bool datosCorrectos = numerosValidos &&
                ValidarDatos(
                    datos[0], datos[1], datos[2], grado, turno
                );
if (datos.Length >= 12 && datos[11] != "")
{
    string nivelGuardado = datos[11];

    bool nivelYGradoValidos =
        (nivelGuardado == "1" && grado >= 3 && grado <= 5) ||
        (nivelGuardado == "2" && grado >= 1 && grado <= 6) ||
        (nivelGuardado == "3" && grado >= 1 && grado <= 5);

    if (!nivelYGradoValidos)
    {
        Console.WriteLine("El nivel y el grado no corresponden.");
        datosCorrectos = false;
    }
}
else
{
    Console.WriteLine(
        "Expediente antiguo: no tiene nivel educativo guardado."
    );
}
if (!ValidarTelefono(datos[5]))
{
    Console.WriteLine(
        "Teléfono inválido: debe contener exactamente 9 dígitos."
    );
    datosCorrectos = false;
}
            Console.WriteLine(
                "Datos personales: " +
                (datosCorrectos ? "VÁLIDOS" : "INVÁLIDOS")
            );

            bool documentosValidos =
                bool.TryParse(datos[7], out bool documentos) &
                bool.TryParse(datos[8], out bool salud) &
                bool.TryParse(datos[9], out bool psicologia);

            if (!documentosValidos)
            {
                Console.WriteLine(
                    "Los indicadores de documentos tienen un formato inválido."
                );
                continue;
            }

            Console.WriteLine(
                "Documentos obligatorios: " +
                (documentos ? "Presentados" : "Pendientes")
            );

            Console.WriteLine(
                "Certificado de salud: " +
                (salud ? "Presentado" : "Pendiente")
            );

            Console.WriteLine(
                "Certificado psicológico: " +
                (psicologia ? "Presentado" : "Pendiente")
            );

            string estadoEsperado = DeterminarEstado(
                documentos, salud, psicologia
            );

            Console.WriteLine("Estado guardado: " + datos[10]);
            Console.WriteLine(
                "Estado coherente con los documentos: " +
                (datos[10] == estadoEsperado ? "SÍ" : "NO")
            );
        }
    }

    if (!encontrado)
    {
        Console.WriteLine("No se encontró al alumno.");
    }
}


static void ReporteAlumnos()
{
    if (cantidad == 0)
    {
        Console.WriteLine("No hay alumnos registrados.");
        return;
    }

    OrdenarPorBurbuja();

    int registrados = 0;
    int observados = 0;
    int confirmados = 0;
    int pendientes = 0;

    Console.WriteLine("\n===== REPORTE GENERAL DE ALUMNOS =====");

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        Console.WriteLine("\nAlumno N° " + (i + 1));
        MostrarExpediente(expedientes[i]);

        if (datos[10] == "REGISTRADO")
            registrados++;
        else if (datos[10] == "OBSERVADO")
            observados++;

        if (datos.Length >= 16)
        {
            if (datos[15] == "CONFIRMADA")
                confirmados++;
            else if (datos[15] == "PENDIENTE")
                pendientes++;
        }
    }

    Console.WriteLine("\n===== RESUMEN GENERAL =====");
    Console.WriteLine("Total de alumnos: " + cantidad);
    Console.WriteLine("Expedientes registrados: " + registrados);
    Console.WriteLine("Expedientes observados: " + observados);
    Console.WriteLine("Matrículas confirmadas: " + confirmados);
    Console.WriteLine("Matrículas pendientes: " + pendientes);
}

static bool AsignarCodigosAnteriores()
{
    bool hayPendientes = false;

    for (int i = 0; i < cantidad; i++)
    {
        if (expedientes[i].Split('|').Length < 13)
        {
            hayPendientes = true;
            break;
        }
    }

    if (!hayPendientes)
        return true;

    // Respalda el archivo antes de actualizarlo.
    try
    {
        if (File.Exists(archivo))
        {
            string respaldo = archivo + "." +
                DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bak";

            File.Copy(archivo, respaldo);
        }
    }
    catch (IOException ex)
    {
        Console.WriteLine("No se pudo crear el respaldo: " + ex.Message);
        return false;
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine("No hay permiso para el respaldo: " + ex.Message);
        return false;
    }

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos.Length >= 13)
            continue;

        string codigo = GenerarCodigoAlumno();

        if (codigo == "")
            return false;

        // Los registros de 11 campos no tenían nivel educativo.
        if (datos.Length == 11)
        {
            expedientes[i] += "|";
        }

        expedientes[i] += "|" + codigo;
    }

    return GuardarExpedientes();
}
static string GenerarCodigoAlumno()
{
    try
    {
        // Recupera el último número utilizado.
        if (File.Exists(archivoContador))
        {
            string contenido = File.ReadAllText(
                archivoContador
            ).Trim();

            if (!int.TryParse(contenido, out int numeroGuardado)
                || numeroGuardado < 0)
            {
                Console.WriteLine(
                    "El contador de alumnos tiene un formato incorrecto."
                );
                return "";
            }

            ultimoCodigo = Math.Max(ultimoCodigo, numeroGuardado);
        }

        // Comprueba también los códigos de alumnos cargados.
        for (int i = 0; i < cantidad; i++)
        {
            string[] datos = expedientes[i].Split('|');

            if (datos.Length >= 13 &&
                datos[12].StartsWith("ALU") &&
                int.TryParse(datos[12].Substring(3), out int numero))
            {
                ultimoCodigo = Math.Max(ultimoCodigo, numero);
            }
        }

        int siguiente = checked(ultimoCodigo + 1);

        // Guarda el número antes de utilizar el código.
        File.WriteAllText(
            archivoContador,
            siguiente.ToString()
        );

        ultimoCodigo = siguiente;

        return "ALU" + siguiente.ToString("D3");
    }
    catch (IOException ex)
    {
        Console.WriteLine("No se pudo guardar el contador: " + ex.Message);
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine("No hay permiso para guardar: " + ex.Message);
    }
    catch (OverflowException)
    {
        Console.WriteLine("Se alcanzó el límite de códigos.");
    }

    return "";
}
static bool ExisteDni(string dni)
{
    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos[0] == dni)
        {
            return true;
        }
    }

    return false;
}
static bool ValidarTelefono(string telefono)
{
    if (telefono.Length != 9)
    {
        return false;
    }

    foreach (char digito in telefono)
    {
        if (digito < '0' || digito > '9')
        {
            return false;
        }
    }

    return true;
}
static bool ValidarDni(string dni)
{
    if (dni.Length != 8 || dni[0] == '0')
    {
        return false;
    }

    for (int i = 0; i < dni.Length; i++)
    {
        if (dni[i] < '0' || dni[i] > '9')
        {
            return false;
        }
    }

    return true;
}

static void ConfirmarMatricula()
{
    ActualizarEstadoMatricula("CONFIRMADA");
}

static void CancelarMatricula()
{
    ActualizarEstadoMatricula("CANCELADA");
}


static void ActualizarEstadoMatricula(string nuevoEstado)
{
    string dni = LeerTexto("\nIngrese DNI del alumno: ");

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos[0] == dni)
        {
            // Si se cancela, se elimina completamente al alumno.
            if (nuevoEstado == "CANCELADA")
            {
                Console.WriteLine("\n===== CANCELAR MATRÍCULA =====");
                Console.WriteLine("Alumno: " + datos[2]);
                Console.WriteLine("DNI: " + datos[0]);

                string[] respaldo = new string[cantidad];
                Array.Copy(expedientes, respaldo, cantidad);

                // Desplaza los registros para eliminar al alumno.
                for (int j = i; j < cantidad - 1; j++)
                {
                    expedientes[j] = expedientes[j + 1];
                }

                cantidad--;
                expedientes[cantidad] = "";

                // Guarda los cambios en el archivo.
                if (!GuardarExpedientes())
                {
                    cantidad++;
                    Array.Copy(respaldo, expedientes, cantidad);

                    Console.WriteLine(
                        "No se pudo eliminar al alumno. Se restauraron sus datos."
                    );
                    return;
                }

                Console.WriteLine("\nMATRÍCULA CANCELADA CORRECTAMENTE.");
                Console.WriteLine("El alumno fue eliminado completamente.");
                Console.WriteLine("El DNI queda disponible para un nuevo registro.");

                return;
            }

            // Confirmación de matrícula.
            if (datos.Length < 16)
            {
                Console.WriteLine(
                    "El expediente no tiene el formato actualizado."
                );
                return;
            }

            if (datos[15] != "PENDIENTE")
            {
                Console.WriteLine(
                    "La matrícula ya fue procesada. Estado actual: "
                    + datos[15]
                );
                return;
            }

            string registroAnterior = expedientes[i];

            datos[15] = nuevoEstado;
            expedientes[i] = string.Join("|", datos);

            if (!GuardarExpedientes())
            {
                expedientes[i] = registroAnterior;

                Console.WriteLine(
                    "No se pudo actualizar la matrícula."
                );
                return;
            }

            Console.WriteLine("\n===== MATRÍCULA ACTUALIZADA =====");
            Console.WriteLine("Alumno: " + datos[2]);
            Console.WriteLine("DNI: " + datos[0]);
            Console.WriteLine("Nuevo estado: " + nuevoEstado);

            return;
        }
    }

    Console.WriteLine("No se encontró al alumno.");
}



static bool NormalizarEstadosMatricula()
{
    string[] respaldo = new string[cantidad];
    Array.Copy(expedientes, respaldo, cantidad);

    bool huboCambios = false;

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos.Length == 13)
        {
            expedientes[i] += "|No registrada|N|PENDIENTE";
            huboCambios = true;
        }
        else if (datos.Length == 15)
        {
            expedientes[i] += "|PENDIENTE";
            huboCambios = true;
        }
        else if (datos.Length != 16 && datos.Length != 19)
        {
            Console.WriteLine(
                "Se encontró un expediente con formato incorrecto."
            );

            Array.Copy(respaldo, expedientes, cantidad);
            return false;
        }

        datos = expedientes[i].Split('|');

        if (datos.Length == 16)
        {
            expedientes[i] += "|SIN FECHA|SIN FECHA|SIN FECHA";
            huboCambios = true;
        }
        else if (datos.Length == 19)
        {
            for (int j = 16; j <= 18; j++)
            {
                string valor = datos[j];

                if (valor == "SIN FECHA" ||
                    (j == 18 && valor == "NO APLICA"))
                {
                    continue;
                }

                if (!DateTime.TryParseExact(
                    valor,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                {
                    Console.WriteLine(
                        "El expediente contiene una fecha inválida."
                    );

                    Array.Copy(respaldo, expedientes, cantidad);
                    return false;
                }
            }
        }
    }

    if (huboCambios && !GuardarExpedientes())
    {
        Array.Copy(respaldo, expedientes, cantidad);
        return false;
    }

    return true;
}


static void BuscarPorNivel()
{
    if (cantidad == 0)
    {
        Console.WriteLine("No hay alumnos registrados.");
        return;
    }

    Console.WriteLine("\n===== BUSCAR ALUMNOS POR NIVEL =====");
    Console.WriteLine("1. Inicial");
    Console.WriteLine("2. Primaria");
    Console.WriteLine("3. Secundaria");
    Console.WriteLine("0. Regresar");

    Console.Write("Seleccione el nivel: ");

    string nivel = Console.ReadLine() ?? "";

    if (nivel == "0")
        return;

    if (nivel != "1" && nivel != "2" && nivel != "3")
    {
        Console.WriteLine("Opción no válida.");
        return;
    }

    string nombreNivel = "";

    switch (nivel)
    {
        case "1":
            nombreNivel = "Inicial";
            break;
        case "2":
            nombreNivel = "Primaria";
            break;
        case "3":
            nombreNivel = "Secundaria";
            break;
    }

    int encontrados = 0;
    int confirmados = 0;
    int pendientes = 0;

    Console.WriteLine("\n===== ALUMNOS DE " + nombreNivel.ToUpper() + " =====");

    for (int i = 0; i < cantidad; i++)
    {
        string[] datos = expedientes[i].Split('|');

        if (datos.Length >= 12 && datos[11] == nivel)
        {
            encontrados++;

            MostrarExpediente(expedientes[i]);

            if (datos.Length >= 16)
            {
                if (datos[15] == "CONFIRMADA")
                    confirmados++;
                else if (datos[15] == "PENDIENTE")
                    pendientes++;
            }

            Console.WriteLine("--------------------------------");
        }
    }

    if (encontrados == 0)
    {
        Console.WriteLine("No hay alumnos registrados en este nivel.");
    }
    else
    {
        Console.WriteLine("\nTotal de alumnos: " + encontrados);
        Console.WriteLine("Matrículas confirmadas: " + confirmados);
        Console.WriteLine("Matrículas pendientes: " + pendientes);
    }
}


}