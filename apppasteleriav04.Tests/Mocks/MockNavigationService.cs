using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.Mocks
{
    public class MockNavigationService
    {
        public List<string> NavigationStack { get; } = new();
        public string? CurrentPage { get; private set; }
        public Dictionary<string, object?> LastNavigationParameters { get; } = new();

        public Task NavigateToAsync(string route, Dictionary<string, object?>? parameters = null)
        {
            NavigationStack.Add(route);
            CurrentPage = route;
            
            if (parameters != null)
            {
                LastNavigationParameters.Clear();
                foreach (var param in parameters)
                {
                    LastNavigationParameters[param.Key] = param.Value;
                }
            }

            return Task.CompletedTask;
        }

        public Task GoBackAsync()
        {
            if (NavigationStack.Count > 0)
            {
                NavigationStack.RemoveAt(NavigationStack.Count - 1);
                CurrentPage = NavigationStack.Count > 0 ? NavigationStack[^1] : null;
            }
            return Task.CompletedTask;
        }

        public void Reset()
        {
            NavigationStack.Clear();
            CurrentPage = null;
            LastNavigationParameters.Clear();
        }

        public bool HasNavigatedTo(string route)
        {
            return NavigationStack.Contains(route);
        }

        public int GetNavigationCount(string route)
        {
            return NavigationStack.FindAll(r => r == route).Count;
        }
    }
}
