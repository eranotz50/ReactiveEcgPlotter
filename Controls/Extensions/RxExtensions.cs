using System;
using System.Reactive.Linq;
using Controls.Timer;

namespace Controls.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<TResult> CombineWithPrevious<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TSource, TResult> resultSelector)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (previous, current) => Tuple.Create(previous.Item2, current))
                .Select(t => resultSelector(t.Item1, t.Item2));
        }

        public static IObservable<object> ToObservable(this MMTimer timer, int interval)
        {
            timer.Period = interval;

            return Observable.FromEventPattern(handler =>
            {
                timer.Tick += handler;
                timer.Start();
            },
            handler =>
            {
                timer.Tick -= handler;
                timer.Stop();
            });
        }  
    }  
}

/* private IObservable<ElapsedEventArgs> CreateTimerObservable2()
        {
            return Observable.Create<ElapsedEventArgs>(
                observer =>
                {
                    var timer = new Timer();
                    timer.Interval = 250;
                    timer.Elapsed += (s, e) => observer.OnNext(e);
                    timer.Start();
                    return Disposable.Create(() =>
                    {
                        timer.Stop();
                    });
                });
        } */