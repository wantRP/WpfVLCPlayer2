using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Vlc.DotNet.Core.Interops;
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
		public class Media : IEquatable<Media> {
			public string FilePath { get; set; }
			public string Name { get; set; }
			public string Length { get; set; }
			public bool Equals(Media other) {
				// Would still want to check for null etc. first.
				return (this.FilePath == other.FilePath) ;

			}
		}
		public ObservableCollection<Media> playlist = new ObservableCollection<Media>();
		public int playlistIndex = 0;
		public string latestPlayingPath;
		public Media lastPlay;
		public class Config {
			public bool Loop{ get; set; }
			public bool Random{ get; set; }
			public bool ShowList{ get; set; }
			public ObservableCollection<Media>Playlist { get; set; }
		}
		public Config conf=new Config();
		public MainWindow() {
			InitializeComponent();
			Loaded += new RoutedEventHandler(Window1_Loaded);
			//playlist = new ObservableCollection<Media>();
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
			
			
		}
		void Window1_Loaded(object sender, RoutedEventArgs e) {
			LoadConfig();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(0.6);
			timer.Tick += timer1_Tick;
		}
		public void LoadConfig() {
			FileStream fileStream = new FileStream("config.json", FileMode.OpenOrCreate);
			fileStream.Close();// prevent expection
			using (StreamReader r = new StreamReader("config.json")) {
				string json = r.ReadToEnd();
				conf = JsonConvert.DeserializeObject<Config>(json);
				if(conf!=null){ 
				checkboxlist.IsChecked = conf.ShowList;
				checkBoxloop.IsChecked = conf.Loop;
				checkBoxrand.IsChecked = conf.Random;
					foreach (Media m in conf.Playlist) playlist.Add(m);
				}
				//List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
			}
		}
		private void pauevent(object sender, VlcMediaPlayerPausedEventArgs e) {
			//throw new NotImplementedException();
			Console.WriteLine("wow i did");
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
				latestPlayingPath = msg;
				playlist.Clear();
				lastPlay = new Media() {
					Name = Path.GetFileName(msg),
					Length = GetLengthByPath(msg),
					FilePath = msg
				};
				playlist.Add(lastPlay);
			}

		}
		private void tpath_DragOver(object sender, DragEventArgs e) {
			e.Effects = DragDropEffects.All;
		}
		private void btnPlayPause_Click(object sender, RoutedEventArgs e) {
			if (isplaying == Isplaying.stop) {//now stopped,begin playing, into pause
				
				this.control?.Dispose();
				this.control = new VlcControl();
				this.ControlContainer.Content = this.control;
				this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
				control.SourceProvider.MediaPlayer.Paused += new EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPausedEventArgs>(pauevent);
				control.SourceProvider.MediaPlayer.EndReached += new EventHandler<VlcMediaPlayerEndReachedEventArgs>(currentover);
				control.SourceProvider.MediaPlayer.Stopped += new EventHandler<VlcMediaPlayerStoppedEventArgs>(stopped);
				if (playlist.Count != 0) {
					//int index;
					if (listviewplaylist.SelectedIndex != -1) playlistIndex = listviewplaylist.SelectedIndex;
					else playlistIndex = 0;
					control.SourceProvider.MediaPlayer.Play(new Uri(playlist[playlistIndex].FilePath));
					lastPlay = playlist[playlistIndex];
					latestPlayingPath = lastPlay.FilePath;
				} else if (lastPlay != null) {
					control.SourceProvider.MediaPlayer.Play(new Uri(lastPlay.FilePath));
					latestPlayingPath = lastPlay.FilePath;
				} else return;
				isplaying = Isplaying.playing;
				btnPause.Content = "暂停";
				//control.SourceProvider.MediaPlayer.Play(new Uri("C:/Users/duchu/Videos/BBC World News Countdown.flv"));

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
		void PlayNext() {
			Thread.Sleep(800);
			Console.WriteLine("next");

			this.btnPause.Dispatcher.Invoke(new Action(delegate {
				btnPause.Content = "播放";
			}));
			isplaying = Isplaying.stop;
			//this.checkboxlist.Dispatcher.Invoke(new Action(delegate {
			if (isloop == true) {
				isplaying = Isplaying.playing;
				this.btnPause.Dispatcher.Invoke(new Action(delegate {
					btnPause.Content = "暂停";
				}));
				if(playlist.Count==0) {
					control.SourceProvider.MediaPlayer.Play(new Uri(latestPlayingPath));
				}else{
					if (israndom == true) {
						Random rand = new Random();
						playlistIndex = rand.Next(playlist.Count);
					} else
						playlistIndex = (playlistIndex + 1) % playlist.Count;
					
					control.SourceProvider.MediaPlayer.Play(new Uri(playlist[playlistIndex].FilePath));
				}
				
			}
		}
		private void stopped(object sender, VlcMediaPlayerStoppedEventArgs e) {
			Console.WriteLine("stopped");
			Thread thread = new Thread(PlayNext);
			thread.Start();
		}
		private void currentover(object sender, VlcMediaPlayerEndReachedEventArgs e) {
			Console.WriteLine("video is over");

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
			Console.WriteLine(listviewplaylist.SelectedIndex);

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
		string IntSecondToString(int a){
			string hms = "";
			int secs = a;
			int hours = secs / 3600;
			int mins = (secs % 3600) / 60;
			secs = secs % 60;
			if (hours != 0) hms = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);
			else hms = string.Format("{0:D2}:{1:D2}", mins, secs);
			return hms;
		}
		string GetLengthByPath(string s){
			VlcControl vcontrol = new VlcControl();
			vcontrol.SourceProvider.CreatePlayer(this.vlcLibDirectory);
			vcontrol.SourceProvider.MediaPlayer.SetMedia(new Uri(s));
			VlcMediaPlayer vlcMediaPlayer = vcontrol.SourceProvider.MediaPlayer;
			VlcMedia media = vlcMediaPlayer.GetMedia();
			media.Parse();
			return IntSecondToString((int)media.Duration.TotalSeconds);
		}
		private void ListView_Drop(object sender, DragEventArgs e) {

			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				// Note that you can have more than one file.
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string str in files) {
					string hms = GetLengthByPath(str);
					Media m = new Media() {
						Name = Path.GetFileName(str),
						FilePath = str,
						Length = hms
					};
					if(playlist.Contains(m)==false) playlist.Add(m);
				}
			}
		}

		private void ListView_DragOver(object sender, DragEventArgs e) {

		}

		private void Button_Click(object sender, RoutedEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			string jsonString;
			if(playlist.Count==0){
				playlist = new ObservableCollection<Media>();
				playlist.Add(lastPlay);
			}
			
			conf = new Config();
			conf.Loop = isloop;
			conf.Playlist = playlist;
			conf.Random = israndom;
			conf.ShowList = true;
			jsonString = JsonConvert.SerializeObject(conf,Formatting.Indented);

			//var options = new JsonSerializerOptions {
			//	WriteIndented = true
			//};
			if(jsonString!=null) System.IO.File.WriteAllText("config.json", jsonString);
			//	MessageBox.Show(jsonString);
		}

		private void listviewplaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if(listviewplaylist.SelectedIndex!=-1){
				int id = listviewplaylist.SelectedIndex;
				isplaying = Isplaying.playing;
				btnPause.Content = "暂停";
				control.SourceProvider.MediaPlayer.Play(new Uri(playlist[id].FilePath));
				playlistIndex = id;
				timer.Start();
			}
		}

		private void listviewplaylist_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Delete) {
				Console.WriteLine("to dele");
				Console.WriteLine(listviewplaylist.SelectedItems.Count);
				List<Media> toremove = (List<Media>)listviewplaylist.SelectedItems.OfType<Media>().ToList();
				foreach (Media m in toremove){
					playlist.Remove(m);
				}
				//Media m = (Media)listviewplaylist.SelectedItems[0];
				//Console.WriteLine(m.Name);
			}
		}
	}
}
