using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.Gameplay
{
    public interface ICanvasFacade
    {
        void AddCard(UniqueDisplayCard card, NormalizedPosition position, double orientationDegrees);
        void RemoveCard(UniqueDisplayCard card);
        void UpdateCard(UniqueDisplayCard cardToFlip, NormalizedPosition canvasPositon = null, double? orientationDegrees = null);
        Task<NormalizedSize> GetNormalizedCardImageSize();
        void QueueAnimationRequest(AnimationRequest animation);
        void QueueAnimationRequests(IEnumerable<AnimationRequest> animations);
        event Action<UniqueDisplayCard> CardClicked; // fires when any card is clicked, passing the card to the handler
        event Action<UniqueDisplayCard> CardPointerEntered;
        event Action<UniqueDisplayCard> CardPointerExited;
    }
}
