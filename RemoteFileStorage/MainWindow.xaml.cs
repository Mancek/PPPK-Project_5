using Azure.Storage.Blobs.Models;
using Microsoft.Win32;
using RemoteFileStorage.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RemoteFileStorage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ItemsViewModel itemsViewModel;
        private readonly string DEFAULT_IMAGE = "/Assets/noimage.png";

        public MainWindow()
        {
            InitializeComponent();
            itemsViewModel = new ItemsViewModel();
            Init();
        }

        private void Init()
        {
            LbBlobs.ItemsSource = itemsViewModel.Items;
            CbDirectories.ItemsSource = itemsViewModel.Directories;
        }

        private void LbBlobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = LbBlobs.SelectedItem as BlobItem;
            if(LbBlobs.SelectedItem is null)
            {
                imgPreview.Source = new BitmapImage(new Uri(DEFAULT_IMAGE, UriKind.Relative));
                return;
            }
            string name = (LbBlobs.SelectedItem as BlobItem).Name;
            string uri = itemsViewModel.GetUri();
            imgPreview.Source = new BitmapImage(new System.Uri($"{uri}/{name}"));
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files(*.JPEG;*.TIFF;*.PNG;*.SVG;*.GIF)|*.JPEG;*.TIFF;*.PNG;*.SVG;*.GIF"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                string directory = path.Substring(path.LastIndexOf('.') + 1).ToUpper();
                await itemsViewModel.UploadAsync(openFileDialog.FileName, directory);
            }
            CbDirectories.Text = itemsViewModel.Directory;
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(LbBlobs.SelectedItem is BlobItem blobItem))
            {
                return;
            }
            await itemsViewModel.DeleteAsync(blobItem);
            CbDirectories.Text = itemsViewModel.Directory;
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (!(LbBlobs.SelectedItem is BlobItem blobItem))
            {
                return;
            }
            var saveFileDialog = new SaveFileDialog()
            {
                FileName = blobItem.Name.Contains(ItemsViewModel.ForwardSlash) 
                ? blobItem.Name.Substring(blobItem.Name.LastIndexOf(ItemsViewModel.ForwardSlash) + 1) 
                : blobItem.Name
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await itemsViewModel.DownloadAsync(blobItem, saveFileDialog.FileName);
            }
            CbDirectories.Text = itemsViewModel.Directory;
        }

        private void CbDirectories_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (itemsViewModel.Directories.Contains(CbDirectories.Text))
            {
                itemsViewModel.Directory = CbDirectories.Text;
                CbDirectories.SelectedItem = itemsViewModel.Directory;
            }
        }
    }
}
