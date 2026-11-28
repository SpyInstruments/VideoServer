using System.IO.Compression;
using System.Configuration.Install;
using System.ComponentModel;
using System.IO;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

[RunInstaller(true)]
public class PostInstallActions : Installer
{
    // Must match "UpgradeCode" in Setup/Setup.vdproj
    const string UpgradeCode = "{931657DC-BA13-4658-B626-86F2D48AE680}";

    public override void Install(System.Collections.IDictionary stateSaver)
    {
        base.Install(stateSaver);
        string baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string archivePath = Path.Combine(baseDir, "libvlc.zip");
        string extractPath = Path.Combine(baseDir, "libvlc");
        if (!Directory.Exists(extractPath))
        {
            Directory.CreateDirectory(extractPath);
        }
        if (File.Exists(archivePath))
        {
            // libvlc already exists after an upgrade from a previous version - overwrite files
            ExtractWithOverwrite(archivePath, extractPath);
            File.Delete(archivePath);
        }
    }
    public override void Uninstall(IDictionary savedState)
    {
        base.Uninstall(savedState);

        // Visual Studio setup projects remove the previous version AFTER the new one is installed.
        // That uninstall must not delete libvlc (just extracted by the new version) and
        // xml (user's camera settings).
        if (IsUpgrade())
            return;

        string baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string extractPath = Path.Combine(baseDir, "libvlc");
        string xmlPath = Path.Combine(baseDir, "xml");

        if (Directory.Exists(extractPath))
        {
            try
            {
                Directory.Delete(extractPath, recursive: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        if (Directory.Exists(xmlPath))
        {
            try
            {
                Directory.Delete(xmlPath, recursive: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    static void ExtractWithOverwrite(string archivePath, string extractPath)
    {
        string root = Path.GetFullPath(extractPath);
        if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
            root += Path.DirectorySeparatorChar;

        using (ZipArchive zip = ZipFile.OpenRead(archivePath))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                // entries may use '\' (Compress-Archive in Windows PowerShell) or '/'
                string relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                string target = Path.GetFullPath(Path.Combine(root, relative));
                if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    continue; // skip entries pointing outside the target folder

                if (relative.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    Directory.CreateDirectory(target);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(target));
                entry.ExtractToFile(target, overwrite: true);
            }
        }
    }

    /// <summary>
    /// true if more than one product with our UpgradeCode is registered,
    /// i.e. the old version is being removed as part of an upgrade
    /// </summary>
    static bool IsUpgrade()
    {
        try
        {
            int count = 0;
            var productCode = new StringBuilder(39);
            for (int i = 0; MsiEnumRelatedProducts(UpgradeCode, 0, i, productCode) == 0; i++)
                count++;
            return count > 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    static extern int MsiEnumRelatedProducts(string lpUpgradeCode, int dwReserved, int iProductIndex, StringBuilder lpProductBuf);
}
