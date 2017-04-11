using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CBR.Components.Controls
{
	/// <summary>
	/// Enumeration for representing state of an animation.
	/// </summary>
	public enum AnimationState
	{
		/// <summary>
		/// The animation is playing.
		/// </summary>
		Playing,

		/// <summary>
		/// The animation is paused.
		/// </summary>
		Paused,

		/// <summary>
		/// The animation is stopped.
		/// </summary>
		Stopped
	}

	/// <summary>
	/// A control that shows a loading animation.
	/// </summary>
	public class WaitSpin : Control
	{
		#region --------------------CONSTRUCTORS--------------------

		static WaitSpin()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitSpin),
				new FrameworkPropertyMetadata(typeof(WaitSpin)));
		}

		/// <summary>
		/// LoadingAnimation constructor.
		/// </summary>
		public WaitSpin()
		{
			this.DefaultStyleKey = typeof(WaitSpin);
		}

		#endregion

		#region --------------------DEPENDENCY PROPERTIES--------------------

		#region -------------------- fill--------------------
		/// <summary>
		/// fill property.
		/// </summary>
		public static readonly DependencyProperty ShapeFillProperty =
			DependencyProperty.Register("ShapeFill", typeof(Brush), typeof(WaitSpin), null);

		/// <summary>
		/// Gets or sets the fill.
		/// </summary>
		[System.ComponentModel.Category("Loading Animation Properties"), System.ComponentModel.Description("The fill for the shapes.")]
		public Brush ShapeFill
		{
			get { return (Brush)GetValue(ShapeFillProperty); }
			set { SetValue(ShapeFillProperty, value); }
		}
		#endregion

		#region -------------------- stroke--------------------
		/// <summary>
		/// Ellipse stroke property.
		/// </summary>
		public static readonly DependencyProperty ShapeStrokeProperty =
			DependencyProperty.Register("ShapeStroke", typeof(Brush), typeof(WaitSpin), null);

		/// <summary>
		/// Gets or sets the ellipse stroke.
		/// </summary>
		[System.ComponentModel.Category("Loading Animation Properties"), System.ComponentModel.Description("The stroke for the shapes.")]
		public Brush ShapeStroke
		{
			get { return (Brush)GetValue(ShapeStrokeProperty); }
			set { SetValue(ShapeStrokeProperty, value); }
		}
		#endregion

		#region --------------------Is playing--------------------
		/// <summary>
		/// Playing status
		/// </summary>
		public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(WaitSpin),
											new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsPlayingChanged)));

		/// <summary>
		/// OnIsPlayingChanged callback
		/// </summary>
		/// <param name="d"></param>
		/// <param name="e"></param>
		private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			WaitSpin element = d as WaitSpin;
			element.ChangePlayMode((bool)e.NewValue);
		}

		/// <summary>
		/// IsPlaying
		/// </summary>
		[System.ComponentModel.Category("Loading Animation Properties"), System.ComponentModel.Description("Incates wheter is playing or not.")]
		public bool IsPlaying
		{
			get { return (bool)GetValue(IsPlayingProperty); }
			set { SetValue(IsPlayingProperty, value); }
		}
		#endregion

		#region --------------------Associated element--------------------

		/// <summary>
		/// Associated element to disable when loading
		/// </summary>
		public static readonly DependencyProperty AssociatedElementProperty = DependencyProperty.Register("AssociatedElement", typeof(UIElement), typeof(WaitSpin), null);

		/// <summary>
		/// Gets or sets the associated element to disable when loading
		/// </summary>
		[System.ComponentModel.Category("Loading Animation Properties"), System.ComponentModel.Description("Associated element that will be disabled when playing.")]
		public UIElement AssociatedElement
		{
			get { return (UIElement)GetValue(AssociatedElementProperty); }
			set { SetValue(AssociatedElementProperty, value); }
		}

		#endregion

		#region --------------------AutoPlay--------------------

		/// <summary>
		/// Gets or sets a value indicating whether the animation should play on load.
		/// </summary>
		public static readonly DependencyProperty AutoPlayProperty = DependencyProperty.Register("AutoPlay", typeof(bool), typeof(WaitSpin),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAutoPlayChanged)));

		private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			WaitSpin element = d as WaitSpin;
			element.ChangePlayMode((bool)e.NewValue);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the animation should play on load.
		/// </summary>
		[System.ComponentModel.Category("Loading Animation Properties"), System.ComponentModel.Description("The animation should play on load.")]
		public bool AutoPlay
		{
			get { return (bool)GetValue(AutoPlayProperty); }
			set { SetValue(AutoPlayProperty, value); }
		}

		#endregion

		#endregion

		#region --------------------PROPERTIES--------------------
		/// <summary>
		/// Stores the loading animation storyboard.
		/// </summary>
		private Storyboard _loadingAnimation;

		/// <summary>
		/// Stores whether the animation is running.
		/// </summary>
		private AnimationState _animationState;

		/// <summary>
		/// Gets the animation state,
		/// </summary>
		public AnimationState AnimationState
		{
			get { return this._animationState; }
		}

		#endregion

		#region --------------------INTERNALS--------------------

		/// <summary>
		/// Gets the parts out of the template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			//retreive the animation part
			this._loadingAnimation = (Storyboard)this.GetTemplateChild("PART_LoadingAnimation");

			if (this.AutoPlay)
				Begin();
		}

		/// <summary>
		/// Begins the loading animation.
		/// </summary>
		internal void ChangePlayMode(bool playing)
		{
			if (this._loadingAnimation == null) return;

			if (playing)
			{
				if (this._animationState != AnimationState.Playing)
					Begin();
			}
			else
			{
				if (this._animationState != AnimationState.Stopped)
					Stop();
			}
		}
		#endregion

		#region --------------------ANIM METHODS--------------------

		/// <summary>
		/// Begins the loading animation.
		/// </summary>
		public void Begin()
		{
			if (this._loadingAnimation != null)
			{
				this._animationState = AnimationState.Playing;
				this._loadingAnimation.Begin();

				this.Visibility = System.Windows.Visibility.Visible;
				if (AssociatedElement != null)
					AssociatedElement.IsEnabled = false;
			}
		}

		/// <summary>
		/// Pauses the animation.
		/// </summary>
		public void Pause()
		{
			if (this._loadingAnimation != null)
			{
				this._animationState = AnimationState.Paused;
				this._loadingAnimation.Pause();
			}
		}

		/// <summary>
		/// Resumes the animation.
		/// </summary>
		public void Resume()
		{
			if (this._loadingAnimation != null)
			{
				this._animationState = AnimationState.Playing;
				this._loadingAnimation.Resume();
			}
		}

		/// <summary>
		/// Stops the animation.
		/// </summary>
		public void Stop()
		{
			if (this._loadingAnimation != null)
			{
				this._animationState = AnimationState.Stopped;
				this._loadingAnimation.Stop();

				this.Visibility = System.Windows.Visibility.Hidden;
				if (AssociatedElement != null)
					AssociatedElement.IsEnabled = true;
			}
		}
		#endregion
	}
}
