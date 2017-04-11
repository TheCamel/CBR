using System;
using System.Windows;
using System.Windows.Input;
using CBR.Components;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using CBR.Core.Services;
using GalaSoft.MvvmLight.CommandWpf;

namespace CBR.ViewModels
{
	public partial class MainViewModel : ViewModelBaseExtended
    {
        #region ----------------BOOK COMMANDS----------------

        #region open command
        private ICommand bookOpenCommand;
        public ICommand BookOpenCommand
        {
            get
            {
                if (bookOpenCommand == null)
                    bookOpenCommand = new RelayCommand(OpenBook, delegate() { return true; });
                return bookOpenCommand;
            }
        }

        void OpenBook()
        {
            try
            {
                using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
                {
                    browser.FilterIndex = DocumentFactory.Instance.BookFilterDefaultIndex;
                    browser.Filter = DocumentFactory.Instance.BookFilterAllEditable;

                    if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
                    {
                        OpenFileBook(browser.FileName);
                    }
                }
            }
            catch (Exception err)
            {
				LogHelper.Manage("MainViewModel:OpenBook", err);
            }
        }
        #endregion

        #region open file command
        private ICommand bookOpenFileCommand;
        public ICommand BookOpenFileCommand
        {
            get
            {
                if (bookOpenFileCommand == null)
                    bookOpenFileCommand = new RelayCommand<string>(OpenFileBook, delegate(string param) { return true; });
                return bookOpenFileCommand;
            }
        }

        void OpenFileBook(string param)
        {
            try
            {
				ReadBook(DocumentFactory.Instance.GetService(param).CreateBook(param));
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:OpenFileBook", err);
            }
        }
        #endregion

        #region read command
        private ICommand bookReadCommand;
        public ICommand BookReadCommand
        {
            get
            {
                if (bookReadCommand == null)
                    bookReadCommand = new RelayCommand<Book>(ReadBook, delegate(Book bk) { return bk != null && !bk.IsSecured; });
                return bookReadCommand;
            }
        }

        void ReadBook(Book bookAsParam)
        {
            try
            {
                if (bookAsParam.IsSecured) return;

				//get type must be done in containing assembly
				string vm = DocumentFactory.Instance.GetViewModel(bookAsParam);
				if (!string.IsNullOrEmpty(vm))
				{
					Type cbr = Type.GetType(vm);
					BookViewModelBase newElem = (BookViewModelBase)ReflectionHelper.CreateInstance(cbr, new object[] { bookAsParam });
					if (newElem != null)
					{
						Documents.Add(newElem);
						SetActiveWorkspace(newElem);
						return;
					}
				}

				ProcessHelper.LaunchShellUri(new Uri(bookAsParam.FilePath));
            }
            catch (Exception err)
            {
				LogHelper.Manage("MainViewModel:ReadBook", err);
            }
        }

        public BookViewModelBase CreateModelFromFile(string bookFilePath)
        {
            try
            {
                Book bookAsParam = DocumentFactory.Instance.GetService(bookFilePath).CreateBook(bookFilePath);
                if (bookAsParam.IsSecured)
                    return null;

                //get type must be done in containing assembly
                string vm = DocumentFactory.Instance.GetViewModel(bookAsParam);
                if (!string.IsNullOrEmpty(vm))
                {
                    Type cbr = Type.GetType(vm);
                    return (BookViewModelBase)ReflectionHelper.CreateInstance(cbr, new object[] { bookAsParam });
                }

                return null;
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:CreateModelFromFile", err);
                return null;
            }
        }
        #endregion

        #region delete command
        private ICommand bookDeleteCommand;
        public ICommand BookDeleteCommand
        {
            get
            {
                if (bookDeleteCommand == null)
                    bookDeleteCommand = new RelayCommand<Book>(DeleteBook, delegate(Book bk) { return bk != null && !bk.IsSecured ; });
                return bookDeleteCommand;
            }
        }

        void DeleteBook(Book bk)
        {
            try
            {
                if (bk.IsSecured) return;

				string msg = CultureManager.Instance.GetLocalization("ByCode", "Warning.Delete", "Please, confirm the deletion");
				string title = CultureManager.Instance.GetLocalization("ByCode", "Warning", "Warning");

                //message de confirmation
                if (MessageBox.Show(msg, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    CatalogService.Instance.DeleteBook(Data, bk);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:EditBook", err);
            }
        }
        #endregion

        #region remove command
        private ICommand bookRemoveCommand;
        public ICommand BookRemoveCommand
        {
            get
            {
                if (bookRemoveCommand == null)
                    bookRemoveCommand = new RelayCommand<Book>(RemoveBook, delegate(Book bk) { return bk != null; });
                return bookRemoveCommand;
            }
        }

        void RemoveBook(Book bk)
        {
            try
            {
				CatalogService.Instance.RemoveBook(Data, bk);
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:RemoveBook", err);
            }
        }
        #endregion

        #region protect command
        private ICommand bookProtectCommand;
        public ICommand BookProtectCommand
        {
            get
            {
                if (bookProtectCommand == null)
                    bookProtectCommand = new RelayCommand<Book>(ProtectBook, delegate(Book bk) { return bk != null; });
                return bookProtectCommand;
            }
        }

        void ProtectBook(Book bk)
        {
            try
            {
                PasswordDialog dlg = new PasswordDialog();
                dlg.Owner = Application.Current.MainWindow;
                dlg.ShowDialog();
                if (dlg.DialogResult == true)
					DocumentFactory.Instance.GetService(bk).Protect(bk, !bk.IsSecured, dlg.PassBox.Password);
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:EditBook", err);
            }
        }
        #endregion

        #region mark as read command
        private ICommand bookMarkAsReadCommand;
        public ICommand BookMarkAsReadCommand
        {
            get
            {
                if (bookMarkAsReadCommand == null)
                    bookMarkAsReadCommand = new RelayCommand<Book>(MarkAsRead, delegate(Book bk) { return bk != null; });
                return bookMarkAsReadCommand;
            }
        }

        void MarkAsRead(Book bk)
        {
            try
            {
                bk.IsRead = !bk.IsRead;
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:MarkAsRead", err);
            }
        }
        #endregion

        #region add book command
        private ICommand bookAddCommand;
        public ICommand BookAddCommand
        {
            get
            {
                if (bookAddCommand == null)
                    bookAddCommand = new RelayCommand<string>(AddBook, delegate(string param) { return Data != null; });
                return bookAddCommand;
            }
        }

        void AddBook(string param)
        {
            try
            {
                //add one file to the library
                if (param == "One")
                {
                    using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
                    {
                        browser.AddExtension = true;
                        browser.Filter = DocumentFactory.Instance.BookFilterAllEditable;
                        browser.FilterIndex = DocumentFactory.Instance.BookFilterDefaultIndex;

                        if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
                        {
                            CatalogService.Instance.AddBook(Data, browser.FileName);
                        }
                    }
                }
                else // or all founded in a folder
                {
                    if (!string.IsNullOrEmpty(Data.BookFolder))
                    {
						string msg = CultureManager.Instance.GetLocalization("ByCode", "Warning.ScanFolder", "Your book folder is allready defined. Do you want to replace it ? Refreshing will work only with the new one.");
						string title = CultureManager.Instance.GetLocalization("ByCode", "Warning", "Warning");

						if (MessageBox.Show(msg, title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                            return;
                    }

                    using (System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
                        {
                            Data.BookFolder = browser.SelectedPath;
                            CatalogService.Instance.Refresh(Data);
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:AddBook", err);
            }
        }

        private ICommand addBookFileCommand;
        public ICommand AddBookFileCommand
        {
            get
            {
                if (addBookFileCommand == null)
                    addBookFileCommand = new RelayCommand<string>(AddFileBook, delegate(string param) { return Data != null; });
                return addBookFileCommand;
            }
        }

        void AddFileBook(string fileBook)
        {
            try
            {
                CatalogService.Instance.AddBook(Data, fileBook);
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:AddFileBook", err);
            }
        }

        #endregion

        #region property command
        private ICommand bookPropertyCommand;
        public ICommand BookPropertyCommand
        {
            get
            {
                if (bookPropertyCommand == null)
                    bookPropertyCommand = new RelayCommand<Book>(DisplayProperty, delegate(Book bk) { return bk != null; });
                return bookPropertyCommand;
            }
        }

        void DisplayProperty(Book bk)
        {
            try
            {
                BackStageIndex = 4;
                BackStageIsOpen = true;
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:DisplayProperty", err);
            }
        }
        #endregion

        #endregion
    }
}
