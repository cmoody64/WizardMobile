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
        public CardGroup(ICanvasFacade canvasFacade, NormalizedPosition origin, double orientationDegress)
        {
            _canvasFacade = canvasFacade;
            _displayCards = new List<UniqueDisplayCard>();
            _cardClickedHandlers = new Queue<Action<UniqueDisplayCard>>();
            Origin = origin;
            OrientationDegress = orientationDegress;

            // async initialization from canvas facade
            _canvasFacade.GetNormalizedCardImageSize().ContinueWith(task => _cardImageSize = task.Result);

            // bind callbacks to handlers
            _canvasFacade.CardClicked += OnCanvasCardClicked;
        }

        public NormalizedPosition Origin { get; }
        public double OrientationDegress { get; }

        protected ICanvasFacade _canvasFacade;
        protected List<UniqueDisplayCard> _displayCards;
        protected NormalizedSize _cardImageSize;

        public void Add(Core.Card card, bool isCardFaceUp = false)
        {
            UniqueDisplayCard displayCard = new UniqueDisplayCard(card, isCardFaceUp);
            _displayCards.Add(displayCard);
            _canvasFacade.AddCard(displayCard, NextOpenPosition, OrientationDegress);
            OnPreCardAddition();
        }

        public void AddRange(IEnumerable<Core.Card> cards, bool isCardFaceUp = false)
        {
            foreach (Core.Card card in cards)
                Add(card);
        }

        // removes the first card in _cards matching the card param
        // animates removal
        public bool Remove(Core.Card card)
        {
            UniqueDisplayCard cardToRemove = GetDisplayCardFromCoreCard(card);
            if (cardToRemove != null)
            {
                _displayCards.Remove(cardToRemove);
                _canvasFacade.RemoveCard(cardToRemove);
                OnPostCardRemoval();
                return true;
            }
            return false;
        }

        // removes all cards without animation
        public void RemoveAll()
        {
            _displayCards.ForEach(displayCard =>
            {
                _canvasFacade.RemoveCard(displayCard);
            });
            _displayCards.Clear();
        }

        // flips a card in place to either face up or face down
        public bool Flip(Core.Card card)
        {
            UniqueDisplayCard cardToFlip = GetDisplayCardFromCoreCard(card);
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

        // transfers the first card in _cards matching the cardName param
        public bool Transfer(Core.Card card, CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            UniqueDisplayCard cardToTransfer = GetDisplayCardFromCoreCard(card);
            if (cardToTransfer != null)
            {
                _displayCards.Remove(cardToTransfer);
                OnPostCardRemoval();

                // resolve rotations so that the animation terminates at the angle of the destination group
                // rotations are rounded up so that the card is flush with the destination
                double resolvedRotations = animationBehavior.Rotations;
                if((this.OrientationDegress + animationBehavior.Rotations * 360) % 360 != destinationGroup.OrientationDegress)
                {
                    var difference = destinationGroup.OrientationDegress - ((this.OrientationDegress + animationBehavior.Rotations * 360) % 360);
                    resolvedRotations += difference / 360;
                }

                var destinationPoint = destinationGroup.NextOpenPosition;
                var transferAnimRequest = new AnimationRequest()
                {
                    Destination = destinationPoint,
                    Duration = animationBehavior.Duration,
                    Delay = animationBehavior.Delay,
                    Rotations = resolvedRotations,
                    ImageGuid = cardToTransfer.Id
                };
                _canvasFacade.QueueAnimationRequest(transferAnimRequest);

                destinationGroup.OnPreCardAddition();
                destinationGroup._displayCards.Add(cardToTransfer);                

                return true;
            }
            return false;
        }

        // queue one shot handlers for when a card within a card group is clicked
        public void QueueClickHandlerForCards(Action<UniqueDisplayCard> cardClickedHandler)
        {
            _cardClickedHandlers.Enqueue(cardClickedHandler);
        }
        private Queue<Action<UniqueDisplayCard>> _cardClickedHandlers;

        private void OnCanvasCardClicked(UniqueDisplayCard displayCard)
        {
            if (_displayCards.Contains(displayCard))
            {
                while (_cardClickedHandlers.Count > 0)
                {
                    var handler = _cardClickedHandlers.Dequeue();
                    handler(displayCard);
                }
            }
        }

        private UniqueDisplayCard GetDisplayCardFromCoreCard(Core.Card card)
        {
            return _displayCards.Find(displayCard => displayCard.CoreCard.Equals(card));
        }

        // added / transfered cards will be placed in this location
        // this determines the layout of a subclass
        protected abstract NormalizedPosition NextOpenPosition { get; }

        protected virtual void OnPreCardAddition() { } // called before a card is added to _displayCards in the far right position
        protected virtual void OnPostCardRemoval() { } // called after a card is removed from _displayCards
    }





    // each card is directly on top of each other, only the top card is visible
    // no addition / removal animations
    public class StackCardGroup : CardGroup
    {
        public StackCardGroup(GamePage parent, NormalizedPosition origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        { }

        protected override NormalizedPosition NextOpenPosition => Origin;
    }

    // cards are in a vertical line and cover up 90% of the card beneath them
    public class TaperedStackCardGroup : CardGroup
    {
        public TaperedStackCardGroup(GamePage parent, NormalizedPosition origin, double orientationDegress)
            : base(parent, origin, orientationDegress)
        {
        }

        protected override NormalizedPosition NextOpenPosition => Origin;

        protected override void OnPreCardAddition() { }
        protected override void OnPostCardRemoval() { }
    }

    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, NormalizedPosition origin, double orientationDegrees)
            : base(parent, origin, orientationDegrees)
        {
        }

        protected override NormalizedPosition NextOpenPosition => GeneratePositions(_displayCards.Count + 1, _cardImageSize, Origin).Last();

        protected override void OnPreCardAddition()
        {
            // because a card has not yet been added (this hook is PRE addition), generate the new positions based off of
            // _displayCards having one more card than it currently has, but only animate for the cards currently in _displayCards
            List<NormalizedPosition> newPositions = GeneratePositions(_displayCards.Count+1, _cardImageSize, Origin);
            for (int i = 0; i < newPositions.Count-1; i++)
            {
                _canvasFacade.QueueAnimationRequest(new AnimationRequest
                {
                    Destination = newPositions[i],
                    Duration = 0.2,
                    ImageGuid = _displayCards[i].Id
                });
            }
        }
        protected override void OnPostCardRemoval()
        {
            //List<CanvasPosition> newPositions = GeneratePositions(_displayCards.Count, _cardImageSize, Origin);
            //for (int i = 0; i < newPositions.Count; i++)
            //{
            //    _canvasFacade.QueueAnimationRequest(new AnimationRequest
            //    {
            //        Destination = newPositions[i],
            //        Duration = 0.2,
            //        ImageGuid = _displayCards[i].Id
            //    });
            //}
        }


        private static List<NormalizedPosition> GeneratePositions(int displayCount, NormalizedSize imageSize, NormalizedPosition origin)
        {
            if (displayCount <= 0)
                return null;

            double margin = imageSize.NormalizedWidth * 1.2 - imageSize.NormalizedWidth * .05 * displayCount;
            List<NormalizedPosition> positions = new List<NormalizedPosition>();
            double startingX = origin.NormalizedX - (((double)displayCount - 1) / 2) * margin;
            //if (displayCount % 2 == 0)
            //    // nonzero even number of cards
            //    startingX = origin.NormalizedX - ((displayCount - 1) / 2) * margin;
            //else
            //    // nonzero odd number of cards
            //    startingX = origin.NormalizedX - ((displayCount - 2) / 2) * margin;

            for(int i = 0; i < displayCount; i++)
            {
                var x = startingX + margin * i;
                var y = origin.NormalizedY;
                positions.Add(new NormalizedPosition(x, y));
            }

            return positions;
        }
    }
}
