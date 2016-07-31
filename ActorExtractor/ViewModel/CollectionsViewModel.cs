using ActorExtractor.Core;
using ActorExtractor.Internal;
using Socrates.IO;
using Socrates.ValueTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ActorExtractor.ViewModel
{
    [Flags]
    public enum ContentType
    {
        None = 0,
        ActorProp = 1,
        Backgrounds = 2,
        Words = 4,
    }

    public class CollectionsViewModel : ViewModelBase
    {
        public ObservableCollection<KeyValuePair<uint, string>> DetectedCollections { get; }
        public uint SelectedCollection { get; set; }
        public static Dictionary<uint, string> CollectionNames { get; }
        public static Dictionary<uint, string> CollectionAuthors { get; }
        public static Dictionary<uint, CollectionFile[]> CollectionFiles { get; }
        public RelayCommand OpenCommand { get; set; }

        static CollectionsViewModel()
        {
            CollectionNames = new Dictionary<uint, string>();
            CollectionNames.Add(2, "Default 3DMM");
            CollectionNames.Add(3, "Nickelodean Expansion");
            CollectionNames.Add(4, "Doraemon Expansion");
            CollectionNames.Add(5, "Frankie's Expansion");
            CollectionAuthors = new Dictionary<uint, string>();
            CollectionAuthors.Add(2, "Microsoft");
            CollectionAuthors.Add(3, "Nickelodean");
            CollectionAuthors.Add(4, "Fujiko-Pro");
            CollectionAuthors.Add(5, "Frankie Weindel");
            CollectionFiles = new Dictionary<uint, CollectionFile[]>();
        }

        public CollectionsViewModel()
        {
            DetectedCollections = new ObservableCollection<KeyValuePair<uint, string>>();
            OpenCommand = new RelayCommand(OnOpen);
            if (!DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                Refresh();
            }
        }

        private void OnOpen()
        {
            if (CollectionFiles.ContainsKey(SelectedCollection))
            {
                MainWindow.SetViewModel(ViewModelLocator.Instance.ActorExtractorViewModel);
                ViewModelLocator.Instance.ActorExtractorViewModel.CurrentCollection = SelectedCollection;
            }
        }

        public void Refresh()
        {
            CollectionFiles.Clear();
            DetectedCollections.Clear();
            var dir = RegistryHelper.GetInstallDirectory();
            if (Directory.Exists(dir))
            {
                foreach (var entry in RegistryHelper.Collections)
                {
                    var list = new List<CollectionFile>();
                    foreach (var subpath in entry.Value.Split('/'))
                    {
                        var subdir = Path.Combine(dir, subpath);
                        if (Directory.Exists(subdir))
                        {
                            foreach (var fileName in Directory.EnumerateFiles(subdir))
                            {
                                switch (Path.GetExtension(fileName).ToUpperInvariant())
                                {
                                    case ".3CN":
                                    case ".3TH":
                                        var content = GetContent(fileName);
                                        list.Add(new CollectionFile() { FileName = fileName, Content = content });
                                        break;
                                }
                            }
                            break;
                        }
                    }
                    if (!CollectionFiles.ContainsKey(entry.Key))
                        CollectionFiles.Add(entry.Key, new CollectionFile[0]);
                    CollectionFiles[entry.Key] = CollectionFiles[entry.Key].Concat(list).ToArray();
                }
            }

            foreach (var entry in CollectionNames)
            {
                if (CollectionFiles.ContainsKey(entry.Key))
                    DetectedCollections.Add(entry);
            }
        }

        private static ContentType GetContent(string path)
        {
            Quad[] quads;
            if (!CnFile.TryPeekQuads(path, out quads))
                return ContentType.None;
            var result = ContentType.None;
            if (quads.Any(q => q == "BKGD" || q == "BKTH"))
                result |= ContentType.Backgrounds;
            if (quads.Any(q => q == "TMPL" || q == "TMTH" || q == "PRTH"))
                result |= ContentType.ActorProp;
            return result;
        }

        public struct CollectionFile
        {
            public string FileName { get; set; }
            public ContentType Content { get; set; }
        }
    }
}
