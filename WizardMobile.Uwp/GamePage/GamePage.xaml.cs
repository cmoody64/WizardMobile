using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WizardMobile.Core;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp
{
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();
            this.InitializeWizardFrontend();
        }
    }
}
