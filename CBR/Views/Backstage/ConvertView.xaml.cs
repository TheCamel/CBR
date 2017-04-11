using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CBR.Components;
using CBR.Core.Files.Conversion;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using CBR.ViewModels;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for ConvertView.xaml
    /// </summary>
    public partial class ConvertView : UserControl
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ConvertView()
        {
			using (new TimeLogger("ConvertView.ConvertView"))
			{
				InitializeComponent();

				Parameters = WorkspaceService.Instance.Settings.ConvertParameters;
			}
        }

        private BackgroundWorker _Worker = null;

        public ContractParameters Parameters { get; set; }

        /// <summary>
        /// init, create the background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
			ctrlBrowseSourceFile.Filters = DocumentFactory.Instance.BookFilterAll;

            if( !string.IsNullOrEmpty(Parameters.InputFile) )
            {
                this.rbOneSource.IsChecked= true;
				this.ctrlBrowseSourceFile.Selection = Parameters.InputFile;
            }

            if( !string.IsNullOrEmpty(Parameters.InputPath) )
            {
                this.rbMultipleSource.IsChecked = true;
				this.ctrlBrowseSourceFolder.Selection = Parameters.InputPath;
            }

            if (!string.IsNullOrEmpty(Parameters.DestinationPath))
            {
                this.rbSameAsSource.IsChecked = true;
				this.ctrlBrowseDestination.Selection = Parameters.DestinationPath;
            }
            else this.rbInOneFolder.IsChecked = true;

            this.chkVerify.IsChecked = Parameters.CheckResult;
            this.chkUpdate.IsChecked = Parameters.ResfreshLibrary;
			this.chkJoin.IsChecked = Parameters.JoinImages;

            if (_Worker == null)
            {
                //init the background worker process
                _Worker = new BackgroundWorker();
                _Worker.WorkerReportsProgress = true;
                _Worker.WorkerSupportsCancellation = true;
                _Worker.DoWork += new DoWorkEventHandler(bw_DoWork);
                _Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                _Worker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            }

            this.comboBoxImputFormat.ItemsSource = DocumentFactory.Instance.BookFilters.Where( p => p.ConversionReader != null );
        }

        /// <summary>
        /// starting the conversion, create parameters and launch the thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
			Parameters.InputFile = this.ctrlBrowseSourceFile.IsEnabled ? this.ctrlBrowseSourceFile.Selection : string.Empty;
			Parameters.InputPath = this.ctrlBrowseSourceFolder.IsEnabled ? this.ctrlBrowseSourceFolder.Selection : string.Empty;
			Parameters.DestinationPath = this.ctrlBrowseDestination.IsEnabled ? this.ctrlBrowseDestination.Selection : string.Empty;
            Parameters.CheckResult = this.chkVerify.IsChecked.Value;
            Parameters.ResfreshLibrary = this.chkUpdate.IsChecked.Value;
			Parameters.JoinImages = this.chkJoin.IsChecked.Value;
            Parameters.ResultFiles = null;

			Parameters.OutputType = comboBoxOuputFormat.SelectedItem as DocumentInfo;

            if (this.rbOneSource.IsChecked.Value == true)
            {
				Parameters.InputType = DocumentFactory.Instance.FindBookFilterByExt(Path.GetExtension(this.ctrlBrowseSourceFile.Selection));
            }
            else
            {
				Parameters.InputType = comboBoxImputFormat.SelectedItem as DocumentInfo;
            }
            if (!Parameters.CheckParameters())
            {
                this.lbResults.Items.Clear();
                this.lbResults.Items.Add("Parameters are incorrect");
                return;
            }
            else WorkspaceService.Instance.Settings.ConvertParameters = Parameters;

            // Start the asynchronous operation.
            _Worker.RunWorkerAsync(Parameters);

            this.lbResults.Items.Clear();
            this.btnConvert.IsEnabled = false;
            this.btnCancel.Visibility = System.Windows.Visibility.Visible;
			this.ResultPanel.Visibility = System.Windows.Visibility.Visible;
			this.paramGrid.IsEnabled = false;
        }

        /// <summary>
        /// worker thread progress method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.lbResults.Items.Add(e.UserState.ToString());
            this.lbResults.Items.MoveCurrentToLast();
            this.lbResults.ScrollIntoView(this.lbResults.Items.CurrentItem);

            this.progressResults.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// worker thread result method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.lbResults.Items.Add( "Operation error !" );
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.lbResults.Items.Add( "Operation cancelled !" );
            }
            else if (e.Result != null)
            {
                // Finally, handle the case where the operation succeeded.
                ContractParameters param = e.Result as ContractParameters;

                if (param.CheckResult)
                    this.lbResults.Items.Add("Operation succeded !");
                else
                    this.lbResults.Items.Add("Errors occured !");

                if (param.ResfreshLibrary)
                {
					Messenger.Default.Send<List<string>>(param.ResultFiles, ViewModelMessages.CatalogUpdate);
                    this.lbResults.Items.Add("Refresing library !");
                }
            }

            //finally
            this.btnConvert.IsEnabled = true;
            this.btnCancel.Visibility = System.Windows.Visibility.Collapsed;
			this.progressResults.Visibility = System.Windows.Visibility.Collapsed;
			this.paramGrid.IsEnabled = true;

            System.Media.SystemSounds.Exclamation.Play();
        }

        /// <summary>
        /// worker thread operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            new BookFileConverter((BackgroundWorker)sender, e).Convert();
        }

        /// <summary>
        /// cancel the conversion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _Worker.CancelAsync();

            this.btnConvert.IsEnabled = true;
            this.btnCancel.Visibility = System.Windows.Visibility.Hidden;
            this.progressResults.Visibility = System.Windows.Visibility.Hidden;
        }

        private void rbSource_Click(object sender, RoutedEventArgs e)
        {
			ctrlBrowseSourceFile.Selection = string.Empty;
			ctrlBrowseSourceFolder.Selection = string.Empty;
        }

        private void rbSameAsSource_Click(object sender, RoutedEventArgs e)
        {
			ctrlBrowseDestination.Selection = string.Empty;
        }

        private void comboBoxImputFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DocumentInfo input = this.comboBoxImputFormat.SelectedItem as DocumentInfo;
            this.comboBoxOuputFormat.ItemsSource = DocumentFactory.Instance.BookFilters.Where(p => input.CanConvertTo.Contains(p.Type));
        }

		private void ctrlBrowseSourceFile_OnBrowseEvent(object sender, Components.Controls.BrowseForControl.BrowseEventArgs e)
		{
			comboBoxImputFormat.SelectedItem =
				DocumentFactory.Instance.FindBookFilterByExt(Path.GetExtension(ctrlBrowseSourceFile.Selection));
		}
    }
}
