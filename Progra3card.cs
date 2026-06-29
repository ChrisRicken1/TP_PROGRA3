using System;
using MySql.Data.MySqlClient; 

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=mi_banco_db;Uid=root;Pwd=;";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA TARJETA (ALTA DE CLIENTE) ---");

            Console.Write("Ingrese Documento/DNI del cliente: ");
            string documento = Console.ReadLine().Trim();
            
            Console.Write("Ingrese Nombre: ");
            string nombre = Console.ReadLine().Trim();
            
            Console.Write("Ingrese Apellido: ");
            string apellido = Console.ReadLine().Trim();
            
            Console.Write("Ingrese Email: ");
            string email = Console.ReadLine().Trim();

            Console.Write("Ingrese Número de Tarjeta (16 dígitos): ");
            string numeroTarjeta = Console.ReadLine().Trim();

            Console.Write("Ingrese Banco Emisor (ej. Banco Nación, Banco Galicia): ");
            string bancoEmisor = Console.ReadLine().Trim();

            Console.Write("Ingrese Saldo Inicial: ");
            decimal saldo = Convert.ToDecimal(Console.ReadLine());

            bool exito = RegistrarClienteYTarjeta(documento, nombre, apellido, email, numeroTarjeta, bancoEmisor, saldo);

            if (exito)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nAlta procesada con éxito.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo se pudo completar el alta. Verifique si el DNI o el número de tarjeta ya existen.");
            }
            Console.ResetColor();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static bool RegistrarClienteYTarjeta(string doc, string nom, string ape, string mail, string nroTarjeta, string banco, decimal saldoInicial)
        {
            string consultaUsuario = "INSERT IGNORE INTO usuarios (documento, nombre, apellido, email) VALUES (@doc, @nom, @ape, @mail)";
            
            string consultaTarjeta = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, saldo, estado, dni_titular) VALUES (@nroTarjeta, @banco, @saldo, 'ACTIVA', @doc)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    using (MySqlCommand comandoUsuario = new MySqlCommand(consultaUsuario, conexion))
                    {
                        comandoUsuario.Parameters.AddWithValue("@doc", doc);
                        comandoUsuario.Parameters.AddWithValue("@nom", nom);
                        comandoUsuario.Parameters.AddWithValue("@ape", ape);
                        comandoUsuario.Parameters.AddWithValue("@mail", mail);
                        comandoUsuario.ExecuteNonQuery(); 
                    }

                    using (MySqlCommand comandoTarjeta = new MySqlCommand(consultaTarjeta, conexion))
                    {
                        comandoTarjeta.Parameters.AddWithValue("@nroTarjeta", nroTarjeta);
                        comandoTarjeta.Parameters.AddWithValue("@banco", banco);
                        comandoTarjeta.Parameters.AddWithValue("@saldo", saldoInicial);
                        comandoTarjeta.Parameters.AddWithValue("@doc", doc);

                        int filas = comandoTarjeta.ExecuteNonQuery();
                        return filas > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de restricción o duplicación en MySQL: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA LIQUIDACIÓN MENSUAL ---");

            Console.Write("Ingrese el Número de Cuenta del cliente: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.Write("Ingrese el Período (Formato AAAA-MM, ej: 2026-06): ");
            string periodo = Console.ReadLine().Trim();

            Console.Write("Ingrese Fecha de Cierre (Formato AAAA-MM-DD): ");
            string fechaCierre = Console.ReadLine().Trim();

            Console.Write("Ingrese Fecha de Vencimiento (Formato AAAA-MM-DD): ");
            string fechaVence = Console.ReadLine().Trim();

            Console.Write("Ingrese el Total a Pagar ($): ");
            decimal totalPagar = Convert.ToDecimal(Console.ReadLine());

            Console.Write("Ingrese el Pago Mínimo ($): ");
            decimal pagoMinimo = Convert.ToDecimal(Console.ReadLine());

            bool exito = RegistrarLiquidacion(numCuenta, periodo, fechaCierre, fechaVence, totalPagar, pagoMinimo);

            if (exito)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n¡Liquidación emitida correctamente! Ya está disponible en el portal web.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError al emitir la liquidación. Verifique si el número de cuenta existe o si el período ya fue cargado.");
            }
            Console.ResetColor();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static bool RegistrarLiquidacion(int cuenta, string per, string cierre, string vence, decimal total, decimal minimo)
        {
            string consulta = @"INSERT INTO liquidaciones (num_cuenta, periodo, fecha_vencimiento, total_a_pagar, pago_minimo) 
                                VALUES (@cuenta, @per, @vence, @total, @minimo)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@cuenta", cuenta);
                        comando.Parameters.AddWithValue("@per", per);
                        comando.Parameters.AddWithValue("@vence", vence);
                        comando.Parameters.AddWithValue("@total", total);
                        comando.Parameters.AddWithValue("@minimo", minimo);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de MySQL al insertar liquidación: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }


        // Funciones a completar:

        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            // === A realizar ===
            // Aquí deben implementar un SELECT sobre la tabla 'tarjetas'
            // para recorrer las filas e imprimirlas en la consola.
            


            
            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            // === A realizar ===
            // Aquí deben realizar un SELECT con un JOIN entre 'tarjetas' y 'usuarios' 
            // filtrando por el numCuenta para traer todos los campos (Nombre, Apellido, Email, Saldo, etc.)
            
            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");
            
            if (Console.ReadLine().ToUpper() == "S")
            {
                // === A realizar ===
                // Aquí deben ejecutar un DELETE sobre la tabla 'tarjetas' donde num_cuenta = numCuenta.
                // Como definimos ON DELETE CASCADE en la base de datos, las liquidaciones se borrarán solas.
                // Opcional: Evaluar si también eliminan al usuario de la tabla 'usuarios' o si lo mantienen.
                
                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                else
                    Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }


        // =========================================================================
        // MÉTODOS BASE QUE DEBEN COMPLETAR CON LA LÓGICA 
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            string consulta = "SELECT num_cuenta, numero_tarjeta, banco_emisor, dni_titular FROM tarjetas";
            
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                    {
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                string numCuenta     = lector["num_cuenta"].ToString();
                                string numeroTarjeta = lector["numero_tarjeta"].ToString();
                                string bancoEmisor   = lector["banco_emisor"].ToString();
                                string dniTitular    = lector["dni_titular"].ToString();

                                Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", numCuenta, numeroTarjeta, bancoEmisor, dniTitular);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de MySQL: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            string consulta = @"SELECT t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.saldo, t.estado, 
                                    u.nombre, u.apellido, u.email, t.dni_titular 
                                FROM tarjetas t 
                                INNER JOIN usuarios u ON t.dni_titular = u.documento 
                                WHERE t.num_cuenta = @cuenta";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@cuenta", cuenta);

                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                Console.WriteLine("\n========================================");
                                Console.WriteLine("DATOS DEL TITULAR:");
                                Console.WriteLine($"Nombre Completo: {lector["apellido"]}, {lector["nombre"]}");
                                Console.WriteLine($"DNI/Documento  : {lector["dni_titular"]}");
                                Console.WriteLine($"Email Contacto : {lector["email"]}");
                                Console.WriteLine("========================================");
                                Console.WriteLine("DATOS DEL PLÁSTICO:");
                                Console.WriteLine($"Nro Cuenta     : {lector["num_cuenta"]}");
                                Console.WriteLine($"Nro Tarjeta    : {lector["numero_tarjeta"]}");
                                Console.WriteLine($"Banco Emisor   : {lector["banco_emisor"]}");
                                Console.WriteLine($"Saldo Actual   : ${lector["saldo"]}");
                                Console.WriteLine($"Estado Cuenta  : {lector["estado"]}");
                                Console.WriteLine("========================================");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"\nNo se encontró ninguna tarjeta con el número de cuenta: {cuenta}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de MySQL al consultar detalle: {ex.Message}");
                Console.ResetColor();
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            string consulta = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@cuenta", cuenta);

                        int filasAfectadas = comando.ExecuteNonQuery();

                        return filasAfectadas > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de MySQL al dar de baja: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}