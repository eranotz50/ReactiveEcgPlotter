using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Controls.Extensions;
using Controls.Timer;
using Domain;


namespace Controls.Plotter
{
	[TemplatePart(Name = "PART_Image", Type = typeof(Image))]
	public class Plotter : Control
	{
		static private int _width, _height;
		static int _xmax, _xmin, _ymax, _ymin;
		static private int _msLimit = 3000;
		private static Color c = Colors.Red;
		private static Color clearColor = Colors.Transparent;
		int _gapSize = 30;
		static WriteableBitmap writeableBmp;

		private static IDisposable _timerDisposable = Disposable.Empty;
		private static IDisposable _delayFromStartDisposable = Disposable.Empty;
		private static IDisposable _samplesDisposable = Disposable.Empty;
		private static IDisposable _renderDisposable = Disposable.Empty;

		private static IObservable<Delta> _samplesObservable; 
		
		private static Queue<Delta> _pending = new Queue<Delta>();

	    private static int _nEnqueue = 0;
        private static int _nDequeue = 0;


		static Plotter()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Plotter), new FrameworkPropertyMetadata(typeof(Plotter)));
		}

		public Plotter()
		{
			_xmax = 3000;
			_xmin = 0;
			_ymax = 550;
			_ymin = -150;

			Loaded += Plotter_Loaded;
		}


		//IObservable<IList<Delta>> zipped = renderer.Zip(samplesObservable.Buffer(4), (r, s) => s);

		//// (3) Test to demonstrate the wanted behavior  
		//_disposable = zipped.ObserveOnDispatcher().Subscribe(Draw);	

		private void Plotter_Loaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded += Plotter_Unloaded;
			
			_width = (int)ActualWidth;
			_height = (int)ActualHeight;

			writeableBmp = BitmapFactory.New(_width, _height);
			var img = (Image)GetTemplateChild("PART_Image");
			img.Source = writeableBmp;


						
			// (2) an Observable of the ms. passed between each render event.			
			var renderer = CompositionTargetToObservable()
				.TimeInterval().Select(ti => ti.Interval.Milliseconds).Where( ms => ms > 0).Publish();

			IDisposable firstSubscriptionDisposable = Disposable.Empty;

			var timer = new MMTimer().ToObservable(100)
                .Take(1).Publish();
			_timerDisposable = timer.Subscribe(_ =>
			{
				Debug.WriteLine("Rendere Connected");
			    renderer.Connect();
				_timerDisposable.Dispose();
			});

			firstSubscriptionDisposable =_samplesObservable.Subscribe(_ =>
			{
				Debug.WriteLine("Timer");
				timer.Connect();

				firstSubscriptionDisposable.Dispose();
			});
			
			_samplesDisposable = _samplesObservable.Subscribe(delta =>
			{
			    _nEnqueue++;
                Debug.WriteLine("Enqueue : {0}", _nEnqueue);
				//_pending.Enqueue(delta);
			});
			
			_renderDisposable = renderer.Subscribe(Draw);
		}				

		void Plotter_Unloaded(object sender, RoutedEventArgs e)
		{
			_samplesDisposable.Dispose();
			_renderDisposable.Dispose();
		}

		#region Private 

		private int x = 0;
		private int y = 223;

		private void Draw(int msSinceLastRender)
		{
			int samplesToDraw = msSinceLastRender/PixelPerMs;

			Debug.WriteLine(string.Format("N : {0}",samplesToDraw));
      
            for (int i = 0; i < samplesToDraw; i++)
            {
                _nDequeue++;
                Debug.WriteLine(" ----- Dequeue : {0} ", _nDequeue);
                //Delta d = _pending.Dequeue();
                //d.Draw(writeableBmp, c);

               // DrawGap(d.P2.X);
            }


			//for (int i = 0; i < 4; i++)
			//{
			//	writeableBmp.DrawLineAa(x, y, x + 1, y, c);
			//	DrawGap(x);
			//	x = (x + 1) % _width;
			//}
		}

		private void DrawGap(int x)
		{
			int xActual = (x + _gapSize) % _width;
			writeableBmp.DrawRectangle(xActual, 0, xActual + 1, _height, clearColor);
		}

		private IObservable<object> CompositionTargetToObservable()
		{
			var observable = Observable.FromEventPattern(
				handler => CompositionTarget.Rendering += handler,
				handler => CompositionTarget.Rendering -= handler);

			return observable;
		}

		private static EcgSample NormalizeSample(EcgSample sample)
		{
			// cyclic value
			sample.X = sample.X % _msLimit;
			sample.X = NormalizeXToBounds(sample.X);

			sample.Y = NormalizeYToBounds(sample.Y);
			// reverse value from normale coordinate system to wpf coordinate system. ( at avarage i get around  550 - (-150) Y - values)
			//sample.Y = height - sample.Y;

			return sample;
		}

		private static int NormalizeYToBounds(int y)
		{
			return _height - ((y - _ymin) * _height / (_ymax - _ymin));
		}

		private static int NormalizeXToBounds(int x)
		{
			return (x - _xmin) * _width / (_xmax - _xmin);
		}

		private static int PixelPerMs
		{
			get { return  _msLimit / _width; }
		}

		#endregion

		public IObservable<EcgSample> InputObservable
		{
			get { return (IObservable<EcgSample>)GetValue(InputObservableProperty); }
			set { SetValue(InputObservableProperty, value); }
		}

		public static readonly DependencyProperty InputObservableProperty =
			DependencyProperty.Register("InputObservable", typeof(IObservable<EcgSample>), typeof(Plotter), new PropertyMetadata(Observable.Empty<EcgSample>(), OnInputObservableChanged));

		private static void OnInputObservableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            var initialValue = Delta.Create(new EcgSample { X = -1 }, new EcgSample { X = -1 });
            
            // (1) An observable of EcgSamples (X,Y) Values. 
			var observable = (IObservable<EcgSample>) e.NewValue;			
			var normalized = observable.Select(NormalizeSample);

			// (1) An observable of Delta (x1,y1) (x2,y2) Values. 
			_samplesObservable = normalized.Scan(new EcgSample {X = -1}, (prev, curr) => prev.X != curr.X ? curr : prev)
				.DistinctUntilChanged().CombineWithPrevious(Delta.Create);
		}
	}
	
	internal class Delta
	{
		public EcgSample P1 { get; set; }
		public EcgSample P2 { get; set; }

		internal static Delta Create(EcgSample P1, EcgSample P2)
		{
			return new Delta
			{
				P1 = P1,
				P2 = P2
			};
		}

		internal void Draw(WriteableBitmap wb, Color c)
		{
			if (P1 == null)
			{
				P1 = P2;
			}
			else if (P2.X < P1.X)
			{
				P2 = P1;
			}

			wb.DrawLineAa(P1.X, P1.Y, P2.X, P2.Y, c);
		}

		public override string ToString()
		{
			if (P1 == null)
			{
				P1 = P2;
			}

			return string.Format(" ( {0} , {1} )  ( {2} , {3} )",P1.X,P1.Y,P2.X,P2.Y);
		}

		public void Print()
		{
			Debug.WriteLine(this.ToString());
		}
	}

}

