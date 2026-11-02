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
        public const string ButtonActivateApi = "Activar API de JD2";
        public const string ButtonOpenJdShort = "Abrir JD2";

        public const string TipAdd = "Añade enlaces de archivos o carpetas de MediaFire.";
        public const string TipNeedsApi = "Esto usa la API de JD2, que está apagada. Pulsa \"" + ButtonActivateApi + "\" para encenderla.";
        public const string TipNeedsJdOpen = "Esto usa la API de JD2, y JD2 está cerrado. Pulsa \"" + ButtonOpenJdShort + "\".";
        public const string TipActivate ="Muestra los pasos para conectar con JD2. Aparece solo cuando hace falta.";
        public const string TipSend ="Manda lo seleccionado a LinkGrabber de JD2. Sin selección, manda todo lo pendiente.";
        public const string TipGetLinks = "Saca el enlace directo de lo que aún no lo tiene, uno por uno, y lo copia. Lo que ya tiene enlace no se vuelve a pedir. No necesita JD2.";
        public const string TipVerify = "Comprueba si los enlaces ya obtenidos siguen funcionando. No pide enlaces nuevos.";
        public const string TipCopy = "Copia el enlace directo de esa fila.";
        public const string TipReload = "Pide un enlace nuevo para esa fila y lo copia. Si el archivo está en JD2, lo cambia allí.";
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
        public const string SelectAll = "Seleccionar todos";
        public const string SelectNone = "Quitar selección";
        public const string SelectNoLink = "Los que no tienen enlace";
        public const string SelectWithLink = "Los que ya tienen enlace";
        public const string SelectUnsent = "Los que faltan por enviar";
        public const string SelectProblems = "Los que tienen problemas";
        public const string ActionCopy = "Copiar";
        public const string ActionCopyAll = "Copiar todos";
        public const string ActionCopyCount = "Copiar ({0})";
        public const string ActionCopied = "¡Copiado!";
        public const string ActionCopiedCount = "¡Copiados {0}!";
        public const string ButtonVerified = "Verificado";
        public const string ButtonSynced = "Sincronizado";
        public const string CopyFailed = "No se pudo copiar: otro programa está usando el portapapeles. Inténtalo de nuevo.";
        public const string StateVerifying = "Verificando...";
        public const string ActionReload = "Recargar";
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
        public const string StateResolving = "Obteniendo enlace...";
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
        public const string AddedWithUnsupported = "Añadidos {0}. {1}";
        public const string Resolving = "Obteniendo enlaces {0} de {1}";
        public const string NothingToSend = "No hay archivos pendientes de enviar.";
        public const string Sent = "Enviados {0} a JD2.";
        public const string SentPending = "Enviados {0} a JD2. {1} necesitan enlace.";
        public const string NothingToVerify = "Aún no hay enlaces que verificar. Pulsa Obtener enlaces primero.";
        public const string Verified = "Verificados {0}. Con problemas: {1}.";
        public const string NothingToGet = "Todo tiene enlace ya. Para pedir uno nuevo, usa Recargar en su fila.";
        public const string GettingBlocked = "Obteniendo {0} enlace(s) protegidos por Cloudflare...";
        public const string LinksObtained = "Enlaces obtenidos: {0} de {1}.";
        public const string LinksCopied = "Enlaces obtenidos: {0} de {1}. Ya están copiados en el portapapeles.";
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
        public const string JdIdle = "conectado, sin descargas";
        public const string JdClosed = "JD2 cerrado: ábrelo";
        public const string JdApiOff = "API de JD2 apagada: clic para activarla";

        public const string ToastClosedTitle = "JDownloader 2 está cerrado";
        public const string ToastClosedBody = "Sin JD2 no se pueden enviar enlaces ni vigilar descargas.";
        public const string ToastApiTitle = "No se puede conectar con JD2";
        public const string ToastApiBody = "Falta activar su API local. Solo hay que hacerlo una vez.";
        public const string ToastBackTitle = "Conectado con JDownloader 2";
        public const string ToastBackBody = "Ya puedes enviar enlaces, y las descargas vuelven a estar vigiladas.";
        public const string ToastOneProblem = "Un enlace necesita atención";
        public const string ToastManyProblems = "{0} enlaces necesitan atención";
        public const string ToastProblemBody = "Revisa: {0}";
        public const string ToastCapturedTitle = "Enlace de MediaFire capturado";
        public const string ToastCapturedBody = "Se añadieron {0} archivo(s) a la lista.";
        public const string ToastActionSteps ="Clic para ver los pasos";
        public const string ToastActionOpen = "Clic para abrir LinkGrab";

        public const string AddTitle = "Añadir enlaces";
        public const string AddHeading = "Pega tus enlaces de MediaFire";
        public const string AddPrompt = "Archivos o carpetas, uno por línea. Ctrl+Enter para añadir.";
        public const string AddPlaceholder = "https://www.mediafire.com/folder/...\r\nhttps://www.mediafire.com/file/...";
        public const string AddCountNone = "Sin enlaces todavía";
        public const string AddCountOne = "1 enlace listo";
        public const string AddCountMany = "{0} enlaces listos";
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
        public const string SettingsNotify = "Mostrar avisos";
        public const string SettingsWatchClipboard = "Añadir solos los enlaces de MediaFire que copies";
        public const string SettingsPoll = "Consultar JD2 cada (segundos):";
        public const string SettingsOnClose = "Al cerrar la ventana:";

        public const string JdWindowTitle = "JDownloader 2";
        public const string JdConnectHeading = "Conecta LinkGrab con JDownloader 2";
        public const string JdConnectSteps = "1.  Abre JDownloader 2 y espera a que cargue.\r\n\r\n"
            + "2.  Entra en Configuración y luego en Ajustes avanzados\r\n"
            + "     (Settings > Advanced Settings; el nombre cambia según el idioma).\r\n\r\n"
            + "3.  En el buscador escribe:  deprecated\r\n\r\n"
            + "4.  Busca RemoteAPI: Deprecated Api y actívala\r\n"
            + "     (deprecatedapienabled = true).\r\n\r\n"
            + "5.  Reinicia JDownloader 2.\r\n\r\n"
            + "Si ya hiciste estos pasos antes, basta con abrir JD2. Esta ventana se cierra sola al conectar.";
        public const string ErrorTitle = "Error en LinkGrab";
        public const string ErrorHeading = "Algo falló, pero puedes seguir usando LinkGrab";
        public const string ErrorHeadingFatal = "Algo falló y LinkGrab tiene que cerrarse";
        public const string ErrorPrompt = "Copia este texto y envíalo a quien te pasó la app. No incluye tu carpeta de usuario, pero puede incluir nombres de archivos o enlaces; revísalo antes de enviarlo.";
        public const string ErrorCopied = "Copiado";
        public const string ErrorCopyFailed = "No se pudo copiar";
        public const string ButtonCopy = "Copiar error";
        public const string ButtonContinue = "Seguir";
        public const string ButtonExit = "Salir";
        public const string StartupChecking ="Cargando...";

        public const string BrowserTitle = "Obtener enlaces";
        public const string BrowserProgress = "{0} de {1}  ·  {2}";
        public const string BrowserHint = "Si aparece un challenge, resuélvelo. El enlace se captura solo.";
        public const string BrowserCaptured = "Enlace capturado, enviando a JD2...";
        public const string BrowserNoRuntime = "Este equipo no tiene WebView2";
        public const string BrowserInstall = "Instálalo desde {0} y vuelve a intentarlo.";
        public const string BrowserFailed = "No pude abrir el navegador integrado";

        public const string ErrorUnsupportedSite = "{0} aún no es compatible: por ahora LinkGrab solo funciona con MediaFire.";
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
        public const string HelpProject = "Proyecto";
    }
}
