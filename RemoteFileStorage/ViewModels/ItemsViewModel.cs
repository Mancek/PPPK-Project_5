using Azure.Storage.Blobs.Models;
using RemoteFileStorage.Dao;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteFileStorage.ViewModels
{
    class ItemsViewModel
    {
        public const string ForwardSlash = "/";
        private string directory;
        public ObservableCollection<BlobItem> Items { get; }
        public ObservableCollection<string> Directories { get; }
        public string Directory
        {
            get => directory;
            set
            {
                directory = value;
                Refresh();
            }
        }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<BlobItem>();
            Directories = new ObservableCollection<string>();
            Refresh();
        }

        private void Refresh()
        {
            Directories.Clear();
            Items.Clear();
            Repository.Container.GetBlobs().ToList().ForEach(item =>
            {
                if (item.Name.Contains(ForwardSlash))
                {
                    string directory = item.Name.Substring(0, item.Name.LastIndexOf(ForwardSlash));
                    if (!Directories.Contains(directory))
                    {
                        Directories.Add(directory);
                    }
                }
                if (string.IsNullOrEmpty(Directory) && !item.Name.Contains(ForwardSlash))
                {
                    Items.Add(item);
                }
                else if (!string.IsNullOrEmpty(Directory) && item.Name.Contains($"{Directory}{ForwardSlash}"))
                {
                    Items.Add(item);
                }
            });
        }

        public async Task UploadAsync(string path, string directory)
        {
            string filename = path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            filename = string.IsNullOrEmpty(directory?.Trim()) ? filename : $"{directory}{Path.DirectorySeparatorChar}{filename}";
            using (var fs = File.OpenRead(path))
            {
                await Repository.Container.GetBlobClient(filename).UploadAsync(fs, true);
            }
            Refresh();
        }

        public async Task DeleteAsync(BlobItem blobItem)
        {
            await Repository.Container.GetBlobClient(blobItem.Name).DeleteAsync();
            Refresh();

        }

        public async Task DownloadAsync(BlobItem blobItem, string filename)
        {
            using (var fs = File.OpenWrite(filename))
            {
                await Repository.Container.GetBlobClient(blobItem.Name).DownloadToAsync(fs);
            }
        }

        public string GetUri()
        {
            return Repository.Container.Uri.AbsoluteUri;
        }
    }
}
