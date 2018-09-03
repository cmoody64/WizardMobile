using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.GamePage
{
    // adjacent card group that is interactive on hover and click
    public class InteractiveAdjacentCardGroup : AdjacentCardGroup
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
            if (_isInInteractiveState && _curInteractiveCards.Contains(card))
            {
                var curPosIndex = _displayCards.FindIndex(displayCard => displayCard.Equals(card));
                var curPos = _curPositions[curPosIndex];

                if (OrientationAxis == Axis.X)
                {
                    double offsetY = _orientation == Orientation.DEGREES_0 ? -2 : 2;
                    _canvasFacade.UpdateCard(card, new NormalizedPosition(curPos.NormalizedX, curPos.NormalizedY + offsetY));
                }
                else
                {
                    double offsetX = _orientation == Orientation.DEGREES_90 ? 1 : 1;
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
