using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;


namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register the encoding provider here
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            PixelWPF.PixelStartup.Initialize();
        }
        
    }
}
