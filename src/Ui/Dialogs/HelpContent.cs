using LinkGrab.Constants;

namespace LinkGrab.Ui.Dialogs
{
    public class HelpEntry
    {
        public HelpEntry(string glyph, string title, string text)
        {
            Glyph = glyph;
            Title = title;
            Text = text;
        }

        public string Glyph { get; private set; }
        public string Title { get; private set; }
        public string Text { get; private set; }

        // Where to get it, for what LinkGrab needs installed; either may be missing.
        public string PageUrl { get; set; }
        public string DirectUrl { get; set; }
    }

    // What the help window explains, section by section.
    public static class HelpContent
    {
        public static readonly HelpEntry[] Buttons =
        {
            new HelpEntry(Glyph.Add, Strings.ButtonAdd, Strings.TipAdd + " Las carpetas se leen con sus subcarpetas."),
            new HelpEntry(Glyph.Warning, Strings.ButtonActivateApi, Strings.TipActivate + " Mientras tanto, lo que usa JD2 queda desactivado."),
            new HelpEntry(Glyph.Send, Strings.ButtonSend,Strings.TipSend + " Cada subcarpeta llega como un paquete."),
            new HelpEntry(Glyph.Link, Strings.ButtonGetLinks, Strings.TipGetLinks + " Si la página lo exige, abre el navegador integrado."),
            new HelpEntry(null, Strings.ActionCopy + " (en la fila)", Strings.TipCopy),
            new HelpEntry(null, Strings.ActionReload + " (en la fila)", Strings.TipReload),
            new HelpEntry(null, Strings.ActionReplace + " (en la fila)", Strings.TipReplace),
            new HelpEntry(null, Strings.ActionCopyAll, "Arriba de la lista. Copia los enlaces de todo lo que se ve, o solo de lo seleccionado, y entonces indica cuántos."),
            new HelpEntry(Glyph.Check, Strings.ButtonVerify, Strings.TipVerify),
            new HelpEntry(Glyph.Play, Strings.ButtonStart, Strings.TipStart),
            new HelpEntry(Glyph.Stop, Strings.ButtonStop, Strings.TipStop),
            new HelpEntry(Glyph.Delete, Strings.ButtonRemove, Strings.TipRemove),
            new HelpEntry(Glyph.Sync, Strings.ButtonSync, Strings.TipSync),
            new HelpEntry(Glyph.Settings, Strings.ButtonSettings, Strings.TipSettings)
        };

        public static readonly HelpEntry[] States =
        {
            new HelpEntry(null, Strings.StateUnsent, "Aún no se ha mandado a JD2."),
            new HelpEntry(null, Strings.StateReady, "Ya tiene enlace directo y falta mandarlo."),
            new HelpEntry(null, Strings.StateGrabber, "Está en LinkGrabber de JD2 con su enlace directo, sin descargar todavía."),
            new HelpEntry(null, Strings.StateJdPage, "JD2 tiene la página de MediaFire, no el enlace directo; suele pasar si JD2 lo tomó del portapapeles. Obtener enlace lo cambia en el mismo paquete."),
            new HelpEntry(null, Strings.StateBlocked, "Cloudflare frenó la página. Pulsa Obtener enlace en esa fila."),
            new HelpEntry(null, Strings.StateExpired, "El enlace directo ya no sirve. Obtener enlace pide otro."),
            new HelpEntry(null, Strings.StateError, "No se pudo conseguir el enlace; el archivo puede estar eliminado o bloqueado por MediaFire."),
            new HelpEntry(null, Strings.StateRemoved, "Lo mandaste, pero ya no está en JD2. Puedes enviarlo de nuevo.")
        };

        public static readonly HelpEntry[] Filters =
        {
            new HelpEntry(Glyph.List, Strings.NavAll, "Toda la lista, ordenada por carpetas."),
            new HelpEntry(Glyph.Warning, Strings.NavProblems, "Solo lo que necesita que hagas algo."),
            new HelpEntry(Glyph.Download, Strings.NavInJd, "Lo que JD2 tiene ahora, en LinkGrabber o en descargas."),
            new HelpEntry(Glyph.Done, Strings.NavDone, "Descargas terminadas."),
            new HelpEntry(Glyph.Gone, Strings.NavRemoved, "Lo que se borró directamente en JD2.")
        };

        public static readonly HelpEntry[] Requirements =
        {
            new HelpEntry(null, "Windows 10 u 11", "De 64 bits. En Windows de 32 bits no abre."),
            new HelpEntry(null, ".NET Framework", "4.6.2 o superior. Windows 10 y 11 ya lo traen; si faltara, LinkGrab no abriría.")
            {
                PageUrl = DotNetFiles.PageUrl,
                DirectUrl = DotNetFiles.InstallUrl
            },
            new HelpEntry(null, "WebView2", "Solo para el navegador integrado, cuando la página pide comprobar que eres una persona. Windows 11 lo trae.")
            {
                PageUrl = WebViewFiles.PageUrl,
                DirectUrl = WebViewFiles.InstallUrl
            },
            new HelpEntry(null, "JDownloader 2", "Para enviar, reemplazar e iniciar descargas. Sin él puedes obtener y copiar enlaces. Su descarga está solo en su página, porque los instaladores cambian con cada versión.")
            {
                PageUrl = JdApi.DownloadPage
            }
        };

        public static readonly HelpEntry[] Keys =
        {
            new HelpEntry(null, "Copiar un enlace", "Con la app abierta o en el área de notificación, los enlaces de MediaFire que copies se añaden solos. Se puede apagar en Ajustes."),
            new HelpEntry(null, "Ctrl+V", "Añade los enlaces del portapapeles."),
            new HelpEntry(null, "Ctrl+clic, Shift+clic, arrastrar", "Selecciona varias filas. Una carpeta incluye lo que tiene dentro."),
            new HelpEntry(null, "Casillas", "La de cada fila suma o quita esa fila. La de arriba selecciona todo, y su flecha ofrece otras formas: sin enlace, con enlace, por enviar o con problemas."),
            new HelpEntry(null, "Ctrl+A", "Selecciona todo."),
            new HelpEntry(null, "Supr", "Quita lo seleccionado de la lista."),
            new HelpEntry(null, "F5", "Sincroniza con JD2."),
            new HelpEntry(null, "F1", "Abre esta ayuda."),
            new HelpEntry(null, "Clic derecho", "Copiar enlace, renombrar carpeta y más.")
        };
    }
}
