using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;

namespace CBR.Components.Controls
{
	public enum CornerOrigin { TopLeft, TopRight, BottomLeft, BottomRight };
	public enum PageStatus { None, Dragging, DraggingWithoutCapture, DropAnimation, TurnAnimation }

    public partial class TwoPageBook : ItemsControl
    {
		static TwoPageBook() 
        {
        }

		#region --------------------DEPENDENCY PROPERTIES--------------------

		#region ScaleProperty

		public static readonly DependencyProperty ScaleProperty =
			   DependencyProperty.Register("Scale", typeof(double), typeof(TwoPageBook),
							new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScaleChanged)));

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			TwoPageBook element = d as TwoPageBook;
			element._scaleTransform.ScaleX = (double)e.NewValue;
			element._scaleTransform.ScaleY = (double)e.NewValue;
		}
		#endregion

		#region FitModeProperty

		public static readonly DependencyProperty FitModeProperty =
			   DependencyProperty.Register("FitMode", typeof(DisplayFitMode), typeof(TwoPageBook),
							new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFitModeChanged)));

		public DisplayFitMode FitMode
		{
			get { return (DisplayFitMode)GetValue(FitModeProperty); }
			set { SetValue(FitModeProperty, value); }
		}

		private static void OnFitModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			TwoPageBook element = d as TwoPageBook;
			element.Fit();
		}
		#endregion

		#region CurrentPageIndexProperty

		public static readonly DependencyProperty CurrentPageIndexProperty =
			   DependencyProperty.Register("CurrentPageIndex", typeof(int), typeof(TwoPageBook),
							new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentPageIndexChanged),
															new CoerceValueCallback(OnCoercePageIndex)));

		public int CurrentPageIndex
		{
			get { return (int)GetValue(CurrentPageIndexProperty); }
			set { SetValue(CurrentPageIndexProperty, value); }
		}

		private static void OnCurrentPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			TwoPageBook element = d as TwoPageBook;
			element._currentSheetIndex = element.CurrentPageIndex / 2;
			element.RefreshSheetsContent();
		}

		private static object OnCoercePageIndex(DependencyObject d, object basevalue)
		{
			TwoPageBook element = d as TwoPageBook;

			int pageIndex = Convert.ToInt32(basevalue);
			if ((pageIndex < 0) || (pageIndex > element.GetItemsCount()))
				basevalue = 0;

			return basevalue;
		}

		#endregion

		#endregion

		private const int FIT_BORDER = 30;
		private int _currentSheetIndex = 0;
		private PageStatus _status = PageStatus.None;
        private DataTemplate defaultDataTemplate;
		private ScrollViewer _ScrollContainer;
		private FrameworkElement _Content;
		private ScaleTransform _scaleTransform = new ScaleTransform();

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			
			_ScrollContainer = (ScrollViewer)GetTemplateChild("PART_ScrollViewer");
			_Content = (FrameworkElement)GetTemplateChild("PART_Content");

			_scaleTransform.CenterX = 0.5;
			_scaleTransform.CenterY = 0.5;
			_Content.LayoutTransform = _scaleTransform;

			TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
			TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;

			bp0.MouseDown += new MouseButtonEventHandler(OnLeftMouseDown);
			bp1.MouseDown += new MouseButtonEventHandler(OnRightMouseDown);
			
			bp0.PageTurned += new RoutedEventHandler(OnLeftPageTurned);
			bp1.PageTurned += new RoutedEventHandler(OnRightPageTurned);

			if ((bp0 == null) || (bp1 == null))
				return;

			defaultDataTemplate = (DataTemplate)Resources["defaultDataTemplate"];
			Read<PageStatus> GetStatus = delegate() { return _status; };
			Action<PageStatus> SetStatus = delegate(PageStatus ps) { _status = ps; };
			bp0.GetStatus += GetStatus;
			bp0.SetStatus += SetStatus;
			bp1.GetStatus += GetStatus;
			bp1.SetStatus += SetStatus;

			RefreshSheetsContent();
		}

		private void Fit()
		{
			if (FitMode == DisplayFitMode.Height)
			{
				Scale = (this._ScrollContainer.ViewportHeight - FIT_BORDER) / (Items[CurrentSheetIndex] as CBR.Core.Models.Page).Image.Height;
			}
			else if (FitMode == DisplayFitMode.Width)
			{
				Scale = (this._ScrollContainer.ViewportWidth - FIT_BORDER) / ((Items[CurrentSheetIndex] as CBR.Core.Models.Page).Image.Width * 2);
			}
		}

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            base.OnItemsChanged(e);
            if (CheckCurrentSheetIndex())
            {
                CurrentSheetIndex = GetItemsCount() / 2;
            }
            else
                RefreshSheetsContent();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (CheckCurrentSheetIndex())
            {
                CurrentSheetIndex = GetItemsCount() / 2;
            }
            else
                RefreshSheetsContent();
        }

        internal object GetPage(int index)
        {
			if ((index >= 0) && (index < Items.Count))
				return Items[index];
			else
				return new Canvas();
        }

        public void AnimateToNextPage(bool fromTop, int duration)
        {
            if (CurrentSheetIndex + 1 <= GetItemsCount() / 2)
            {
				TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
				TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;

                if ((bp0 == null) || (bp1 == null))
                    return;

				Canvas.SetZIndex((bp0 as TripleSheet), 0);
				Canvas.SetZIndex((bp1 as TripleSheet), 1);
                bp1.AutoTurnPage(fromTop ? CornerOrigin.TopRight : CornerOrigin.BottomRight, duration);
            }
        }

        public void AnimateToPreviousPage(bool fromTop, int duration)
        {
            if (CurrentSheetIndex > 0)
            {
				TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
				TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;

                if ((bp0 == null) || (bp1 == null))
                    return;

				Canvas.SetZIndex((bp1 as TripleSheet), 0);
				Canvas.SetZIndex((bp0 as TripleSheet), 1);
                bp0.AutoTurnPage(fromTop ? CornerOrigin.TopLeft : CornerOrigin.BottomLeft, duration);
            }
        }

		public int CurrentSheetIndex
		{
			get { return _currentSheetIndex; }
			set
			{
				if (_status != PageStatus.None) return;

				if (_currentSheetIndex != value)
				{
					if ((value >= 0) && (value <= GetItemsCount() / 2))
					{
						_currentSheetIndex = value;
						CurrentPageIndex = _currentSheetIndex * 2;
					}
					else
						throw new Exception("Index out of bounds");
				}
			}
		}

        protected virtual bool CheckCurrentSheetIndex()
        {
            return CurrentSheetIndex > (GetItemsCount() / 2);
        }
        
        public int GetItemsCount() 
        {
            if (ItemsSource != null) {
                if (ItemsSource is ICollection)
                    return (ItemsSource as ICollection).Count;
                int count = 0;
                foreach (object o in ItemsSource) count++;
                return count;
            }
            return Items.Count;
        }

        private void RefreshSheetsContent() 
        {
			TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
            if (bp0 == null)
                return;

			TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;
            if (bp1 == null)
                return;

            ContentPresenter sheet0Page0Content = bp0.FindName("page0") as ContentPresenter;
            ContentPresenter sheet0Page1Content = bp0.FindName("page1") as ContentPresenter;
            ContentPresenter sheet0Page2Content = bp0.FindName("page2") as ContentPresenter;

            ContentPresenter sheet1Page0Content = bp1.FindName("page0") as ContentPresenter;
            ContentPresenter sheet1Page1Content = bp1.FindName("page1") as ContentPresenter;
            ContentPresenter sheet1Page2Content = bp1.FindName("page2") as ContentPresenter;

            Visibility bp0Visibility = Visibility.Visible;
            Visibility bp1Visibility = Visibility.Visible;
            
            bp1.IsTopRightCornerEnabled = true;
            bp1.IsBottomRightCornerEnabled = true;
            
            Visibility sheet0Page0ContentVisibility = Visibility.Visible;
            Visibility sheet0Page1ContentVisibility = Visibility.Visible;
            Visibility sheet0Page2ContentVisibility = Visibility.Visible;
            Visibility sheet1Page0ContentVisibility = Visibility.Visible;
            Visibility sheet1Page1ContentVisibility = Visibility.Visible;
            Visibility sheet1Page2ContentVisibility = Visibility.Visible;

			DataTemplate dt = ItemTemplate;
			if (dt == null)
				dt = defaultDataTemplate;

			sheet0Page0Content.ContentTemplate = dt;
			sheet0Page1Content.ContentTemplate = dt;
			sheet0Page2Content.ContentTemplate = dt;
			sheet1Page0Content.ContentTemplate = dt;
			sheet1Page1Content.ContentTemplate = dt;
			sheet1Page2Content.ContentTemplate = dt;
                
            sheet0Page2ContentVisibility = _currentSheetIndex == 1 ? Visibility.Hidden : Visibility.Visible;
            int count = GetItemsCount();
            int sheetCount = count / 2;
            bool isOdd = (count % 2) == 1;

            if (_currentSheetIndex == sheetCount)
            {
                if (isOdd)
                {
                    bp1.IsTopRightCornerEnabled = false;
                    bp1.IsBottomRightCornerEnabled = false;
                }
                else
                    bp1Visibility = Visibility.Hidden;
            }
            
            if (_currentSheetIndex == sheetCount - 1) 
            {
                if (!isOdd)
                    sheet1Page2ContentVisibility = Visibility.Hidden;
            }

            if (_currentSheetIndex == 0)
            {
                sheet0Page0Content.Content = null;
                sheet0Page1Content.Content = null;
                sheet0Page2Content.Content = null;
                bp0.IsEnabled = false;
                bp0Visibility = Visibility.Hidden;
            }
            else
            {
                sheet0Page0Content.Content = GetPage(2 * (CurrentSheetIndex - 1) + 1);
                sheet0Page1Content.Content = GetPage(2 * (CurrentSheetIndex - 1));
                sheet0Page2Content.Content = GetPage(2 * (CurrentSheetIndex - 1) - 1);
                bp0.IsEnabled = true;
            }

			sheet1Page0Content.Content = GetPage(2 * CurrentSheetIndex);
			sheet1Page1Content.Content = GetPage(2 * CurrentSheetIndex + 1);
			sheet1Page2Content.Content = GetPage(2 * CurrentSheetIndex + 2);

            bp0.Visibility = bp0Visibility;
            bp1.Visibility = bp1Visibility;
            
            sheet0Page0Content.Visibility = sheet0Page0ContentVisibility;
            sheet0Page1Content.Visibility = sheet0Page1ContentVisibility;
            sheet0Page2Content.Visibility = sheet0Page2ContentVisibility;
            sheet1Page0Content.Visibility = sheet1Page0ContentVisibility;
            sheet1Page1Content.Visibility = sheet1Page1ContentVisibility;
            sheet1Page2Content.Visibility = sheet1Page2ContentVisibility;
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs args) 
        {
			TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
			TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;
			Canvas.SetZIndex((bp0 as TripleSheet), 1);
			Canvas.SetZIndex((bp1 as TripleSheet), 0);
        }

        private void OnRightMouseDown(object sender, MouseButtonEventArgs args) 
        {
			TripleSheet bp0 = GetTemplateChild("sheet0") as TripleSheet;
			TripleSheet bp1 = GetTemplateChild("sheet1") as TripleSheet;
			Canvas.SetZIndex((bp0 as TripleSheet), 0);
			Canvas.SetZIndex((bp1 as TripleSheet), 1);
        }

        private void OnLeftPageTurned(object sender, RoutedEventArgs args) 
        {
            CurrentSheetIndex--;
        }

        private void OnRightPageTurned(object sender, RoutedEventArgs args) 
        {
            CurrentSheetIndex++;
        }
    }
}
