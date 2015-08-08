using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Castle.Windsor;
using Controls.Extensions;


namespace ReactiveEcgTester
{
    public class Bootstrapper : BootstrapperBase
    {
        private IWindsorContainer container = new WindsorContainer();

		static Bootstrapper()
		{
		}

		public Bootstrapper()
		{
			Initialize();
		}

		protected override void Configure()
		{
			container.Install(new WindsorInstaller());
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			DisplayRootViewFor<MainViewModel>();
		}

		#region IoC
		protected override object GetInstance(Type service, string key)
		{
			return string.IsNullOrWhiteSpace(key)
				   ? container.Resolve(service)
				   : container.Resolve(key, service);
		}

		protected override IEnumerable<object> GetAllInstances(Type service)
		{
			return (IEnumerable<object>)container.ResolveAll(service);
		}

		protected override void BuildUp(object instance)
		{
			instance.GetType().GetProperties()
				.Where(property => property.CanWrite && property.PropertyType.IsPublic)
				.Where(property => container.Kernel.HasComponent(property.PropertyType))
				.ForEach(property => property.SetValue(instance, container.Resolve(property.PropertyType), null));
		}
		#endregion
	}	    
}
