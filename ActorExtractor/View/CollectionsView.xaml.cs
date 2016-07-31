using ActorExtractor.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActorExtractor.View
{
    public partial class CollectionsView
    {
        public CollectionsView()
        {
            InitializeComponent();
        }

        protected CollectionsViewModel ViewModel
        {
            get { return (CollectionsViewModel)DataContext; }
            set { DataContext = value; }
        }

        private void OnListBoxItemDoubleClick(object sender, MouseEventArgs e)
        {
            ViewModel.OpenCommand.Execute(null);
        }
    }
}
