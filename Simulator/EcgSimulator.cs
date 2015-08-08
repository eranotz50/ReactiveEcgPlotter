using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Domain;
using System.Reactive.Disposables;
using Controls.Extensions;
using Controls.Timer;

namespace Simulator
{
    public static class EcgSimulator
    {        
        private static TimeSpan initTs = TimeSpan.FromMilliseconds(-250);
        private static int _globalBufferIndex = 0;

        public static IObservable<EcgSample> SimulateEcgAsObservable()
        {
            IEnumerable<EcgSample> list = CreateSampels();
            var sampleObservable = list.ToObservable(new EventLoopScheduler());

            IObservable<EcgSample> reapeted = sampleObservable.Repeat(1); //.Do(Print);
            IObservable<IList<EcgSample>> buffered = reapeted.Buffer(250);
            IObservable<object> timerObservable = new MMTimer().ToObservable(250);

            IObservable<EcgBuffer> zipped = timerObservable
                .TimeInterval()
                .Scan(initTs, (prevTs, currTs) => currTs.Interval + prevTs)
                .Zip(buffered, (interval, b) =>
                {
                    return new EcgBuffer { Buffer = b, TimeStamp = interval, Index = _globalBufferIndex++ };
                });

            var res = zipped.SelectMany(buf =>
            {
                buf.Buffer.ForEach(s => s.TimeStamp = buf.TimeStamp);
                Debug.WriteLine(buf);
                return buf.Buffer;
            })
            .Select((item, index) =>
            {
                item.X = index;
                return item;
            });

            return res;
        }

        private static IEnumerable<EcgSample> CreateSampels()
        {
            for (short i = 0; i < 1400; i++)
            {
                yield return new EcgSample(50);
            }
        }

    }
}
