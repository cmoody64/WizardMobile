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
        void AddCard(UniqueDisplayCard card, CanvasPosition position, double orientationDegrees);
        void RemoveCard(UniqueDisplayCard card);
        void UpdateCard(UniqueDisplayCard cardToFlip);
        void QueueAnimationRequest(AnimationRequest animation);
        void QueueAnimationRequests(IEnumerable<AnimationRequest> animations);
    }
}
