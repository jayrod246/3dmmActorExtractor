using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System;

namespace ActorExtractor.Internal
{
    public static class RegistryHelper
    {
        static string fullInstallDir;
        static string installDir;

        public static Dictionary<uint, string> Collections { get; }

        static RegistryHelper()
        {
            Collections = new Dictionary<uint, string>();
            Reset();
        }

        public static void Reset()
        {
            fullInstallDir = null;
            installDir = null;
            Collections.Clear();
            using (var t = Registry.LocalMachine)
            {
                var products = t.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft Kids\3D Movie Maker\Products");
                if (products != null)
                {
                    try
                    {
                        foreach (var name in products?.GetValueNames())
                        {
                            uint key;
                            if (!uint.TryParse(name, out key))
                                continue;
                            Collections.Add(key, products.GetValue(name) as string);
                        }
                    }
                    finally
                    {
                        (products as System.IDisposable).Dispose();
                    }
                }
            }
        }

        public static string GetInstallDirectory()
        {
            if (installDir == null)
            {
                using (var t = Registry.LocalMachine)
                {
                    var c3dmm = t.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft Kids\3D Movie Maker");
                    if (c3dmm != null)
                    {
                        try
                        {
                            var maindir = c3dmm.GetValue("InstallDirectory", "") as string;
                            var subdir = c3dmm.GetValue("InstallSubDir", "") as string;
                            installDir = maindir;
                            fullInstallDir = Path.Combine(maindir, subdir);
                        }
                        finally
                        {
                            (c3dmm as System.IDisposable).Dispose();
                        }
                    }
                }
            }
            return installDir;
        }

        public static string GetFullInstallDirectory()
        {
            if (fullInstallDir == null)
                GetInstallDirectory();
            return fullInstallDir;
        }
    }
}
