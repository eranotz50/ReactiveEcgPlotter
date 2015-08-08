using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using ReactiveEcgTester.Resources;

namespace ReactiveEcgTester
{
    public class WindsorInstaller : IWindsorInstaller

    {
        public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
			container.Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
					   Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton());

            container.Register(Component.For<IEcgParser>()
		        .ImplementedBy<EcgFileParser>()
                .DependsOn(Dependency.OnValue("ecgFilePath", "Resources\\ecgData.csv"))
		        .LifestyleTransient());


            container.Register(Classes.FromThisAssembly()
                        .Where(x => x.Name.EndsWith("ViewModel"))
                        .Configure(x => x.LifestyleTransient().PropertiesIgnore(t => true)));
        }
    }
}
