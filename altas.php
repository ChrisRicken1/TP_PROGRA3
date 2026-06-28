<?php
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
} catch (\PDOException $e) {
    die("Error al conectar a la base de datos: " . $e->getMessage());
}

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    
    $tipo_doc         = trim($_POST['tipo_doc'] ?? '');
    $documento        = trim($_POST['documento'] ?? '');
    $nombre           = trim($_POST['nombre'] ?? '');
    $apellido         = trim($_POST['apellido'] ?? '');
    $fecha_nacimiento = trim($_POST['fecha_nacimiento'] ?? '');
    $email            = trim($_POST['email'] ?? '');
    $usuario          = trim($_POST['usuario'] ?? '');
    $passwordA        = trim($_POST['passwordA'] ?? '');
    $passwordB        = trim($_POST['passwordB'] ?? '');

    if ($passwordA !== $passwordB) {
        die("Error: Las contraseñas no coinciden.");
    }

    if ($tipo_doc !== 'DNI' && $tipo_doc !== 'PASAPORTE') {
        die("Error: El tipo de documento debe ser DNI o PASAPORTE.");
    }

    try {
        $stmt = $pdo->prepare("SELECT * FROM usuarios WHERE documento = ?");
        $stmt->execute([$documento]);
        $user_existe = $stmt->fetch();

        if (!$user_existe) {
            die("Error: El documento ingresado no se encuentra registrado en el sistema comercial del banco. Contacte a soporte.");
        }
        if (!is_null($user_existe['usuario'])) {
            die("Error: Este usuario ya tiene una cuenta web activa.");
        }


        $sql_update = "UPDATE usuarios 
                       SET usuario = ?, password = ?, email = ?, tipo_doc = ? 
                       WHERE documento = ?";
        
        $stmt = $pdo->prepare($sql_update);
        $stmt->execute([$usuario, $passwordA, $email, $tipo_doc, $documento]);

        echo "<script>
                alert('¡Tu usuario web ha sido activado con éxito! Ya podés iniciar sesión.');
                window.location.href = 'ingreso.html';
              </script>";
        exit;

    } catch (Exception $e) {
        die("Error al activar la cuenta: " . $e->getMessage());
    }
}

