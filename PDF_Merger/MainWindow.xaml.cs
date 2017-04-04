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

namespace PDF_Merger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
        ObservableCollection<File_class> AddedPDFs = new ObservableCollection<File_class>(); //All the added PDFs are added here

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

         void CreateMergedPDF(string pdfname,string endfloc)//ending file location 
        {
            
            using (FileStream stream = new FileStream(pdfname, FileMode.Create)) 
            {

                Document pdfDoc = new Document(PageSize.A4);
                PdfCopy pdf = new PdfCopy(pdfDoc, stream);
                pdfDoc.Open();

                foreach (File_class newpdf in AddedPDFs) { 
                    if(newpdf.toMerge)
                        pdf.AddDocument(new PdfReader(newpdf.file_path));
                }


                if (pdfDoc != null)
                    pdfDoc.Close();

                string from = AppDomain.CurrentDomain.BaseDirectory + @"\" + pdfname;

                if (File.Exists(endfloc))
                {
                    File.Delete(endfloc);
                }
                File.Move(from, endfloc); //Move from .exe path to desired path
                Process.Start(System.IO.Path.GetDirectoryName(endfloc));
            }


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
                CreateMergedPDF(System.IO.Path.GetFileName(svFd.FileName), svFd.FileName); //Send JUST the filename , and the actual path
            }
        }


       

        private void filelist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var clicked = filelist.SelectedIndex;

            AddedPDFs[clicked].toMerge = !AddedPDFs[clicked].toMerge; //Change the property to the opposite (False to True and vv)
  
            
        }


        private void Show_Instructions(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("1)Add the .pdf files you want to merge(Merge happens in the order you add them)\n\n2)Exclude any file you do not want to merge by double-clicking on it \n\n3)Hit Merge!", "Instructions");
        }





        public class File_class : INotifyPropertyChanged //The class under which we save the files the user chooses
        {
            private bool _tomerge;

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

            public int file_id { get; set; }

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
