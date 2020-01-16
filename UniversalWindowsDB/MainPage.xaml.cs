using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Isam.Esent.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalWindowsDB
{
    /// <summary>
    /// A basic sample UWP application to make sure that Esent.Collections, Esent.Interop and Isam are
    /// available to UWP applications. Esent.Collections makes use of both the other two, so it serves
    /// as an ideal test bed.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The dictionary that will be persisted
        /// </summary>
        private PersistentDictionary<string, string> universalWindowsDB;

        public MainPage()
        {
            this.InitializeComponent();

            // Persist the dictionary to app's Local storage
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string dbPath = Path.Combine(localFolder.Path, "Dictionary");
            universalWindowsDB = new PersistentDictionary<string, string>(dbPath);
        }

        /// <summary>
        /// If the key and value boxes are filled in, save that as an entry in the dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertKeyValue_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(key.Text) && !string.IsNullOrEmpty(value.Text))
            {
                universalWindowsDB[key.Text] = value.Text;
                key.Text = "Key1";
                value.Text = "Value1";
                universalWindowsDB.Flush();
            }
        }

        /// <summary>
        /// If the key box is filled in, lookup that key in the dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookupKeyValue_Click(object sender, RoutedEventArgs e)
        {
            universalWindowsDB.TryGetValue(keyLookup.Text, out string value);
            if (!string.IsNullOrEmpty(value))
            {
                valueLookup.Text = value;
            }
            else
            {
                valueLookup.Text = "Value not found";
            }
        }
    }
}
