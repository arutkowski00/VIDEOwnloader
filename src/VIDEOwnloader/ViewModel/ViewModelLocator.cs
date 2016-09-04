#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using VIDEOwnloader.Services.DataService;

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
                RegisterType<AboutViewModel>().
                RegisterType<DownloadsViewModel>().
                RegisterType<NewDownloadViewModel>(new TransientLifetimeManager()).
                Configure<Interception>().
                SetInterceptorFor<MainViewModel>(new VirtualMethodInterceptor());
        }

        public AboutViewModel About => ServiceLocator.Current.GetInstance<AboutViewModel>();
        public DownloadsViewModel Downloads => ServiceLocator.Current.GetInstance<DownloadsViewModel>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public NewDownloadViewModel NewDownload => ServiceLocator.Current.GetInstance<NewDownloadViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}