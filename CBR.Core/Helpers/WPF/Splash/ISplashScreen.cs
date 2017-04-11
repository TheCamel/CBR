using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Helpers.Splash
{
    /// <summary>
    /// Contract through which client application can talk to the splash screen
    /// </summary>
    public interface ISplashScreen : IDisposable
    {
        /// <summary>
        /// The text message being displayed in the splash screen
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// The content object displayed in the splash screen window
        /// </summary>
        /// <remarks>
        /// This method sets the Content element in the Splash Screen. Cannot 
        /// accept an object because any UI element needs to be initialized in 
        /// the splash screen's UI thread.</remarks>
        void SetContentObject(Type objectType);
    }
}
