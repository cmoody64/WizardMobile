using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;

namespace WizardMobile.Uwp.Common
{
    public static class DispatcherTaskExtensions
    {
        // extension method to CoreDispatcher that allows a value to be returned from RunAsync
        public static async Task<T> RunAsyncWithResult<T>(this CoreDispatcher dispatcher, CoreDispatcherPriority priority, Func<Task<T>> func)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }
    }

    public static class TimelineCollectionExtensions
    {
        // Binding to a timeline collection ensures that a timeline is only present in the timeline collection while the timeline is active
        // thus, it is bound to the timeline collection for its lifetime, and unbound after
        public static void Bind(this TimelineCollection timelineCollection, Timeline timeline)
        {
            timelineCollection.Add(timeline);
            timeline.Completed += (sender, eventArgs) =>
            {
                //timeline.
            };
        }

        public static void AddRange(this TimelineCollection timelineCollection, IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines)
                timelineCollection.Add(timeline);
        }
    }    
}
