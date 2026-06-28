<?php
session_start();

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
    
    $tipo_doc  = trim($_POST['tipo_doc'] ?? '');
    $documento = trim($_POST['documento'] ?? '');
    $usuario   = trim($_POST['usuario'] ?? '');
    $password  = trim($_POST['password'] ?? '');

    if ($tipo_doc === "" || $documento === "" || $usuario === "" || $password === "") {
        die("Error: Debe completar todos los campos.");
    }

    try {
        $stmt = $pdo->prepare("SELECT * FROM usuarios WHERE documento = ? AND tipo_doc = ? AND usuario = ? AND password = ?");
        $stmt->execute([$documento, $tipo_doc, $usuario, $password]);
        $usuario_db = $stmt->fetch();

        if ($usuario_db) {
            $_SESSION['usuario_logueado'] = $usuario_db;
            $_SESSION['loggedin'] = true;
            header("Location: resumen.php");
            exit;
        } else {
            echo "<script>alert('Datos de ingreso incorrectos. Verifique documento, usuario y contraseña.'); window.location.href='ingreso.html';</script>";
        }

    } catch (PDOException $e) {
        die("Error de conexión: " . $e->getMessage());
    }
}
?>
