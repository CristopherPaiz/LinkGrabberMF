using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;

namespace LinkGrab.Tests
{
    public static class FolderPathsTests
    {
        public static void SplitsPaths()
        {
            Check.Equal("Season 01", FolderPaths.Leaf(@"Parte 1\Season 01"));
            Check.Equal("Parte 1", FolderPaths.Parent(@"Parte 1\Season 01"));
            Check.Equal("", FolderPaths.Parent("Parte 1"));
            Check.Equal(-1, FolderPaths.Depth(""));
            Check.Equal(1, FolderPaths.Depth(@"a\b"));
        }

        public static void InsideNeedsAWholeFolderName()
        {
            Check.True(FolderPaths.IsInside(@"a\b", "a"), "subcarpeta");
            Check.True(FolderPaths.IsInside("a", "a"), "ella misma");
            Check.True(!FolderPaths.IsInside("ab", "a"), "solo comparte el prefijo");
        }

        public static void ChainGoesFromRootDown()
        {
            Check.Equal(@"a|a\b|a\b\c", string.Join("|", FolderPaths.Chain(@"a\b\c")));
        }

        public static void ConvertsJdPackageNames()
        {
            Check.Equal(@"Padre\Hijo", FolderPaths.FromPackage("Padre⁄Hijo"));
            Check.Equal(@"Padre\Hijo", FolderPaths.FromPackage("Padre/Hijo"));
        }
    }

    public static class LinkResolverTests
    {
        static readonly Rule MediaFire = new Rule
        {
            Name = "mediafire",
            Element = "id=\"downloadButton\"",
            Attrs = new[] { "data-scrambled-url:base64", "href" }
        };

        static string Encode(string text) { return Convert.ToBase64String(Encoding.UTF8.GetBytes(text)); }

        public static void ReadsHref()
        {
            string html = "<a href=\"http://otro\"><a id=\"downloadButton\" href=\"https://download1.mediafire.com/x/f.zip\">";
            Check.Equal("https://download1.mediafire.com/x/f.zip", LinkResolver.Extract(html, MediaFire));
        }

        public static void PrefersTheEncodedAttribute()
        {
            string html = "<a id=\"downloadButton\" href=\"https://viejo\" data-scrambled-url=\"" + Encode("https://nuevo") + "\">";
            Check.Equal("https://nuevo", LinkResolver.Extract(html, MediaFire));
        }

        public static void BadBase64FallsBackToHref()
        {
            string html = "<a id=\"downloadButton\" data-scrambled-url=\"%%%\" href=\"https://bueno\">";
            Check.Equal("https://bueno", LinkResolver.Extract(html, MediaFire));
        }

        public static void DecodesHtmlEntities()
        {
            string html = "<a id=\"downloadButton\" href=\"https://x/?a=1&amp;b=2\">";
            Check.Equal("https://x/?a=1&b=2", LinkResolver.Extract(html, MediaFire));
        }

        public static void IgnoresWhatIsNotALink()
        {
            Check.Equal(null, LinkResolver.Extract("<a id=\"downloadButton\" href=\"javascript:void(0)\">", MediaFire));
            Check.Equal(null, LinkResolver.Extract("<p>sin boton</p>", MediaFire));
        }
    }

    public static class AppFilesTests
    {
        public static void ProfileFollowsTheSmokeTestVariable()
        {
            string real = AppFiles.ProfileRoot;
            Environment.SetEnvironmentVariable(AppFolders.ProfileVariable, @"C:\sandbox");
            try
            {
                Check.Equal(@"C:\sandbox", AppFiles.ProfileRoot);
                Check.True(WebViewLibs.LoaderFolder.StartsWith(@"C:\sandbox"), "las DLL irían a la carpeta real");
                Check.True(WebViewLibs.BrowserProfile.StartsWith(@"C:\sandbox"), "el navegador usaría la carpeta real");
            }
            finally
            {
                Environment.SetEnvironmentVariable(AppFolders.ProfileVariable, null);
            }
            Check.Equal(real, AppFiles.ProfileRoot);
        }
    }

    public static class SettingsTests
    {
        public static void ClipboardCaptureIsOnByDefault()
        {
            Check.True(new Settings().WatchClipboard, "nuevo");
            Check.True(Json.Read<Settings>("{\"PollSeconds\":5}").WatchClipboard, "ajustes guardados antes de existir esta opción");
        }

        public static void ClipboardChoiceIsRemembered()
        {
            Check.True(!Json.Read<Settings>(Json.Write(new Settings { WatchClipboard = false })).WatchClipboard, "apagada");
        }
    }

    public static class CopyRepeatFilterTests
    {
        static readonly DateTime Start = new DateTime(2026, 1, 1);

        public static void WindowsAnnouncingOneCopyTwiceCountsOnce()
        {
            var filter = new CopyRepeatFilter();
            Check.True(!filter.IsRepeat("a", Start), "primera vez");
            Check.True(filter.IsRepeat("a", Start.AddMilliseconds(50)), "aviso doble");
        }

        public static void CopyingTheSameTextLaterIsANewCopy()
        {
            var filter = new CopyRepeatFilter();
            filter.IsRepeat("a", Start);
            Check.True(!filter.IsRepeat("a", Start.AddSeconds(5)), "copiado de nuevo");
        }

        public static void DifferentTextIsNeverARepeat()
        {
            var filter = new CopyRepeatFilter();
            filter.IsRepeat("a", Start);
            Check.True(!filter.IsRepeat("b", Start.AddMilliseconds(50)), "otro texto");
        }
    }

    public static class LinkTextTests
    {
        public static void FindsUrlsHoweverTheyAreSeparated()
        {
            Check.Equal("http://a|https://b/c", string.Join("|", LinkText.Urls("mira http://a\r\n  https://b/c ")));
        }

        public static void OtherSitesAreCalledByName()
        {
            AppFiles.EnsureRules();
            foreach (string url in new[] { "https://mega.nz/file/abc#key", "https://www.uptobox.com/abc" })
            {
                try
                {
                    MediaFireClient.Expand(url);
                    Check.True(false, "debía rechazar " + url);
                }
                catch (UnsupportedSiteException e)
                {
                    Check.True(e.Message.StartsWith(LinkText.SiteOf(url) + " aún no es compatible"), e.Message);
                    Check.True(!e.Message.Contains(".ini"), "habla de archivos internos");
                    Check.True(!e.Message.Contains("www."), e.Message);
                }
            }
        }

        public static void ClipboardOnlyCapturesMediaFirePages()
        {
            string copied = "mira https://www.mediafire.com/file_premium/abc/x.rar/file y https://www.mediafire.com/folder/def/Carpeta\r\n"
                + "https://download2345.mediafire.com/tok/abc/x.rar https://example.com/file/abc https://google.com";
            Check.Equal("https://www.mediafire.com/file_premium/abc/x.rar/file|https://www.mediafire.com/folder/def/Carpeta",
                string.Join("|", LinkText.MediaFirePages(copied)));
        }

        public static void TextThatIsNotALinkIsNotCaptured()
        {
            Check.Equal(0, LinkText.MediaFirePages("mi contraseña es mediafire.com/file/abc").Count);
            Check.Equal(0, LinkText.MediaFirePages(null).Count);
        }

        public static void EmptyTextHasNoUrls()
        {
            Check.Equal(0, LinkText.Urls("").Count);
            Check.Equal(0, LinkText.Urls(null).Count);
            Check.Equal(0, LinkText.Urls("sin enlaces").Count);
        }
    }

    public static class JdConnectionsTests
    {
        public static void OnlyAnswersWhenJdCanBeUsed()
        {
            Check.True(JdConnections.CanUse(JdConnection.Online), "conectado");
            Check.True(JdConnections.CanUse(JdConnection.Unknown), "sin comprobar");
            Check.True(!JdConnections.CanUse(JdConnection.ApiOff), "API apagada");
            Check.True(!JdConnections.CanUse(JdConnection.Closed), "cerrado");
        }
    }

    public static class LinkWithoutJdTests
    {
        public static void JustAddedFileWantsALinkButIsNotAProblem()
        {
            var item = new FileItem();
            Check.True(item.WantsLink, "le falta el enlace");
            Check.True(!item.NeedsAttention, "no es un problema");
        }

        public static void FilesThatHaveALinkOrAreInJdDoNotWantOne()
        {
            Check.True(!new FileItem { DirectUrl = "u", LinkState = LinkState.Ready }.WantsLink, "ya tiene enlace");
            Check.True(!new FileItem { JdWhere = JdLocation.Grabber }.WantsLink, "JD2 ya lo tiene");
            Check.True(!new FileItem { JdFinished = true }.WantsLink, "terminado");
        }

        static string Actions(FileItem item)
        {
            return string.Join(",", LinkGrab.Ui.Grid.ItemPresenter.ActionsOf(item));
        }

        public static void NewRowOffersToGetItsLink()
        {
            Check.Equal("GetLink,Send", Actions(new FileItem()));
        }

        public static void RowWithALinkOffersCopyAndReload()
        {
            Check.Equal("Copy,Reload,Send", Actions(new FileItem { DirectUrl = "u", LinkState = LinkState.Ready }));
        }

        public static void RowWithABrokenLinkOffersToGetAnother()
        {
            Check.Equal("GetLink,Send", Actions(new FileItem { DirectUrl = "u", LinkState = LinkState.Expired }));
        }

        public static void RowInJdKeepsCopyAndReloadButCannotBeSentAgain()
        {
            Check.Equal("Copy,Reload", Actions(new FileItem { DirectUrl = "u", LinkState = LinkState.Ready, JdWhere = JdLocation.Downloads, Sent = true }));
        }

        public static void BusyOrFinishedRowOffersNothing()
        {
            Check.Equal("", Actions(new FileItem { Resolving = true }));
            Check.Equal("", Actions(new FileItem { DirectUrl = "u", JdFinished = true }));
        }

        public static void FolderOffersOnlyWhatSomeFileNeeds()
        {
            var done = new FileItem { DirectUrl = "u", LinkState = LinkState.Ready, JdWhere = JdLocation.Grabber, Sent = true };
            var missing = new FileItem();
            Check.Equal("", string.Join(",", LinkGrab.Ui.Grid.ItemPresenter.ActionsOf(new System.Collections.Generic.List<FileItem> { done })));
            Check.Equal("GetLink,SendFolder", string.Join(",", LinkGrab.Ui.Grid.ItemPresenter.ActionsOf(new System.Collections.Generic.List<FileItem> { done, missing })));
        }

        public static void CopyAllTakesEveryWorkingLinkInView()
        {
            var good = new FileItem { DirectUrl = "u", LinkState = LinkState.Ready };
            var missing = new FileItem();
            var expired = new FileItem { DirectUrl = "v", LinkState = LinkState.Expired };
            var all = new System.Collections.Generic.List<FileItem> { good, missing, expired };
            var none = new System.Collections.Generic.List<FileItem>();

            Check.Equal(1, LinkGrab.Ui.Grid.ItemPresenter.CopyTargets(all, none, LinkGrab.Ui.Grid.ItemFilter.All).Count);
            Check.Equal(0, LinkGrab.Ui.Grid.ItemPresenter.CopyTargets(all, none, LinkGrab.Ui.Grid.ItemFilter.Done).Count);
            Check.Equal(0, LinkGrab.Ui.Grid.ItemPresenter.CopyTargets(all, new System.Collections.Generic.List<FileItem> { missing }, LinkGrab.Ui.Grid.ItemFilter.All).Count);
        }

        public static void CopyLabelCountsTheSelection()
        {
            Check.Equal("Copiar todos", LinkGrab.Ui.Grid.ItemPresenter.CopyLabel(false, 5));
            Check.Equal("Copiar (2)", LinkGrab.Ui.Grid.ItemPresenter.CopyLabel(true, 2));
        }

        public static void OnlySendingNeedsJd()
        {
            Check.True(!LinkGrab.Ui.Grid.ItemPresenter.RequiresJd(LinkGrab.Ui.Grid.RowAction.GetLink), "obtener");
            Check.True(!LinkGrab.Ui.Grid.ItemPresenter.RequiresJd(LinkGrab.Ui.Grid.RowAction.Copy), "copiar");
            Check.True(!LinkGrab.Ui.Grid.ItemPresenter.RequiresJd(LinkGrab.Ui.Grid.RowAction.Reload), "recargar");
            Check.True(!LinkGrab.Ui.Grid.ItemPresenter.RequiresJd(LinkGrab.Ui.Grid.RowAction.None), "ninguna");
            Check.True(LinkGrab.Ui.Grid.ItemPresenter.RequiresJd(LinkGrab.Ui.Grid.RowAction.Send), "enviar");
        }

        public static void ResolvingIsNotSavedToTheSession()
        {
            Check.True(!Json.Write(new FileItem { Resolving = true }).Contains("Resolving"), "no debe guardarse");
        }
    }

    public static class IconButtonTests
    {
        sealed class ClickableButton : LinkGrab.Ui.Controls.IconButton
        {
            public ClickableButton() : base("x") { }
            public void Press() { OnClick(EventArgs.Empty); }
        }

        public static void BlockedButtonIgnoresClicksUntilReleased()
        {
            int clicks = 0;
            using (var button = new ClickableButton { Blocked = true })
            {
                button.Click += (s, e) => clicks++;
                button.Press();
                Check.Equal(0, clicks);
                button.Blocked = false;
                button.Press();
                Check.Equal(1, clicks);
            }
        }
    }

    public static class IconButtonSizeTests
    {
        public static void FlashShowsTheMessageAndKeepsTheOriginalForLater()
        {
            using (var button = new LinkGrab.Ui.Controls.IconButton("Verificar"))
            {
                button.Flash("Verificado");
                Check.Equal("Verificado", button.Text);
                button.Flash("Otra vez");
                Check.Equal("Otra vez", button.Text);
                var restore = typeof(LinkGrab.Ui.Controls.IconButton).GetMethod("EndFlash", BindingFlags.NonPublic | BindingFlags.Instance);
                restore.Invoke(button, null);
                Check.Equal("Verificar", button.Text);
            }
        }

        public static void TemporaryStatesAreNotSaved()
        {
            string saved = Json.Write(new FileItem { Verifying = true });
            Check.True(!saved.Contains("Verifying"), "no debe guardarse");
        }

        public static void WidthFitsTheWholeText()
        {
            string text = "Área de notificación";
            int needed = System.Windows.Forms.TextRenderer.MeasureText(text, LinkGrab.Ui.Theme.Normal, System.Drawing.Size.Empty, System.Windows.Forms.TextFormatFlags.NoPadding).Width;
            using (var button = new LinkGrab.Ui.Controls.IconButton(text))
                Check.True(button.Width > needed, "el texto se recortaría");
        }
    }

    public static class ToastPlacementTests
    {
        static readonly System.Drawing.Size Size = new System.Drawing.Size(360, 122);

        static void CheckInside(System.Drawing.Rectangle screen)
        {
            var spot = new System.Drawing.Rectangle(LinkGrab.Ui.Dialogs.Toast.Place(screen, Size, 0), Size);
            Check.True(screen.Contains(spot), "se sale de " + screen);
            Check.True(spot.Right > screen.Right - 40 && spot.Bottom > screen.Bottom - 40, "no está en la esquina de " + screen);
        }

        public static void StaysInTheCornerOfAnyScreen()
        {
            CheckInside(new System.Drawing.Rectangle(0, 0, 1920, 1020));
            CheckInside(new System.Drawing.Rectangle(1920, 0, 1920, 1032));
            CheckInside(new System.Drawing.Rectangle(-1366, 200, 1366, 728));
            CheckInside(new System.Drawing.Rectangle(0, 40, 3840, 2120));
        }

        public static void NextOneGoesAboveTheOthers()
        {
            var screen = new System.Drawing.Rectangle(0, 0, 1920, 1020);
            int first = LinkGrab.Ui.Dialogs.Toast.Place(screen, Size, 0).Y;
            Check.True(LinkGrab.Ui.Dialogs.Toast.Place(screen, Size, 132).Y == first - 132, "debe subir lo que ocupan las de abajo");
        }
    }

    public static class ThemeTests
    {
        public static void BlendGoesFromOneColorToTheOther()
        {
            var black = System.Drawing.Color.FromArgb(0, 0, 0);
            var white = System.Drawing.Color.FromArgb(200, 100, 50);
            Check.Equal(black.ToArgb(), LinkGrab.Ui.Theme.Blend(black, white, 0).ToArgb());
            Check.Equal(white.ToArgb(), LinkGrab.Ui.Theme.Blend(black, white, 1).ToArgb());
            Check.Equal(System.Drawing.Color.FromArgb(100, 50, 25).ToArgb(), LinkGrab.Ui.Theme.Blend(black, white, 0.5f).ToArgb());
        }
    }

    public static class RuleSetTests
    {
        public static void BundledRulesMatchMediaFireFiles()
        {
            AppFiles.EnsureRules();
            Check.Equal("mediafire", RuleSet.For("https://www.mediafire.com/file/abc/f.zip/file").Name);
            Check.Equal("mediafire", RuleSet.For("http://mediafire.com/download/abc").Name);
        }

        public static void OtherSitesHaveNoRule()
        {
            AppFiles.EnsureRules();
            Check.Equal(null, RuleSet.For("https://example.com/file/abc"));
        }
    }

    public static class MediaFireKeyTests
    {
        public static void ReadsKeyFromPageAndDirectUrls()
        {
            Check.Equal("abc123", MediaFireClient.KeyOf("https://www.mediafire.com/file/abc123/f.zip/file"));
            Check.Equal("abc123", MediaFireClient.KeyOf("https://www.mediafire.com/download/abc123"));
            Check.Equal("abc123", MediaFireClient.KeyOf("https://download2345.mediafire.com/tok/abc123/f.zip"));
        }

        public static void RecognizesTheErrorPage()
        {
            Check.True(MediaFireClient.IsErrorPage("https://www.mediafire.com/error.php?errno=386&quickkey=abc&origin=download"), "bloqueado");
            Check.True(!MediaFireClient.IsErrorPage("https://www.mediafire.com/file/abc/f.zip/file"), "página normal");
            Check.True(!MediaFireClient.IsErrorPage(null), "sin dirección");
        }

        public static void OtherUrlsHaveNoKey()
        {
            Check.Equal(null, MediaFireClient.KeyOf("https://example.com/file/abc123"));
            Check.Equal(null, MediaFireClient.KeyOf(null));
        }
    }

    public static class FileItemTests
    {
        public static void NewItemCanBeSent()
        {
            Check.True(new FileItem().CanSend, "nuevo");
        }

        public static void JustSentIsNotRemovedYet()
        {
            var item = new FileItem();
            item.MarkSent();
            Check.True(!item.IsRemoved && !item.CanSend, "acaba de enviarse");
        }

        public static void SentAndGoneFromJdCanBeSentAgain()
        {
            var item = new FileItem { Sent = true, SentAt = DateTime.UtcNow - Timing.SentGrace - TimeSpan.FromSeconds(1) };
            Check.True(item.IsRemoved && item.CanSend, "ya no está en JD2");
        }

        public static void ItemInJdIsNeverRemoved()
        {
            var item = new FileItem { Sent = true, SentAt = DateTime.UtcNow.AddDays(-1), JdWhere = JdLocation.Grabber };
            Check.True(!item.IsRemoved && !item.CanSend, "sigue en JD2");
        }

        public static void BrokenLinksNeedAttentionUntilFinished()
        {
            var item = new FileItem { LinkState = LinkState.Expired };
            Check.True(item.NeedsLink && item.NeedsAttention, "caducado");
            item.JdFinished = true;
            Check.True(!item.NeedsLink && !item.NeedsAttention, "terminado");
        }

        public static void ClearKeepsTheLinkButForgetsJd()
        {
            var item = new FileItem { DirectUrl = "u", JdWhere = JdLocation.Downloads, JdUuid = 5, JdRunning = true, JdStatus = "x" };
            item.ClearJdState();
            Check.Equal("u", item.DirectUrl);
            Check.Equal(JdLocation.None, item.JdWhere);
            Check.Equal(0L, item.JdUuid);
            Check.Equal("", item.JdStatus);
        }
    }

    public static class PackagingTests
    {
        public static void TheExeCarriesEveryLibraryCompressed()
        {
            Assembly app = typeof(WebViewLibs).Assembly;
            foreach (string name in WebViewFiles.All)
            {
                using (Stream packed = app.GetManifestResourceStream(name + WebViewFiles.PackedSuffix))
                {
                    Check.True(packed != null, "falta " + name);
                    using (var library = new GZipStream(packed, CompressionMode.Decompress))
                    {
                        Check.Equal((int)'M', library.ReadByte());
                        Check.Equal((int)'Z', library.ReadByte());
                    }
                }
            }
        }

        public static void TheExeCarriesTheRules()
        {
            using (Stream rules = typeof(WebViewLibs).Assembly.GetManifestResourceStream(AppFileNames.Rules))
                Check.True(rules != null, "falta rules.ini");
        }

        public static void RepositoryLinkIsTheRealOne()
        {
            Check.Equal("https://github.com/CristopherPaiz/LinkGrabberMF", AppInfo.Repository);
        }
    }
}
