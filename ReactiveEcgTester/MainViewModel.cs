using Caliburn.Micro;
using ReactiveEcgTester.ViewModels;

namespace ReactiveEcgTester
{
	public class MainViewModel : Screen
	{
		private readonly EcgViewModel _ecgViewModel;

		public MainViewModel(EcgViewModel ecgViewModel)
		{
			_ecgViewModel = ecgViewModel;
		    _ecgViewModel.ConductWith(this);
		}

		public EcgViewModel EcgViewModel
		{
			get { return _ecgViewModel; }
		}
	}
}
