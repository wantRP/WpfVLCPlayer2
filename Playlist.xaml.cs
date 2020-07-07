using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1 {
	/// <summary>
	/// playlist.xaml 的交互逻辑
	/// </summary>
	public partial class Playlist : Window {
		public Playlist() {
			InitializeComponent();
			Window mainwindow = (MainWindow)Application.Current.MainWindow;
			this.Height = mainwindow.Height;
			this.Left = mainwindow.Width + mainwindow.Left-14;
			this.Top = mainwindow.Top;
		}

		private void close_MouseMove(object sender, MouseEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				this.DragMove();
			}
		}

		private void buttontest_Click(object sender, RoutedEventArgs e) {
			Window mainwindow = (MainWindow)Application.Current.MainWindow;
			this.Height = mainwindow.Height;
			this.Left = mainwindow.Width + mainwindow.Left;
			this.Top = mainwindow.Top;
		}
	}
}
