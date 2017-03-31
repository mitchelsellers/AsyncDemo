using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
/*
 * 
 * Sample Copyright Mitchel Sellers 2013.  www.mitchelsellers.com 
 */
namespace AsyncWebDemos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Helpers
        private List<string> SetUpURLList()
        {
            var urls = new List<string> 
                                { 
                                    "http://msdn.microsoft.com/library/windows/apps/br211380.aspx",
                                    "http://msdn.microsoft.com",
                                    "http://msdn.microsoft.com/en-us/library/hh290136.aspx",
                                    "http://msdn.microsoft.com/en-us/library/ee256749.aspx",
                                    "http://msdn.microsoft.com/en-us/library/hh290138.aspx",
                                    "http://msdn.microsoft.com/en-us/library/hh290140.aspx",
                                    "http://msdn.microsoft.com/en-us/library/dd470362.aspx",
                                    "http://msdn.microsoft.com/en-us/library/aa578028.aspx",
                                    "http://msdn.microsoft.com/en-us/library/ms404677.aspx",
                                    "http://msdn.microsoft.com/en-us/library/ff730837.aspx"
                                };
            return urls;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Sync Code
        /// <summary>
        /// Handles the Click event of the StartSyncButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StartSyncButton_Click(object sender, RoutedEventArgs e)
        {
            var timer = new Stopwatch();
            timer.Start();
            ResultsTextBox.Clear();
            SumPageSizes();
            ResultsTextBox.Text += "Control returned to our click event!";
            timer.Stop();
            ResultsTextBox.Text += "\nExecution Duration: " + timer.Elapsed.ToString();
        }

        private void SumPageSizes()
        {
            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            var total = 0;
            foreach (var url in urlList)
            {
                // GetURLContents returns the contents of url as a byte array. 
                byte[] urlContents = GetURLContents(url);

                DisplayResults(url, urlContents);

                // Update the total.
                total += urlContents.Length;
            }

            // Display the total count for all of the web addresses.
            ResultsTextBox.Text +=
                string.Format("\r\n\r\nTotal bytes returned:  {0}\r\n", total);
        }

        private async Task SumPageSizesAsync()
        {
            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            var total = 0;
            foreach (var url in urlList)
            {
                // GetURLContents returns the contents of url as a byte array. 
               // byte[] urlContents = await GetURLContentsAsync(url);
                var client = new HttpClient() { MaxResponseContentBufferSize = 1000000 };
                byte[] urlContents = await client.GetByteArrayAsync(url);  

                DisplayResults(url, urlContents);

                // Update the total.
                total += urlContents.Length;
            }

            // Display the total count for all of the web addresses.
            ResultsTextBox.Text +=
                string.Format("\r\n\r\nTotal bytes returned:  {0}\r\n", total);
        }

        private async Task SumPageSizesAsyncAwaitAll()
        {
            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            // Create a query. 
            IEnumerable<Task<int>> downloadTasksQuery =
                from url in urlList select ProcessURLAsync(url);

            // Use ToArray to execute the query and start the download tasks.
            Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

            //Wait for them to finish
            int[] lengths = await Task.WhenAll(downloadTasks);
            int total = lengths.Sum();

            // Display the total count for all of the web addresses.
            ResultsTextBox.Text +=
                string.Format("\r\n\r\nTotal bytes returned:  {0}\r\n", total);
        }

        private byte[] GetURLContents(string url)
        {
            // The downloaded resource ends up in the variable named content. 
            var content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL. 
            var webReq = (HttpWebRequest)WebRequest.Create(url);

            // Send the request to the Internet resource and wait for 
            // the response. 
            // Note: you can't use HttpWebRequest.GetResponse in a Windows Store app. 
            using (WebResponse response = webReq.GetResponse())
            {
                // Get the data stream that is associated with the specified URL. 
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Read the bytes in responseStream and copy them to content.  
                    responseStream.CopyTo(content);
                }
            }

            // Return the result as a byte array. 
            return content.ToArray();
        }

        private async Task<Byte[]> GetURLContentsAsync(string url)
        {
            // The downloaded resource ends up in the variable named content. 
            var content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL. 
            var webReq = (HttpWebRequest)WebRequest.Create(url);

            using (WebResponse response = await webReq.GetResponseAsync())
            {
                // Get the data stream that is associated with the specified URL. 
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Read the bytes in responseStream and copy them to content.  
                    await responseStream.CopyToAsync(content);
                }
            }

            // Return the result as a byte array. 
            return content.ToArray();
        }

        private void DisplayResults(string url, byte[] content)
        {
            // Display the length of each website. The string format  
            // is designed to be used with a monospaced font, such as 
            // Lucida Console or Global Monospace. 
            var bytes = content.Length;
            // Strip off the "http://".
            var displayURL = url.Replace("http://", "");
            ResultsTextBox.Text += string.Format("\n{0,-58} {1,8}", displayURL, bytes);
        }

        private async Task<int> ProcessURLAsync(string url)
        {
            var byteArray = await GetURLContentsAsync(url);
            DisplayResults(url, byteArray);
            return byteArray.Length;
        }
        #endregion

        private async void StartAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            var timer = new Stopwatch();
            timer.Start();
            ResultsTextBox.Clear();
            await SumPageSizesAsync();
            ResultsTextBox.Text += "Control returned to our click event!";
            timer.Stop();
            ResultsTextBox.Text += "\nExecution Duration: " + timer.Elapsed.ToString();
        }

        private async void StartAsyncAwaitAllButton_Click(object sender, RoutedEventArgs e)
        {
            var timer = new Stopwatch();
            timer.Start();
            ResultsTextBox.Clear();
            await SumPageSizesAsyncAwaitAll();
            ResultsTextBox.Text += "Control returned to our click event!";
            timer.Stop();
            ResultsTextBox.Text += "\nExecution Duration: " + timer.Elapsed.ToString();
        }

        private async void TriggerLongRunningBackgroundTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Clear();
            var myTask = Task.Run(() => System.Threading.Thread.Sleep(15000));

            //Notify the user
            ResultsTextBox.Text = "Started Task";

            //Now we wait
            await myTask;

            ResultsTextBox.Text += "\nCompleted";
        }

        private void LongRunningProcessWithIndicator(IProgress<string> progress)
        {
            progress.Report("Started");
            System.Threading.Thread.Sleep(5000);
            progress.Report("Waited 5 seconds");
            System.Threading.Thread.Sleep(5000);
            progress.Report("Waited 10 seonds");
            System.Threading.Thread.Sleep(5000);
            progress.Report("Completed");
        }

        private async void TriggerLongRunningBackgroundTaskWithProgressButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Clear();
            var myIndicator = new Progress<string>();
            myIndicator.ProgressChanged += myIndicator_ProgressChanged;
            await Task.Run(() =>  LongRunningProcessWithIndicator(myIndicator));
            ResultsTextBox.Text += "\nBack to caller!";
        }

        void myIndicator_ProgressChanged(object sender, string e)
        {
            ResultsTextBox.Text += "\n" + e;
        }



    }
}
