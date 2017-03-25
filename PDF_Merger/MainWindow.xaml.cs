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

namespace PDF_Merger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
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
                this.filelist.Items.Add(new File { toMerge = true, file_id = i, file_path = newfile });//Create a new item on the "filelist" list with the correct attributes --!! toMerge should be a checkbox
                i++;//Iterate the id
            }
        }
    }

    public class File //The class under which we save the files the user chooses
    {
        public bool toMerge { get; set; }

        public int file_id { get; set; }

        public string file_path  { get; set; }
    }
}
