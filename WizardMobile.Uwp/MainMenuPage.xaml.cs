using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WizardMobile.Core;
using WizardMobile.Uwp.GamePage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WizardMobile.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            this.InitializeComponent();
            this.new_game_button.Click += this.OnNewGameButtonClick;
            GetResumableGame().ContinueWith(task =>
            {
                _prevGameContextDto = task.Result;
                if (_prevGameContextDto != null)
                    resume_game_button.Visibility = Visibility.Visible;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnNewGameButtonClick(object sender, RoutedEventArgs e)
        {
            var navParams = new GamePageNavigationParams()
            {
                StartNavigationType = GamePageNavigationParams.NavigationType.NEW_GAME
            };
            (Application.Current as WizardUwpApp).NavigateToPage(WizardUwpApp.Page.GAMEPLAY, navParams);
        }

        private void OnResumeGameButtonClick(object sender, RoutedEventArgs e)
        {
            var navParams = new GamePageNavigationParams()
            {
                StartNavigationType = GamePageNavigationParams.NavigationType.NEW_GAME,
                PrevGameContextDto = _prevGameContextDto
            };

            (Application.Current as WizardUwpApp).NavigateToPage(WizardUwpApp.Page.GAMEPLAY, navParams);
        }

        // gets a resumable game context if present, returns null if there is no resumable game context
        private async Task<GameContextDto> GetResumableGame()
        {
            StorageFile gameContextFile = null;
            GameContextDto resumableGameContextDto;
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                gameContextFile = await localFolder.GetFileAsync(WizardFrontend.GamePageController.GAME_STATE_FILENAME);
                string gameStateRawData = await FileIO.ReadTextAsync(gameContextFile);
                resumableGameContextDto = JsonConvert.DeserializeObject<GameContextDto>(gameStateRawData);
            }
            catch (Exception)
            {
                // no previous game state was stored        
                resumableGameContextDto = null;
            }

            // clear out previous game file
            if (gameContextFile != null)
                File.Delete(gameContextFile.Path);

            return resumableGameContextDto;
        }

        private GameContextDto _prevGameContextDto;
    }
}
