using System;
using System.Collections.Generic;
using System.IO;

namespace GestionACB
{
    public class Partido
    {
        public string Jornada { get; set; }
        public string EquipoLocal { get; set; }
        public string EquipoVisitante { get; set; }
        public string Resultado { get; set; }
        public string MvpPartido { get; set; }
        public int PuntosMvp { get; set; }
        public int AsistenciasMvp { get; set; }
        public int RebotesMvp { get; set; }
        public string EquipoMvp { get; set; }

        public int PuntosLocal => int.Parse(Resultado.Split('-')[0].Trim());
        public int PuntosVisitante => int.Parse(Resultado.Split('-')[1].Trim());
    }

    public class EstadisticaEquipo
    {
        public string Nombre { get; set; }
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public int PuntosFavor { get; set; }
        public int PuntosContra { get; set; }
        public int DiferenciaPuntos => PuntosFavor - PuntosContra;
    }

    public class ContadorMvp
    {
        public string Jugador { get; set; }
        public string Equipo { get; set; }
        public int Apariciones { get; set; }
    }

    class Program
    {
        // Ruta por defecto del fichero de datos de entrada
        static string csvPath = "acb2526.csv";

        static void Main(string[] args)
        {
            // Cargar la lista completa de partidos al iniciar la aplicacion
            List<Partido> partidos = CargarCSV(csvPath);

            // Validar si el archivo exista o contenia datos
            if (partidos.Count == 0)
            {
                Console.WriteLine("No se pudieron cargar los datos del archivo CSV. Comprueba que el archivo acb2526.csv existe.");
                return;
            }

            int opcion;
            do
            {
                Console.Clear();
                Console.WriteLine("     GESTION DE PARTIDOS ACB (TEMPORADA 25-26)    ");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Ver resultados de un equipo");
                Console.WriteLine("2. Ver resultados de una jornada en concreto");
                Console.WriteLine("3. Ver Top 3 MVPs con sus apariciones");
                Console.WriteLine("4. Ver Clasificacion de la liga");
                Console.WriteLine("5. Guardar archivos individuales de cada equipo");
                Console.WriteLine("0. Salir");
                Console.WriteLine("==================================================");
                Console.Write("Selecciona una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion)) opcion = -1;

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        VisualizarResultadosEquipo(partidos);
                        break;
                    case 2:
                        VisualizarResultadosJornada(partidos);
                        break;
                    case 3:
                        VisualizarTopMVPs(partidos);
                        break;
                    case 4:
                        VisualizarClasificacion(partidos);
                        break;
                    case 5:
                        GuardarArchivosPorEquipo(partidos);
                        break;
                    case 0:
                        Console.WriteLine("Gracias por usar la aplicacion!");
                        break;
                    default:
                        Console.WriteLine("Opcion no valida. Intentalo de nuevo.");
                        break;
                }

                // Pausa antes de volver a limpiar la pantalla si no se ha elegido salir
                if (opcion != 0)
                {
                    Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                    Console.ReadKey();
                }

            } while (opcion != 0);
        }


        // Metodo que lee el fichero CSV linea por linea y devuelve la lista de partidos
        static List<Partido> CargarCSV(string ruta)
        {
            List<Partido> lista = new List<Partido>();

            // Comprobar si la ruta del fichero existe en el sistema
            if (!File.Exists(ruta))
            {
                return lista;
            }

            // Leer todas las lineas de texto del archivo de golpe
            string[] lineas = File.ReadAllLines(ruta);

            // Empezar en i = 1 para omitir la linea 0 que contiene los encabezados
            for (int i = 1; i < lineas.Length; i++)
            {
                // Saltar lineas vacias si las hubiera
                if (string.IsNullOrWhiteSpace(lineas[i])) continue;

                // Separar los campos respetando comillas si existen
                string[] campos = ParsearLineaCSV(lineas[i]);

                // Comprobar que la linea contenga al menos las 9 columnas requeridas
                if (campos.Length >= 9)
                {
                    lista.Add(new Partido
                    {
                        Jornada = campos[0].Trim(),
                        EquipoLocal = campos[1].Trim(),
                        EquipoVisitante = campos[2].Trim(),
                        Resultado = campos[3].Trim(),
                        MvpPartido = campos[4].Trim(),
                        PuntosMvp = int.TryParse(campos[5].Trim(), out int p) ? p : 0,
                        AsistenciasMvp = int.TryParse(campos[6].Trim(), out int a) ? a : 0,
                        RebotesMvp = int.TryParse(campos[7].Trim(), out int r) ? r : 0,
                        EquipoMvp = campos[8].Trim()
                    });
                }
            }

            return lista;
        }

        // Metodo para separar columnas por coma sin romper datos envueltos entre comillas
        static string[] ParsearLineaCSV(string linea)
        {
            List<string> resultado = new List<string>();
            bool enComillas = false;
            string campoActual = "";

            foreach (char c in linea)
            {
                if (c == '"')
                {
                    // Cambiar el estado del indicador al encontrar comillas
                    enComillas = !enComillas;
                }
                else if (c == ',' && !enComillas)
                {
                    // Guardar el campo al detectar una coma fuera de comillas
                    resultado.Add(campoActual);
                    campoActual = "";
                }
                else
                {
                    // Ir construyendo el texto de la columna caracter por caracter
                    campoActual += c;
                }
            }
            // Guardar la ultima columna restante de la linea
            resultado.Add(campoActual);
            return resultado.ToArray();
        }



        // Metodo para obtener la lista de equipos sin duplicados y ordenada alfabeticamente
        static List<string> ObtenerEquiposUnicos(List<Partido> partidos)
        {
            List<string> equipos = new List<string>();

            // Recorrer los partidos para ir extrayendo nombres locales y visitantes
            foreach (var p in partidos)
            {
                bool existeLocal = false;
                bool existeVisitante = false;

                // Comprobar manualmente si el equipo ya esta en nuestra lista
                foreach (var eq in equipos)
                {
                    if (eq.Equals(p.EquipoLocal, StringComparison.OrdinalIgnoreCase)) existeLocal = true;
                    if (eq.Equals(p.EquipoVisitante, StringComparison.OrdinalIgnoreCase)) existeVisitante = true;
                }

                // Si no existian previamente, se agregan
                if (!existeLocal) equipos.Add(p.EquipoLocal);
                if (!existeVisitante) equipos.Add(p.EquipoVisitante);
            }

            // Ordenacion alfabetica 
            for (int i = 0; i < equipos.Count - 1; i++)
            {
                for (int j = 0; j < equipos.Count - 1 - i; j++)
                {
                    // Comparar cadenas de texto orden alfabeticamente
                    if (string.Compare(equipos[j], equipos[j + 1], StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // Intercambiar elementos de sitio
                        string aux = equipos[j];
                        equipos[j] = equipos[j + 1];
                        equipos[j + 1] = aux;
                    }
                }
            }

            return equipos;
        }



        // Opcion 1: Filtrar y mostrar los partidos jugados por un equipo
        static void VisualizarResultadosEquipo(List<Partido> partidos)
        {
            List<string> equipos = ObtenerEquiposUnicos(partidos);

            // Imprimir menu con la lista numerada de todos los equipos disponibles
            Console.WriteLine("--- Lista de Equipos Disponibles ---");
            for (int i = 0; i < equipos.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {equipos[i]}");
            }

            Console.Write("\nEscribe el nombre del equipo o su numero en la lista: ");
            string entrada = Console.ReadLine()?.Trim();
            string equipoSeleccionado = "";

            // Evaluar si el usuario introdujo el indice numerico del equipo
            if (int.TryParse(entrada, out int idx) && idx >= 1 && idx <= equipos.Count)
            {
                equipoSeleccionado = equipos[idx - 1];
            }
            else
            {
                // Buscar si introdujo el nombre escrito del equipo directamente
                foreach (var eq in equipos)
                {
                    if (eq.Equals(entrada, StringComparison.OrdinalIgnoreCase))
                    {
                        equipoSeleccionado = eq;
                        break;
                    }
                }
            }

            // Mostrar mensaje si no hubo coincidencia por numero ni por texto
            if (string.IsNullOrEmpty(equipoSeleccionado))
            {
                Console.WriteLine("El equipo introducido no existe.");
                return;
            }

            // Filtrar mediante bucle los partidos donde haya participado dicho equipo
            List<Partido> resultados = new List<Partido>();
            foreach (var p in partidos)
            {
                if (p.EquipoLocal.Equals(equipoSeleccionado, StringComparison.OrdinalIgnoreCase) ||
                    p.EquipoVisitante.Equals(equipoSeleccionado, StringComparison.OrdinalIgnoreCase))
                {
                    resultados.Add(p);
                }
            }

            // Formatear la tabla de resultados en una lista de lineas de texto
            List<string> lineasSalida = new List<string>();
            lineasSalida.Add($"RESULTADOS DE: {equipoSeleccionado.ToUpper()}");
            lineasSalida.Add(new string('-', 85));
            lineasSalida.Add(string.Format("{0,-12} | {1,-32} | {2,-7} | {3,-20} (Pts/Asis/Reb)", "Jornada", "Encuentro", "Result.", "MVP"));
            lineasSalida.Add(new string('-', 85));

            foreach (var p in resultados)
            {
                string encuentro = $"{p.EquipoLocal} vs {p.EquipoVisitante}";
                string mvpInfo = $"{p.MvpPartido} ({p.PuntosMvp}/{p.AsistenciasMvp}/{p.RebotesMvp})";
                lineasSalida.Add(string.Format("{0,-12} | {1,-32} | {2,-7} | {3,-20}", p.Jornada, encuentro, p.Resultado, mvpInfo));
            }

            lineasSalida.Add(new string('-', 85));

            // Imprimir cada linea en la pantalla del usuario
            foreach (var l in lineasSalida) Console.WriteLine(l);

            PreguntarYGuardarFichero(lineasSalida);
        }

        // Opcion 2: Filtrar y mostrar los partidos correspondientes a una jornada concreta
        static void VisualizarResultadosJornada(List<Partido> partidos)
        {
            Console.Write("Introduce el numero de jornada (1 - 34): ");
            string numJornada = Console.ReadLine()?.Trim();
            string nombreJornada = numJornada.StartsWith("Jornada", StringComparison.OrdinalIgnoreCase) ? numJornada : $"Jornada {numJornada}";

            // Filtrar los partidos que coincidan con la jornada pedida
            List<Partido> partidosJornada = new List<Partido>();
            foreach (var p in partidos)
            {
                if (p.Jornada.Equals(nombreJornada, StringComparison.OrdinalIgnoreCase))
                {
                    partidosJornada.Add(p);
                }
            }

            if (partidosJornada.Count == 0)
            {
                Console.WriteLine($"No se encontraron datos para la '{nombreJornada}'.");
                return;
            }

            List<string> lineasSalida = new List<string>();
            lineasSalida.Add($"RESULTADOS DE LA {nombreJornada.ToUpper()}");
            lineasSalida.Add(new string('-', 85));
            lineasSalida.Add(string.Format("{0,-35} vs {1,-35} | {2,-7} | {3}", "Local", "Visitante", "Result.", "MVP"));
            lineasSalida.Add(new string('-', 85));

            foreach (var p in partidosJornada)
            {
                lineasSalida.Add(string.Format("{0,-35} vs {1,-35} | {2,-7} | {3} ({4})", p.EquipoLocal, p.EquipoVisitante, p.Resultado, p.MvpPartido, p.EquipoMvp));
            }

            lineasSalida.Add(new string('-', 85));

            foreach (var l in lineasSalida) Console.WriteLine(l);

            PreguntarYGuardarFichero(lineasSalida);
        }

        // Opcion 3: Calcular y visualizar el Top 3 de los jugadores con mas premios MVP
        static void VisualizarTopMVPs(List<Partido> partidos)
        {
            List<ContadorMvp> listaContadores = new List<ContadorMvp>();

            // Agrupar y contar manualmente los MVPs recibidos por cada jugador
            foreach (var p in partidos)
            {
                ContadorMvp encontrado = null;
                foreach (var c in listaContadores)
                {
                    if (c.Jugador.Equals(p.MvpPartido, StringComparison.OrdinalIgnoreCase))
                    {
                        encontrado = c;
                        break;
                    }
                }

                // Sumar 1 si el jugador ya estaba en la lista, o crearlo si es la primera vez
                if (encontrado != null)
                {
                    encontrado.Apariciones++;
                }
                else
                {
                    listaContadores.Add(new ContadorMvp
                    {
                        Jugador = p.MvpPartido,
                        Equipo = p.EquipoMvp,
                        Apariciones = 1
                    });
                }
            }

            // Ordenar segun la cantidad de MVPs (Apariciones)
            for (int i = 0; i < listaContadores.Count - 1; i++)
            {
                for (int j = 0; j < listaContadores.Count - 1 - i; j++)
                {
                    if (listaContadores[j].Apariciones < listaContadores[j + 1].Apariciones)
                    {
                        var aux = listaContadores[j];
                        listaContadores[j] = listaContadores[j + 1];
                        listaContadores[j + 1] = aux;
                    }
                }
            }

            List<string> lineasSalida = new List<string>();
            lineasSalida.Add("TOP 3 JUGADORES CON MAS MVPs DE LA TEMPORADA");
            lineasSalida.Add(new string('-', 60));
            lineasSalida.Add(string.Format("{0,-6} | {1,-25} | {2,-18} | {3}", "Pos", "Jugador", "Equipo", "Nº MVPs"));
            lineasSalida.Add(new string('-', 60));

            // Asegurar no exceder el tamano si la lista tiene menos de 3 elementos
            int limite = listaContadores.Count < 3 ? listaContadores.Count : 3;
            for (int i = 0; i < limite; i++)
            {
                var mvp = listaContadores[i];
                lineasSalida.Add(string.Format("{0,-6} | {1,-25} | {2,-18} | {3} veces", i + 1, mvp.Jugador, mvp.Equipo, mvp.Apariciones));
            }

            lineasSalida.Add(new string('-', 60));

            // Mostrar en pantalla
            foreach (var l in lineasSalida) Console.WriteLine(l);

            PreguntarYGuardarFichero(lineasSalida);
        }

        // Opcion 4: Calcular la tabla de clasificacion por balance de victorias y derrotas
        static void VisualizarClasificacion(List<Partido> partidos)
        {
            List<EstadisticaEquipo> listaEquipos = new List<EstadisticaEquipo>();

            // Procesar las victorias, derrotas y puntos a favor/contra de cada equipo
            foreach (var p in partidos)
            {
                EstadisticaEquipo local = null;
                EstadisticaEquipo visitante = null;

                // Buscar el registro de cada equipo participante
                foreach (var eq in listaEquipos)
                {
                    if (eq.Nombre.Equals(p.EquipoLocal, StringComparison.OrdinalIgnoreCase)) local = eq;
                    if (eq.Nombre.Equals(p.EquipoVisitante, StringComparison.OrdinalIgnoreCase)) visitante = eq;
                }

                // Si no existian en la tabla acumulada, se crean nuevos registros
                if (local == null)
                {
                    local = new EstadisticaEquipo { Nombre = p.EquipoLocal };
                    listaEquipos.Add(local);
                }

                if (visitante == null)
                {
                    visitante = new EstadisticaEquipo { Nombre = p.EquipoVisitante };
                    listaEquipos.Add(visitante);
                }

                // Sumar puntos a favor y en contra de cada equipo
                local.PuntosFavor += p.PuntosLocal;
                local.PuntosContra += p.PuntosVisitante;

                visitante.PuntosFavor += p.PuntosVisitante;
                visitante.PuntosContra += p.PuntosLocal;

                // Asignar victoria y derrota segun el resultado del encuentro
                if (p.PuntosLocal > p.PuntosVisitante)
                {
                    local.Victorias++;
                    visitante.Derrotas++;
                }
                else
                {
                    visitante.Victorias++;
                    local.Derrotas++;
                }
            }

            // Ordenar tabla aplicando criterios de desempate: 1º Victorias, 2º Diferencia de Puntos, 3º Puntos Favor
            for (int i = 0; i < listaEquipos.Count - 1; i++)
            {
                for (int j = 0; j < listaEquipos.Count - 1 - i; j++)
                {
                    bool intercambiar = false;

                    // Criterio 1: Mayor numero de victorias
                    if (listaEquipos[j].Victorias < listaEquipos[j + 1].Victorias)
                    {
                        intercambiar = true;
                    }
                    else if (listaEquipos[j].Victorias == listaEquipos[j + 1].Victorias)
                    {
                        // Criterio 2 (Desempate): Mayor diferencia de puntos
                        if (listaEquipos[j].DiferenciaPuntos < listaEquipos[j + 1].DiferenciaPuntos)
                        {
                            intercambiar = true;
                        }
                        else if (listaEquipos[j].DiferenciaPuntos == listaEquipos[j + 1].DiferenciaPuntos)
                        {
                            // Criterio 3 (Desempate): Mayor cantidad de puntos anotados a favor
                            if (listaEquipos[j].PuntosFavor < listaEquipos[j + 1].PuntosFavor)
                            {
                                intercambiar = true;
                            }
                        }
                    }

                    // Intercambiar si el elemento actual debe ir debajo del siguiente
                    if (intercambiar)
                    {
                        var aux = listaEquipos[j];
                        listaEquipos[j] = listaEquipos[j + 1];
                        listaEquipos[j + 1] = aux;
                    }
                }
            }

            List<string> lineasSalida = new List<string>();
            lineasSalida.Add("CLASIFICACION GENERAL DE LA LIGA ACB 2025-2026");
            lineasSalida.Add(new string('-', 70));
            lineasSalida.Add(string.Format("{0,-4} | {1,-33} | {2,-3} | {3,-3} | {4,-6} | {5,-6} | {6,-5}", "Pos", "Equipo", "V", "D", "PF", "PC", "Dif"));
            lineasSalida.Add(new string('-', 70));

            int ranking = 1;
            foreach (var e in listaEquipos)
            {
                string difStr = e.DiferenciaPuntos > 0 ? $"+{e.DiferenciaPuntos}" : e.DiferenciaPuntos.ToString();
                lineasSalida.Add(string.Format("{0,-4} | {1,-33} | {2,-3} | {3,-3} | {4,-6} | {5,-6} | {6,-5}",
                    ranking++, e.Nombre, e.Victorias, e.Derrotas, e.PuntosFavor, e.PuntosContra, difStr));
            }

            lineasSalida.Add(new string('-', 70));

            // Imprimir tabla completa por pantalla
            foreach (var l in lineasSalida) Console.WriteLine(l);

            PreguntarYGuardarFichero(lineasSalida);
        }

        // Opcion 5: Generar un archivo individual .txt por cada equipo de la liga en el directorio de ejecucion
        static void GuardarArchivosPorEquipo(List<Partido> partidos)
        {
            List<string> equipos = ObtenerEquiposUnicos(partidos);
            
            string rutaCarpeta = Directory.GetCurrentDirectory();

            foreach (var equipo in equipos)
            {
                // Filtrar partidos del equipo analizado
                List<Partido> resultados = new List<Partido>();
                foreach (var p in partidos)
                {
                    if (p.EquipoLocal.Equals(equipo, StringComparison.OrdinalIgnoreCase) ||
                        p.EquipoVisitante.Equals(equipo, StringComparison.OrdinalIgnoreCase))
                    {
                        resultados.Add(p);
                    }
                }

                // Generar contenido del fichero del equipo
                List<string> lineas = new List<string>();
                lineas.Add($"RESULTADOS DE LA TEMPORADA - {equipo.ToUpper()}");
                lineas.Add(new string('-', 85));
                lineas.Add(string.Format("{0,-12} | {1,-32} | {2,-7} | {3,-20}", "Jornada", "Encuentro", "Result.", "MVP"));
                lineas.Add(new string('-', 85));

                foreach (var p in resultados)
                {
                    string encuentro = $"{p.EquipoLocal} vs {p.EquipoVisitante}";
                    string mvpInfo = $"{p.MvpPartido} ({p.PuntosMvp}pts)";
                    lineas.Add(string.Format("{0,-12} | {1,-32} | {2,-7} | {3,-20}", p.Jornada, encuentro, p.Resultado, mvpInfo));
                }

                // Reemplazar espacios y quitar caracteres no validos para crear el nombre del archivo
                string nombreLimpio = string.Join("_", equipo.Split(Path.GetInvalidFileNameChars()));
                string rutaCompleta = Path.Combine(rutaCarpeta, $"{nombreLimpio}.txt");

                // Escribir el fichero en disco dentro del directorio actual
                File.WriteAllLines(rutaCompleta, lineas);
            }

            Console.WriteLine($"\n¡Se han generado correctamente {equipos.Count} ficheros .txt en la carpeta:\n{rutaCarpeta}");
        }


        static void PreguntarYGuardarFichero(List<string> contenido)
        {
            Console.Write("\n¿Deseas guardar esta informacion en un fichero .txt? (S/N): ");
            string resp = Console.ReadLine()?.Trim().ToUpper();

            if (resp == "S" || resp == "SI")
            {
                Console.Write("Introduce el nombre del fichero (ej. resultados.txt): ");
                string nombreFichero = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(nombreFichero))
                {
                    if (!nombreFichero.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        nombreFichero += ".txt";
                    }

                    // Guardar todas las lineas de texto generadas en el fichero indicado
                    File.WriteAllLines(nombreFichero, contenido);
                    Console.WriteLine($"Informacion guardada con exito en '{nombreFichero}'.");
                }
                else
                {
                    Console.WriteLine("Nombre de archivo no valido. No se guardaron los datos.");
                }
            }
        }

    }
}