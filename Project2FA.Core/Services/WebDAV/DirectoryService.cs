using Prism.Mvvm;
using Prism.Ioc;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDAVClient;
using WebDAVClient.Exceptions;
using WebDAVClient.Types;

namespace Project2FA.Core.Services.WebDAV
{
    public class DirectoryService : BindableBase
    {
        [ThreadStatic]
        private static DirectoryService _instance;
        private ObservableCollection<WebDAVFileOrFolderModel> _folders;
        private ObservableCollection<WebDAVFileOrFolderModel> _filesAndFolders;
        private ObservableCollection<WebDAVFileOrFolderModel> _groupedFilesAndFolders;
        private ObservableCollection<PathInfoModel> _pathStack;
        private int _selectedPathIndex;
        private bool _isSorting;
        private bool _continueListing;
        private bool _isSelecting;
        private string _selectionMode;
        private Task<List<ResourceInfoModel>> _listingTask;
        private IResponseErrorHandlerService _responseErrorHandlerService { get; }

        public DirectoryService()
        {
            //_responseErrorHandlerService = responseErrorHandlerService;
            PathStack = new ObservableCollection<PathInfoModel>
            {
                new PathInfoModel
                {
                    ResourceInfo = new ResourceInfoModel()
                    {
                        Name = "Main",
                        Path = "/"
                    },
                    IsRoot = true
                }
            };
            if (_instance == null)
            {
                _instance = this;
            }
            // Arrange for the first time, so that the collections get filled.
            //_groupedFilesAndFolders.ArrangeItems(new NameSorter(SortSequence.Asc), x => x.Name.First().ToString().ToUpper());
            //_groupedFolders.ArrangeItems(new NameSorter(SortSequence.Asc), x => x.Name.First().ToString().ToUpper());
        }

        public static DirectoryService Instance => _instance ??(_instance = new DirectoryService());


        public ObservableCollection<PathInfoModel> PathStack
        {
            get
            {
                return _pathStack;
            }
            set
            {
                _pathStack = value;
                //SetProperty(ref _pathStack, value);
            }
        }


        public int GetPathDepth
        {
            get
            {
                return PathStack.Count - 1;
            }
        }

        public ObservableCollection<WebDAVFileOrFolderModel> FilesAndFolders
        {
            get
            {
                if (_filesAndFolders == null)
                {
                    _filesAndFolders = new ObservableCollection<WebDAVFileOrFolderModel>();
                }
                return _filesAndFolders;
            }
            set => SetProperty(ref _filesAndFolders, value);
        }
        public ObservableCollection<WebDAVFileOrFolderModel> Folders
        {
            get
            {
                if (_folders == null)
                {
                    _folders = new ObservableCollection<WebDAVFileOrFolderModel>();
                }
                return _folders;
            }
            set => SetProperty(ref _folders, value);
        }
        public ObservableCollection<WebDAVFileOrFolderModel> Files { get; } = new ObservableCollection<WebDAVFileOrFolderModel>();
        public ObservableCollection<WebDAVFileOrFolderModel> GroupedFilesAndFolders
        {
            get
            {
                if (_groupedFilesAndFolders == null)
                {
                    _groupedFilesAndFolders = new ObservableCollection<WebDAVFileOrFolderModel>();
                }
                return _groupedFilesAndFolders;
            }
            set => SetProperty(ref _groupedFilesAndFolders, value);
        }


        public void ToggleSelectionMode()
        {
            IsSelecting = !IsSelecting;
        }

        private static string GetSizeHeader(ResourceInfoModel fileOrFolder)
        {
            var sizeMb = fileOrFolder.Size / 1024f / 1024f;

            long[] sizesValuesMb = { 1, 5, 10, 25, 50, 100, 250, 500, 1024, 5120, 10240, 102400, 1048576 };
            string[] sizesDisplay = { "<1MB", ">1MB", ">5MB", ">10MB", ">25MB", ">50MB", ">100MB", ">250MB", ">500MB", ">1GB", ">5GB", ">10GB", ">100GB", ">1TB" };

            var index = 0;

            foreach (var t in sizesValuesMb)
            {
                if (sizeMb > t)
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return sizesDisplay[index];
        }

        public async Task Refresh(bool createDatafile = false)
        {
            await StartDirectoryListing(createDatafile).ConfigureAwait(false);
        }

        private async void SelectedIndexPathChanged()
        {
            await StartDirectoryListing(null).ConfigureAwait(false);
        }

        public async Task StartDirectoryListing(bool createDatafile = false)
        {
            await StartDirectoryListing(null).ConfigureAwait(false);
        }

        public async Task StartDirectoryListing(ResourceInfoModel resourceInfoToExclude, string viewName = null)
        {
            // cancel a current running listing
            if (_listingTask != null)
            {
                _listingTask.Dispose();
                _listingTask = null;
            }

            // clear instantly, so the user will not see invalid listings
            FilesAndFolders.Clear();
            Folders.Clear();
            GroupedFilesAndFolders.Clear();

            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null || IsSelecting)
            {
                return;
            }

            _continueListing = true;

            if (PathStack.Count == 0)
            {
                PathStack.Add(new PathInfoModel
                {
                    ResourceInfo = new ResourceInfoModel()
                    {
                        Name = "Main",
                        Path = "/"
                    },
                    IsRoot = true
                });
            }

            var path = PathStack.Count > 0 ? PathStack[GetPathDepth].ResourceInfo.Path : "/";
            List<ResourceInfoModel> list = null;

            try
            {
                if (viewName == "sharesIn" | viewName == "sharesOut" | viewName == "sharesLink")
                {
                    PathStack.Clear();
                    _listingTask = client.GetSharesView(viewName);
                }
                else if (viewName == "favorites")
                {
                    PathStack.Clear();
                    _listingTask = client.GetFavorites();
                }
                else
                {
                    _listingTask = client.List(path);
                }
                list = await _listingTask;
            }
            catch (ResponseError e)
            {
                _responseErrorHandlerService.HandleException(e);
            }

            if (list != null)
            {
                foreach (var item in list)
                {
                    if (resourceInfoToExclude != null && item == resourceInfoToExclude)
                    {
                        continue;
                    }
                    if (item.Name.Contains(".2fa"))
                    {
                        FilesAndFolders.Add(new WebDAVFileOrFolderModel(item));
                    }

                    if (RemoveResourceInfos != null)
                    {
                        var index = RemoveResourceInfos.FindIndex(
                            res => res.Path.Equals(item.Path, StringComparison.Ordinal));
                        if (index == -1)
                        {
                            if (item.Name.Contains(".2fa"))
                            {
                                FilesAndFolders.Add(new WebDAVFileOrFolderModel(item));
                            }
                            Folders.Add(new WebDAVFileOrFolderModel(item));
                        }
                    }
                    else
                    {
                        if (item.IsDirectory)
                        {
                            FilesAndFolders.Add(new WebDAVFileOrFolderModel(item));
                            Folders.Add(new WebDAVFileOrFolderModel(item));
                        }
                        
                    }
                }
                //Files.AddRange(FilesAndFolders.Where(x => x.IsDirectory == false));
            }

            SortList();
        }

        private void SortList()
        {
            var orderedFilesAndFolders = FilesAndFolders.OrderBy(x => !x.IsDirectory).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            GroupedFilesAndFolders.AddRange(orderedFilesAndFolders);

            var orderedFolders = Folders.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            Folders.Clear();
            Folders.AddRange(orderedFolders);
        }


        public bool IsSorting
        {
            get
            { return _isSorting; }
            set
            {
                if (_isSorting == value)
                {
                    return;
                }
                SetProperty(ref _isSorting, value);
                SelectionMode = _isSorting ? "None" : "Single";
            }
        }

        public string SelectionMode
        {
            get
            { return _selectionMode; }
            set
            {
                if (_selectionMode == value)
                {
                    return;
                }
                SetProperty(ref _selectionMode, value);
            }
        }

        public bool IsSelecting
        {
            get
            { return _isSelecting; }
            set
            {
                if (_isSelecting == value)
                {
                    return;
                }
                SetProperty(ref _isSelecting, value);
                SelectionMode = _isSelecting ? "Multiple" : "Single";
            }
        }

        public int SelectedPathIndex
        {
            get
            {
                return _selectedPathIndex;
            }

            set
            {
                if (value == -1)
                {
                    return;
                }
                if (SetProperty(ref _selectedPathIndex, value))
                {
                    //stop the directory listening, if the selected path was changed
                    StopDirectoryListing();
                }

                while (PathStack.Count > 0 && PathStack.Count > SelectedPathIndex + 1)
                {
                    PathStack.RemoveAt(GetPathDepth);
                }
                RaisePropertyChanged(nameof(PathStack));
                SelectedIndexPathChanged();
                //ContinueDirectoryListing();
            }
        }

        public List<ResourceInfoModel> RemoveResourceInfos { get; set; }

        public void StopDirectoryListing()
        {
            _continueListing = false;
        }

        public void ContinueDirectoryListing()
        {
            _continueListing = true;
        }

        public async Task<bool> CreateDirectory(string directoryName)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            var path = PathStack.Count > 0 ? PathStack[PathStack.Count - 1].ResourceInfo.Path : "";
            var success = false;

            try
            {
                success = await client.CreateDirectory(path + directoryName);
            }
            catch (ResponseError e)
            {
                if (e.StatusCode != "400") // ProtocolError
                {
                    _responseErrorHandlerService.HandleException(e);
                }
            }

            if (success)
            {
                await StartDirectoryListing();
            }

            return success;
        }

        public async Task<bool> DeleteResource(ResourceInfoModel resourceInfo)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            var path = resourceInfo.ContentType.Equals("dav/directory")
                ? resourceInfo.Path
                : resourceInfo.Path + "/" + resourceInfo.Name;

            var success = await client.Delete(path);
            await StartDirectoryListing();
            return success;
        }

        public async Task<bool> DeleteSelected(List<ResourceInfoModel> resourceInfos)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            foreach (var resourceInfo in resourceInfos)
            {
                var path = resourceInfo.ContentType.Equals("dav/directory")
                ? resourceInfo.Path
                : resourceInfo.Path + "/" + resourceInfo.Name;
                var success = await client.Delete(path);
                if (!success)
                {
                    return false;
                }
            }

            await StartDirectoryListing();
            return true;
        }

        public async Task<bool> Rename(string oldName, string newName)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            var path = PathStack.Count > 0 ? PathStack[PathStack.Count - 1].ResourceInfo.Path : "";
            var success = false;

            try
            {
                success = await client.Move(path + oldName, path + newName);
            }
            catch (ResponseError e)
            {
                if (e.StatusCode != "400") // ProtocolError
                {
                    _responseErrorHandlerService.HandleException(e);
                }
            }

            if (success)
            {
                await StartDirectoryListing();
            }
            return success;
        }

        public async Task<bool> Move(string oldPath, string newPath)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            var success = false;

            try
            {
                success = await client.Move(oldPath, newPath);
            }
            catch (ResponseError e)
            {
                if (e.StatusCode != "400") // ProtocolError
                {
                    _responseErrorHandlerService.HandleException(e);
                }
            }

            if (success)
            {
                await StartDirectoryListing();
            }
            return success;
        }

        public async Task<bool> ToggleFavorite(ResourceInfoModel resourceInfo)
        {
            var client = await WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            return await client.ToggleFavorite(resourceInfo);
        }

        public void RebuildPathStackFromResourceInfo(ResourceInfoModel resourceInfo)
        {
            // remove all except root node
            while (PathStack.Count > 1)
            {
                PathStack.RemoveAt(PathStack.Count - 1);
            }

            // get path array from resource info
            var path = resourceInfo.Path.Split('/');
            var newPath = string.Empty;

            // register path
            foreach (var p in path)
            {
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }
                newPath += "/" + p;
                PathStack.Add(new PathInfoModel
                {
                    ResourceInfo = new ResourceInfoModel()
                    {
                        Name = p,
                        Path = newPath + "/"
                    }
                });
            }
        }
    }
}
