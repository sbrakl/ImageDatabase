using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using ImageDatabase.Indexers;
using ImageDatabase.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace ImageDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string host = "localhost";
        public string IndexDirectory { get; set; }
        public string QueryImageFullPath { get; set; }
        public string CodeBookFullPath { get; set; }
        public bool ExecInParallel { get; set; }
        public emAlgo SelectedAlgo { get; set; }
        public emCEDDAlgo SelectedCEDDAlgo { get; set; }

        private BackgroundWorker IndexBgWorker;
        private BackgroundWorker QueryBgWorker;
        private Stopwatch _stopWatch;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitIndexDirectory();
            cmbAlgo.SelectedIndex = Properties.Settings.Default.SelectedAlgo;
            SelectedAlgo = GetSelectedAlgo();
            //if (SelectedAlgo == emAlgo.CEDD)
            LoadCEDDSettings();
            //else if (SelectedAlgo == emAlgo.SURF || SelectedAlgo  == emAlgo.AccordSurf)
            LoadSurfSettings();
            //else if (SelectedAlgo == emAlgo.Locate)
            LoadLocateSettings();
            SetBackgroundWorker();
        }

        private void LoadSurfSettings()
        {
            cmbSurfThreshold.SelectedIndex = Properties.Settings.Default.SurfThresholdSelectedIndex;
            nudsurfUniqueThreshold.Value = Properties.Settings.Default.SurfUniquenessThreshold;
            nudsurfGoodMatchThreshold.Value = Properties.Settings.Default.SurfGoodMatchThreshold;
            cmbSurfApproach.SelectedIndex = Properties.Settings.Default.SurfAlgo;
        }

        private void LoadCEDDSettings()
        {
            cmbCeddAlgoType.SelectedIndex = Properties.Settings.Default.CEDDAlgo;
        }

        private void LoadLocateSettings()
        {
            HideCreateCodeBookSettings();
            chkCreateCodeBook.IsChecked = Properties.Settings.Default.Locate_IsCodeBookNeedToBeCreated;
            txtCodeBookPath.Text = Properties.Settings.Default.CodeBookFolder;
            nudLocateDistanceThreshold.Value = Properties.Settings.Default.Locate_GoodThresholdDistance;
            nudCodeBookLength.Value = Properties.Settings.Default.Locate_SizeOfCookbook;
            chkLTEScheme.IsChecked = Properties.Settings.Default.Locate_IsLteSchemeNeedToAppy;
            cmbLocateQueryAlgo.SelectedIndex = Properties.Settings.Default.Locate_SelectedMethod;
        }

        private void InitIndexDirectory()
        {
            string indexDirecroty = Properties.Settings.Default.IndexFolder;
            if (string.IsNullOrEmpty(indexDirecroty))
            {
                indexDirecroty = DirectoryHelper.PictureObserverDirectory;
                Properties.Settings.Default.IndexFolder = indexDirecroty;
            }
            SetIndexDirectory(indexDirecroty);
        }

        private void SetIndexDirectory(string folder)
        {
            lblDirectoryPath.Text = folder;
            IndexDirectory = folder;
            Properties.Settings.Default.IndexFolder = folder;
            Properties.Settings.Default.Save();
        }

        private void SetBackgroundWorker()
        {
            IndexBgWorker = new BackgroundWorker();

            IndexBgWorker.DoWork += new DoWorkEventHandler(IndexBgWorker_DoWork);
            IndexBgWorker.ProgressChanged += new ProgressChangedEventHandler
                    (IndexBgWorker_ProgressChanged);
            IndexBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (IndexBgWorker_RunWorkerCompleted);
            IndexBgWorker.WorkerReportsProgress = true;
            IndexBgWorker.WorkerSupportsCancellation = true;

            QueryBgWorker = new BackgroundWorker();
            QueryBgWorker.DoWork += new DoWorkEventHandler(QueryBgWorker_DoWork);
            QueryBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                   (QueryBgWorker_RunWorkerCompleted);
            QueryBgWorker.WorkerReportsProgress = true;
        }

        private void SaveSurfSettingValues()
        {
            this.Dispatcher.Invoke(() =>
            {
                Properties.Settings.Default.SurfThresholdSelectedIndex = cmbSurfThreshold.SelectedIndex;
                Properties.Settings.Default.SurfUniquenessThreshold = nudsurfUniqueThreshold.Value.Value;
                Properties.Settings.Default.SurfGoodMatchThreshold = nudsurfGoodMatchThreshold.Value.Value;
                Properties.Settings.Default.SurfAlgo = cmbSurfApproach.SelectedIndex;
                Properties.Settings.Default.Save();
            });
        }

        private emAlgo GetSelectedAlgo()
        {
            emAlgo selectAlgo = emAlgo.Undetermined;
            if (cmbAlgo.SelectedIndex > -1)
                selectAlgo = (emAlgo)cmbAlgo.SelectedIndex + 1;
            return selectAlgo;
        }

        private emCEDDAlgo GetSelectedCEDDAlgo()
        {
            emCEDDAlgo selectAlgo = emCEDDAlgo.BKTree;
            if (!cmbCeddAlgoType.CheckAccess())
            {
                cmbCeddAlgoType.Dispatcher.Invoke((Action)(() => selectAlgo = (emCEDDAlgo)cmbCeddAlgoType.SelectedIndex));
            }
            else
            {
                if (cmbCeddAlgoType.SelectedIndex > -1)
                    selectAlgo = (emCEDDAlgo)cmbCeddAlgoType.SelectedIndex;
            }

            return selectAlgo;
        }
        public FileInfo[] getFiles(string SourceFolder, string Filter,
                System.IO.SearchOption searchOption)
        {
            List<FileInfo> files = new List<FileInfo>();

            DirectoryInfo di = new DirectoryInfo(SourceFolder);

            // Create an array of filter string
            string[] MultipleFilters = Filter.Split('|');

            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                files.AddRange(di.GetFiles(FileFilter, searchOption));
            }

            // returns string array of relevant file names
            return files.ToArray();
        }
        private SurfSettings GetSurfSetting()
        {
            SurfSettings surfSetting = new SurfSettings();

            this.Dispatcher.Invoke(() =>
            {
                surfSetting.HessianThresh = Convert.ToDouble((cmbSurfThreshold.SelectedItem as ListBoxItem).Content);
                surfSetting.UniquenessThreshold = nudsurfUniqueThreshold.Value;
                surfSetting.GoodMatchThreshold = nudsurfGoodMatchThreshold.Value;
                surfSetting.Algorithm = (SurfAlgo)cmbSurfApproach.SelectedIndex;
            });

            return surfSetting;
        }
        private void cmbAlgo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectedAlgo = cmbAlgo.SelectedIndex;
            Properties.Settings.Default.Save();
            SelectedAlgo = GetSelectedAlgo();
            if (SelectedAlgo == emAlgo.SURF)
            {
                btnShowHideSettings.Visibility = System.Windows.Visibility.Visible;
                btnShowHideSettings.Content = "Hide Setting";
                VectorSetting.Visibility = System.Windows.Visibility.Visible;
                grpSurf.Visibility = System.Windows.Visibility.Visible;
                grpLocate.Visibility = System.Windows.Visibility.Hidden;
                CeddSetting.Visibility = System.Windows.Visibility.Hidden;
                var selectedIndex = cmbSurfApproach.SelectedIndex;
                (cmbSurfApproach.Items[1] as ListBoxItem).Content = "flann";
                cmbSurfApproach.SelectedIndex = -1;
                cmbSurfApproach.SelectedIndex = selectedIndex;
            }
            else if (SelectedAlgo == emAlgo.AccordSurf)
            {
                btnShowHideSettings.Visibility = System.Windows.Visibility.Visible;
                btnShowHideSettings.Content = "Hide Setting";
                VectorSetting.Visibility = System.Windows.Visibility.Visible;
                grpSurf.Visibility = System.Windows.Visibility.Visible;
                grpLocate.Visibility = System.Windows.Visibility.Hidden;
                CeddSetting.Visibility = System.Windows.Visibility.Hidden;
                var selectedIndex = cmbSurfApproach.SelectedIndex;
                (cmbSurfApproach.Items[1] as ListBoxItem).Content = "kd-tree";
                cmbSurfApproach.SelectedIndex = -1;
                cmbSurfApproach.SelectedIndex = selectedIndex;
            }
            else if (SelectedAlgo == emAlgo.Locate)
            {
                btnShowHideSettings.Visibility = System.Windows.Visibility.Visible;
                btnShowHideSettings.Content = "Hide Setting";
                VectorSetting.Visibility = System.Windows.Visibility.Visible;
                grpSurf.Visibility = System.Windows.Visibility.Hidden;
                grpLocate.Visibility = System.Windows.Visibility.Visible;
                CeddSetting.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (SelectedAlgo == emAlgo.CEDD)
            {
                btnShowHideSettings.Visibility = System.Windows.Visibility.Visible;
                btnShowHideSettings.Content = "Hide Setting";
                VectorSetting.Visibility = System.Windows.Visibility.Hidden;
                CeddSetting.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnShowHideSettings.Visibility = System.Windows.Visibility.Hidden;
                VectorSetting.Visibility = System.Windows.Visibility.Hidden;
                CeddSetting.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            List<string> indexFileNames = new List<string>{
                "pHashIndex.bin", "rgbHistoIndex.bin", "BhattacharyyaIndex.bin", "CEDDIndex.bin", "CeddTreeIndex.bin"
            };
            foreach (var file in indexFileNames)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }           
        }
        private void btnShowHideSettings_Click(object sender, RoutedEventArgs e)
        {
            if (btnShowHideSettings.Content.ToString().ToLower() == "hide setting")
            {
                btnShowHideSettings.Content = "Show Setting";
                if (SelectedAlgo == emAlgo.CEDD)
                {
                    CeddSetting.Visibility = System.Windows.Visibility.Hidden;
                }
                else if (SelectedAlgo == emAlgo.SURF
                    || SelectedAlgo == emAlgo.AccordSurf
                    || SelectedAlgo == emAlgo.Locate)
                {
                    VectorSetting.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                btnShowHideSettings.Content = "Hide Setting";
                if (SelectedAlgo == emAlgo.CEDD)
                {
                    CeddSetting.Visibility = System.Windows.Visibility.Visible;
                }
                else if (SelectedAlgo == emAlgo.SURF
                    || SelectedAlgo == emAlgo.AccordSurf
                    || SelectedAlgo == emAlgo.Locate)
                {
                    VectorSetting.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        #region Indexing Section
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            string _folderName = IndexDirectory;

            var dlg1 = new FolderBrowserDialogEx
            {
                Description = "Select a folder for the extracted files:",
                ShowNewFolderButton = false,
                ShowEditBox = true,
                SelectedPath = _folderName,
                ShowFullPathInEditBox = false,
            };
            dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            System.Windows.Forms.DialogResult result = dlg1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SetIndexDirectory(dlg1.SelectedPath);
            }
        }

        private void chkIndexAysc_Click(object sender, RoutedEventArgs e)
        {
            ExecInParallel = chkIndexAysc.IsChecked.HasValue ? chkIndexAysc.IsChecked.Value : false;
        }

        private void btnIndex_Click(object sender, RoutedEventArgs e)
        {
            btnIndex.IsEnabled = false;
            if (SelectedAlgo == emAlgo.SURF || SelectedAlgo == emAlgo.AccordSurf)
            {
                //Save Settings
                SaveSurfSettingValues();

                //Pass Surf Settings
                SurfSettings surfSetting = GetSurfSetting();

                IndexBgWorker.RunWorkerAsync(surfSetting);
            }
            else if (SelectedAlgo == emAlgo.Locate)
            {
                LocateSettings locateSetting = GetLocateSetting();
                SaveLocateSetting();
                IndexBgWorker.RunWorkerAsync(locateSetting);
            }
            else
            {
                IndexBgWorker.RunWorkerAsync();
            }
        }


        private void IndexBgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                WriteIndexStatus("Indexing...");
                IIndexer indexer = null;
                object setting = null;
                SurfSettings surfSetting;
                LocateSettings locateSetting;
                switch (SelectedAlgo)
                {
                    case emAlgo.Undetermined:
                        MessageBox.Show("Please select Algorithm for indexing");
                        return;
                    case emAlgo.pHash:
                        indexer = new PHashIndexer();
                        BinaryAlgoIndexing(indexer, setting);
                        break;
                    case emAlgo.RBGHistogram:
                        indexer = new RGBProjectionIndexer();
                        BinaryAlgoIndexing(indexer, setting);
                        break;
                    case emAlgo.bhattacharyya:
                        indexer = new BhattacharyyaIndexer();
                        BinaryAlgoIndexing(indexer, setting);
                        break;
                    case emAlgo.CEDD:
                        emCEDDAlgo selectedCeddAlgo = GetSelectedCEDDAlgo();
                        if (selectedCeddAlgo == emCEDDAlgo.BKTree)
                            indexer = new CEDDIndexer2();
                        else
                            indexer = new CEDDIndexer();
                        BinaryAlgoIndexing(indexer, setting);
                        break;
                    case emAlgo.SURF:
                        surfSetting = (e.Argument as SurfSettings);
                        if (surfSetting == null)
                        {
                            MessageBox.Show("SurfSetting not supplied");
                            return;
                        };
                        SurfAlgoIndexing(surfSetting);
                        break;
                    case emAlgo.AccordSurf:
                        surfSetting = (e.Argument as SurfSettings);
                        if (surfSetting == null)
                        {
                            MessageBox.Show("SurfSetting not supplied");
                            return;
                        };
                        AccordSurfAlgoIndexing(surfSetting);
                        break;
                    case emAlgo.Locate:
                        locateSetting = (e.Argument as LocateSettings);
                        LocateAlgoIndexing(locateSetting);
                        break;
                    default:
                        MessageBox.Show("Algorithm not currently supported");
                        return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void IndexBgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbIndex.Value = e.ProgressPercentage + 1;
        }

        private void IndexBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.ToString());
            btnIndex.IsEnabled = true;
        }

        private void WriteIndexStatus(string msg)
        {
            var txtdispatcher = lblIndexStatus.Dispatcher;
            if (!txtdispatcher.CheckAccess())
            {
                txtdispatcher.Invoke((Action)(() => lblIndexStatus.Text = msg));
            }
            else
            {
                lblIndexStatus.Text = msg;
            }
        }

        private void BinaryAlgoIndexing(IIndexer indexer, object setting)
        {

            FileInfo[] imageFiles = getFiles(IndexDirectory, "*.gif|*.jpg|*.png|*.bmp|*.jpeg", SearchOption.TopDirectoryOnly);
            int totalFileCount = imageFiles.Length;

            Dispatcher.Invoke((Action)delegate
            {
                pbIndex.Minimum = 0; pbIndex.Maximum = totalFileCount; lblIndexStatus.Text = "Indexing..";
            });

            if (indexer != null)
            {
                _stopWatch = Stopwatch.StartNew();
                if (ExecInParallel)
                    indexer.IndexFilesAsync(imageFiles, IndexBgWorker, setting);
                else
                    indexer.IndexFiles(imageFiles, IndexBgWorker, setting);
                _stopWatch.Stop();
            }

            long timeElapse = _stopWatch.ElapsedMilliseconds;
            Dispatcher.Invoke((Action)delegate
            {
                lblIndexStatus.Text
                    = string.Format("Indexing completed, it tooked {0} ms to index", timeElapse);
                btnIndex.IsEnabled = true;
            });


        }

        private void SurfAlgoIndexing(SurfSettings surfSetting)
        {

            var imageFiles = getFiles(IndexDirectory, "*.gif|*.jpg|*.png|*.bmp|*.jpeg", SearchOption.TopDirectoryOnly);
            int totalFileCount = imageFiles.Length;

            Dispatcher.Invoke((Action)delegate
            {
                pbIndex.Minimum = 0; pbIndex.Maximum = totalFileCount; lblIndexStatus.Text = "Indexing..";
            });

            if (surfSetting.Algorithm == SurfAlgo.Linear)
            {
                SurfIndexer2 surfIndexer = new SurfIndexer2();
                if (ExecInParallel)
                    surfIndexer.IndexFilesAsync(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
                else
                    surfIndexer.IndexFiles(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
            }
            else
            {
                SurfIndexer1 surfIndexer = new SurfIndexer1();
                if (ExecInParallel)
                    throw new InvalidOperationException("Parallel exec not supported!");
                surfIndexer.IndexFiles(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
            }
        }

        private void AccordSurfAlgoIndexing(SurfSettings surfSetting)
        {

            var imageFiles = getFiles(IndexDirectory, "*.gif|*.jpg|*.png|*.bmp|*.jpeg", SearchOption.TopDirectoryOnly);
            int totalFileCount = imageFiles.Length;

            Dispatcher.Invoke((Action)delegate
            {
                pbIndex.Minimum = 0; pbIndex.Maximum = totalFileCount; lblIndexStatus.Text = "Indexing..";
            });

            if (surfSetting.Algorithm == SurfAlgo.Linear)
            {
                SurfIndexer4 surfIndexer = new SurfIndexer4();
                if (ExecInParallel)
                    surfIndexer.IndexFilesAsync(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
                else
                    surfIndexer.IndexFiles(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
            }
            else
            {
                SurfIndexer3 surfIndexer = new SurfIndexer3();
                if (ExecInParallel)
                    surfIndexer.IndexFilesAsync(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
                else
                    surfIndexer.IndexFiles(imageFiles, IndexBgWorker, WriteIndexStatus, surfSetting);
            }
        }

        private void LocateAlgoIndexing(LocateSettings locateSetting)
        {

            var imageFiles = getFiles(IndexDirectory, "*.gif|*.jpg|*.png|*.bmp|*.jpeg", SearchOption.TopDirectoryOnly);
            int totalFileCount = imageFiles.Length;

            Dispatcher.Invoke((Action)delegate
            {
                pbIndex.Minimum = 0; pbIndex.Maximum = totalFileCount; lblIndexStatus.Text = "Indexing..";
            });

            LocateIndexer LocateIndexer = new LocateIndexer();
            if (ExecInParallel)
                LocateIndexer.IndexFilesAsync(imageFiles, IndexBgWorker, WriteIndexStatus, locateSetting);
            else
                LocateIndexer.IndexFiles(imageFiles, IndexBgWorker, WriteIndexStatus, locateSetting);
        }
        #endregion

        #region Context Menu
        private void brdImage_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement fe = e.Source as FrameworkElement;
            ContextMenu cm = fe.ContextMenu;
            while (cm == null)
            {
                fe = (FrameworkElement)fe.Parent;
                if (fe == null) break;
                cm = fe.ContextMenu;
            }
            MenuItem openSurfDetails = null;
            foreach (MenuItem mi in cm.Items)
            {
                if (mi.Name == "mnuOpenSurfDetail")
                {
                    openSurfDetails = mi;
                }
            }
            if (SelectedAlgo == emAlgo.SURF
                || SelectedAlgo == emAlgo.AccordSurf)
            {
                if (openSurfDetails != null) openSurfDetails.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                if (openSurfDetails != null) openSurfDetails.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void mnuOpenSurfDetail_Click(object sender, RoutedEventArgs e)
        {
            string selectedObserverImage = string.Empty;
            string selectedModelImage = string.Empty;
            MenuItem mnu = sender as MenuItem;
            Border selectedBorder = null;
            if (mnu != null)
            {
                selectedBorder = ((ContextMenu)mnu.Parent).PlacementTarget as Border;
                selectedObserverImage = (string)selectedBorder.Tag;
            }
            selectedModelImage = QueryImageFullPath;
            bool isObserverImagePathFound = !string.IsNullOrWhiteSpace(selectedObserverImage);
            isObserverImagePathFound = isObserverImagePathFound && File.Exists(selectedObserverImage);
            bool isModelImagePathFound = !string.IsNullOrWhiteSpace(selectedModelImage);
            isModelImagePathFound = isObserverImagePathFound && File.Exists(selectedModelImage);


            if (isObserverImagePathFound & isModelImagePathFound)
            {
                SurfSettings surfSetting = GetSurfSetting();
                if (SelectedAlgo == emAlgo.SURF)
                {
                    DrawSurfMatches.MatchInWindow(selectedModelImage, selectedObserverImage, surfSetting);
                }
                else
                {
                    AccordSurfWindow win = new AccordSurfWindow(selectedModelImage, selectedObserverImage, surfSetting);
                    win.Show();
                }

            }
        }

        private void mnuOpenPic_Click(object sender, RoutedEventArgs e)
        {
            string selectedObserverImage = string.Empty;
            string selectedModelImage = string.Empty;
            MenuItem mnu = sender as MenuItem;
            Border selectedBorder = null;
            if (mnu != null)
            {
                selectedBorder = ((ContextMenu)mnu.Parent).PlacementTarget as Border;
                selectedObserverImage = (string)selectedBorder.Tag;
            }
            selectedModelImage = QueryImageFullPath;
            bool isObserverImagePathFound = !string.IsNullOrWhiteSpace(selectedObserverImage);
            isObserverImagePathFound = isObserverImagePathFound && File.Exists(selectedObserverImage);
            if (isObserverImagePathFound)
            {
                System.Diagnostics.Process.Start(selectedObserverImage);
            }
        }
        #endregion

        #region Query Image Section
        private void btnSelectQueryImage_Click(object sender, RoutedEventArgs e)
        {
            string queryImagePath = SelectQueryImage();
            QueryImage(queryImagePath);
        }

        private void btnQueryImage_Click(object sender, RoutedEventArgs e)
        {
            QueryImage(QueryImageFullPath);
        }

        private string SelectQueryImage()
        {
            string queryImageFilePath = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.QueryFolder))
            {
                dlg.InitialDirectory = IndexDirectory;
            }
            else
            {
                dlg.InitialDirectory = Properties.Settings.Default.QueryFolder;
            }


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                queryImageFilePath = dlg.FileName;
                txtQuery.Text = queryImageFilePath;
                QueryImageFullPath = queryImageFilePath;
                imqQuery.Source = new BitmapImage(new Uri(queryImageFilePath));
                Properties.Settings.Default.QueryFolder = Path.GetDirectoryName(queryImageFilePath);
                Properties.Settings.Default.Save();
            }

            return queryImageFilePath;
        }

        private void QueryBgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                btnSelectQueryImage.IsEnabled = false;
                btnQueryImage.IsEnabled = false;
            });
            switch (SelectedAlgo)
            {
                case emAlgo.SURF:
                    QuerySurfAlgo(QueryImageFullPath);
                    break;
                case emAlgo.AccordSurf:
                    QueryAccordSurfAlgo(QueryImageFullPath);
                    break;
                case emAlgo.Locate:
                    QueryLocate(QueryImageFullPath);
                    break;
                default:
                    MessageBox.Show("Algorithm not currently supported");
                    return;
            }
            this.Dispatcher.Invoke(() =>
            {
                btnSelectQueryImage.IsEnabled = true;
                btnQueryImage.IsEnabled = true;
            });
        }

        private void QueryBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
                WriteQueryStatus("Error in querying! Couldn't query");
            }
            btnQueryImage.IsEnabled = true;
            btnSelectQueryImage.IsEnabled = true;
        }

        private void QueryImage(string queryImagePath)
        {
            try
            {
                ImageList.ItemsSource = null;
                if (!string.IsNullOrEmpty(queryImagePath))
                {
                    object setting = null;
                    QueryImageFullPath = queryImagePath;
                    IImageQuery imageQuery = null;
                    switch (SelectedAlgo)
                    {
                        case emAlgo.Undetermined:
                            MessageBox.Show("Please select Algorithm for indexing");
                            return;
                        case emAlgo.pHash:
                            imageQuery = new pHashQuery();
                            QueryBinaryAlgo(imageQuery, queryImagePath, setting);
                            break;
                        case emAlgo.RBGHistogram:
                            imageQuery = new RGBProjectQuery();
                            QueryBinaryAlgo(imageQuery, queryImagePath, setting);
                            break;
                        case emAlgo.bhattacharyya:
                            imageQuery = new BhattacharyyaQuery();
                            QueryBinaryAlgo(imageQuery, queryImagePath, setting);
                            break;
                        case emAlgo.CEDD:
                            emCEDDAlgo selectedCeddAlgo = GetSelectedCEDDAlgo();
                            if (selectedCeddAlgo == emCEDDAlgo.BKTree)
                                imageQuery = new CEDDQuery2();
                            else
                                imageQuery = new CEDDQuery();
                            int goodMatchDistance = nudCEDDGoodMatchThreshold.Value.Value;
                            QueryBinaryAlgo(imageQuery, queryImagePath, goodMatchDistance);
                            break;
                        case emAlgo.SURF:
                        case emAlgo.AccordSurf:
                        case emAlgo.Locate:
                            QueryBgWorker.RunWorkerAsync();
                            break;
                        default:
                            MessageBox.Show("Algorithm not currently supported");
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error in Image Query", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QueryBinaryAlgo(IImageQuery imageQuery, string queryImagePath, object setting)
        {
            btnSelectQueryImage.IsEnabled = false;
            btnQueryImage.IsEnabled = false;

            var disphacher = this.Dispatcher;
            WriteQueryStatus("Start querying...");
            List<ImageRecord> searchImage = new List<ImageRecord>();

            System.Threading.Tasks.Task queryTask = System.Threading.Tasks.Task.Factory.StartNew(
                () =>
                {
                    _stopWatch = Stopwatch.StartNew();
                    searchImage = imageQuery.QueryImage(queryImagePath, setting);
                    _stopWatch.Stop();
                    return new
                    {
                        SearchImage = searchImage,
                        TimeLapse = _stopWatch.ElapsedMilliseconds
                    };
                });

            System.Threading.Tasks.Task waitTask = System.Threading.Tasks.Task.Factory.StartNew(
                () =>
                {
                    //const int incr = 1;
                    //int t = incr; string msg = "";
                    //while (!queryTask.Wait(incr * 500))
                    //{
                    //    msg = string.Format("Query is taking more than {0} second...", t);
                    //    WriteQueryStatus(msg);
                    //    t += incr;                            
                    //}

                    queryTask.Wait();
                    disphacher.Invoke((Action)(() =>
                    {
                        ImageList.ItemsSource = searchImage;
                        lblTotalCount.Text = searchImage.Count.ToString();
                        lblQueryStatus.Text = string.Format("Image Query Completed, it took {0} ms", _stopWatch.ElapsedMilliseconds);
                        btnSelectQueryImage.IsEnabled = true;
                        btnQueryImage.IsEnabled = true;
                    }));
                });

        }

        private void QuerySurfAlgo(string queryImagePath)
        {
            WriteQueryStatus("Start querying...");
            SaveSurfSettingValues();
            SurfSettings surfSetting = GetSurfSetting();
            List<ImageRecord> searchImage = new List<ImageRecord>();
            bool didRepoLoadingtookplace = false;
            long repoLoadingTimeInMs = 0;

            if (!SurfRepository.IsRepositoryInMemoryLoaded(surfSetting.Algorithm))
            {
                didRepoLoadingtookplace = true;
                WriteQueryStatus("Loading Repository in memory...");
                _stopWatch = Stopwatch.StartNew();
                SurfRepository.LoadRespositoryFromFile(surfSetting.Algorithm);
                _stopWatch.Stop();
                repoLoadingTimeInMs = _stopWatch.ElapsedMilliseconds;
                WriteQueryStatus(string.Format("Repository loading took {0} ms", repoLoadingTimeInMs));
            }

            string msg;
            if (surfSetting.Algorithm == SurfAlgo.Linear)
            {
                SurfQuery2 surfQuery = new SurfQuery2();
                _stopWatch = Stopwatch.StartNew();
                searchImage = surfQuery.QueryImage(queryImagePath, surfSetting);
                _stopWatch.Stop();
                if (didRepoLoadingtookplace)
                    msg = string.Format("Loading tooked {0} ms, quering tooked {1} ms", repoLoadingTimeInMs, _stopWatch.ElapsedMilliseconds);
                else
                    msg = string.Format("Image Query Completed, it took {0} ms", _stopWatch.ElapsedMilliseconds);
                WriteQueryStatus(msg);
            }
            else if (surfSetting.Algorithm == SurfAlgo.Flaan)
            {
                SurfQuery1 surfQuery = new SurfQuery1();
                _stopWatch = Stopwatch.StartNew();
                searchImage = surfQuery.QueryImage(queryImagePath, out msg, surfSetting);
                _stopWatch.Stop();
                if (didRepoLoadingtookplace)
                    msg = string.Format("Loading Repo: {0} ms, Quering Total {1} ms, Details - ", repoLoadingTimeInMs, _stopWatch.ElapsedMilliseconds) + msg;
                else
                    msg += string.Format(" Total {0} ms", _stopWatch.ElapsedMilliseconds);
                WriteQueryStatus(msg);
            }
            this.Dispatcher.Invoke(() =>
            {
                ImageList.ItemsSource = searchImage;
                lblTotalCount.Text = searchImage.Count.ToString();
            });
        }

        private void QueryAccordSurfAlgo(string queryImagePath)
        {
            WriteQueryStatus("Start querying...");
            SaveSurfSettingValues();
            List<ImageRecord> searchImage = new List<ImageRecord>();
            SurfSettings surfSetting = GetSurfSetting();
            string msg;
            if (surfSetting.Algorithm == SurfAlgo.Linear)
            {
                SurfQuery4 surfQuery = new SurfQuery4();
                _stopWatch = Stopwatch.StartNew();
                searchImage = surfQuery.QueryImage(queryImagePath, out msg, surfSetting);
                _stopWatch.Stop();
            }
            else
            {
                SurfQuery3 surfQuery = new SurfQuery3();
                _stopWatch = Stopwatch.StartNew();
                searchImage = surfQuery.QueryImage(queryImagePath, out msg, surfSetting);
                _stopWatch.Stop();
            }
            msg += string.Format(" Total {0} ms", _stopWatch.ElapsedMilliseconds);
            WriteQueryStatus(msg);
            this.Dispatcher.Invoke(() =>
               {
                   ImageList.ItemsSource = searchImage;
                   lblTotalCount.Text = searchImage.Count.ToString();
               });
        }

        private void QueryLocate(string queryImagePath)
        {
            List<ImageRecord> searchImage = new List<ImageRecord>();
            WriteQueryStatus("Start querying...");
            _stopWatch = Stopwatch.StartNew();

            LocateSettings locateSetting = GetLocateSetting();
            SaveLocateSetting();

            string msg;
            LocateQuery locateQuery = new LocateQuery();
           
                searchImage = locateQuery.QueryImage(queryImagePath, out msg, locateSetting);
          

            _stopWatch.Stop();
            msg += string.Format(" Total {0} ms", _stopWatch.ElapsedMilliseconds);
            WriteQueryStatus(msg);
            this.Dispatcher.Invoke(() =>
            {
                ImageList.ItemsSource = searchImage;
                lblTotalCount.Text = searchImage.Count.ToString();
            });
        }

        private void WriteQueryStatus(string msg)
        {
            var txtdispatcher = lblQueryStatus.Dispatcher;
            if (!txtdispatcher.CheckAccess())
            {
                txtdispatcher.Invoke((Action)(() => lblQueryStatus.Text = msg));
            }
            else
            {
                lblQueryStatus.Text = msg;
            }
        }
        #endregion

        #region Locate Setting Region
        private void CheckBox_changed(object sender, RoutedEventArgs e)
        {
            CheckBox createCodeBook = (CheckBox)sender;
            if (createCodeBook.IsChecked.Value)
            {
                btnSelectCodeBook.Visibility = System.Windows.Visibility.Hidden;
                nudCodeBookLength.Visibility = System.Windows.Visibility.Visible;
                lblCodeBookLength.Visibility = System.Windows.Visibility.Visible;
                UpdateCodeBookName();
            }
            else
            {
                HideCreateCodeBookSettings();
            }
        }

        private void UpdateCodeBookName()
        {
            lblCodeBookMsg.Content = "CodeBook would be create at";
            int totalNumberOfImageFiles = GetNumberOfImageFilesInIndexDirectory();
            int codeBookSize = nudCodeBookLength.Value.Value;
            string CodebookSaveFullPath = DirectoryHelper.CodebookFullPath(codeBookSize, totalNumberOfImageFiles);
            txtCodeBookPath.Text = CodebookSaveFullPath;
        }

        private void btnSelectCodeBook_Click(object sender, RoutedEventArgs e)
        {
            string codeBookFilePath = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.CodeBookFolder))
            {
                dlg.InitialDirectory = DirectoryHelper.SaveDirectoryPath;
            }
            else
            {
                dlg.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.CodeBookFolder);
            }


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".cb";
            dlg.Filter = "Codebook Files (*.cb)|*.cb";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                codeBookFilePath = dlg.FileName;
                txtCodeBookPath.Text = codeBookFilePath;
                CodeBookFullPath = codeBookFilePath;
                Properties.Settings.Default.CodeBookFolder = codeBookFilePath;
                Properties.Settings.Default.Save();
            }
        }

        private void HideCreateCodeBookSettings()
        {
            btnSelectCodeBook.Visibility = System.Windows.Visibility.Visible;
            nudCodeBookLength.Visibility = System.Windows.Visibility.Hidden;
            lblCodeBookLength.Visibility = System.Windows.Visibility.Hidden;
            lblCodeBookMsg.Content = "Selected CodeBook path";
            txtCodeBookPath.Text = "";
        }

        private int GetNumberOfImageFilesInIndexDirectory()
        {
            FileInfo[] imageFiles = getFiles(IndexDirectory, "*.gif|*.jpg|*.png|*.bmp|*.jpeg", SearchOption.TopDirectoryOnly);
            int totalFileCount = imageFiles.Length;
            return totalFileCount;
        }

        private LocateSettings GetLocateSetting()
        {
            LocateSettings rtnSetting = new LocateSettings();
            this.Dispatcher.Invoke(() =>
            {
                rtnSetting.IsCodeBookNeedToBeCreated = chkCreateCodeBook.IsChecked.Value;
                if (rtnSetting.IsCodeBookNeedToBeCreated)
                    UpdateCodeBookName();
                rtnSetting.CodeBookFullPath = txtCodeBookPath.Text;
                rtnSetting.isLteSchemeNeedToAppy = chkLTEScheme.IsChecked.Value;
                rtnSetting.SizeOfCodeBook = nudCodeBookLength.Value.Value;
                rtnSetting.GoodThresholdDistance = nudLocateDistanceThreshold.Value.Value;
                ListBoxItem selectedItem = cmbLocateQueryAlgo.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    string method = selectedItem.Content.ToString();
                    if (method.ToLower() == "extended")
                        rtnSetting.IsExtendedSearch = true;
                    else
                        rtnSetting.IsExtendedSearch = false;
                }
            });
            return rtnSetting;
        }

        private void SaveLocateSetting()
        {
            LocateSettings rtnSetting = new LocateSettings();
            this.Dispatcher.Invoke(() =>
            {
                Properties.Settings.Default.Locate_IsCodeBookNeedToBeCreated
                    = chkCreateCodeBook.IsChecked.Value;
                Properties.Settings.Default.CodeBookFolder = txtCodeBookPath.Text;
                Properties.Settings.Default.Locate_GoodThresholdDistance
                    = nudLocateDistanceThreshold.Value.Value;
                Properties.Settings.Default.Locate_SizeOfCookbook = nudCodeBookLength.Value.Value;
                Properties.Settings.Default.Locate_IsLteSchemeNeedToAppy =
                    chkLTEScheme.IsChecked.Value;
                Properties.Settings.Default.Locate_SelectedMethod = cmbLocateQueryAlgo.SelectedIndex;
                Properties.Settings.Default.Save();
            });
        }
        #endregion




    }
}
