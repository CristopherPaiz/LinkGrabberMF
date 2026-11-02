namespace LinkGrab.Constants
{
    // Every text the app shows, so wording and translation live in one place.
    public static class Strings
    {
        public const string AlreadyRunning = "LinkGrab ya está abierto. Búscalo en el área de notificación.";
        public const string Hello = "Pega enlaces con Ctrl+V.";

        public const string ButtonAdd = "Añadir";
        public const string ButtonSend = "Enviar a JD2";
        public const string ButtonGetLinks = "Obtener enlaces";
        public const string ButtonGetLinksCount = "Obtener enlaces ({0})";
        public const string ButtonVerify = "Verificar";
        public const string ButtonStart = "Iniciar";
        public const string ButtonStop = "Detener";
        public const string ButtonRemove = "Quitar";
        public const string ButtonSync = "Sincronizar";
        public const string ButtonSettings = "Ajustes";
        public const string ButtonHelp = "Ayuda";
        public const string ButtonSave = "Guardar";
        public const string ButtonCancel = "Cancelar";
        public const string ButtonClose = "Cerrar";
        public const string ButtonAccept = "Aceptar";
        public const string ButtonBrowse = "Elegir...";
        public const string ButtonReload = "Recargar";
        public const string ButtonSkip = "Omitir";
        public const string ButtonOpenJd = "Abrir JDownloader 2";

        public const string TipAdd = "Añade enlaces de archivos o carpetas de MediaFire.";
        public const string TipSend = "Manda lo seleccionado a LinkGrabber de JD2. Sin selección, manda todo lo pendiente.";
        public const string TipGetLinks = "Pide un enlace nuevo y lo cambia en JD2. Sin selección, actúa sobre los que tienen problemas.";
        public const string TipVerify = "Comprueba si los enlaces siguen vivos, sin descargarlos.";
        public const string TipStart = "Pasa lo seleccionado a descargas e inicia JD2.";
        public const string TipStop = "Detiene las descargas de JD2.";
        public const string TipRemove = "Quita de esta lista. JD2 no cambia.";
        public const string TipSync = "Importa lo que solo está en JD2 y marca lo que ya no está.";
        public const string TipSettings = "Carpeta de descarga, avisos y comportamiento al cerrar.";
        public const string TipHelp = "Explica qué hace cada botón y estado.";

        public const string NavAll = "Todos";
        public const string NavProblems = "Con problemas";
        public const string NavInJd = "En JD2";
        public const string NavDone = "Completados";
        public const string NavRemoved = "Quitados en JD2";

        public const string ColumnName = "Nombre";
        public const string ColumnSize = "Tamaño";
        public const string ColumnProgress = "Progreso";
        public const string ColumnState = "Estado";

        public const string MenuGetLink = "Obtener enlace";
        public const string MenuSend = "Enviar a JD2";
        public const string MenuVerify = "Verificar enlace";
        public const string MenuCopy = "Copiar enlace";
        public const string MenuRename = "Renombrar carpeta";
        public const string MenuRemove = "Quitar de la lista";
        public const string TrayOpen = "Abrir LinkGrab";
        public const string TrayExit = "Salir";
        public const string TrayProblems = "LinkGrab: {0} con problemas";

        public const string ActionGetLink = "Obtener enlace";
        public const string ActionSend = "Enviar";
        public const string ActionSendAgain = "Enviar de nuevo";
        public const string ActionSendFolder = "Enviar carpeta";

        public const string StateDone = "Completado";
        public const string StateDownloading = "Descargando";
        public const string StateBlocked = "Bloqueado por Cloudflare";
        public const string StateExpired = "Enlace caducado";
        public const string StateError = "No se pudo leer";
        public const string StateGrabber = "En LinkGrabber";
        public const string StateGrabberOffline = "Sin conexión en LinkGrabber";
        public const string StateRemoved = "Quitado en JD2";
        public const string StateSending = "Enviando a JD2";
        public const string StateReady = "Listo para enviar";
        public const string StateUnsent = "Sin enviar";
        public const string StateStopped = "Detenido";
        public const string StateQueued = "En cola";
        public const string FolderProblems = "{0} con problemas";
        public const string FolderDone = "{0} de {1} completados";

        public const string EmptyList = "Pega enlaces con Ctrl+V o pulsa Añadir";
        public const string EmptyFilter = "Nada en este filtro";
        public const string CountOne = "1 archivo";
        public const string CountMany = "{0} archivos";
        public const string CountSelected = "{0} seleccionados  ·  {1}";
        public const string CountWithSize = "{0}  ·  {1}";

        public const string NoLinksFound = "No encontré enlaces.";
        public const string Reading = "Leyendo {0}";
        public const string Added = "Añadidos {0} archivos.";
        public const string AddedWithFailure = "Añadidos {0}. Falló {1}: {2}";
        public const string Resolving = "Obteniendo enlaces {0} de {1}";
        public const string NothingToSend = "No hay archivos pendientes de enviar.";
        public const string Sent = "Enviados {0} a JD2.";
        public const string SentPending = "Enviados {0} a JD2. {1} necesitan enlace.";
        public const string NothingToVerify = "No hay enlaces que verificar.";
        public const string Verified = "Verificados {0}. Con problemas: {1}.";
        public const string NothingToGet = "Nada que obtener (se omiten completados y en descarga).";
        public const string LinksObtained = "Enlaces obtenidos: {0} de {1}.";
        public const string SelectFolder = "Selecciona una carpeta.";
        public const string FolderInJd = "Esa carpeta ya está en JD2; renómbrala allí.";
        public const string FolderRenamed = "Carpeta renombrada.";
        public const string Copied = "Copiados {0} enlaces.";
        public const string MovedAndStarted = "Pasados {0} a descargas e iniciado.";
        public const string Started = "Descargas iniciadas.";
        public const string Stopped = "Descargas detenidas.";
        public const string SelectToRemove = "Selecciona archivos para quitar.";
        public const string Removed = "Quitados {0} de la lista. JD2 no cambia.";
        public const string Importing = "Importando desde JD2: {0} de {1}";
        public const string Synced = "Sincronizado: {0} importados, {1} ya no están en JD2.";
        public const string AutoRenewed = "Renovados automáticamente: {0}.";
        public const string SessionUnreadable = "No pude leer la sesión anterior.";
        public const string SessionUnsaved = "No pude guardar la sesión: {0}";

        public const string JdPrefix = "JD2 {0}";
        public const string JdWithSpeed = "JD2 {0}  ·  {1}/s";
        public const string JdCollecting = "procesando enlaces";
        public const string JdDownloading = "descargando";
        public const string JdPaused = "en pausa";
        public const string JdStopping = "deteniendo";
        public const string JdIdle = "detenido";
        public const string JdClosed = "JD2 cerrado: ábrelo";
        public const string JdApiOff = "API de JD2 apagada: clic para activarla";

        public const string ToastClosedTitle = "JD2 está cerrado";
        public const string ToastClosedBody = "Ábrelo para enviar enlaces y seguir vigilando.";
        public const string ToastApiTitle = "Falta activar la API de JD2";
        public const string ToastApiBody = "Pulsa para ver los pasos.";
        public const string ToastBackTitle = "JD2 conectado";
        public const string ToastBackBody = "Vigilando descargas otra vez.";
        public const string ToastOneProblem = "Un enlace necesita atención";
        public const string ToastManyProblems = "{0} enlaces necesitan atención";

        public const string AddTitle = "Añadir enlaces";
        public const string AddPrompt = "Pega enlaces de archivos o carpetas, uno por línea:";
        public const string RenameTitle = "Renombrar carpeta";
        public const string RenamePrompt = "Nombre de la carpeta en JD2:";

        public const string CloseTitle = "Cerrar LinkGrab";
        public const string CloseMessage = "En el área de notificación sigue vigilando JD2 y te avisa si un enlace falla. Si sales, deja de vigilar.";
        public const string CloseRemember = "Recordar mi elección";
        public const string CloseTray = "Área de notificación";
        public const string CloseMinimize = "Minimizar";
        public const string CloseExit = "Salir";
        public const string CloseAsk = "Preguntar";

        public const string SettingsTitle = "Ajustes";
        public const string SettingsUseJdFolder = "Descargar en la carpeta de JD2";
        public const string SettingsUseJdFolderAt = "Descargar en la carpeta de JD2  ({0})";
        public const string SettingsAutostart = "Descargar en cuanto llegue a JD2";
        public const string SettingsAutoRenew = "Renovar solo los enlaces confirmados como caducados";
        public const string SettingsNotify = "Mostrar avisos cuando un enlace falle";
        public const string SettingsPoll = "Consultar JD2 cada (segundos):";
        public const string SettingsOnClose = "Al cerrar la ventana:";

        public const string JdWindowTitle = "JDownloader 2";
        public const string JdClosedHeading = "Abre JDownloader 2";
        public const string JdClosedSteps = "LinkGrab trabaja junto a JDownloader 2, y ahora mismo no está abierto.\r\n\r\n"
            + "Ábrelo y espera a que cargue. Esta ventana se cierra sola cuando lo detecte.";
        public const string JdApiHeading = "Falta activar la API local de JDownloader 2";
        public const string JdApiSteps = "1.  En JDownloader 2 entra en Configuración y luego en Ajustes avanzados.\r\n"
            + "     (Settings > Advanced Settings; el nombre cambia según el idioma).\r\n\r\n"
            + "2.  En el buscador escribe:  deprecated\r\n\r\n"
            + "3.  Busca RemoteAPI: Deprecated Api y actívala\r\n"
            + "     (deprecatedapienabled = true).\r\n\r\n"
            + "4.  Reinicia JDownloader 2.\r\n\r\n"
            + "La API solo escucha en este equipo (127.0.0.1) y no tiene contraseña.";

        public const string BrowserTitle = "Obtener enlaces";
        public const string BrowserProgress = "{0} de {1}  ·  {2}";
        public const string BrowserHint = "Si aparece un challenge, resuélvelo. El enlace se captura solo.";
        public const string BrowserCaptured = "Enlace capturado, enviando a JD2...";
        public const string BrowserNoRuntime = "Este equipo no tiene WebView2";
        public const string BrowserInstall = "Instálalo desde {0} y vuelve a intentarlo.";
        public const string BrowserFailed = "No pude abrir el navegador integrado";

        public const string ErrorNoRule = "ninguna regla de rules.ini coincide";
        public const string ErrorRuleBrowser = "esta regla usa el navegador";
        public const string ErrorLinkNotFound = "enlace no encontrado; revisa la regla [{0}]";
        public const string ErrorNeedsBrowser = "la página necesita el navegador";
        public const string ErrorChallenge = "Cloudflare pide resolver un challenge";
        public const string ErrorMissingFile = "falta {0}";
        public const string ErrorBadRuleLine = "línea inválida en rules.ini: {0}";
        public const string ErrorUnknownRuleKey = "clave desconocida en rules.ini: {0}";
        public const string ErrorIncompleteRule = "la regla [{0}] necesita match, element y attr";
        public const string ErrorMediaFireRejected = "MediaFire rechazó la consulta (¿privado o eliminado?): {0}";
        public const string ErrorMediaFire = "MediaFire: {0}";
        public const string ErrorJdNoLink = "JD2 no recibió el enlace nuevo de {0}";
        public const string ErrorJdNoMove = "JD2 no pasó a descargas el enlace nuevo de {0}";

        public const string HelpTitle = "Ayuda de LinkGrab";
        public const string HelpIntro = "LinkGrab saca el enlace directo de MediaFire y lo manda a JDownloader 2. "
            + "También vigila JD2 y repara los enlaces que fallan, sin cambiar su posición en la lista de descargas.";
        public const string HelpButtons = "Botones";
        public const string HelpStates = "Estados";
        public const string HelpFilters = "Filtros";
        public const string HelpKeys = "Atajos y selección";
    }
}
