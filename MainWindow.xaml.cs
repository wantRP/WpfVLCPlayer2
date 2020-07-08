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
			public string progress { get; set; }
		}
		public videotime vtime = new videotime();
		private DispatcherTimer timer;
		public enum Isplaying { playing, stop, pause };
		public Isplaying isplaying = Isplaying.stop;
		public bool SliderDirectMoveMask = false;
		public bool isloop { get; set; }
		public bool israndom { get; set; }
		public class Media {
			public string FilePath { get; set; }
			public string Name { get; set; }
			public string Length { get; set; }
		}
		public List<Media> playlist=new List<Media>();
		public MainWindow() {
			InitializeComponent();
			playlist = new List<Media>();
			var currentAssembly = Assembly.GetEntryAssembly();
			var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
			// Default installation path of VideoLAN.LibVLC.Windows
			tb.DataContext = vtime;
			slider1.DataContext = vtime;
			vtime.time = "ee";

			vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
			this.control = new VlcControl();
			this.ControlContainer.Content = this.control;
			//this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
			control?.Dispose();
			control = null;
			checkBoxloop.DataContext = this;
			checkBoxrand.DataContext = this;
			this.listviewplaylist.ItemsSource = playlist;
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
				double ct = GetCurrentTime() * 100.0 / GetLength();
				//vtime.progress =Convert.ToString( ct);
				slider1.Value = ct;
			} catch { }
		}
		protected override void OnClosing(CancelEventArgs e) {
			this.control?.Dispose();
			base.OnClosing(e);
		}
		private void playbystr(string str) {
			/*
				this.control?.Dispose();
				this.control = new VlcControl();
				this.ControlContainer.Content = this.control;

				this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);

				*/
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
			this.control.SourceProvider.MediaPlayer.Rate = (this.control.SourceProvider.MediaPlayer.Rate == 2 ? 1 : 2);
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
				timer.Start();
				msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
				//playbystr(msg);
				this.control?.Dispose();
				this.control = new VlcControl();
				this.ControlContainer.Content = this.control;
				//control.SourceProvider.MediaPlayer.EndReached += MediaPlayer_EndReached;
				this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
				control.SourceProvider.MediaPlayer.Paused += new EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPausedEventArgs>(pauevent);
				control.SourceProvider.MediaPlayer.EndReached += new EventHandler<VlcMediaPlayerEndReachedEventArgs>(currentover);
				control.SourceProvider.MediaPlayer.Stopped += new EventHandler<VlcMediaPlayerStoppedEventArgs>(stopped);

				control.SourceProvider.MediaPlayer.Play(new Uri(msg));

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
				isplaying = Isplaying.playing;
				control.SourceProvider.MediaPlayer.Play();
				Console.WriteLine("继续");
				return;
			}
		}
		void tt() {
			Thread.Sleep(800);
			Console.WriteLine("next");

			this.btnPause.Dispatcher.Invoke(new Action(delegate {
				btnPause.Content = "播放";
			}));
			isplaying = Isplaying.stop;
			//this.checkboxlist.Dispatcher.Invoke(new Action(delegate {
			if (isloop == true)
				isplaying = Isplaying.playing;
			this.btnPause.Dispatcher.Invoke(new Action(delegate {
				btnPause.Content = "暂停";
			}));
			control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
			//}));

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
			Console.WriteLine(string.Format("loop: {0}, rand: {1}", isloop, israndom));
			Console.WriteLine("---playlist---");
			foreach (Media m in playlist) {
				Console.WriteLine(m.FilePath);

			}
			Console.WriteLine("---that\'s all---");
			Console.WriteLine(listviewplaylist.Items.Count+" files");
			//control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));
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

		private void checkboxlist_Checked(object sender, RoutedEventArgs e) {
			try { gridPlaylist.Visibility = Visibility.Visible; } catch { }
		}

		private void checkboxlist_Unchecked(object sender, RoutedEventArgs e) {
			try { gridPlaylist.Visibility = Visibility.Collapsed; } catch { }
		}

		private void ListView_Drop(object sender, DragEventArgs e) {

			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				// Note that you can have more than one file.
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string str in files) {
					Console.WriteLine(str);
					string name = str;
					playlist.Add(new Media() {
						Name = Path.GetFileName(str),
						FilePath = str,
						Length = "1"
					}) ;
				}
			}
		}

		private void ListView_DragOver(object sender, DragEventArgs e) {

		}

		private void Button_Click(object sender, RoutedEventArgs e) {

		}
	}
}
