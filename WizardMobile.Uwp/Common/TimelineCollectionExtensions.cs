using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

namespace WizardMobile.Uwp.Common
{
    public static class TimelineCollectionExtensions
    {
        public static void AddRange(this TimelineCollection timelineCollection, IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines)
                timelineCollection.Add(timeline);
        }
    }
}
