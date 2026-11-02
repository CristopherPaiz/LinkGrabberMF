# LinkGrab

Aplicación de escritorio para Windows que saca el enlace directo de MediaFire y lo manda a JDownloader 2 (JD2). También vigila JD2, avisa cuando un enlace falla y lo repara sin cambiarlo de posición en la lista de descargas.

## Qué hace

- Lee enlaces de archivos y carpetas de MediaFire, con sus subcarpetas.
- Añade solos los enlaces de MediaFire que copies, y avisa. Solo mira si lo copiado es un enlace de MediaFire; se puede apagar en Ajustes.
- Saca el enlace directo y lo deja copiado, para usarlo con o sin JD2.
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

Cada compilación reemplaza `LinkGrab.exe` en `dist\` y borra lo demás, salvo `LinkGrabData\`, que guarda tu lista y tus ajustes. Para compartir la app, pasa solo `LinkGrab.exe`. La primera compilación descarga WebView2 desde NuGet y comprueba su hash SHA-256.

Cada compilación también ejecuta los tests de `tests\`, y falla si alguno no pasa. Usan un JD2 simulado, así que no hace falta tenerlo abierto. Para ejecutarlos solos:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\test.ps1
```

Cierra LinkGrab antes de compilar; si se está ejecutando desde `dist\`, la compilación se detiene sin borrar nada.
## Errores

Un error que nadie controló abre una ventana con el botón `Copiar error`, para mandar el texto a quien mantiene la app. Con los errores de la ventana principal la app sigue abierta. Con los de un hilo en segundo plano .NET siempre cierra la app, así que se muestra el error antes. Fallos de memoria o de código nativo pueden cerrarla sin aviso.

## Probar desde cero

Este script abre la app en una carpeta temporal vacía y sin datos guardados. No toca tus datos reales, y al cerrar la app borra esa carpeta:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\smoke.ps1
```

JD2 no se reinicia con el script. Para probar la primera vez de verdad, apaga antes `deprecatedapienabled` y cierra JD2. Luego comprueba:

1. Sale la pantalla de carga y después la ventana, con los botones de JD2 en gris y el botón naranja.
2. El aviso aparece al pasar el ratón sobre un botón gris.
3. El botón naranja abre la guía, y seguir sus pasos la cierra sola y activa los botones.
4. Una carpeta de MediaFire añadida muestra sus subcarpetas.
5. Enviar a JD2 la deja en LinkGrabber, con un paquete por subcarpeta.
6. Obtener enlaces, con un challenge, abre el navegador integrado y captura el enlace.
7. Cerrar la ventana pregunta qué hacer, y al volver a abrir sigue la lista.

## Archivos que crea

- `LinkGrabData\` junto al exe: `rules.ini`, `session.json` y `settings.json`. Si no puede escribir ahí, usa `%LocalAppData%\LinkGrab\data`.
- `%LocalAppData%\LinkGrab`: el perfil del navegador integrado y las DLL de WebView2, que van comprimidas dentro del exe y se extraen la primera vez que se abre el navegador.

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
scripts/         restore, make-icon, build, test y smoke
tests/           Tests sin dependencias y JD2 simulado
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

El exe no está firmado, es nuevo, sin reputación, y lleva DLL comprimidas dentro. Por eso algunos antivirus pueden marcarlo aunque no tenga código malicioso. El código está aquí para revisarlo y compilarlo uno mismo.

Lo que reduce esas alertas es firmar el exe con un certificado de código y reportar el falso positivo a cada proveedor.
