using ActorExtractor.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
namespace ActorExtractor.View
{
    public partial class ActorExtractorView
    {
        public ActorExtractorView()
        {
            InitializeComponent();
        }

        protected ActorExtractorViewModel ViewModel
        {
            get { return (ActorExtractorViewModel)DataContext; }
            set { DataContext = value; }
        }

        private void OnMouseEnterListBoxItem(object sender, MouseEventArgs e)
        {
            var item = e.Source as ListBoxItem;
            if (item != null)
            {
                var pair = (KeyValuePair<uint, string>)item.DataContext;
                PreviewImage.Source = ViewModel.GetPreviewImage(pair.Key);
            }
        }

        private void OnListBoxItemDoubleClick(object sender, MouseEventArgs e)
        {
            ViewModel.ExtractCommand.Execute(null);
        }
    }
}
