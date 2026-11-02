using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LinkGrab.Models;
using LinkGrab.Ui.Grid;

namespace LinkGrab.Tests
{
    public static class FileKindTests
    {
        public static void KnowsCommonTypes()
        {
            Check.Equal(LinkGrab.Services.FileKind.Archive, LinkGrab.Services.FileKinds.Of("Juego.RAR"));
            Check.Equal(LinkGrab.Services.FileKind.Video, LinkGrab.Services.FileKinds.Of("peli.mkv"));
            Check.Equal(LinkGrab.Services.FileKind.Audio, LinkGrab.Services.FileKinds.Of("tema.mp3"));
            Check.Equal(LinkGrab.Services.FileKind.Image, LinkGrab.Services.FileKinds.Of("foto.png"));
            Check.Equal(LinkGrab.Services.FileKind.Pdf, LinkGrab.Services.FileKinds.Of("libro.pdf"));
            Check.Equal(LinkGrab.Services.FileKind.Disc, LinkGrab.Services.FileKinds.Of("win.iso"));
            Check.Equal(LinkGrab.Services.FileKind.Program, LinkGrab.Services.FileKinds.Of("setup.exe"));
        }

        public static void SplitArchivesAreArchives()
        {
            Check.Equal(LinkGrab.Services.FileKind.Archive, LinkGrab.Services.FileKinds.Of("parte.r00"));
            Check.Equal(LinkGrab.Services.FileKind.Archive, LinkGrab.Services.FileKinds.Of("datos.7z.001"));
            Check.Equal(LinkGrab.Services.FileKind.Archive, LinkGrab.Services.FileKinds.Of("sp29.part2.rar"));
        }

        public static void UnknownOrOddNamesAreOther()
        {
            Check.Equal(LinkGrab.Services.FileKind.Other, LinkGrab.Services.FileKinds.Of("sin_extension"));
            Check.Equal(LinkGrab.Services.FileKind.Other, LinkGrab.Services.FileKinds.Of(null));
            Check.Equal(LinkGrab.Services.FileKind.Video, LinkGrab.Services.FileKinds.Of("raro|nombre\".mp4"));
        }
    }

    public static class GridSelectionTests
    {
        const int SizeColumn = 2;

        static void Click(ItemGrid grid, int row)
        {
            // The grid's own handler, not DataGridView's protected method of the same name.
            typeof(ItemGrid).GetMethod("OnCellClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                    null, new[] { typeof(object), typeof(DataGridViewCellEventArgs) }, null)
                .Invoke(grid, new object[] { grid, new DataGridViewCellEventArgs(SizeColumn, row) });
        }

        public static void ClickingAFolderMarksWhatItHolds()
        {
            using (var grid = new ItemGrid())
            {
                grid.Bind(new List<FileItem>
                {
                    new FileItem { Key = "a", Name = "a.rar", Folder = "Parte 1" },
                    new FileItem { Key = "b", Name = "b.rar", Folder = "Parte 1" },
                    new FileItem { Key = "c", Name = "c.rar", Folder = "Parte 2" }
                });
                grid.CreateControl();
                int folder = grid.Rows.Cast<DataGridViewRow>().First(r => !(r.Tag is FileItem)).Index;

                grid.ClearSelection();
                grid.Rows[folder].Selected = true;
                Click(grid, folder);
                Check.Equal("a.rar,b.rar", string.Join(",", grid.Rows.Cast<DataGridViewRow>()
                    .Where(r => r.Selected && r.Tag is FileItem).Select(r => ((FileItem)r.Tag).Name)));

                grid.Rows[folder].Selected = false;
                Click(grid, folder);
                Check.Equal(0, grid.SelectedRows.Count);
            }
        }
    }
}
