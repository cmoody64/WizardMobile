using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace WizardMobile.Uwp.Gameplay
{
    public interface ICanvasFacade
    {
        void AddToCanvas(UniqueCard card, Point position, double orientationDegrees);
        void RemoveFromCanvas(UniqueCard card);
        void ReplaceCardBitmap(UniqueCard cardToReplace, string newCardName);
        void QueueAnimation(DoubleAnimation animation);
        void QueueAnimations(IEnumerable<DoubleAnimation> animations);
    }
}
