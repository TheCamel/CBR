using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight;

namespace CBR.Core.Helpers.Splash
{
	internal class SplashScreenWindowViewModel : ViewModelBase, ISplashScreen
    {
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (Message == value) return;
                message = value;
				RaisePropertyChanged("Message");
            }
        }

        private object content;
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                if (Content == value) return;
                content = value;
                RaisePropertyChanged("Content");
            }
        }

        public void SetContentObject(Type contentType)
        {
            Dispatcher.BeginInvoke((Action<Type>)delegate(Type input)
            {
                object result = Activator.CreateInstance(input);
                Content = result;
            }, contentType);
        }

        public Dispatcher Dispatcher
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Dispatcher != null)
            {
                Dispatcher.InvokeShutdown();
                Dispatcher = null;
            }
        }
    }
}
