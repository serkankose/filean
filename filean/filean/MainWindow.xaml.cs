using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace filean
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int MaxItems = 100;
		private ActionBlock<FileSystemEventArgs> _fileChanged;
		private ActionBlock<RenamedEventArgs> _fileRenamed;
		private ActionBlock<ErrorEventArgs> _fileError;

		public MainWindow()
		{
			InitializeComponent();
			_fileChanged = new ActionBlock<FileSystemEventArgs>(args => OnChanged(args));
			_fileRenamed = new ActionBlock<RenamedEventArgs>(args => OnRenamed(args));
			_fileError = new ActionBlock<ErrorEventArgs>(args => OnError(args));
		}

		private void Start_Click(object sender, RoutedEventArgs e)
		{
			string text = Directory.Text;
			string filter = Filter.Text;
			new Task(() =>
				{
					var filesChanging = new FilesChanging();
					var directoryInfo = new DirectoryInfo(text);
					Dispatcher.InvokeAsync(() => DirectoryInfo.Text = directoryInfo.FullName);
					
					try
					{
						filesChanging.Watch(
							directoryInfo,
							filter,
							(sender3, eventArgs) => _fileChanged.Post(eventArgs),
							(sender2, renamedArgs) => _fileRenamed.Post(renamedArgs),
							(sender1, errorEventArgs) => _fileError.Post(errorEventArgs));
					}
					catch (Exception exception)
					{
						throw;
					}
				}).Start();
			

		}

		private void OnRenamed(RenamedEventArgs renamedArgs)
		{
			Dispatcher.InvokeAsync(() =>
				{
					Files.Items.Insert(0, new {Message = string.Format("{2} \tRenamed:\t{0} -> {1}", renamedArgs.OldName, renamedArgs.Name, DateTime.Now.TimeOfDay)});
					if(Files.Items.Count > MaxItems) Files.Items.RemoveAt(MaxItems);
				});
		}

		private void OnError(ErrorEventArgs errorEventArgs)
		{
			Dispatcher.InvokeAsync(() =>
				{
					Files.Items.Insert(0, new {Message = string.Format("{1} \t{0}", errorEventArgs.GetException().Message, DateTime.Now.TimeOfDay)});
					if (Files.Items.Count > MaxItems) Files.Items.RemoveAt(MaxItems);

				});
		}

		private void OnChanged(FileSystemEventArgs eventArgs)
		{
			//if (eventArgs.ChangeType == WatcherChangeTypes.Created) Console.ForegroundColor = ConsoleColor.Green;
			//if (eventArgs.ChangeType == WatcherChangeTypes.Changed) Console.ForegroundColor = ConsoleColor.Blue;
			//if (eventArgs.ChangeType == WatcherChangeTypes.Deleted) Console.ForegroundColor = ConsoleColor.Red;
			Dispatcher.InvokeAsync(() =>
				{
					Files.Items.Insert(0, new {Message = string.Format("{2} \t{0}:\t{1}", eventArgs.ChangeType, eventArgs.Name, DateTime.Now.TimeOfDay)});
					if (Files.Items.Count > MaxItems) Files.Items.RemoveAt(MaxItems);

				});
		}
	}
}
