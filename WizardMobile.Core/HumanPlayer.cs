using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(IWizardFrontend frontend, string Name): base(frontend, Name)
        {                
        }

        public override int MakeBid(GameContext gameContext)
        {
            return _frontend.PromptPlayerBid(this);
        }

        public override Card MakeTurn(GameContext gameContext)
        {
            var cardToPlay = _frontend.PromptPlayerCardSelection(this);
            _hand.Remove(cardToPlay);
            return cardToPlay;
        }
    }
}
