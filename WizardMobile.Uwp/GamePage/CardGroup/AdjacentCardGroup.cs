using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.GamePage
{
    public class AdjacentCardGroup : CardGroup
    {
        public AdjacentCardGroup(GamePage parent, NormalizedPosition origin, Orientation orientation)
            : base(parent, origin, orientation)
        {
        }

        protected Axis OrientationAxis => _orientation == Orientation.DEGREES_0 || _orientation == Orientation.DEGREES_180 ? Axis.X : Axis.Y;
        protected override List<Tuple<NormalizedPosition, int>> NextOpenPositions(int numPositions)
        {
            return GeneratePositions(_displayCards.Count + numPositions)
                .GetRange(_displayCards.Count, numPositions)
                .Select((position, index) => new Tuple<NormalizedPosition, int>(position, _curZIndex + index))
                .ToList();
        }

        protected override void OnPreCardsAddition(List<UniqueDisplayCard> cardsToBeAdded)
        {

            // because a card has not yet been added (this hook is PRE addition), generate the new positions based off of
            // _displayCards having one more card than it currently has, but only animate for the cards currently in _displayCards
            List<NormalizedPosition> newPositions = GeneratePositions(_displayCards.Count + cardsToBeAdded.Count);
            for (int i = 0; i < _displayCards.Count; i++)
            {
                _canvasFacade.QueueAnimationRequest(new AnimationRequest
                {
                    Destination = newPositions[i],
                    Duration = 0.3,
                    ImageGuid = _displayCards[i].Id
                });
            }
            _curPositions = newPositions;
            _curZIndex += cardsToBeAdded.Count();
        }

        protected override void OnPostCardsRemoval()
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
            _curZIndex = _curPositions.Count;
        }

        protected List<NormalizedPosition> _curPositions;

        protected List<NormalizedPosition> GeneratePositions(int positionCount)
        {
            List<NormalizedPosition> positions = new List<NormalizedPosition>();

            if (positionCount > 0)
            {
                double margin = _cardImageSize.NormalizedWidth * 0.8 - _cardImageSize.NormalizedWidth * .03 * positionCount;

                if (OrientationAxis == Axis.X)
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
}
