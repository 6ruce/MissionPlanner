using System;
using System.Collections.Generic;

namespace GeoMesh
{
    public class Subscriptions : IDisposable
    {
        private List<Action> _subscriptions = new List<Action>();

        public void Add(Action unsubscribe) => _subscriptions.Add(unsubscribe);
        
        public void Dispose()
        {
            foreach (var unsubscribe in _subscriptions)
            {
                unsubscribe();
            }
        }
    }
}