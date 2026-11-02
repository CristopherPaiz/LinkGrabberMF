# LinkGrab

Aplicación de escritorio para Windows que saca el enlace directo de MediaFire y lo manda a JDownloader 2 (JD2). También vigila JD2, avisa cuando un enlace falla y lo repara sin cambiarlo de posición en la lista de descargas.

## Qué hace

- Lee enlaces de archivos y carpetas de MediaFire, con sus subcarpetas.
- Manda cada carpeta a JD2 como un paquete, en la carpeta de descargas de JD2.
- Vigila JD2 desde el área de notificación y avisa si una descarga se bloquea.
- Pide un enlace nuevo y lo coloca en el lugar del viejo.
- Abre un navegador integrado cuando una página, como un challenge de Cloudflare, necesita a una persona.
- Sincroniza la lista con JD2: importa lo que solo está allí y marca lo que se borró.

Dentro de la app, `Ayuda` o `F1` explica cada botón, estado y atajo.

## Requisitos para usarla

- Windows 10 o 11 de 64 bits, con .NET Framework 4.6.2 o superior.
- JDownloader 2 con la API local activada:
  1. Abre `Configuración > Ajustes avanzados`.
  2. Busca `deprecated`.
  3. Activa `deprecatedapienabled`.
  4. Reinicia JD2.
- WebView2 Runtime, solo para el navegador integrado. Windows 11 suele traerlo.

## Compilar

No hace falta instalar SDK: se usa el compilador de C# incluido en Windows. Haz doble clic en `build.cmd`, o ejecuta:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build.ps1
```

El resultado es `dist\LinkGrab.exe`, un solo archivo que puedes copiar a otro PC. La primera compilación descarga WebView2 desde NuGet y comprueba su hash SHA-256.

## Archivos que crea

- `LinkGrabData\` junto al exe: `rules.ini`, `session.json` y `settings.json`. Si no puede escribir ahí, usa `%LocalAppData%\LinkGrab\data`.
- `%LocalAppData%\LinkGrab`: las DLL de WebView2 y el perfil del navegador integrado.

## Si un sitio cambia

Edita `LinkGrabData\rules.ini`, sin recompilar:

```ini
[mediafire]
match = ^https?://(www\.)?mediafire\.com/(file|file_premium|download)/
element = id="downloadButton"
attr = data-scrambled-url:base64, href
render = auto
```

| Clave | Significado |
|---|---|
| `match` | Expresión regular que debe cumplir la URL |
| `element` | Texto que identifica la etiqueta HTML con el enlace |
| `attr` | Atributos a leer, en orden; `:base64` si van codificados |
| `render` | `auto`, `http` o `browser` |

## Estructura

```
assets/          Manifiesto y rules.ini por defecto
scripts/         restore, make-icon y build
src/
  Constants/     Textos de la interfaz y constantes
  Models/        FileItem y Settings
  Services/      MediaFire, JD2, reglas y lógica de enlaces
  Ui/Controls/   Botones, navegación y barra de actividad
  Ui/Dialogs/    Ajustes, ayuda, avisos y navegador integrado
  Ui/Grid/       Lista de archivos
  Ui/Main/       Ventana principal
```

## Antivirus y falsos positivos

El exe no está firmado y contiene las DLL de WebView2, que copia a `%LocalAppData%\LinkGrab` para cargarlas. Por eso algunos antivirus con detección heurística pueden marcarlo aunque no tenga código malicioso. El código está aquí para revisarlo y compilarlo uno mismo.

Lo que reduce esas alertas es firmar el exe con un certificado de código y reportar el falso positivo a cada proveedor.
