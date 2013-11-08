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
		private const int MaxItems = 1000;
		private TransformBlock<FileSystemEventArgs, int> _fileChanged;
		private TransformBlock<RenamedEventArgs, int> _fileRenamed;
		private TransformBlock<ErrorEventArgs, int> _fileError;
		private ActionBlock<int> _removeFirst;

		public MainWindow()
		{
			InitializeComponent();
			_fileChanged = new TransformBlock<FileSystemEventArgs, int>(args => OnChanged(args));
			_fileRenamed = new TransformBlock<RenamedEventArgs, int>(args => OnRenamed(args));
			_fileError = new TransformBlock<ErrorEventArgs, int>(args => OnError(args));

			_removeFirst = new ActionBlock<int>(i => Dispatcher.InvokeAsync(() => { if (i > MaxItems) Files.Items.RemoveAt(MaxItems); }));

			_fileChanged.LinkTo(_removeFirst);
			_fileRenamed.LinkTo(_removeFirst);
			_fileError.LinkTo(_removeFirst);
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

		private int OnRenamed(RenamedEventArgs renamedArgs)
		{
			int count = Files.Items.Count;
			Dispatcher.InvokeAsync(() => Files.Items.Insert(0, new {Message = string.Format("{2} \tRenamed:\t{0} -> {1}", renamedArgs.OldName, renamedArgs.Name, DateTime.Now.TimeOfDay)}));
			return count;
		}

		private int OnError(ErrorEventArgs errorEventArgs)
		{
			int count = Files.Items.Count;
			Dispatcher.InvokeAsync(() => Files.Items.Insert(0, new {Message = string.Format("{1} \t{0}", errorEventArgs.GetException().Message, DateTime.Now.TimeOfDay)}));
			return count;
		}

		private int OnChanged(FileSystemEventArgs eventArgs)
		{
			int count = Files.Items.Count;
			//if (eventArgs.ChangeType == WatcherChangeTypes.Created) Console.ForegroundColor = ConsoleColor.Green;
			//if (eventArgs.ChangeType == WatcherChangeTypes.Changed) Console.ForegroundColor = ConsoleColor.Blue;
			//if (eventArgs.ChangeType == WatcherChangeTypes.Deleted) Console.ForegroundColor = ConsoleColor.Red;
			Dispatcher.InvokeAsync(() => Files.Items.Insert(0, new {Message = string.Format("{2} \t{0}:\t{1}", eventArgs.ChangeType, eventArgs.Name, DateTime.Now.TimeOfDay)}));
			return count;
		}
	}
}
