using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace WpfApp1 {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		private readonly DirectoryInfo vlcLibDirectory;
		private VlcControl control;
		public class videotime {
			public string time { get; set; }
			public string progress{ get; set; }
		}
		public videotime vtime = new videotime();
		private DispatcherTimer timer;
		public enum Isplaying { playing, stop, pause };
		public Isplaying isplaying = Isplaying.stop;
		public bool SliderDirectMoveMask = false;
		bool _over;
		public bool playover {
			get { return _over; }
			set {
				_over = value;
				if (_over) {
					//Do stuff here.
					Console.WriteLine("true");
					Thread.Sleep(800);
					//control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
					playover = false;
				}
			}
		}
		public MainWindow() {
			InitializeComponent();
			var currentAssembly = Assembly.GetEntryAssembly();
			var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
			// Default installation path of VideoLAN.LibVLC.Windows
			tb.DataContext = vtime;
			slider1.DataContext = vtime;
			vtime.time = "ee";
			vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
			playover = false;
			this.control = new VlcControl();
			
			this.ControlContainer.Content = this.control;

			//this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
			control?.Dispose();
			control = null;
			Loaded += new RoutedEventHandler(Window1_Loaded);
		}

		private void pauevent(object sender, VlcMediaPlayerPausedEventArgs e) {
			//throw new NotImplementedException();
			Console.WriteLine("wow i did");
		}

		void Window1_Loaded(object sender, RoutedEventArgs e) {
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(0.6);
			timer.Tick += timer1_Tick;
		}
		private void timer1_Tick(object sender, EventArgs e) {
			try {
				double ct=GetCurrentTime() *100.0/ GetLength();
				//vtime.progress =Convert.ToString( ct);
				slider1.Value = ct;
			} catch { }
		}
		protected override void OnClosing(CancelEventArgs e) {
			this.control?.Dispose();
			base.OnClosing(e);
		}
		private void playbystr(string str) {
			this.control?.Dispose();
			this.control = new VlcControl();
			this.ControlContainer.Content = this.control;

			this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);


			control.SourceProvider.MediaPlayer.Play(new Uri(str));
		}
		private void OnStopButtonClick(object sender, RoutedEventArgs e) {
			timer.Stop();
			this.control?.Dispose();
			this.control = null;
			btnPause.Content = "播放";
			isplaying = Isplaying.stop;
			bool protectmask = SliderDirectMoveMask;
			SliderDirectMoveMask = true;
			slider1.Value = 0;
			SliderDirectMoveMask = protectmask;
		}
		private void OnForwardButtonClick(object sender, RoutedEventArgs e) {
			if (this.control == null) {
				return;
			}

			this.control.SourceProvider.MediaPlayer.Rate = 2;
		}
		private long GetLength() {
			if (this.control == null) {
				return 0;
			}

			return this.control.SourceProvider.MediaPlayer.Length;
		}
		private long GetCurrentTime() {
			if (this.control == null) {
				return 0;
			}

			return this.control.SourceProvider.MediaPlayer.Time;
		}
		private void tpath_Drop(object sender, DragEventArgs e) {
			string msg = "Drop";
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
				playbystr(msg);
			}

		}
		private void tpath_DragOver(object sender, DragEventArgs e) {
			e.Effects = DragDropEffects.All;
		}
		private void btnPause_Click(object sender, RoutedEventArgs e) {
			if (isplaying == Isplaying.stop) {//into pause
				isplaying = Isplaying.playing;
				btnPause.Content = "暂停";
				this.control?.Dispose();
				this.control = new VlcControl();
				this.ControlContainer.Content = this.control;
				//control.SourceProvider.MediaPlayer.EndReached += MediaPlayer_EndReached;
				this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
				control.SourceProvider.MediaPlayer.Paused += new EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPausedEventArgs>(pauevent);
				control.SourceProvider.MediaPlayer.EndReached += new EventHandler<VlcMediaPlayerEndReachedEventArgs>(currentover);
				control.SourceProvider.MediaPlayer.Stopped += new EventHandler<VlcMediaPlayerStoppedEventArgs>(stopped);
				control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
				timer.Start();
				return;
			}
			if (isplaying == Isplaying.playing) {//into continue
				isplaying = Isplaying.pause;
				btnPause.Content = "继续";
				control.SourceProvider.MediaPlayer.Pause();
				
				return;
			}
			if (isplaying == Isplaying.pause) {//into play
				btnPause.Content = "继续";
				isplaying=Isplaying.playing;
				control.SourceProvider.MediaPlayer.Play();
				Console.WriteLine("继续");
				return;
			}
		}
		void tt(){
			Thread.Sleep(600);
			Console.WriteLine("zzz");
			control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
		}
		private void stopped(object sender, VlcMediaPlayerStoppedEventArgs e) {
			Console.WriteLine("stopped");
			Thread thread = new Thread(tt);
			thread.Start();
		}

		private void currentover(object sender, VlcMediaPlayerEndReachedEventArgs e) {
			//btnPause.Content = "播放";
			//isplaying = Isplaying.stop;
			//throw new NotImplementedException();
			playover = true;
			Console.WriteLine("video is over");
			//control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
			//this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
			//control.SourceProvider.MediaPlayer.Play("C:/Users/duchu/Videos/1.mkv");
		}
		private void slider1_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
			SliderDirectMoveMask = true;
			timer.Stop();
		}
		private void slider1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			SliderDirectMoveMask = false;
			long length = GetLength();
			long toskiptotime = ((long)(slider1.Value * length / slider1.Maximum));
			if (this.control == null) {
				return;
			}
			Console.WriteLine(toskiptotime / 1000);
			this.control.SourceProvider.MediaPlayer.Time = toskiptotime;
			if (isplaying == Isplaying.playing) timer.Start();
		}
		private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

		}
		private void buttontest_Click(object sender, RoutedEventArgs e) {
			//playbystr("C:/Users/duchu/Videos/BBC World News Countdown.flv");
			//control.SourceProvider
			//this.control?.Dispose();
			//this.control = new VlcControl();
			//this.ControlContainer.Content = this.control;

			//this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
			control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
		}

		private void slider1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			
			//Console.WriteLine("up");
			timer.Stop();
			long length = GetLength();
			long toskiptotime = ((long)(slider1.Value * length / slider1.Maximum));
			if (this.control == null) {
				return;
			}
			//Console.WriteLine(toskiptotime / 1000);
			this.control.SourceProvider.MediaPlayer.Time = toskiptotime;
			timer.Start();
		}

	
	}
}
