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
        public CardGroup(GamePage parent, Point origin, double orientationDegress)
        {
            _gamePage = parent;
            Origin = origin;
            OrientationDegress = orientationDegress;
        }

        public Point Origin { get; }
        public double OrientationDegress { get; }

        protected GamePage _gamePage;
        private List<string> _cards;

        public void Add(string cardName)
        {
            _cards.Add(cardName);
            // add card to canvas at next location
            OnAnimateCardAddition();
        }

        public void Remove(string cardName)
        {
            _cards.Remove(cardName);
            // remove card from canvas
            OnAnimateCardRemoval();
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
