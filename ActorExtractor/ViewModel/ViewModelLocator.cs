using System;

namespace ActorExtractor.ViewModel
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; private set; }
        public CollectionsViewModel CollectionsViewModel { get; }
        public ActorExtractorViewModel ActorExtractorViewModel { get; }

        public ViewModelLocator()
        {
            if (Instance != null)
                throw new MemberAccessException("Instance of ViewModelLocator already exists.");
            Instance = this;
            CollectionsViewModel = new CollectionsViewModel();
            ActorExtractorViewModel = new ActorExtractorViewModel();
        }
    }
}
