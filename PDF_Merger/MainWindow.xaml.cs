using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;

namespace PDF_Merger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
        List<File_class> AddedPDFs = new List<File_class>(); //All the added PDFs are added here

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
                this.filelist.Items.Add(new File_class  { toMerge = true, file_id = i, file_path = newfile });//Create a new item on the "filelist" list with the correct attributes
                AddedPDFs.Add(new File_class { toMerge = true, file_id = i, file_path = newfile });//Add the new pdf to the array
                i++;//Iterate the id
            }
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
                File.Move(from, endfloc); //Move from .exe path to desired path
                Process.Start(endfloc);
            }


        }

        private void MergePDF(object sender, RoutedEventArgs e)
        {
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


        public class File_class //The class under which we save the files the user chooses
        {
            public bool toMerge { get; set; }

            public int file_id { get; set; }

            public string file_path { get; set; }
        }
    }
}
