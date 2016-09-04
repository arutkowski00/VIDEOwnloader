using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using VIDEOwnloader.Properties;

namespace VIDEOwnloader.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        public string TitleText => _assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public string VersionText => _assembly.GetName().Version.ToString();

        public string CopyrightText => _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

        public string DescriptionText => _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public string LicenseText => Resources.LicenseText;
    }
}
