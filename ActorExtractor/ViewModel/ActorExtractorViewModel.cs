using ActorExtractor.Core;
using Socrates.Chunks;
using Socrates.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel.DataAnnotations;
using ActorExtractor.Validation;
using Socrates.ValueTypes;

namespace ActorExtractor.ViewModel
{
    public class ActorExtractorViewModel : ViewModelBase
    {
        private uint currentCollection;
        private int selectedIndex;
        private uint id;
        private string name;
        private string shortName;
        private VistaFolderBrowserDialog FolderBrowserDialog;

        public uint CurrentCollection
        {
            get { return currentCollection; }
            set { Set(ref currentCollection, value); }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { Set(ref selectedIndex, value); }
        }

        [Required(ErrorMessage = "{0} required."), Minimum(1000000)]
        public uint Id
        {
            get { return id; }
            set { Set(ref id, value); }
        }

        [Required(ErrorMessage = "{0} required."), MinLength(8, ErrorMessage = "{0} length < 8")]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        [Required(ErrorMessage = "{0} required."), MinLength(8, ErrorMessage = "{0} length < 8"), ShortName(ErrorMessage = "{0} has invalid chars.")]
        public string ShortName
        {
            get { return shortName; }
            set { Set(ref shortName, value); }
        }

        [Required(ErrorMessage = "{0} required.")]
        public string Author
        {
            get { return Properties.Settings.Default.AuthorName; }
            set
            {
                if (value != Properties.Settings.Default.AuthorName)
                {
                    Properties.Settings.Default.AuthorName = value;
                    OnPropertyChanged("Author");
                }
            }
        }

        public ObservableCollection<KeyValuePair<uint, string>> Actors { get; set; }

        public RelayCommand BackCommand { get; set; }
        public RelayCommand ExtractCommand { get; set; }
        public RelayCommand GenerateIdCommand { get; set; }

        public List<CnFile> MainFiles { get; set; }
        public List<CnFile> ThumFiles { get; set; }
        public Dictionary<uint, Mbmp> MbmpDictionary { get; }
        public Dictionary<uint, Chunk> TmplDictionary { get; }
        public Dictionary<uint, Th> ThumDictionary { get; }
        public BitmapPalette Palette { get; private set; }

        public ActorExtractorViewModel()
        {
            var pal = new Custom.Pal(0) { SectionData = Properties.Resources.pal_default };
            Palette = new BitmapPalette(pal.Colors);
            FolderBrowserDialog = new VistaFolderBrowserDialog();
            BackCommand = new RelayCommand(() => MainWindow.SetViewModel(ViewModelLocator.Instance.CollectionsViewModel));
            ExtractCommand = new RelayCommand(OnExtract);
            GenerateIdCommand = new RelayCommand(OnGenerateId);
            Actors = new ObservableCollection<KeyValuePair<uint, string>>();
            MainFiles = new List<CnFile>();
            ThumFiles = new List<CnFile>();
            MbmpDictionary = new Dictionary<uint, Mbmp>();
            TmplDictionary = new Dictionary<uint, Chunk>();
            ThumDictionary = new Dictionary<uint, Th>();
            CurrentCollection = 2;
        }

        private void OnExtract()
        {
            if (SelectedIndex < 0 || HasErrors)
                return;

            FolderBrowserDialog.Reset();
            FolderBrowserDialog.Description = $"Output:\n\"{ShortName}\"";
            if (FolderBrowserDialog.ShowDialog() == true)
            {
                var dir = Path.Combine(FolderBrowserDialog.SelectedPath, ShortName);
                if (Directory.Exists(dir))
                {
                    System.Windows.MessageBox.Show($"Output folder already exists.\n\n{dir}", "Extract Aborted");
                    return;
                }
                // Creates the directory.
                Directory.CreateDirectory(dir);

                var key = Actors[SelectedIndex].Key;
                string origAuthor;
                if (!CollectionsViewModel.CollectionAuthors.TryGetValue(CurrentCollection, out origAuthor))
                    origAuthor = "Microsoft";

                // -----  Create the 3TH  -----
                var thumTree = GetQuadTree(ThumDictionary[key]);
                var newThumCollection = thumTree.Select(c => CreateDeepCopy(c, Id)).ToList();
                foreach (var chunk in newThumCollection)
                {
                    for (int i = 0; i < chunk.References.Count; i++)
                        chunk.References[i] = new Reference(chunk.References[i].Quad, Id, chunk.References[i].RefId);
                }

                // Change TH SourceId.
                var thum = VirtualChunk.Create<Th>(newThumCollection[0]);
                thum.SourceId = Id;
                newThumCollection[0] = thum;
                new CnFile(newThumCollection) { MagicNumber = thum.MagicNumber }.SaveAs(Path.Combine(dir, string.Concat(ShortName, ".3th")));

                // -----  Create the 3CN  -----
                var tmpl = TmplDictionary[key];
                var tree = GetQuadTree(tmpl);
                var newCollection = new Chunk[] { CreateDeepCopy(tree.First(), Id) }.Concat(CreateDeepCopies(tree.Skip(1)));
                new CnFile(newCollection) { MagicNumber = thum.MagicNumber }.SaveAs(Path.Combine(dir, string.Concat(ShortName, ".3cn")));

                bool isActor = thum.Quad == "PRTH" ? false : true;

                // -----  Create the CFG  -----
                using (var writer = new StreamWriter(System.IO.File.OpenWrite(Path.Combine(dir, string.Concat(ShortName, ".cfg")))))
                {
                    writer.WriteLine("Name={0}", Name);
                    writer.WriteLine("Author={0}", Author);
                    writer.WriteLine("Original Author={0}", origAuthor);
                    writer.WriteLine("Type=Portable");
                    writer.WriteLine("Content={0}", isActor ? "Actors" : "Props");
                    writer.WriteLine("Date={0}", (int)((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds());
                    writer.WriteLine("Generator=Actor Extractor for 3DMM v1.0");
                }

                // -----  Create the MakeVXP  -----
                System.IO.File.WriteAllBytes(Path.Combine(dir, "MakeVXP.exe"), Properties.Resources.MakeVXP);
            }
        }

        private IEnumerable<Chunk> CreateDeepCopies(IEnumerable<Chunk> collection)
        {
            return collection.Select(c => CreateDeepCopy(c, c.Id));
        }

        private Chunk CreateDeepCopy(Chunk chunk, uint newId)
        {
            var result = new SimpleChunk(chunk.Quad, newId, chunk.SectionData)
            {
                String = chunk.String,
                Mode = chunk.Mode
            };
            foreach (var r in chunk.References)
                result.References.Add(new Reference(r.Quad, r.Id, r.RefId));
            return result;
        }

        private IEnumerable<Chunk> GetQuadTree(Chunk chunk)
        {
            var list = new List<Chunk>();
            list.Add(chunk);
            foreach (var subchunk in chunk.References.Dereference())
                list.AddRange(GetQuadTree(subchunk));
            return list.Distinct();
        }

        private void OnGenerateId()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                while (true)
                {
                    var bytes = new byte[4];
                    rng.GetNonZeroBytes(bytes);
                    var num = BitConverter.ToUInt32(bytes, 0);
                    if (num > 1000000)
                    {
                        Id = num;
                        break;
                    }
                }
            }
        }

        public ImageSource GetPreviewImage(uint key)
        {
            Mbmp mbmp;
            if (!MbmpDictionary.TryGetValue(key, out mbmp) || mbmp.Width == 0 || mbmp.Height == 0 || mbmp.Pixels.Length != mbmp.Width * mbmp.Height)
                return null;
            return BitmapSource.Create(mbmp.Width, mbmp.Height, 72, 72, PixelFormats.Indexed8, Palette, mbmp.Pixels, mbmp.Width);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(CurrentCollection) && CollectionsViewModel.CollectionFiles.ContainsKey(CurrentCollection))
            {
                PopulateFileContainers();
                PopulateActorsCollection();
            }
            base.OnPropertyChanged(propertyName);
        }

        private void PopulateFileContainers()
        {
            MainFiles.Clear();
            ThumFiles.Clear();
            foreach (var file in CollectionsViewModel.CollectionFiles[CurrentCollection].Where(f => f.Content.HasFlag(ContentType.ActorProp)))
            {
                var tmp = new CnFile(file.FileName);
                switch (Path.GetExtension(file.FileName).ToUpperInvariant())
                {
                    case ".3CN":
                        MainFiles.Add(tmp);
                        break;
                    case ".3TH":
                        ThumFiles.Add(tmp);
                        break;
                }
            }
        }

        private void PopulateActorsCollection()
        {
            Actors.Clear();
            MbmpDictionary.Clear();
            TmplDictionary.Clear();
            ThumDictionary.Clear();
            foreach (var thFile in ThumFiles)
            {
                foreach (var chunk in thFile)
                {
                    string prefix;
                    switch (chunk.Quad)
                    {
                        case "TMTH":
                            prefix = "Actor";
                            break;
                        case "PRTH":
                            prefix = "Prop";
                            break;
                        default:
                            continue;
                    }

                    Th thum = VirtualChunk.Create<Th>(chunk);
                    var gokd = thum?.References.Dereference().FirstOrDefault(c => c.Quad == "GOKD");
                    var img = gokd?.References.Dereference().FirstOrDefault(c => c.Quad == "MBMP");
                    if (thum != null)
                    {
                        var source = MainFiles.SelectMany(cn => cn.Where(c => c.Quad == thum.SourceQuad)).FirstOrDefault(c => c.Id == thum.SourceId);
                        if (source != null)
                        {
                            ThumDictionary.Add(thum.SourceId, thum);
                            Actors.Add(new KeyValuePair<uint, string>(thum.SourceId, string.Format("{0}: {1}", prefix, source.String)));
                            if (!TmplDictionary.ContainsKey(thum.SourceId))
                                TmplDictionary.Add(thum.SourceId, source);
                            if (!MbmpDictionary.ContainsKey(thum.SourceId))
                                MbmpDictionary.Add(thum.SourceId, VirtualChunk.Create<Mbmp>(img));
                        }
                    }
                }
            }
        }
    }
}
