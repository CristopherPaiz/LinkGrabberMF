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
    }

    // What the help window explains, section by section.
    public static class HelpContent
    {
        public static readonly HelpEntry[] Buttons =
        {
            new HelpEntry(Glyph.Add, Strings.ButtonAdd, Strings.TipAdd + " Las carpetas se leen con sus subcarpetas."),
            new HelpEntry(Glyph.Send, Strings.ButtonSend, Strings.TipSend + " Cada subcarpeta llega como un paquete."),
            new HelpEntry(Glyph.Link, Strings.ButtonGetLinks, Strings.TipGetLinks + " Si la página lo exige, abre el navegador integrado."),
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
            new HelpEntry(null, Strings.StateGrabber, "Está en LinkGrabber de JD2, sin descargar todavía."),
            new HelpEntry(null, Strings.StateBlocked, "Cloudflare frenó la página. Pulsa Obtener enlace en esa fila."),
            new HelpEntry(null, Strings.StateExpired, "El enlace directo ya no sirve. Obtener enlace pide otro."),
            new HelpEntry(null, Strings.StateError, "No se pudo leer la página; puede estar eliminada o haber cambiado."),
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

        public static readonly HelpEntry[] Keys =
        {
            new HelpEntry(null, "Ctrl+V", "Añade los enlaces del portapapeles."),
            new HelpEntry(null, "Ctrl+clic, Shift+clic, arrastrar", "Selecciona varias filas. Una carpeta incluye lo que tiene dentro."),
            new HelpEntry(null, "Ctrl+A", "Selecciona todo."),
            new HelpEntry(null, "Supr", "Quita lo seleccionado de la lista."),
            new HelpEntry(null, "F5", "Sincroniza con JD2."),
            new HelpEntry(null, "F1", "Abre esta ayuda."),
            new HelpEntry(null, "Clic derecho", "Copiar enlace, renombrar carpeta y más.")
        };
    }
}
