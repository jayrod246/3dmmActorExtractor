using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ActorExtractor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            // Try to resolve missing assemblies with embedded resources.
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var finalName = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                var resources = assembly.GetManifestResourceNames();
                string resourceName = null;
                for (int i = 0; i <= resources.Length - 1; i++)
                {
                    var name = resources[i];
                    if (name.EndsWith(finalName))
                    {
                        resourceName = name;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(resourceName))
                    return null;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    byte[] block = new byte[stream.Length];
                    stream.Read(block, 0, block.Length);
                    // Return the loaded assembly.
                    return Assembly.Load(block);
                }
            }
            catch
            {
                return null;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Save user preferences before exit.
            ActorExtractor.Properties.Settings.Default.Save();
            base.OnExit(e);
        }
    }
}
