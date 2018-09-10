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

namespace WizardMobile.Uwp.GamePage
{
    // implements basic card group functionality - adds / remove image from canvas and execute animated transfer
    // no animated add / remove / onIncomingTransfer
    // only top card is visible
    public abstract class CardGroup
    {
        public CardGroup(ICardCanvasProvider canvasFacade, NormalizedPosition origin, Orientation orientation)
        {
            _canvasFacade = canvasFacade;
            _displayCards = new List<UniqueDisplayCard>();
            Origin = origin;
            _orientation = orientation;
            OrientationDegress = (double)orientation;
            _curZIndex = 0;

            // async initialization from canvas facade
            _canvasFacade.GetNormalizedCardImageSize().ContinueWith(task => _cardImageSize = task.Result);
        }

        public NormalizedPosition Origin { get; }
        public double OrientationDegress { get; }

        protected Orientation _orientation;
        protected ICardCanvasProvider _canvasFacade;
        protected List<UniqueDisplayCard> _displayCards;
        protected NormalizedSize _cardImageSize;
        protected int _curZIndex;

        public void Add(Core.Card card, bool isCardFaceUp = false)
        {            
            UniqueDisplayCard displayCard = new UniqueDisplayCard(card, isCardFaceUp);
            var nextOpenPositionInfo = NextOpenPositions(1)[0];
            OnPreCardsAddition(new List<UniqueDisplayCard> { displayCard });
            _displayCards.Add(displayCard);

            _canvasFacade.AddCard(displayCard, nextOpenPositionInfo.Item1 /*pos*/, OrientationDegress, nextOpenPositionInfo.Item2 /*zIndex*/); 
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
                OnPostCardsRemoval();
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
                var nextOpenPositionInfo = destinationGroup.NextOpenPositions(1)[0];
                var destinationPoint = nextOpenPositionInfo.Item1;
                var newZIndex = nextOpenPositionInfo.Item2;

                this._displayCards.Remove(cardToTransfer);
                this.OnPostCardsRemoval();

                // resolve rotations so that the animation terminates at the angle of the destination group
                // rotations are rounded up so that the card is flush with the destination
                double resolvedRotations = ResolveRotations(destinationGroup, animationBehavior);

                var transferAnimRequest = new NamedAnimationRequest()
                {
                    Destination = destinationPoint,
                    Duration = animationBehavior.Duration,
                    Delay = animationBehavior.Delay,
                    Rotations = resolvedRotations,
                    TargetElementName = cardToTransfer.Id
                };
                _canvasFacade.QueueAnimationRequest(transferAnimRequest);

                destinationGroup.OnPreCardsAddition(new List<UniqueDisplayCard> { cardToTransfer });
                destinationGroup._displayCards.Add(cardToTransfer);

                // finish transfer to destination group by updating the card to have the correct zIndex as determined by its destination group
                _canvasFacade.UpdateCard(cardToTransfer, zIndex: newZIndex);
                return true;
            }
            return false;
        }

        public void TransferAll(CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            var newPositions = destinationGroup.NextOpenPositions(_displayCards.Count);

            var cardsToTransfer = new List<UniqueDisplayCard>(_displayCards);
            this._displayCards.Clear();
            OnPostCardsRemoval();

            // resolve rotations so that the animation terminates at the angle of the destination group
            // rotations are rounded up so that the card is flush with the destination
            double resolvedRotations = ResolveRotations(destinationGroup, animationBehavior);

            for(int i = 0; i < cardsToTransfer.Count; i++)
            {
                var cardToTransfer = cardsToTransfer[i];
                var destinationPoint = newPositions[i].Item1;
                var newZIndex = newPositions[i].Item2;

                var transferAnimRequest = new NamedAnimationRequest()
                {
                    Destination = destinationPoint,
                    Duration = animationBehavior.Duration,
                    Delay = animationBehavior.Delay,
                    Rotations = resolvedRotations,
                    TargetElementName = cardToTransfer.Id
                };
                _canvasFacade.QueueAnimationRequest(transferAnimRequest);      

                // finish transfer to destination group by updating the card to have the correct zIndex as determined by its destination group
                _canvasFacade.UpdateCard(cardToTransfer, zIndex: newZIndex);
            }



            destinationGroup.OnPreCardsAddition(cardsToTransfer);
            destinationGroup._displayCards.AddRange(cardsToTransfer);
        }

        // sets the given card to have the highest z Index in the card group, thus displaying it on top of any other card it may overlap with
        // returns true if successfully found and updated, false otherwise
        public bool BringToFront(Core.Card card)
        {
            var displayCardIndex = _displayCards.FindIndex(displayCard => displayCard.CoreCard == card);
            if(displayCardIndex != -1)
            {
                // bring to front switches the current card with the last card in displayCards and updates the zIndex accordingly (left to right)
                // if the current card is already at the end of the list, it already has the highest zIndex - otherwise, reorder the cards
                if(displayCardIndex < _displayCards.Count - 1)
                {
                    var temp = _displayCards.Last();
                    _displayCards[_displayCards.Count - 1] = _displayCards[displayCardIndex];
                    _displayCards[displayCardIndex] = temp;

                    // iterate through all cards and assign them sequential zIndex values except for the passed in card, which is assigned the
                    // highest zIndex of _curZIndex
                    int zCount = 0;
                    for (int i = 0; i < _displayCards.Count; i++)
                    {
                        int curZIndex = i == displayCardIndex ? _curZIndex : zCount++;
                        _canvasFacade.UpdateCard(_displayCards[i], zIndex: zCount);
                    }
                }
                return true;
            }
            return false;
        }

        protected UniqueDisplayCard GetDisplayCardFromCoreCard(Core.Card card)
        {
            return _displayCards.Find(displayCard => displayCard.CoreCard.Equals(card));
        }

        // added / transfered cards will be placed in this location
        // this determines the layout of a subclass
        protected abstract List<Tuple<NormalizedPosition, int /*zIndex*/>> NextOpenPositions(int numPositions);
        
        protected virtual void OnPreCardsAddition(List<UniqueDisplayCard> cardsToBeAdded) { } // called before cards are added to _displayCards
        protected virtual void OnPostCardsRemoval() { } // called after cards are removed from _displayCards

        // adjust the rotations of an animation behavaior so that a card will end its rotations with the correct orientation for its destination
        private double ResolveRotations(CardGroup destinationGroup, AnimationBehavior animationBehavior)
        {
            double resolvedRotations = animationBehavior.Rotations;
            if ((this.OrientationDegress + animationBehavior.Rotations * 360) % 360 != destinationGroup.OrientationDegress)
            {
                var difference = destinationGroup.OrientationDegress - ((this.OrientationDegress + animationBehavior.Rotations * 360) % 360);
                resolvedRotations += difference / 360;
            }
            return resolvedRotations;
        }

        public enum Orientation
        {
            DEGREES_0 = 0,
            DEGREES_90 = 90,
            DEGREES_180 = 180,
            DEGREES_270 = 270
        }

        protected enum Axis
        {
            X,
            Y
        }
    }
}
