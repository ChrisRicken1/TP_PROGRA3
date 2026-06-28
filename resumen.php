<?php

session_start();

if (!isset($_SESSION['usuario_logueado'])) {
    header("Location: ingreso.html");
    exit;
}


$usuario_actual = $_SESSION['usuario_logueado'];

$host = "localhost";
$db   = "mi_banco_db";
$user = "root";
$pass = ""; 
$charset = "utf8mb4";

$dsn = "mysql:host=$host;dbname=$db;charset=$charset";
$options = [
    PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    PDO::ATTR_EMULATE_PREPARES   => false,
];

try {
    $pdo = new PDO($dsn, $user, $pass, $options);


    $stmt = $pdo->prepare("SELECT * FROM tarjetas WHERE dni_titular = ? LIMIT 1");
    $stmt->execute([$usuario_actual['documento']]);
    $tarjeta = $stmt->fetch();

    if (!$tarjeta) {
        die("Error: No se encontró ninguna tarjeta asociada a este usuario.");
    }

    $num_cuenta = $tarjeta['num_cuenta'];


    $stmt = $pdo->prepare("SELECT * FROM liquidaciones WHERE num_cuenta = ? ORDER BY periodo DESC LIMIT 1");
    $stmt->execute([$num_cuenta]);
    $liquidacion_actual = $stmt->fetch();


    $stmt = $pdo->prepare("SELECT * FROM liquidaciones WHERE num_cuenta = ? ORDER BY periodo DESC");
    $stmt->execute([$num_cuenta]);
    $historial_liquidaciones = $stmt->fetchAll();


} catch (\PDOException $e) {
    die("Error en la base de datos: " . $e->getMessage());
}


?>


<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mis Tarjetas - Resumen</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col justify-between">

    <header class="bg-[#004691] text-white text-center py-4 shadow-md">
        <div class="flex justify-between items-center px-6">
            <h1 class="text-xl font-semibold">Mis <span class="font-bold">Tarjetas</span></h1>
            <div class="flex items-center space-x-4">
                <span class="text-sm">¡Hola, <?php echo htmlspecialchars($usuario_actual['nombre']); ?>!</span>
                <a href="cerrar_sesion.php" class="text-sm hover:underline">Cerrar Sesión</a>
            </div>
        </div>
    </header>

    <main class="flex-grow container mx-auto p-6">
        

        <div class="mb-8">
            <h2 class="text-xl font-bold text-gray-700 mb-4">Tarjeta Actual</h2>
            <div class="bg-gradient-to-r from-blue-900 to-[#004691] text-white p-6 rounded-lg shadow-lg relative overflow-hidden min-h-[200px] flex flex-col justify-between">
                <div class="flex justify-between items-start">
                    <h3 class="text-xl font-semibold uppercase tracking-wider">Progra3card</h3>
                    <div class="text-4xl font-bold bg-white/20 p-2 rounded">Visa</div>
                </div>
                
                <div class="mt-4">
                    <p class="text-xl font-mono tracking-widest mb-2">**** **** **** <?php echo htmlspecialchars(substr($tarjeta['numero_tarjeta'], -4)); ?></p>
                    <p class="text-sm opacity-90">Titular: <?php echo htmlspecialchars($tarjeta['titular']); ?></p>
                </div>

                <div class="flex justify-between items-end mt-4">
                    <div>
                        <p class="text-xs opacity-80">Vence</p>
                        <p class="text-sm font-bold"><?php echo htmlspecialchars($tarjeta['fecha_vencimiento']); ?></p>
                    </div>
                    <div class="text-4xl font-bold bg-white/10 p-3 rounded-full">C</div>
                </div>
            </div>
        </div>


        <div>
            <h2 class="text-xl font-bold text-gray-700 mb-4">Resumen de Liquidación</h2>
            
            <?php if ($liquidacion_actual): ?>
                
                <div class="bg-white p-6 rounded-lg shadow-lg border-t-4 border-[#004691]">
                    

                    <div class="flex justify-between items-center border-b pb-4 mb-4">
                        <div>
                            <h3 class="text-2xl font-bold text-[#004691]">Liquidación <?php echo htmlspecialchars($liquidacion_actual['periodo']); ?></h3>
                            <p class="text-sm text-gray-500">Período: <?php echo htmlspecialchars($liquidacion_actual['periodo_inicio']); ?> al <?php echo htmlspecialchars($liquidacion_actual['periodo_fin']); ?></p>
                        </div>
                        <span class="text-3xl font-bold $<?php echo htmlspecialchars($liquidacion_actual['total_resumen']); ?>"></span>
                    </div>


                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                        <div class="space-y-2">
                            <div class="flex justify-between">
                                <span class="text-gray-600">Compras:</span>
                                <span>$<?php echo htmlspecialchars($liquidacion_actual['compras']); ?></span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-gray-600">Vencimientos:</span>
                                <span>$<?php echo htmlspecialchars($liquidacion_actual['vencimientos']); ?></span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-gray-600">Costo Total:</span>
                                <span>$<?php echo htmlspecialchars($liquidacion_actual['costo_total']); ?></span>
                            </div>
                        </div>
                        
                        <div class="space-y-2">
                            <div class="flex justify-between">
                                <span class="text-gray-600">Saldo a Pagar:</span>
                                <span class="text-green-600 font-bold">$<?php echo htmlspecialchars($liquidacion_actual['saldo_a_pagar']); ?></span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-gray-600">Créditos:</span>
                                <span class="text-red-600 font-bold">-$<?php echo htmlspecialchars($liquidacion_actual['creditos']); ?></span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-gray-600">Saldo a Favor:</span>
                                <span class="text-red-600 font-bold">-$<?php echo htmlspecialchars($liquidacion_actual['saldo_a_favor']); ?></span>
                            </div>
                        </div>
                    </div>

                    <div class="mt-6 flex justify-center space-x-4">
                        <button class="bg-[#004691] hover:bg-blue-800 text-white px-6 py-2 rounded-full transition duration-200">Ver Liquidación Completa</button>
                        <button class="border border-[#004691] hover:bg-gray-50 text-[#004691] px-6 py-2 rounded-full transition duration-200">Descargar</button>
                    </div>
                </div>

            <?php else: ?>
                <div class="text-center py-12">
                    <p class="text-gray-500 text-lg">No hay liquidaciones disponibles</p>
                </div>
            <?php endif; ?>
        </div>


        <div class="mt-8">
            <h2 class="text-xl font-bold text-gray-700 mb-4">Historial de Liquidaciones</h2>
            
            <div class="bg-white rounded-lg shadow overflow-hidden">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Período</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Compras</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Vencimientos</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Saldo</th>
                            <th scope="col" class="relative px-6 py-3">
                                <span class="sr-only">Acciones</span>
                            </th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <?php if (!empty($historial_liquidaciones)): ?>
                            <?php foreach ($historial_liquidaciones as $liq): ?>
                                <tr>
                                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900"><?php echo htmlspecialchars($liq['periodo']); ?></td>
                                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">$<?php echo htmlspecialchars($liq['compras']); ?></td>
                                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">$<?php echo htmlspecialchars($liq['vencimientos']); ?></td>
                                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">$<?php echo htmlspecialchars($liq['total_resumen']); ?></td>
                                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium $<?php echo htmlspecialchars($liq['saldo_a_favor']); ?>">-$<?php echo htmlspecialchars($liq['saldo_a_favor']); ?></td>
                                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <a href="#" class="text-[#004691] hover:text-blue-900">Ver</a>
                                    </td>
                                </tr>
                            <?php endforeach; ?>
                        <?php else: ?>
                            <tr>
                                <td colspan="6" class="px-6 py-4 text-center text-gray-500">No hay liquidaciones en el historial.</td>
                            </tr>
                        <?php endif; ?>
                    </tbody>
                </table>
            </div>
        </div>

    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200">
        Portal Oficial de Consultas de Liquidaciones Progra3card.
    </footer>
</body>
</html>