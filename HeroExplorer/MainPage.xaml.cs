using HeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeroExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Character> marvelCharacters { get; set; }
        public ObservableCollection<ComicBook> marvelComics { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            marvelCharacters = new ObservableCollection<Character>();
            marvelComics = new ObservableCollection<ComicBook>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {


            //load voice command dictionary for Cortana
            await LoadVoiceCommandDictionaryAsync();
            await Refresh();

        }

        private async Task LoadVoiceCommandDictionaryAsync()
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDictionary.xml"));
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
        }

        public async Task Refresh()
        {
            myProgressRing.IsActive = true;
            myProgressRing.Visibility = Visibility.Visible;

            marvelCharacters.Clear();
            while (marvelCharacters.Count < 10)
            {
                Task T = MarvelFacade.PopulateMarvelCharactersAsync(marvelCharacters);
                await T;
            }

            myProgressRing.IsActive = false;
            myProgressRing.Visibility = Visibility.Collapsed;
        }

        private async void masterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            myProgressRing.IsActive = true;
            myProgressRing.Visibility = Visibility.Visible;

            comicDetailNameTextBlock.Text = "";
            comicDetailDescriptionTextBlock.Text = "";
            comicDetailImage.Source = null;
            var selectedCharacter = (Character)e.ClickedItem;

            detailNameTextBlock.Text = selectedCharacter.name;
            detailDescriptionTextBlock.Text = selectedCharacter.description;

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedCharacter.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            detailImage.Source = largeImage;

            marvelComics.Clear();

            /*
            while (marvelComics.Count < 10)
            {
                Task T = MarvelFacade.PopulateMarvelComicsAsync(selectedCharacter.id, marvelComics);
                await T;
            }
            */

            await MarvelFacade.PopulateMarvelComicsAsync(selectedCharacter.id, marvelComics);

            myProgressRing.IsActive = false;
            myProgressRing.Visibility = Visibility.Collapsed;
        }

        private void ComicsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedComic = (ComicBook)e.ClickedItem;

            comicDetailNameTextBlock.Text = selectedComic.title;
            if (selectedComic.description != null)
            {
                comicDetailDescriptionTextBlock.Text = selectedComic.description;
            }

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedComic.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            comicDetailImage.Source = largeImage;

        }
    }
}
