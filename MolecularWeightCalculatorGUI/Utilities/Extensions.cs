using System;
using System.Reactive.Linq;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal static class Extensions
    {
        /// <summary>
        /// Subscribes an element handler to an observable sequence, not acting on the initial values. Uses .Skip(1)
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">Observable sequence to subscribe to.</param>
        /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
        public static IDisposable SubscribeOnChange<T>(this IObservable<T> source, Action<T> onNext)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Skip(1).Subscribe(onNext);
        }
    }
}
