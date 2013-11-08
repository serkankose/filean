using System.IO;
using System.Threading;

namespace filean
{
	internal class FilesChanging
	{
		readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

		public void Watch(DirectoryInfo directoryInfo, string filter, FileSystemEventHandler onChanged, RenamedEventHandler onRenamed, ErrorEventHandler onError)
		{
			using (var watcher = new FileSystemWatcher(directoryInfo.FullName) { IncludeSubdirectories = true, Filter = filter, EnableRaisingEvents = true })
			{
				watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime | NotifyFilters.Size;
				watcher.Renamed += onRenamed;
				watcher.Created += onChanged;
				watcher.Changed += onChanged;
				watcher.Deleted += onChanged;
				watcher.Error += onError;
				_manualResetEvent.WaitOne();
			}
		}

		public void Stop()
		{
			_manualResetEvent.Set();
		}
	}
}
