using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.Gameplay
{
    // implements basic card group functionality - adds / remove image from canvas and execute animated transfer
    // no animated add / remove / onIncomingTransfer
    // only top card is visible
    public abstract class CardGroup
    {
        public CardGroup(ICanvasFacade canvasFacade, Point origin, double orientationDegress)
        {
            _canvasFacade = canvasFacade;
            Origin = origin;
            OrientationDegress = orientationDegress;
        }

        public Point Origin { get; }
        public double OrientationDegress { get; }

        protected ICanvasFacade _canvasFacade;
        private List<UniqueCard> _cards;

        public void Add(string cardName)
        {
            _cards.Add(new UniqueCard(cardName));            
            // add card to canvas at next location
            OnAnimateCardAddition();
        }

        // removes the first card in _cards matching the cardName param
        public bool Remove(string cardName)
        {
            int indexToRemove = _cards.FindIndex(card => card.Name == cardName);
            if(indexToRemove > -1)
            {
                // remove card from canvas
                OnAnimateCardRemoval();
                return true;
            }
            return false;
        }

        // replaces the first card in _cards matching the toReplace param
        // if no match is found, a replacement cannot be done and false is returned
        public bool Replace(string cardToReplace, string newCard)
        {
            var oldCard = _cards.FirstOrDefault(card => card == cardToReplace);
            if(oldCard != null)
            {
                // replace card bitmap with newCard bitmap
                return true;
            }
            return false;
        }


        public void Transfer(string cardName, CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            _cards.Remove(cardName);
            OnAnimateCardRemoval();

            var destinationPoint = destinationGroup.NextLocation;
            // todo animate card to destination point
            destinationGroup._cards.Add(cardName);
            destinationGroup.OnAnimateCardRemoval();
        }

        // added / transfered cards will be placed in this location
        // this determines the layout of a subclass
        protected abstract Point NextLocation { get; }

        protected virtual void OnAnimateCardAddition() { }
        protected virtual void OnAnimateCardRemoval() { }
    }

    // each card is directly on top of each other, only the top card is visible
    // no addition / removal animations
    public class StackCardGroup : CardGroup
    {
        public StackCardGroup(GamePage parent, Point origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        { }

        protected override Point NextLocation => Origin;
    }

    // cards are in a vertical line and cover up 90% of the card beneath them
    public class TaperedStackCardGroup : CardGroup
    {
        public TaperedStackCardGroup(GamePage parent, Point origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        {
        }

        protected override Point NextLocation => Origin;

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }

    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, Point origin, double orientationDegrees)
            : base(parent, origin, orientationDegrees)
        {
        }

        protected override Point NextLocation => throw new NotImplementedException();

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }
}
