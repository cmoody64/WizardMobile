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
        public CardGroup(ICanvasFacade canvasFacade, NormalizedPosition origin, Orientation orientation)
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
        protected ICanvasFacade _canvasFacade;
        protected List<UniqueDisplayCard> _displayCards;
        protected NormalizedSize _cardImageSize;
        protected int _curZIndex;

        public void Add(Core.Card card, bool isCardFaceUp = false)
        {
            OnPreCardAddition();
            UniqueDisplayCard displayCard = new UniqueDisplayCard(card, isCardFaceUp);
            _displayCards.Add(displayCard);
            _canvasFacade.AddCard(displayCard, NextOpenPosition, OrientationDegress, _curZIndex);            
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
                // finalize transfer by adding the card to the destination groups display cards and updating its zIndex to that of the destination group
                destinationGroup._displayCards.Add(cardToTransfer);
                _canvasFacade.UpdateCard(cardToTransfer, zIndex: destinationGroup._curZIndex);

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
        protected abstract NormalizedPosition NextOpenPosition { get; }

        // called before a card is added to _displayCards in the far right position
        protected virtual void OnPreCardAddition()
        {
            _curZIndex++;
        }
        protected virtual void OnPostCardRemoval() { } // called after a card is removed from _displayCards

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


    // each card is directly on top of each other, only the top card is visible
    // no addition / removal animations
    public class StackCardGroup : CardGroup
    {
        public StackCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
            : base(parent, origin, orientation)
        { }

        protected override NormalizedPosition NextOpenPosition => Origin;
    }

    // cards are in a vertical line and cover up 90% of the card beneath them
    public class TaperedStackCardGroup : CardGroup
    {
        public TaperedStackCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
            : base(parent, origin, orientation)
        {
        }

        protected override NormalizedPosition NextOpenPosition => Origin;

        protected override void OnPreCardAddition() { base.OnPreCardAddition(); }
        protected override void OnPostCardRemoval() { }
    }

    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
            : base(parent, origin, orientation)
        {
        }

        protected override NormalizedPosition NextOpenPosition => GeneratePositions(_displayCards.Count + 1).Last();
        protected Axis OrientationAxis => _orientation == Orientation.DEGREES_0 || _orientation == Orientation.DEGREES_180 ? Axis.X : Axis.Y;

        protected override void OnPreCardAddition()
        {
            base.OnPreCardAddition();

            // because a card has not yet been added (this hook is PRE addition), generate the new positions based off of
            // _displayCards having one more card than it currently has, but only animate for the cards currently in _displayCards
            List<NormalizedPosition> newPositions = GeneratePositions(_displayCards.Count+1);
            for (int i = 0; i < newPositions.Count-1; i++)
            {
                _canvasFacade.QueueAnimationRequest(new AnimationRequest
                {
                    Destination = newPositions[i],
                    Duration = 0.3,
                    ImageGuid = _displayCards[i].Id
                });
            }
            _curPositions = newPositions;
        }

        protected override void OnPostCardRemoval()
        {
            List<NormalizedPosition> newPositions = GeneratePositions(_displayCards.Count);
            for (int i = 0; i < newPositions.Count; i++)
            {
                _canvasFacade.QueueAnimationRequest(new AnimationRequest
                {
                    Destination = newPositions[i],
                    Duration = 0.3,
                    ImageGuid = _displayCards[i].Id
                });
            }
            _curPositions = newPositions;
        }

        protected List<NormalizedPosition> _curPositions;

        protected List<NormalizedPosition> GeneratePositions(int positionCount)
        {
            List<NormalizedPosition> positions = new List<NormalizedPosition>();

            if(positionCount > 0)
            {
                double margin = _cardImageSize.NormalizedWidth * 0.8 - _cardImageSize.NormalizedWidth * .03 * positionCount;

                if(OrientationAxis == Axis.X)
                {
                    double startingX = Origin.NormalizedX - (((double)positionCount - 1) / 2) * margin;
                    for (int i = 0; i < positionCount; i++)
                    {
                        var x = startingX + margin * i;
                        var y = Origin.NormalizedY;
                        positions.Add(new NormalizedPosition(x, y));
                    }
                }
                else
                {
                    double startingY = Origin.NormalizedY - (((double)positionCount - 1) / 2) * margin;
                    for (int i = 0; i < positionCount; i++)
                    {
                        var x = Origin.NormalizedX;
                        var y = startingY + margin * i;                        
                        positions.Add(new NormalizedPosition(x, y));
                    }
                }

            }
            return positions;
        }
    }


    // adjacent card group that is interactive on hove
    public class InteractiveAdjacentCardGroup: AdjacentCardGroup
    {
        public InteractiveAdjacentCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
        : base(parent, origin, orientation)
        {
            EndInteractiveSession();

            // bind callbacks to handlers
            _canvasFacade.CardClicked += OnCardClicked;
            _canvasFacade.CardPointerEntered += OnCardPointerEntered;
            _canvasFacade.CardPointerExited += OnCardPointerExited;
        }

        private bool _isInInteractiveState;
        private IReadOnlyList<UniqueDisplayCard> _curInteractiveCards;
        private Action<UniqueDisplayCard> _curCardClickedHandler;

        public void StartInteractiveSession(IReadOnlyList<Core.Card> interactiveCards, Action<UniqueDisplayCard> cardClickedHandler)
        {
            if (_isInInteractiveState)
                throw new InvalidOperationException("attempted to start interactive card group session while session was already in progress");

            List<UniqueDisplayCard> interactiveDisplayCards = new List<UniqueDisplayCard>();
            foreach (Core.Card card in interactiveCards)
                interactiveDisplayCards.Add(GetDisplayCardFromCoreCard(card));

            _isInInteractiveState = true;
            _curCardClickedHandler = cardClickedHandler;
            _curInteractiveCards = interactiveDisplayCards;

            // dim cards that are non-interactive as a hint to the user
            IEnumerable<UniqueDisplayCard> nonInteractiveCards = _displayCards.Where(displayCard => !_curInteractiveCards.Contains(displayCard));
            foreach (UniqueDisplayCard nonInteractiveCard in nonInteractiveCards)
                _canvasFacade.UpdateCard(nonInteractiveCard, dimmed: true);

        }

        private void EndInteractiveSession()
        {
            // un-dim cards that are non-interactive which were dimmed at the beginning of an interactive session
            IEnumerable<UniqueDisplayCard> nonInteractiveCards = _displayCards.Where(displayCard => !_curInteractiveCards.Contains(displayCard));
            foreach (UniqueDisplayCard nonInteractiveCard in nonInteractiveCards)
                _canvasFacade.UpdateCard(nonInteractiveCard, dimmed: false);

            _isInInteractiveState = false;
            _curInteractiveCards = null;
            _curCardClickedHandler = null;
        }

        private void OnCardClicked(UniqueDisplayCard clickedCard)
        {
            if (_isInInteractiveState && _curInteractiveCards.Contains(clickedCard))
            {
                _curCardClickedHandler(clickedCard);
                EndInteractiveSession();
            }
        }

        private void OnCardPointerEntered(UniqueDisplayCard card)
        {
            if(_isInInteractiveState && _curInteractiveCards.Contains(card))
            {
                var curPosIndex = _displayCards.FindIndex(displayCard => displayCard.Equals(card));
                var curPos = _curPositions[curPosIndex];

                if (OrientationAxis == Axis.X)
                {
                    double offsetY = _orientation == Orientation.DEGREES_0 ? -3 : 3;
                    _canvasFacade.UpdateCard(card, new NormalizedPosition(curPos.NormalizedX, curPos.NormalizedY + offsetY));
                }
                else
                {
                    double offsetX = _orientation == Orientation.DEGREES_90 ? 2 : 2;
                    _canvasFacade.UpdateCard(card, new NormalizedPosition(curPos.NormalizedX + offsetX, curPos.NormalizedY));
                }                    
            }
        }

        private void OnCardPointerExited(UniqueDisplayCard card)
        {
            if (_isInInteractiveState && _curInteractiveCards.Contains(card))
            {
                var curPosIndex = _displayCards.FindIndex(displayCard => displayCard.Equals(card));
                _canvasFacade.UpdateCard(card, _curPositions[curPosIndex]);
            }
        }

    }
}
