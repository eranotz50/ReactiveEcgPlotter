using Controls.Timer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Controls.Extensions;
using Domain;

namespace RxTester
{    
    public partial class MainWindow : Window
    {
        private TimeSpan initTs = TimeSpan.FromMilliseconds(-250);
        private int globalBufferIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            
           var rendering = CompositionTargetToObservable();


            var ecgStream = CreateEcgObservable();
            ecgStream.Subscribe(Print);

        }

        private IObservable<EcgSample> CreateEcgObservable()
        {
            IEnumerable<EcgSample> list = CreateSamples();
            var sampleObservable = list.ToObservable(new EventLoopScheduler());

            IObservable<EcgSample> reapeted = sampleObservable.Repeat(3); //.Do(Print);
            IObservable<IList<EcgSample>> buffered = reapeted.Buffer(250);
            IObservable<object> timerObservable = CreateTimerObservable();

            IObservable<EcgBuffer> zipped = timerObservable
                .TimeInterval()
                .Scan(initTs, (prevTs, currTs) => currTs.Interval + prevTs)
                .Zip(buffered, (interval, b) => 
                {
                    return new EcgBuffer{ Buffer = b, TimeStamp = interval, Index = globalBufferIndex++};
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

        private void Print(EcgSample s)
        {
            Debug.WriteLine("X : {0} , Y : {1} , TS : {2}", s.X, s.Y, s.TimeStamp);
        }

        private IEnumerable<EcgSample> CreateSamples()
        {
            for (short i = 0; i < 1400; i++)
            {
                yield return new EcgSample(50);
            }
        }


        private IObservable<object> CompositionTargetToObservable()
        {
            var observable = Observable.FromEventPattern(
                handler => CompositionTarget.Rendering += handler,
                handler => CompositionTarget.Rendering -= handler);

            return observable;
        }

        private IObservable<object> CreateTimerObservable()
        {
            MMTimer timer = new MMTimer();
            timer.Period = 250;

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
