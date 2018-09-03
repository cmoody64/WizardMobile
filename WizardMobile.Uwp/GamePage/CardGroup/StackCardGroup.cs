using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.GamePage
{
    // each card is directly on top of each other, only the top card is visible
    // no addition / removal animations
    // due to the fancy addition / removal animations, more than one transfer can't take place at a given time - will result in runtime error
    public class StackCardGroup : CardGroup
    {
        public StackCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
            : base(parent, origin, orientation)
        { }

        protected override List<Tuple<NormalizedPosition, int>> NextOpenPositions(int numPositions) => Enumerable.Repeat(Origin, numPositions)
            .Select((position, index) => new Tuple<NormalizedPosition, int>(position, _curZIndex + index))
            .ToList();

        protected override void OnPostCardsRemoval()
        {
            _curZIndex = _displayCards.Count;
        }

        protected override void OnPreCardsAddition(List<UniqueDisplayCard> cardsToBeAdded)
        {
            _curZIndex += cardsToBeAdded.Count;
        }
    }
}
