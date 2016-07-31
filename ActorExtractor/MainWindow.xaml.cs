using ActorExtractor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ActorExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IEnumerable<Type> views;

        static MainWindow()
        {
            views = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name.EndsWith("View"));
        }

        public MainWindow()
        {
            InitializeComponent();
            new ViewModelLocator();
            SetViewModel(ViewModelLocator.Instance.CollectionsViewModel);
        }

        public static void SetViewModel(ViewModelBase viewModel)
        {
            var mainWindow = (Application.Current.MainWindow as MainWindow);
            if(mainWindow != null)
            {
                var type = viewModel.GetType();
                var viewName = type.Name.Replace("ViewModel", "View");
                var viewType = views.FirstOrDefault(v => v.Name == viewName);
                UIElement view = null;
                if (viewType != null)
                {
                    view = Activator.CreateInstance(viewType) as UIElement;
                    view?.SetValue(DataContextProperty, viewModel);
                }
                mainWindow.ViewContent.Content = view;
            }
        }
    }
}
