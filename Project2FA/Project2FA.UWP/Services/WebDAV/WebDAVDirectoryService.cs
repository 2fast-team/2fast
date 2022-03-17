using Prism.Mvvm;
using Project2FA.Core.Services.WebDAV;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WebDAVClient.Exceptions;
using WebDAVClient.Types;

namespace Project2FA.UWP.Services.WebDAV
{
    // based on https://github.com/nextcloud/windows-universal/blob/master/NextcloudApp/Services/DirectoryService.cs
    public class WebDAVDirectoryService : BindableBase
    {
        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static WebDAVDirectoryService Instance { get; } = new WebDAVDirectoryService();
        private ObservableCollection<WebDAVFileOrFolderModel> _folders;
        private ObservableCollection<WebDAVFileOrFolderModel> _filesAndFolders;
        private ObservableCollection<WebDAVFileOrFolderModel> _groupedFilesAndFolders;
        private int _selectedPathIndex;
        private bool _isSorting;
        private bool _createDatafile;
        private bool _isSelecting;
        private bool _isLoading;
        private bool _isBackPossible;
        private bool _isFolderEmpty;
        private string _selectionMode;
        private Task<List<ResourceInfoModel>> _listingTask;
        private IResponseErrorHandlerService _responseErrorHandlerService { get; }

        public WebDAVDirectoryService()
        {
            //_responseErrorHandlerService = responseErrorHandlerService;
            PathStack = new ObservableCollection<PathInfoModel>
            {
                new PathInfoModel
                {
                    ResourceInfo = new ResourceInfoModel()
                    {
                        Name = "Start",
                        Path = "/"
                    },
                    IsRoot = true,
                    PathIndex = 0
                }
            };
            // Arrange for the first time, so that the collections get filled.
            //_groupedFilesAndFolders.ArrangeItems(new NameSorter(SortSequence.Asc), x => x.Name.First().ToString().ToUpper());
            //_groupedFolders.ArrangeItems(new NameSorter(SortSequence.Asc), x => x.Name.First().ToString().ToUpper());
        }


        public ObservableCollection<PathInfoModel> PathStack { get; set; }


        public int GetPathDepth => PathStack.Count - 1;

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
            float sizeMb = fileOrFolder.Size / 1024f / 1024f;

            long[] sizesValuesMb = { 1, 5, 10, 25, 50, 100, 250, 500, 1024, 5120, 10240, 102400, 1048576 };
            string[] sizesDisplay = { "<1MB", ">1MB", ">5MB", ">10MB", ">25MB", ">50MB", ">100MB", ">250MB", ">500MB", ">1GB", ">5GB", ">10GB", ">100GB", ">1TB" };

            int index = 0;

            foreach (long t in sizesValuesMb)
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

        public Task Refresh(bool createDatafile = false)
        {
            return StartDirectoryListing(createDatafile);
        }

        private Task SelectedIndexPathChanged()
        {
            return StartDirectoryListing(null);
        }

        public Task StartDirectoryListing(bool createDatafile = false)
        {
            _createDatafile = createDatafile;
            return StartDirectoryListing(null);
        }

        public async Task StartDirectoryListing(ResourceInfoModel resourceInfoToExclude, string viewName = null)
        {
            IsLoading = true;
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

            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null || IsSelecting)
            {
                return;
            }

            //_continueListing = true;

            if (PathStack.Count == 0)
            {
                PathStack.Add(new PathInfoModel
                {
                    ResourceInfo = new ResourceInfoModel()
                    {
                        Name = "Main",
                        Path = "/"
                    },
                    IsRoot = true,
                    PathIndex = 0
                });
            }

            IsBackPossible = PathStack.Count > 1;

            string path = PathStack.Count > 0 ? PathStack[GetPathDepth].ResourceInfo.Path : "/";
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
                foreach (ResourceInfoModel item in list)
                {
                    if (resourceInfoToExclude != null && item == resourceInfoToExclude)
                    {
                        continue;
                    }

                    if (!_createDatafile)
                    {
                        if (item.Name.Contains(".2fa"))
                        {
                            FilesAndFolders.Add(new WebDAVFileOrFolderModel(item));
                        }
                    }

                    if (RemoveResourceInfos != null)
                    {
                        int index = RemoveResourceInfos.FindIndex(
                            res => res.Path.Equals(item.Path, StringComparison.Ordinal));
                        if (index == -1)
                        {
                            if (!_createDatafile)
                            {
                                if (item.Name.Contains(".2fa"))
                                {
                                    FilesAndFolders.Add(new WebDAVFileOrFolderModel(item));
                                }
                                Folders.Add(new WebDAVFileOrFolderModel(item));
                            }
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
            if (FilesAndFolders.Count == 0)
            {
                IsFolderEmpty = true;
            }
            else
            {
                IsFolderEmpty = false;
            }

            SortList();
            IsLoading = false;
        }

        private void SortList()
        {
            List<WebDAVFileOrFolderModel> orderedFilesAndFolders = FilesAndFolders.OrderBy(x => !x.IsDirectory).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            GroupedFilesAndFolders.AddRange(orderedFilesAndFolders);

            List<WebDAVFileOrFolderModel> orderedFolders = Folders.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            Folders.Clear();
            Folders.AddRange(orderedFolders);
        }


        public bool IsSorting
        {
            get => _isSorting;
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
            get => _selectionMode;
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
            get => _isSelecting;
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
            get => _selectedPathIndex;

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

        public bool IsLoading 
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public bool IsBackPossible
        {
            get => _isBackPossible;
            set => SetProperty(ref _isBackPossible, value);
        }
        public bool IsFolderEmpty 
        { 
            get => _isFolderEmpty;
            set => SetProperty(ref _isFolderEmpty, value);
        }

        public void StopDirectoryListing()
        {
            //_continueListing = false;
        }

        public void ContinueDirectoryListing()
        {
            //_continueListing = true;
        }

        public async Task<bool> CreateDirectory(string directoryName)
        {
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            string path = PathStack.Count > 0 ? PathStack[PathStack.Count - 1].ResourceInfo.Path : "";
            bool success = false;

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
                //Folders.Add
                await StartDirectoryListing();
            }

            return success;
        }

        public async Task<bool> DeleteResource(ResourceInfoModel resourceInfo)
        {
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            string path = resourceInfo.ContentType.Equals("dav/directory")
                ? resourceInfo.Path
                : resourceInfo.Path + "/" + resourceInfo.Name;

            bool success = await client.Delete(path);
            await StartDirectoryListing();
            return success;
        }

        public async Task<bool> DeleteSelected(List<ResourceInfoModel> resourceInfos)
        {
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            foreach (ResourceInfoModel resourceInfo in resourceInfos)
            {
                string path = resourceInfo.ContentType.Equals("dav/directory")
                ? resourceInfo.Path
                : resourceInfo.Path + "/" + resourceInfo.Name;
                bool success = await client.Delete(path);
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
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            string path = PathStack.Count > 0 ? PathStack[PathStack.Count - 1].ResourceInfo.Path : "";
            bool success = false;

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
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            if (client == null)
            {
                return false;
            }

            bool success = false;

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
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();

            return client != null && await client.ToggleFavorite(resourceInfo);
        }

        public void RebuildPathStackFromResourceInfo(ResourceInfoModel resourceInfo)
        {
            // remove all except root node
            while (PathStack.Count > 1)
            {
                PathStack.RemoveAt(PathStack.Count - 1);
            }

            // get path array from resource info
            string[] path = resourceInfo.Path.Split('/');
            string newPath = string.Empty;

            // register path
            foreach (string p in path)
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
                    },
                    PathIndex = GetPathDepth + 1
                });
            }
        }
    }
}
