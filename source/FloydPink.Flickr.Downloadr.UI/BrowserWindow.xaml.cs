﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CachedImage;
using FloydPink.Flickr.Downloadr.Bootstrap;
using FloydPink.Flickr.Downloadr.Model;
using FloydPink.Flickr.Downloadr.Model.Enums;
using FloydPink.Flickr.Downloadr.Model.Extensions;
using FloydPink.Flickr.Downloadr.Presentation;
using FloydPink.Flickr.Downloadr.Presentation.Views;
using FloydPink.Flickr.Downloadr.UI.Extensions;
using FloydPink.Flickr.Downloadr.UI.Helpers;

namespace FloydPink.Flickr.Downloadr.UI
{
    /// <summary>
    ///     Interaction logic for BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window, IBrowserView, INotifyPropertyChanged
    {
        private readonly IBrowserPresenter _presenter;
        private bool _doNotSyncSelectedItems;
        private string _page;
        private string _pages;
        private string _perPage;
        private ObservableCollection<Photo> _photos;
        private string _total;

        public BrowserWindow(User user, Preferences preferences)
        {
            InitializeComponent();
            Title += VersionHelper.GetVersionString();
            Preferences = preferences;
            User = user;
            AllSelectedPhotos = new Dictionary<string, Dictionary<string, Photo>>();

            PagePhotoList.SelectionChanged += (sender, args) =>
            {
                if (_doNotSyncSelectedItems) return;
                AllSelectedPhotos[Page] = PagePhotoList.SelectedItems.Cast<Photo>().
                    ToDictionary(p => p.Id, p => p);
                PropertyChanged.Notify(() => SelectedPhotosExist);
                PropertyChanged.Notify(() => SelectedPhotosCountText);
                PropertyChanged.Notify(() => AreAnyPagePhotosSelected);
                PropertyChanged.Notify(() => AreAllPagePhotosSelected);
            };

            SpinnerInner.SpinnerCanceled += (sender, args) => _presenter.CancelDownload();

            FileCache.AppCacheDirectory = Preferences.CacheLocation;

            _presenter = Bootstrapper.GetPresenter<IBrowserView, IBrowserPresenter>(this);
            _presenter.InitializePhotoset();
        }

        public int SelectedPhotosCount
        {
            get { return AllSelectedPhotos.Values.SelectMany(d => d.Values).Count(); }
        }

        public string SelectedPhotosCountText
        {
            get
            {
                string selectionCount = SelectedPhotosExist
                    ? SelectedPhotosCount.ToString(CultureInfo.InvariantCulture)
                    : string.Empty;
                return string.IsNullOrEmpty(selectionCount)
                    ? "Selection"
                    : string.Format("Selection ({0})", selectionCount);
            }
        }

        public bool SelectedPhotosExist
        {
            get { return SelectedPhotosCount != 0; }
        }

        public bool AreAnyPagePhotosSelected
        {
            get { return Page != null && AllSelectedPhotos.ContainsKey(Page) && AllSelectedPhotos[Page].Count != 0; }
        }

        public bool AreAllPagePhotosSelected
        {
            get
            {
                return Photos != null &&
                       (!AllSelectedPhotos.ContainsKey(Page) || Photos.Count != AllSelectedPhotos[Page].Count);
            }
        }

        public string FirstPhoto
        {
            get
            {
                return (((Convert.ToInt32(Page) - 1)*Convert.ToInt32(PerPage)) + 1).
                    ToString(CultureInfo.InvariantCulture);
            }
        }

        public string LastPhoto
        {
            get
            {
                int maxLast = Convert.ToInt32(Page)*Convert.ToInt32(PerPage);
                return maxLast > Convert.ToInt32(Total) ? Total : maxLast.ToString(CultureInfo.InvariantCulture);
            }
        }

        public User User { get; set; }
        public Preferences Preferences { get; set; }

        public ObservableCollection<Photo> Photos
        {
            get { return _photos; }
            set
            {
                _photos = value;
                PropertyChanged.Notify(() => AreAllPagePhotosSelected);
                _doNotSyncSelectedItems = true;
                PagePhotoList.DataContext = Photos;
                SelectAlreadySelectedPhotos();
                _doNotSyncSelectedItems = false;
            }
        }

        public IDictionary<string, Dictionary<string, Photo>> AllSelectedPhotos { get; set; }

        public bool ShowAllPhotos
        {
            get { return PublicAllToggleButton.IsChecked != null && (bool) PublicAllToggleButton.IsChecked; }
        }

        public string Page
        {
            get { return _page; }
            set
            {
                _page = value;
                PropertyChanged.Notify(() => Page);
                PropertyChanged.Notify(() => AreAnyPagePhotosSelected);
            }
        }

        public string Pages
        {
            get { return _pages; }
            set
            {
                _pages = value;
                PropertyChanged.Notify(() => Pages);
            }
        }

        public string PerPage
        {
            get { return _perPage; }
            set
            {
                _perPage = value;
                PropertyChanged.Notify(() => PerPage);
            }
        }

        public string Total
        {
            get { return _total; }
            set
            {
                _total = value;
                PropertyChanged.Notify(() => Total);
                PropertyChanged.Notify(() => FirstPhoto);
                PropertyChanged.Notify(() => LastPhoto);
            }
        }

        public void ShowSpinner(bool show)
        {
            Visibility visibility = show ? Visibility.Visible : Visibility.Collapsed;
            Spinner.Dispatch(s => s.Visibility = visibility);
        }

        public void UpdateProgress(string percentDone, string operationText, bool cancellable)
        {
            SpinnerInner.Dispatch(sc => sc.Cancellable = cancellable);
            SpinnerInner.Dispatch(sc => sc.OperationText = operationText);
            SpinnerInner.Dispatch(sc => sc.PercentDone = percentDone);
        }

        public bool ShowWarning(string warningMessage)
        {
            MessageBoxResult result = MessageBox.Show(warningMessage, "Please confirm...",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result != MessageBoxResult.Yes;
        }

        public void DownloadComplete(string downloadedLocation, bool downloadComplete)
        {
            const string proTip = "\r\n\r\n(ProTip™: CTRL+C will copy all of this message with the location.)";
            if (downloadComplete)
            {
                ClearSelectedPhotos();
                MessageBox.Show(
                    string.Format(
                        "Download completed to the directory: \r\n{0}{1}",
                        downloadedLocation, proTip), "Done");
            }
            else
            {
                MessageBox.Show(
                    string.Format(
                        "Incomplete download could be found at: \r\n{0}{1}",
                        downloadedLocation, proTip), "Cancelled");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void SelectAlreadySelectedPhotos()
        {
            if (!AllSelectedPhotos.ContainsKey(Page) || AllSelectedPhotos[Page].Count <= 0) return;
            List<Photo> photos = Photos.Where(photo => AllSelectedPhotos[Page].ContainsKey(photo.Id)).ToList();
            foreach (Photo photo in photos)
            {
                ((ListBoxItem) PagePhotoList.ItemContainerGenerator.ContainerFromItem(photo)).IsSelected = true;
            }
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow {User = User};
            loginWindow.Show();
            Close();
        }

        private async void TogglePhotosButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            ClearSelectedPhotos();
            await _presenter.InitializePhotoset();
        }

        private async void FirstPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.NavigateTo(PhotoPage.First);
        }

        private async void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.NavigateTo(PhotoPage.Previous);
        }

        private async void NextPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.NavigateTo(PhotoPage.Next);
        }

        private async void LastPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.NavigateTo(PhotoPage.Last);
        }

        private async void DownloadSelectionButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.DownloadSelection();
        }

        private async void DownloadThisPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.DownloadThisPage();
        }

        private async void DownloadAllPagesButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            await _presenter.DownloadAllPages();
        }

        private void LoseFocus(UIElement element)
        {
            // http://stackoverflow.com/a/6031393/218882
            DependencyObject scope = FocusManager.GetFocusScope(element);
            FocusManager.SetFocusedElement(scope, null);
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(scope, PagePhotoList);
        }

        private void SelectAllButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            PagePhotoList.SelectAll();
        }

        private void DeselectAllButtonClick(object sender, RoutedEventArgs e)
        {
            LoseFocus((UIElement) sender);
            PagePhotoList.SelectedItems.Clear();
        }

        private void ClearSelectedPhotos()
        {
            AllSelectedPhotos.Clear();
            PagePhotoList.SelectedItems.Clear();
            PropertyChanged.Notify(() => SelectedPhotosExist);
            PropertyChanged.Notify(() => SelectedPhotosCountText);
        }

    }
}