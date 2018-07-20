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
        public static void AddRange(this TimelineCollection timelineCollection, IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines)
                timelineCollection.Add(timeline);
        }
    }
}
