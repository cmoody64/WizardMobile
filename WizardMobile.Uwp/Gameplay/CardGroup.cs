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
        public CardGroup(ICanvasFacade canvasFacade, CanvasPosition origin, double orientationDegress)
        {
            _canvasFacade = canvasFacade;
            _cards = new List<UniqueCard>();
            Origin = origin;
            OrientationDegress = orientationDegress;
        }

        public CanvasPosition Origin { get; }
        public double OrientationDegress { get; }

        protected ICanvasFacade _canvasFacade;
        private List<UniqueCard> _cards;

        public void Add(string cardName)
        {
            UniqueCard card = new UniqueCard(cardName);
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
            UniqueCard cardToRemove = _cards.FirstOrDefault(card => card.Name == cardName);
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
            UniqueCard cardToUpdate = _cards.FirstOrDefault(card => card.Name == cardToReplace);
            if(cardToUpdate != null)
            {
                cardToUpdate.Name = newCard;
                _canvasFacade.ReplaceCardBitmap(cardToUpdate, newCard);
                return true;
            }
            return false;
        }

        // transfers the first card in _cards matching the cardName param
        public bool Transfer(string cardName, CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            UniqueCard cardToTransfer = _cards.FirstOrDefault(card => card.Name == cardName);
            if(cardToTransfer != null)
            {
                _cards.Remove(cardToTransfer);
                OnAnimateCardRemoval();

                // resolve rotations so that the animation terminates at the angle of the destination group
                // rotations are rounded up so that the card is flush with the destination
                double resolvedRotations = animationBehavior.Rotations;
                if(this.OrientationDegress + animationBehavior.Rotations * 360 != destinationGroup.OrientationDegress)
                {
                    var difference = destinationGroup.OrientationDegress - (this.OrientationDegress + animationBehavior.Rotations * 360);
                    resolvedRotations += difference;
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
        public StackCardGroup(GamePage parent, CanvasPosition origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        { }

        protected override CanvasPosition NextLocation => Origin;
    }

    // cards are in a vertical line and cover up 90% of the card beneath them
    public class TaperedStackCardGroup : CardGroup
    {
        public TaperedStackCardGroup(GamePage parent, CanvasPosition origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        {
        }

        protected override CanvasPosition NextLocation => Origin;

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }

    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, CanvasPosition origin, double orientationDegrees)
            : base(parent, origin, orientationDegrees)
        {
        }

        protected override CanvasPosition NextLocation => throw new NotImplementedException();

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }
}
