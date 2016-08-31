/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:VIDEOwnloader"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using VIDEOwnloader.DataService;

namespace VIDEOwnloader.ViewModel
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var container = new UnityContainer();
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));

            if (ViewModelBase.IsInDesignModeStatic)
            {
                container.RegisterType<IDataService, VideoInfoDesignDataService>();
            }
            else
            {
#if REST_SERVICE
                container.RegisterType<IDataService, VideoInfoDataService>();
#else
                container.RegisterType<IDataService, VideoInfoLocalDataService>();
#endif
            }


            container.AddNewExtension<Interception>();
            container.RegisterType<MainViewModel>().
                RegisterType<DownloadsViewModel>().
                RegisterType<NewDownloadViewModel>(new TransientLifetimeManager()).
                Configure<Interception>().
                SetInterceptorFor<MainViewModel>(new VirtualMethodInterceptor());
        }

        public DownloadsViewModel Downloads => ServiceLocator.Current.GetInstance<DownloadsViewModel>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public NewDownloadViewModel NewDownload => ServiceLocator.Current.GetInstance<NewDownloadViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}