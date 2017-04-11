using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Models
{
	/// <summary>
	///  Class to hold the proxy settings for the application.
	/// </summary>
	public class ProxyInfo
	{
		/// <summary>
		///The URI of the proxy server.
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// The port number on host to use.
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the domain.
		/// </summary>
		public string Domain { get; set; }
	}
}
