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
            _displayCards = new List<UniqueDisplayCard>();
            Origin = origin;
            OrientationDegress = orientationDegress;
        }

        public CanvasPosition Origin { get; }
        public double OrientationDegress { get; }

        protected ICanvasFacade _canvasFacade;
        private List<UniqueDisplayCard> _displayCards;

        public void Add(Core.Card card, bool isCardFaceUp = true)
        {
            UniqueDisplayCard displayCard = new UniqueDisplayCard(card, isCardFaceUp);
            _displayCards.Add(displayCard);
            _canvasFacade.AddCard(displayCard, NextLocation, OrientationDegress);
            OnAnimateCardAddition();
        }

        public void AddRange(IEnumerable<Core.Card> cards, bool isCardFaceUp = true)
        {
            foreach (Core.Card card in cards)
                Add(card);
        }

        // removes the first card in _cards matching the cardName param
        public bool Remove(Core.Card card)
        {
            UniqueDisplayCard cardToRemove = _displayCards.FirstOrDefault(displayCard => displayCard.DisplayKey == card.Name);
            if(cardToRemove != null)
            {
                _displayCards.Remove(cardToRemove);
                _canvasFacade.RemoveCard(cardToRemove);
                OnAnimateCardRemoval();
                return true;
            }
            return false;
        }

        // flips a card in place to either face up or face down
        public bool Flip(Core.Card card)
        {
            UniqueDisplayCard cardToFlip = _displayCards.Find(displayCard => displayCard.DisplayKey == card.Name);
            return FlipImpl(cardToFlip);
        }

        public void FlipAll()
        {
            _displayCards.ForEach(displayCard => FlipImpl(displayCard));
        }

        private bool FlipImpl(UniqueDisplayCard card)
        {
            if (card != null)
            {
                card.IsFaceUp = !card.IsFaceUp;
                _canvasFacade.UpdateCard(card);
            }
            return false;
        }


        // replaces the first card in _cards matching the toReplace param
        // if no match is found, a replacement cannot be done and false is returned
        //public bool Replace(Core.Card cardToReplace, Core.Card newCard)
        //{
        //    UniqueDisplayCard cardToUpdate = _displayCards.FirstOrDefault(displayCard => displayCard.DisplayKey == cardToReplace.Name);
        //    if(cardToUpdate != null)
        //    {
        //        cardToUpdate.DisplayKey = newCard;
        //        _canvasFacade.ReplaceCardBitmap(cardToUpdate, newCard);
        //        return true;
        //    }
        //    return false;
        //}

        // transfers the first card in _cards matching the cardName param
        public bool Transfer(Core.Card card, CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            UniqueDisplayCard cardToTransfer = _displayCards.FirstOrDefault(displayCard => displayCard.DisplayKey == card.Name);
            if(cardToTransfer != null)
            {
                _displayCards.Remove(cardToTransfer);
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

                destinationGroup._displayCards.Add(cardToTransfer);
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

        protected override CanvasPosition NextLocation => Origin;

        protected override void OnAnimateCardAddition() { }
        protected override void OnAnimateCardRemoval() { }
    }
}
