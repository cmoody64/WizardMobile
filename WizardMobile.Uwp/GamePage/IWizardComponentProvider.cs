using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace WizardMobile.Uwp.GamePage
{
    public interface IWizardComponentProvider: IAnimationProvider
    {
        void SetMessageBoxText(string message);
        void SetPlayerCreationInputVisibility(bool isVisible);
        void SetHumanPlayerBidInputVisibility(bool isVisible);
        void SetAllPersonasVisibility(bool isVisible); // Persona refers to the element grouping of player name, status, and avatar
        void SetPlayerName(PlayerOrdinal player, string name);
        void SetPlayerStatus(PlayerOrdinal player, string status); // status referred to round score vs bids (e.g. "2/4")
        void SetPlayerScore(PlayerOrdinal player, int score);
        void SetScoreboardVisibility(bool isVisible);
        void OnPlayerCreationInputEntered(Action<string> action); // action receives (string playerInput)
        void OnPlayerBidInputEntered(Action<int> action); // action receives (string playerName, int bid)

        void OnPauseButtonClick(Action handler);
        void OnScoresButtonClick(Action handler);
        void OnQuitButtonClick(Action handler);

        double OPACITY_HIGH { get; }
        double OPACITY_LOW { get; }


        // card groups 
        StackCardGroup CenterShuffleCardGroup { get; }
        StackCardGroup LeftShuffleCardGroup { get; }
        StackCardGroup RightShuffleCardGroup { get; }
        AdjacentCardGroup DiscardCardGroup { get; }
        StackCardGroup CollapsedDiscardCardGroup { get; }
        InteractiveAdjacentCardGroup Player1CardGroup { get; }
        AdjacentCardGroup Player2CardGroup { get; }
        AdjacentCardGroup Player3CardGroup { get; }
        AdjacentCardGroup Player4CardGroup { get; }
        StackCardGroup DeckCardGroup { get; }
        StackCardGroup TrumpCardGroup { get; }
        StackCardGroup OffScreenPlayer1CardGroup { get; }
        StackCardGroup OffScreenPlayer2CardGroup { get; }
        StackCardGroup OffScreenPlayer3CardGroup { get; }
        StackCardGroup OffScreenPlayer4CardGroup { get; }
    }

    // player positions for which components are provided for
    public enum PlayerOrdinal
    {
        PLAYER1,
        PLAYER2,
        PLAYER3,
        PLAYER4
    }
}
