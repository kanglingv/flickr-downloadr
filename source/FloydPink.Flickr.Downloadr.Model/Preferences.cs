﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FloydPink.Flickr.Downloadr.Model.Enums;
using FloydPink.Flickr.Downloadr.Model.Extensions;

namespace FloydPink.Flickr.Downloadr.Model
{
    public class Preferences : INotifyPropertyChanged
    {
        private bool _titleAsFilename;
        private string _downloadLocation;
        private PhotoDownloadSize _downloadSize;
        private int _photosPerPage;
        private string _safetyLevel;
        private bool _needOriginalTags;

        public bool TitleAsFilename
        {
            get { return _titleAsFilename; }
            set
            {
                _titleAsFilename = value;
                PropertyChanged.Notify(() => TitleAsFilename);
            }
        }

        public string DownloadLocation
        {
            get { return _downloadLocation; }
            set
            {
                _downloadLocation = value;
                PropertyChanged.Notify(() => DownloadLocation);
            }
        }

        public PhotoDownloadSize DownloadSize
        {
            get { return _downloadSize; }
            set
            {
                _downloadSize = value;
                PropertyChanged.Notify(() => DownloadSize);
            }
        }

        public int PhotosPerPage
        {
            get { return _photosPerPage; }
            set
            {
                _photosPerPage = value;
                PropertyChanged.Notify(() => PhotosPerPage);
            }
        }

        public string SafetyLevel
        {
            get { return _safetyLevel; }
            set
            {
                _safetyLevel = value;
                PropertyChanged.Notify(() => SafetyLevel);
            }
        }

        public ObservableCollection<string> Metadata { get; set; }

        public bool NeedOriginalTags
        {
            get { return _needOriginalTags; }
            set
            {
                _needOriginalTags = value;
                PropertyChanged.Notify(() => NeedOriginalTags);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Preferences GetDefault()
        {
            return new Preferences
            {
                TitleAsFilename = false,
                PhotosPerPage = 25,
                DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Metadata =
                    new ObservableCollection<string>
                    {
                        PhotoMetadata.Title,
                        PhotoMetadata.Description,
                        PhotoMetadata.Tags
                    },
                DownloadSize = PhotoDownloadSize.Original,
                SafetyLevel = "Safe",
                NeedOriginalTags = false
            };
        }
    }
}