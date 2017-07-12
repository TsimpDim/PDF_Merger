using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Threading;

namespace PDF_Merger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
        ObservableCollection<File_class> AddedPDFs = new ObservableCollection<File_class>(); //All the added PDFs are added here
        bool open_file_after_merge, open_dir_after_merge;
        string pdfname, endfloc;

        public MainWindow()
        {
            InitializeComponent();
        }



        private void Explorer_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog(); //Create a new FileDialog
            openDlg.Multiselect = true;//Allow the user to select multiple files
            openDlg.Filter = "PDF Files (*.pdf) |*.pdf"; //Allow only .pdf's
            openDlg.ShowDialog();

            foreach (string newfile in openDlg.FileNames)//For every file he chose
            {
                AddedPDFs.Add(new File_class { toMerge = true, file_id = i, file_path = newfile });//Add the new pdf to the array
                i++;//Iterate the id
            }

            filelist.ItemsSource = AddedPDFs; //Let the list grab the items from the Collection
        }



        private void MergePDF(object sender, RoutedEventArgs e)
        {
            if (filelist.Items.Count <= 0) //If NO PDFs have been added, don't even bother with merging
            {
                System.Windows.MessageBox.Show("Please add the files you want to merge", "Error");
                return;
            }
            if (!AddedPDFs.Any(c => c.toMerge == true)) //If NO PDFS have the toMerge value set to TRUE , don't continue
            {
                System.Windows.MessageBox.Show("No files are set to be included", "Error");
                return;
            }


            SaveFileDialog svFd = new SaveFileDialog();
            svFd.FileName = "Merged.pdf";
            svFd.Filter = "PDF File (*.pdf) |*.pdf";
            svFd.Title = "Save file";
            svFd.DefaultExt = ".pdf";
            Nullable<bool> result = svFd.ShowDialog();


            if (result == true)
            {
                progB.Maximum = AddedPDFs.Count - 1; //Count - 1 for the pdfs
                progBcont.Visibility = Visibility.Visible;

                pdfname = System.IO.Path.GetFileName(svFd.FileName);
                endfloc = svFd.FileName;

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += CreateMergedPdf;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerAsync();

        

            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progB.Value = e.ProgressPercentage;

        }

        private void CreateMergedPdf(object sender, DoWorkEventArgs e)
        {
            using (FileStream stream = new FileStream(pdfname, FileMode.Create))
            {

                Document pdfDoc = new Document(PageSize.A4);
                PdfCopy pdf = new PdfCopy(pdfDoc, stream);
                pdfDoc.Open();

                int i = 0;


                foreach (File_class newpdf in AddedPDFs)
                {

                    (sender as BackgroundWorker).ReportProgress(i++);

                    if (newpdf.toMerge)
                    {

                        PdfReader reader = new PdfReader(newpdf.file_path);

                        pdf.AddDocument(reader);
                        this.Dispatcher.Invoke(() => progBtxt.Text = "Merging file #" + newpdf.file_id + "..."); //Dispatcher.Invoke since UI is on seperate thread

                    }
                }

                if (pdfDoc != null)
                    pdfDoc.Close();

                string from = AppDomain.CurrentDomain.BaseDirectory + @"\" + pdfname;
                
                this.Dispatcher.Invoke(() => progBtxt.Text = "Moving file...");
                if (File.Exists(endfloc))
                {
                    File.Delete(endfloc);
                }
                File.Move(from, endfloc); //Move from .exe path to desired path
                (sender as BackgroundWorker).ReportProgress(i++);

           



            }//End of stream


            if (open_dir_after_merge)
            {
                Process.Start(System.IO.Path.GetDirectoryName(endfloc));
            }

            if (open_file_after_merge)
            {
                Process.Start(endfloc);
            }

            this.Dispatcher.Invoke(() => progBtxt.Text = "Merge complete");
            System.Windows.MessageBox.Show("Merge Complete", "Done!");


        }



        private void ChangeInclusion(object sender, MouseButtonEventArgs e)
        {
            var clicked = filelist.SelectedIndex;

            AddedPDFs[clicked].toMerge = !AddedPDFs[clicked].toMerge; //Change the property to the opposite (False to True and vv)

        }



        //Checkboxes
        //Open file after merge
        private void CheckBox_Checked_file(object sender, RoutedEventArgs e)
        {
            Handler_file(sender as CheckBox);
        }

        private void CheckBox_Unchecked_file(object sender, RoutedEventArgs e)
        {
            Handler_file(sender as CheckBox);
        }

        private void Handler_file(CheckBox checkBox)
        {
            open_file_after_merge = checkBox.IsChecked.Value;
        }
        //Open directory after merge
        private void CheckBox_Checked_dir(object sender, RoutedEventArgs e)
        {
            Handler_dir(sender as CheckBox);
        }

        private void CheckBox_Unchecked_dir(object sender, RoutedEventArgs e)
        {
            Handler_dir(sender as CheckBox);
        }

        private void Handler_dir(CheckBox checkBox)
        {
            open_dir_after_merge = checkBox.IsChecked.Value;
        }



        private void Move_Up(object sender, RoutedEventArgs e)//Move up pdf for insertion
        {
            var selectedIndex = filelist.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = AddedPDFs[selectedIndex];


                /*Make sure the file_id's keep increasing order

                 if (AddedPDFs[selectedIndex - 1].file_id < AddedPDFs[selectedIndex].file_id)
                 {
                     int tmp = itemToMoveUp.file_id;
                     itemToMoveUp.file_id = AddedPDFs[filelist.SelectedIndex - 1].file_id;
                     AddedPDFs[filelist.SelectedIndex - 1].file_id = tmp;
                 }
                 */
                AddedPDFs.RemoveAt(selectedIndex);
                AddedPDFs.Insert(selectedIndex - 1, itemToMoveUp);

                filelist.SelectedIndex = selectedIndex - 1;

            }
        }

        private void Move_Down(object sender, RoutedEventArgs e)//Move down pdf for insertion
        {
            var selectedIndex = filelist.SelectedIndex;

            if (selectedIndex + 1 < AddedPDFs.Count)
            {
                var itemToMoveDown = AddedPDFs[selectedIndex];


                /*Make sure the file_id's keep increasing order
                 
                if (AddedPDFs[selectedIndex].file_id < AddedPDFs[selectedIndex + 1].file_id)
                {
                    int tmp = itemToMoveDown.file_id;
                    itemToMoveDown.file_id = AddedPDFs[filelist.SelectedIndex + 1].file_id;
                    AddedPDFs[filelist.SelectedIndex + 1].file_id = tmp;
                }
                */
                AddedPDFs.RemoveAt(selectedIndex);
                AddedPDFs.Insert(selectedIndex + 1, itemToMoveDown);
                filelist.SelectedIndex = selectedIndex + 1;
            }
        }


        private void Delete_Button(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }

        private void DeleteShortcut(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteFile();
            }
        }



        private void DeleteFile()
        {
            if (AddedPDFs.Count > 0)
            {
                AddedPDFs.RemoveAt(filelist.SelectedIndex);//Remove the item from the list
                filelist.SelectedIndex += 1;//Select the next one
            }
        }




        //FILE CLASS
        public class File_class : INotifyPropertyChanged //The class under which we save the files the user chooses
        {
            private bool _tomerge;
            private int _file_id;

            public bool toMerge
            {
                get { return _tomerge; }
                set
                {
                    if (value != toMerge)
                    {
                        _tomerge = value;
                        NotifyPropertyChanged();
                    }

                }

            }

            public int file_id
            {
                get { return _file_id; }
                set
                {

                    if (value != file_id)
                    {
                        _file_id = value;
                        NotifyPropertyChanged();
                    }
                }

            }


            public string file_path { get; set; }


            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "bool")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

    }
}
