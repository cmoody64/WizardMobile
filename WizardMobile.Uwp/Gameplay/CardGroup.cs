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
        public CardGroup(ICanvasFacade canvasFacade, CanvasPosition origin, double orientationDegress, bool isFaceUp = false)
        {
            _canvasFacade = canvasFacade;
            _cards = new List<UniqueDisplayCard>();
            Origin = origin;
            OrientationDegress = orientationDegress;
            IsFaceUp = isFaceUp;
        }

        public CanvasPosition Origin { get; }
        public double OrientationDegress { get; }
        public bool IsFaceUp { get; }

        protected ICanvasFacade _canvasFacade;
        private List<UniqueDisplayCard> _cards;

        public void Add(string cardName)
        {
            UniqueDisplayCard card = new UniqueDisplayCard(cardName);
            _cards.Add(card);
            _canvasFacade.AddToCanvas(card, NextLocation, OrientationDegress);
            OnAnimateCardAddition();
        }

        public void AddRange(IEnumerable<string> cardNames)
        {
            foreach (string cardName in cardNames)
                Add(cardName);
        }

        // removes the first card in _cards matching the cardName param
        public bool Remove(string cardName)
        {
            UniqueDisplayCard cardToRemove = _cards.FirstOrDefault(card => card.DisplayKey == cardName);
            if(cardToRemove != null)
            {
                _cards.Remove(cardToRemove);
                _canvasFacade.RemoveFromCanvas(cardToRemove);
                OnAnimateCardRemoval();
                return true;
            }
            return false;
        }

        // replaces the first card in _cards matching the toReplace param
        // if no match is found, a replacement cannot be done and false is returned
        public bool Replace(string cardToReplace, string newCard)
        {
            UniqueDisplayCard cardToUpdate = _cards.FirstOrDefault(card => card.DisplayKey == cardToReplace);
            if(cardToUpdate != null)
            {
                cardToUpdate.DisplayKey = newCard;
                _canvasFacade.ReplaceCardBitmap(cardToUpdate, newCard);
                return true;
            }
            return false;
        }

        // transfers the first card in _cards matching the cardName param
        public bool Transfer(string cardName, CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            UniqueDisplayCard cardToTransfer = _cards.FirstOrDefault(card => card.DisplayKey == cardName);
            if(cardToTransfer != null)
            {
                _cards.Remove(cardToTransfer);
                OnAnimateCardRemoval();

                // resolve rotations so that the animation terminates at the angle of the destination group
                // rotations are rounded up so that the card is flush with the destination
                double resolvedRotations = animationBehavior.Rotations;
                if((this.OrientationDegress + animationBehavior.Rotations * 360) % 360 != destinationGroup.OrientationDegress)
                {
                    var difference = destinationGroup.OrientationDegress - ((this.OrientationDegress + animationBehavior.Rotations * 360) % 360);
                    resolvedRotations += difference / 360;
                }

                var destinationPoint = destinationGroup.NextLocation;
                var transferAnimRequest = new AnimationRequest()
                {
                    Destination = destinationPoint,
                    Duration = animationBehavior.Duration,
                    Delay = animationBehavior.Delay,
                    Rotations = resolvedRotations,
                    ImageGuid = cardToTransfer.Id
                };
                _canvasFacade.QueueAnimationRequest(transferAnimRequest);

                destinationGroup._cards.Add(cardToTransfer);
                destinationGroup.OnAnimateCardAddition();

                return true;
            }
            return false;
        }

        // queues a one shot event handler for each card
        // the handler receives the card that was clicked
        public void QueueClickHandlerForCards(Action<UniqueDisplayCard> handler) { }

        // added / transfered cards will be placed in this location
        // this determines the layout of a subclass
        protected abstract CanvasPosition NextLocation { get; }

        protected virtual void OnAnimateCardAddition() { }
        protected virtual void OnAnimateCardRemoval() { }
    }

    // each card is directly on top of each other, only the top card is visible
    // no addition / removal animations
    public class StackCardGroup : CardGroup
    {
        public StackCardGroup(GamePage parent, CanvasPosition origin, double orientationDegress, bool isFaceUp = false)
            : base(parent, origin, orientationDegress, isFaceUp)
        { }

        protected override CanvasPosition NextLocation => Origin;
    }

    // cards are in a vertical line and cover up 90% of the card beneath them
    public class TaperedStackCardGroup : CardGroup
    {
        public TaperedStackCardGroup(GamePage parent, CanvasPosition origin, double orientationDegress, bool isFaceUp = false)
            : base(parent, origin, orientationDegress, isFaceUp)
        {
        }

        protected override CanvasPosition NextLocation => Origin;

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }

    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, CanvasPosition origin, double orientationDegrees, bool isFaceUp = false)
            : base(parent, origin, orientationDegrees, isFaceUp)
        {
        }

        protected override CanvasPosition NextLocation => Origin;

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }
}
