using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Services;
using CBR.Core.Models;

namespace CBR.ViewModels
{
    public class ViewModelFactory
    {
        #region ----------------SINGLETON----------------

        public static readonly ViewModelFactory Instance = new ViewModelFactory();

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private ViewModelFactory()
        {
        }

        #endregion

		public ViewModelBaseExtended CreateFrom(MainViewModel mvm, string contentId)
        {
            try
            {
				ViewModelBaseExtended created = null;
                Type vm = null;

                string[] param = contentId.Split(';');
                if (param.Length == 2)
                {
                    switch (param[0])
                    {
                        case "BookViewModelBase":
                            created = CreateBookModelFromFile(param[1]);
                            break;
                        case "FeedViewModel":
                            FeedItemInfo fii = WorkspaceService.Instance.Settings.Feed.Feeds.Single(p => p.Url == param[1]);
                            vm = Type.GetType("CBR.ViewModels." + param[0]);
							created = (ViewModelBaseExtended)ReflectionHelper.CreateInstance(vm, new object[] { fii });
                            break;
                    }

                }
                else if (param.Length == 1)
                {
                    if (!string.IsNullOrEmpty(param[0]))
                    {
                        vm = Type.GetType("CBR.ViewModels." + param[0]);

                        if (mvm.Tools.Count(p => p.GetType() == vm) == 0)
							created = (ViewModelBaseExtended)ReflectionHelper.CreateInstance(vm);
                        else
                            mvm.Tools.Cast<ToolViewModel>().First(p => p.GetType() == vm).IsVisible = true;
                    }
                }

                if (created != null)
                {
                    if (created is DocumentViewModel)
                        mvm.Documents.Add(created as DocumentViewModel);

                    if (created is ToolViewModel)
                        mvm.Tools.Add(created as ToolViewModel);
                }

                return created;
            }
            catch (Exception err)
            {
                LogHelper.Manage("ViewModelFactory:CreateModelFrom", err);
                return null;
            }
        }

		private ViewModelBaseExtended CreateBookModelFromFile(string bookFilePath)
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
					return (ViewModelBaseExtended)ReflectionHelper.CreateInstance(cbr, new object[] { bookAsParam });
                }

                return null;
            }
            catch (Exception err)
            {
                LogHelper.Manage("ViewModelFactory:CreateBookModelFromFile", err);
                return null;
            }
        }

    }
}
