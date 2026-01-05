using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.Helpers
{
    public static class TestHelpers
    {
        /// <summary>
        /// Waits for a property change event to be raised
        /// </summary>
        public static Task<bool> WaitForPropertyChanged(INotifyPropertyChanged obj, string propertyName, int timeoutMs = 1000)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timeout = Task.Delay(timeoutMs);

            PropertyChangedEventHandler handler = null!;
            handler = (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    obj.PropertyChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            obj.PropertyChanged += handler;

            return Task.WhenAny(tcs.Task, timeout).ContinueWith(t =>
            {
                obj.PropertyChanged -= handler;
                return tcs.Task.IsCompleted && tcs.Task.Result;
            });
        }

        /// <summary>
        /// Executes an action and verifies that a property changed event was raised
        /// </summary>
        public static async Task<bool> AssertPropertyChanged(INotifyPropertyChanged obj, string propertyName, Action action)
        {
            bool eventRaised = false;
            PropertyChangedEventHandler handler = (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    eventRaised = true;
                }
            };

            obj.PropertyChanged += handler;
            action();
            await Task.Delay(100); // Give time for async events
            obj.PropertyChanged -= handler;

            return eventRaised;
        }

        /// <summary>
        /// Creates a delay for async operations
        /// </summary>
        public static Task Delay(int milliseconds = 100)
        {
            return Task.Delay(milliseconds);
        }
    }
}
