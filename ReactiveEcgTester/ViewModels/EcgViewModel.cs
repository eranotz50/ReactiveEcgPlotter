﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Caliburn.Micro;
using Controls.Timer;
using ReactiveEcgTester.Resources;
using System.Reactive.Concurrency;
using System.Timers;
using Controls.Extensions;
using Domain;
using Simulator;

namespace ReactiveEcgTester.ViewModels
{

    public class EcgViewModel : Screen
    {
        private  IDisposable _disposable = Disposable.Empty;
     
        public EcgViewModel(IEcgParser filePharser)
        {
            //.Subscribe(Print); 
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
             var _ecgObservable = EcgSimulator.SimulateEcgAsObservable();
            _disposable = _ecgObservable.Subscribe(s => Debug.WriteLine(s));
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _disposable.Dispose();
        }


        /*public IObservable<EcgSample> EcgObservable
        {
            get { return EcgSimulator.SimulateEcgAsObservable(); } //; }
        }*/
    }
}


