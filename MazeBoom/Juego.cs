using Spectre.Console;

partial class Maze
{
    class Juego
    {
        private static readonly Random rnd = new Random();
        public List<Personaje> Jugadores { get; set; }
        public Casilla[,] Laberinto { get; set; }
        private (int x, int y) Meta { get; set; }

        public Juego(List<Personaje> jugadores, Casilla[,] laberinto)
        {
            Jugadores = jugadores;
            Laberinto = laberinto;
            Meta = (laberinto.GetLength(0) - 1, laberinto.GetLength(1) - 2);
        }

        public void Iniciar()
        {
            bool juegoEnCurso = true;

            while (juegoEnCurso)
            {
                foreach (var jugador in Jugadores.ToList())
                {
                    MostrarEstadoJugador(jugador);

                    if (!jugador.HabilidadDisponible && jugador.TiempoEnfriamiento > 0)
                    {
                        jugador.TiempoEnfriamiento--;
                        if (jugador.TiempoEnfriamiento == 0)
                        {
                            jugador.HabilidadDisponible = true;
                            MostrarMensajeConColor($"¡La habilidad de {jugador.Nombre} está disponible nuevamente!", "green");
                        }
                    }

                    if (jugador.TurnosSinJugar > 0)
                    {
                        MostrarMensajeConColor($"{jugador.Nombre} te has quedado atorado {jugador.TurnosSinJugar} turnos más.", "yellow");
                        jugador.TurnosSinJugar--;
                        continue;
                    }

                    MostrarOpcionesJugador(jugador);
                    PrintMaze(Laberinto, Jugadores);

                    if (jugador.Posicion == Meta)
                    {
                        if (jugador.TieneLlave)
                        {
                            MostrarMensajeConColor($"¡{jugador.Nombre} ha ganado el juego!", "green");
                            juegoEnCurso = false;
                            break;
                        }
                        else
                        {
                            MostrarMensajeConColor("No puedes salir. Te falta una llave para salir.", "red");
                        }
                    }

                    if (jugador.TurnosDeInmunidad > 0)
                    {
                        jugador.TurnosDeInmunidad--;
                        if (jugador.TurnosDeInmunidad == 0)
                        {
                            MostrarMensajeConColor($"{jugador.Nombre}, tu inmunidad ha terminado. Ahora eres vulnerable a las trampas.", "yellow");
                        }
                    }
                }

                if (!juegoEnCurso)
                {
                    MostrarMenuFinDeJuego();
                }
            }
        }

        void MostrarMenuFinDeJuego()
        {
            Console.WriteLine("Elige una opción:");
            Console.WriteLine("[1] Jugar una nueva partida");
            Console.WriteLine("[2] Salir");

            EntradaValida();
        }

        void MostrarEstadoJugador(Personaje jugador)
        {
            AnsiConsole.Write(new Markup($"\nJugador {jugador.Nombre} - Habilidad: {jugador.Habilidad} (Enfriamiento: {jugador.TiempoEnfriamiento})\n"));
            AnsiConsole.Write(new Markup($"Posición: ({jugador.Posicion.x}, {jugador.Posicion.y})\n"));
            AnsiConsole.Write(new Markup(jugador.TieneLlave ? "[yellow]Llave: Sí[/]\n" : "[yellow]Llave: No[/]\n"));
        }

        void MostrarOpcionesJugador(Personaje jugador)
        {
            Console.WriteLine("Opciones:");
            Console.WriteLine("[1] Mover hacia arriba");
            Console.WriteLine("[2] Mover hacia abajo");
            Console.WriteLine("[3] Mover hacia la izquierda");
            Console.WriteLine("[4] Mover hacia la derecha");
            Console.WriteLine("[5] Activar habilidad");
            Console.WriteLine("[Esc] Abandonar partida");
            Console.WriteLine(""); // Dejar una línea en blanco para mostrar mensajes

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                        if (Mover(jugador, -1, 0)) return;
                        break;
                    case ConsoleKey.D2:
                        if (Mover(jugador, 1, 0)) return;
                        break;
                    case ConsoleKey.D3:
                        if (Mover(jugador, 0, -1)) return;
                        break;
                    case ConsoleKey.D4:
                        if (Mover(jugador, 0, 1)) return;
                        break;
                    case ConsoleKey.D5:
                        ActivarHabilidad(jugador);
                        return;
                    case ConsoleKey.Escape:
                        AbandonarPartida(jugador);
                        return;
                    default:
                        MostrarMensajeConColor("Opción no válida. Intenta de nuevo.", "red");
                        break;
                }
            }
        }
        // Método para manejar la opción de abandonar partida
        void AbandonarPartida(Personaje jugador)
        {
            MostrarMensajeConColor($"{jugador.Nombre} ha abandonado la partida.", "red");
            Jugadores.Remove(jugador);

            if (Jugadores.Count == 1)
            {
                Console.WriteLine($"Felicidades {Jugadores[0].Nombre} ganaste, te has quedado sin oponente.");
                MostrarMenuFinDeJuego();
            }
            else if (Jugadores.Count >= 2)
            {
                var jugadoresRestantes = new List<Personaje>(Jugadores);
                foreach (var j in jugadoresRestantes)
                {
                    Console.WriteLine($"{j.Nombre}, ¿deseas continuar jugando? (S/N)");
                    var respuesta = Console.ReadLine()?.ToUpper();
                    if (respuesta == "N")
                    {
                        MostrarMensajeConColor($"{j.Nombre} ha decidido abandonar la partida.", "red");
                        Jugadores.Remove(j);
                    }
                }

                if (Jugadores.Count == 1)
                {
                    Console.WriteLine($"Felicidades {Jugadores[0].Nombre} ganaste, te has quedado sin oponente.");
                    MostrarMenuFinDeJuego();
                }
                else if (Jugadores.Count >= 2)
                {
                    Console.WriteLine("El juego continúa con los jugadores restantes.");
                }
                else
                {
                    MostrarMenuFinDeJuego();
                }
            }
        }

        // Método para activar la habilidad del jugador
        void ActivarHabilidad(Personaje jugador)
        {
            if (!jugador.HabilidadDisponible)
            {
                MostrarMensajeConColor("La habilidad aún está en enfriamiento.", "yellow");
                return;
            }

            MostrarMensajeConColor($"{jugador.Nombre} activó su habilidad: {jugador.Habilidad}", "green");

            switch (jugador.Habilidad)
            {
                case "Teletransporte":
                    Teletransporte(jugador);
                    break;
                case "Envenenar":
                    Envenenar(jugador);
                    break;
                case "Robar la llave":
                    RobarLlave(jugador);
                    break;
                case "Sabotaje":
                    Sabotaje(jugador);
                    break;
                case "Curación":
                    Curacion(jugador);
                    break;
                case "Boo":
                    Boo(jugador);
                    break;
                default:
                    MostrarMensajeConColor("Habilidad desconocida!", "red");
                    break;
            }

            jugador.HabilidadDisponible = false;
        }

        // Método para la habilidad Sabotaje del ingeniero
        void Sabotaje(Personaje jugador)
        {
            int dx = 0, dy = 0;

            while (true)
            {
                Console.WriteLine("Elige la dirección para colocar la trampa de tuberías rotas:");
                Console.WriteLine("[1] Arriba");
                Console.WriteLine("[2] Abajo");
                Console.WriteLine("[3] Izquierda");
                Console.WriteLine("[4] Derecha");

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                        dx = -1;
                        dy = 0;
                        break;
                    case ConsoleKey.D2:
                        dx = 1;
                        dy = 0;
                        break;
                    case ConsoleKey.D3:
                        dx = 0;
                        dy = -1;
                        break;
                    case ConsoleKey.D4:
                        dx = 0;
                        dy = 1;
                        break;
                    default:
                        MostrarMensajeConColor("Opción no válida. Intenta de nuevo.", "red");
                        continue;
                }

                var nuevaPosicion = (x: jugador.Posicion.x + dx, y: jugador.Posicion.y + dy);
                if (EsPosicionValidaParaTrampa(nuevaPosicion))
                {
                    ColocarTrampa(nuevaPosicion, jugador);
                    break;
                }
                else
                {
                    MostrarMensajeConColor("No puedes colocar la trampa en esa posición.", "red");
                }
            }

            jugador.HabilidadDisponible = false;
            jugador.TiempoEnfriamiento = 4; // Ajustar el tiempo de enfriamiento según la habilidad
        }

        bool EsPosicionValidaParaTrampa((int x, int y) nuevaPosicion)
        {
            int ancho = Laberinto.GetLength(1);
            int altura = Laberinto.GetLength(0);
            return nuevaPosicion.x >= 0 && nuevaPosicion.x < altura && nuevaPosicion.y >= 0 && nuevaPosicion.y < ancho && Laberinto[nuevaPosicion.x, nuevaPosicion.y] == Casilla.Free && nuevaPosicion != (0, 1);
        }
        void ColocarTrampa((int x, int y) nuevaPosicion, Personaje jugador)
        {
            var jugadorEnCasilla = Jugadores.FirstOrDefault(j => j.Posicion == nuevaPosicion);
            
            if (jugadorEnCasilla != null)
            {
                MostrarMensajeConColor($"¡{jugadorEnCasilla.Nombre} ha caído en la trampa de tuberías rotas!", "red");
                Laberinto[nuevaPosicion.x, nuevaPosicion.y] = Casilla.BrokenPipes;
                MoverJugadorAtras(jugadorEnCasilla);
                Laberinto[nuevaPosicion.x, nuevaPosicion.y] = Casilla.Free; // Desactivar trampa
            }
            else
            {
                Laberinto[nuevaPosicion.x, nuevaPosicion.y] = Casilla.BrokenPipes;
                MostrarMensajeConColor("¡Sabotaje exitoso! Has colocado una trampa de tuberías rotas.", "green");
            }
        }

        void MoverJugadorAtras(Personaje jugador)
        {
            if (jugador.PosicionesAnteriores.Count == 4)
            {
                jugador.Posicion = jugador.PosicionesAnteriores.Dequeue();
                MostrarMensajeConColor("El agua te ha llevado 5 hacia donde estabas hace 5 turnos.", "red");
            }
            else
            {
                jugador.Posicion = (0, 1);
                MostrarMensajeConColor("El agua te ha llevado hasta el inicio.", "red");
                if (Laberinto[0, 1] == Casilla.BrokenPipes)
                {
                    ActivarTrampaInicio(jugador);
                }
            }
        }

        // Método para manejar la activación inmediata de trampas en el inicio
        void ActivarTrampaInicio(Personaje jugador)
        {
            MostrarMensajeConColor($"¡{jugador.Nombre} ha caído en la trampa de tuberías rotas en el inicio!", "red");
            MoverJugadorAtras(jugador);
        }

        // Método para la habilidad de teletransporte del físico
        void Teletransporte(Personaje jugador)
        {
            int filas = Laberinto.GetLength(0);
            int columnas = Laberinto.GetLength(1);
            var casillasLibresEnRadio = new List<(int x, int y)>();

            for (int i = Math.Max(0, jugador.Posicion.x - 7); i <= Math.Min(filas - 1, jugador.Posicion.x + 7); i++)
            {
                for (int j = Math.Max(0, jugador.Posicion.y - 7); j <= Math.Min(columnas - 1, jugador.Posicion.y + 7); j++)
                {
                    if (Laberinto[i, j] == Casilla.Free && (i != jugador.Posicion.x || j != jugador.Posicion.y))
                    {
                        casillasLibresEnRadio.Add((i, j));
                    }
                }
            }

            if (casillasLibresEnRadio.Count > 0)
            {
                var nuevaPosicion = casillasLibresEnRadio[rnd.Next(casillasLibresEnRadio.Count)];
                jugador.PosicionesAnteriores.Enqueue(jugador.Posicion);
                if (jugador.PosicionesAnteriores.Count > 4) jugador.PosicionesAnteriores.Dequeue();
                jugador.Posicion = nuevaPosicion;
                MostrarMensajeConColor("¡Teletransporte exitoso!", "green");
            }
            else
            {
                MostrarMensajeConColor("No hay casillas libres disponibles para teletransportarse dentro del radio permitido.", "red");
            }
            jugador.TiempoEnfriamiento = 5;
        }
            // Método para la habilidad Envenenar del químico
        void Envenenar(Personaje jugador)
        {
            var jugadoresEnMismaCasilla = Jugadores.Where(j => j != jugador && j.Posicion == jugador.Posicion).ToList();

            if (jugadoresEnMismaCasilla.Count == 0)
            {
                MostrarMensajeConColor("No puedes envenenar a nadie. No hay otros jugadores en la misma casilla.", "red");
            }
            else
            {
                foreach (var otroJugador in jugadoresEnMismaCasilla)
                {
                    MostrarMensajeConColor($"{otroJugador.Nombre} ha sido envenenado y ha regresado a la casilla de inicio.", "red");
                    otroJugador.Posicion = (0, 1);
                    if (Laberinto[0, 1] == Casilla.BrokenPipes)
                    {
                        ActivarTrampaInicio(otroJugador);
                    }
                }
            }
        }

        // Método para la habilidad Robar la llave del director
        void RobarLlave(Personaje jugador)
        {
            if (!jugador.HabilidadDisponible)
            {
                MostrarMensajeConColor("La habilidad aún está en enfriamiento.", "yellow");
                return;
            }

            bool llaveRobada = false;
                    foreach (var otroJugador in Jugadores)
            {
                if (otroJugador != jugador && otroJugador.Posicion == jugador.Posicion && otroJugador.TieneLlave)
                {
                    MostrarMensajeConColor($"{otroJugador.Nombre} ha sido robado. {jugador.Nombre} ahora tiene la llave.", "red");
                    otroJugador.TieneLlave = false;

                    if (!jugador.TieneLlave)
                    {
                        jugador.TieneLlave = true;
                    }
                    else
                    {
                        var posicionLlaveNueva = ObtenerPosicionLibreAleatoria();
                        if (posicionLlaveNueva.HasValue)
                        {
                            Laberinto[posicionLlaveNueva.Value.x, posicionLlaveNueva.Value.y] = Casilla.Llave;
                            MostrarMensajeConColor("La nueva llave ha aparecido en una casilla libre del laberinto.", "green");
                        }
                    }
                    llaveRobada = true;
                    break;
                }
            }

            if (!llaveRobada)
            {
                MostrarMensajeConColor("No hay ningún jugador con una llave en la misma casilla.", "red");
            }

            jugador.HabilidadDisponible = false;
            jugador.TiempoEnfriamiento = 7; // Tiempo de enfriamiento de 7 turnos
        }

        // Método para la habilidad Boo del monstruo radioactivo
        void Boo(Personaje jugador)
        {
            while (true)
            {
                Console.WriteLine("Elige el jugador al que quieres aparecer:");
                for (int i = 0; i < Jugadores.Count; i++)
                {
                    if (Jugadores[i] != jugador)
                    {
                        Console.WriteLine($"[{i + 1}] {Jugadores[i].Nombre}");
                    }
                }

                if (int.TryParse(Console.ReadLine(), out int eleccion) && eleccion > 0 && eleccion <= Jugadores.Count && Jugadores[eleccion - 1] != jugador)
                {
                    jugador.Posicion = Jugadores[eleccion - 1].Posicion;
                    MostrarMensajeConColor($"¡{jugador.Nombre} ha aparecido en la casilla de {Jugadores[eleccion - 1].Nombre}!", "green");
                    break;
                }
                else
                {
                    MostrarMensajeConColor("Selección no válida. Intenta de nuevo.", "red");
                }
            }
        }

        // Método para la habilidad de curación del médico
        void Curacion(Personaje jugador)
        {
            if (jugador.HabilidadDisponible)
            {
                MostrarMensajeConColor("El médico ha activado su habilidad de curación. Las trampas no le afectarán durante los próximos 3 turnos.", "green");
                jugador.HabilidadDisponible = false;
                jugador.TiempoEnfriamiento = 15; // Tiempo de enfriamiento de 15 turnos
                jugador.TurnosDeInmunidad = 4; // Proporcionar inmunidad durante 3 turnos
            }
            else
            {
                MostrarMensajeConColor("La habilidad de curación aún está en enfriamiento.", "yellow");
            }
        }

        // Método para mover al jugador en la dirección indicada
        bool Mover(Personaje jugador, int dx, int dy)
        {
            int nuevoY = jugador.Posicion.x + dx;
            int nuevoX = jugador.Posicion.y + dy;
            int ancho = Laberinto.GetLength(1);
            int altura = Laberinto.GetLength(0);

            if (!EsMovimientoValido(nuevoX, nuevoY, ancho, altura))
            {
                MostrarMensajeConColor($"Movimiento no válido: ({nuevoY}, {nuevoX}) está fuera del rango del laberinto", "red");
                return false;
            }

            Casilla casillaDestino = Laberinto[nuevoY, nuevoX];

            switch (casillaDestino)
            {
                case Casilla.Free:
                case Casilla.Llave:
                    ManejarCasillaLibreOLlave(jugador, nuevoY, nuevoX, casillaDestino);
                    return true;

                case Casilla.BrokenPipes:
                case Casilla.Debris:
                case Casilla.Fire:
                    if (jugador.TurnosDeInmunidad > 0)
                    {
                        MostrarMensajeConColor("¡Has evitado la trampa gracias a tu inmunidad!", "green");
                        jugador.TurnosDeInmunidad--;
                        jugador.Posicion = (nuevoY, nuevoX);
                        RestaurarTrampa(nuevoY, nuevoX, casillaDestino);
                    }
                    else
                    {
                        AplicarEfectoTrampa(jugador, nuevoY, nuevoX, casillaDestino);
                    }
                    return true;

                default:
                    MostrarMensajeConColor($"Movimiento no válido: ({nuevoY}, {nuevoX}) no es una casilla transitable", "red");
                    return false;
            }
        }

        bool EsMovimientoValido(int nuevoX, int nuevoY, int ancho, int altura)
        {
            return nuevoX >= 0 && nuevoX < ancho && nuevoY >= 0 && nuevoY < altura;
        }

        void ManejarCasillaLibreOLlave(Personaje jugador, int nuevoY, int nuevoX, Casilla casillaDestino)
        {
            if (casillaDestino == Casilla.Llave)
            {
                if (!jugador.TieneLlave)
                {
                    jugador.TieneLlave = true;
                    Laberinto[nuevoY, nuevoX] = Casilla.Free;
                    MostrarMensajeConColor("¡Has encontrado una llave!", "green");
                }
                else
                {
                    MostrarMensajeConColor("Ya tienes una llave", "yellow");
                }
            }
            jugador.PosicionesAnteriores.Enqueue(jugador.Posicion);
            if (jugador.PosicionesAnteriores.Count > 4) jugador.PosicionesAnteriores.Dequeue();
            jugador.Posicion = (nuevoY, nuevoX);
        }
        void AplicarEfectoTrampa(Personaje jugador, int nuevoY, int nuevoX, Casilla casillaDestino)
        {
            switch (casillaDestino)
            {
                case Casilla.BrokenPipes:
                    MoverJugadorAtras(jugador);
                    Laberinto[nuevoY, nuevoX] = Casilla.Free; // Desactivar trampa
                    break;
                case Casilla.Debris:
                    jugador.Posicion = (nuevoY, nuevoX);
                    MostrarMensajeConColor("Se ha caído algo y salir de los escombros te tomará 3 turnos.", "red");
                    jugador.TurnosSinJugar = 3;
                    Laberinto[nuevoY, nuevoX] = Casilla.Free; // Desactivar trampa
                    break;
                case Casilla.Fire:
                    jugador.Posicion = (nuevoY, nuevoX);
                    MostrarMensajeConColor("Se ha incendiado la habitación y te han llevado a un lugar seguro.", "red");
                    jugador.Posicion = (0, 1);
                    Laberinto[nuevoY, nuevoX] = Casilla.Free; // Desactivar trampa
                    if (Laberinto[0, 1] == Casilla.BrokenPipes)
                    {
                        ActivarTrampaInicio(jugador);
                    }
                    break;
            }
        }

        void MostrarMensajeConColor(string mensaje, string color)
        {
            AnsiConsole.Write(new Markup($"[{color}]{mensaje}[/]"));
            Thread.Sleep(2000);
            Console.WriteLine("");
        }

        void RestaurarTrampa(int x, int y, Casilla tipoTrampa)
        {
            Task.Delay(1000).ContinueWith(_ => Laberinto[x, y] = tipoTrampa);
        }

        public static void PrintMaze(Casilla[,] laberinto, List<Personaje> jugadores)
        {
            int filas = laberinto.GetLength(0);
            int columnas = laberinto.GetLength(1);
            var posicionesJugadores = new Dictionary<(int, int), string>();

            for (int i = 0; i < jugadores.Count; i++)
            {
                var jugador = jugadores[i];
                posicionesJugadores[jugador.Posicion] = (i + 1).ToString();
            }

            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            int leftPadding = (windowWidth - columnas * 3) / 2;
            int topPadding = (windowHeight - filas) / 2;

            Console.Clear();
            for (int i = 0; i < topPadding; i++) Console.WriteLine();

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < leftPadding; j++) Console.Write(" ");

                for (int j = 0; j < columnas; j++)
                {
                    if (posicionesJugadores.ContainsKey((i, j)))
                    {
                        AnsiConsole.Markup($"[bold green]{posicionesJugadores[(i, j)]}[/]");
                    }
                    else
                    {
                        switch (laberinto[i, j])
                        {
                            case Casilla.Walls:
                                AnsiConsole.Markup("[yellow]■[/]");
                                break;
                            case Casilla.Free:
                                AnsiConsole.Markup("[black]■[/]");
                                break;
                            case Casilla.BrokenPipes:
                                AnsiConsole.Markup("[blue]■[/]");
                                break;
                            case Casilla.Debris:
                                AnsiConsole.Markup("[red]■[/]");
                                break;
                            case Casilla.Fire:
                                AnsiConsole.Markup("[magenta]■[/]");
                                break;
                            case Casilla.Llave:
                                AnsiConsole.Markup("[green]■[/]");
                                break;
                        }
                    }
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        // Método para obtener una posición libre aleatoria en el laberinto
        (int x, int y)? ObtenerPosicionLibreAleatoria()
        {
            List<(int x, int y)> casillasLibres = new List<(int x, int y)>();
            for (int i = 0; i < Laberinto.GetLength(0); i++)
            {
                for (int j = 0; j < Laberinto.GetLength(1); j++)
                {
                    if (Laberinto[i, j] == Casilla.Free)
                    {
                        casillasLibres.Add((i, j));
                    }
                }
            }
            if (casillasLibres.Count > 0)
            {
                return casillasLibres[rnd.Next(casillasLibres.Count)];
            }
            return null;
        }
    }
}
