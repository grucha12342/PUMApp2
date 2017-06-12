using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Net;
using System.IO;
using Android.Graphics;
using System.ComponentModel;

namespace App4
{
    [Activity(Label = "App4", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int counter = 0;
        EditText urlTextBox;
        ImageView imageView;
        Button downloadButtonAsync;
        Button cancelDownload;
        ProgressBar progressBar;
        EditText nameText;
        EditText surnameText;
        string url;
        WebClient wc;
        string localPath;
        BitmapFactory.Options options;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            EditText counterText = FindViewById<EditText>(Resource.Id.counterText);
            Button counterButton = FindViewById<Button>(Resource.Id.counterButton);
            urlTextBox = FindViewById<EditText>(Resource.Id.urlText);
            downloadButtonAsync = FindViewById<Button>(Resource.Id.downloadButton);
            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            nameText = FindViewById<EditText>(Resource.Id.nameText);
            surnameText = FindViewById<EditText>(Resource.Id.surnameText);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            cancelDownload = FindViewById<Button>(Resource.Id.buttonCancel);
            cancelDownload.Enabled = false;
            progressBar.Max = 100;
            progressBar.Progress = 0;
            counterButton.Click += (object sender, EventArgs e) =>
            {
                counter++;
                counterText.Text = counter.ToString();
            };
            //if (clickCounter == 0)
            //{
            //    ++clickCounter;
                downloadButtonAsync.Click += new EventHandler(downloadButtonAsync_Click);
                //Console.WriteLine("If download button counter = "+clickCounter);
                
            //} else {
                cancelDownload.Click += new EventHandler(cancelDownload_Click);
                //clickCounter = 0;
                //Console.WriteLine("If cancel button counter = " + clickCounter);
            //}
        }

        private async void downloadButtonAsync_Click(object sender, EventArgs e)
        {
            try {
                progressBar.Progress = 0;
                //downloadButtonAsync.Text = "Anuluj pobieranie";
                wc = new WebClient();
                url = urlTextBox.Text;
                downloadButtonAsync.Enabled = false;
                cancelDownload.Enabled = true;
                Console.WriteLine("Download new file");
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadImageProgress);
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(downloadImageCompleted);
                var bytes = await wc.DownloadDataTaskAsync(url);

                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string localFilename = "downloaded.png";
                localPath = System.IO.Path.Combine(documentsPath, localFilename);
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Close();
                restoreFromDisc(localPath);
          
            } catch (Exception ex) {
                Console.WriteLine("URL Address is empty or invalid");
                downloadButtonAsync.Text = "Pobierz";
                downloadButtonAsync.Enabled = true;
            }
        }

        private async void restoreFromDisc(string fileOnDisc)
        {
            try {
                Console.WriteLine("File of last image");
                options = new BitmapFactory.Options();
                options.InJustDecodeBounds = true;
                await BitmapFactory.DecodeFileAsync(fileOnDisc, options);
                options.InSampleSize = options.OutWidth > options.OutHeight ? options.OutHeight /
                imageView.Height : options.OutWidth / imageView.Width;
                options.InJustDecodeBounds = false;
                Bitmap bitmap = await BitmapFactory.DecodeFileAsync(fileOnDisc, options);
                imageView.SetImageBitmap(bitmap);
            } catch(Exception e) {
                Console.WriteLine("Dividing by 0");
            }
        }
        private void cancelDownload_Click(object sender, EventArgs e) {
            wc.CancelAsync();
            Console.WriteLine("Download aborted");
        }

        private void downloadImageCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                wc.Dispose();
                Console.WriteLine("Download aborted, partial files deleted");
                downloadButtonAsync.Enabled = true;
                cancelDownload.Enabled = false;
            }
            else {
                Console.WriteLine("Download complete");
                downloadButtonAsync.Enabled = true;
                cancelDownload.Enabled = false;
            }
        }

        private void downloadImageProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            int incrementProgress = (int)percentage;
            Console.WriteLine("Start download... "+bytesIn+" / "+totalBytes +" percentage: "+incrementProgress);
            
            progressBar.Progress = incrementProgress;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try {
                if (string.IsNullOrEmpty(localPath)) {
                    Console.WriteLine("No image file to save");
                } else {
                    outState.PutString("pathToImage", localPath);
                }
                outState.PutInt("counter", counter);
                outState.PutString("name", nameText.Text);
                outState.PutString("surname", surnameText.Text);
                outState.PutString("imageURL", urlTextBox.Text);
                base.OnSaveInstanceState(outState);
                Console.WriteLine("State saved");
            } catch (Exception e) {
                Console.WriteLine("Something went wrong");
            }
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            try
            {
                base.OnRestoreInstanceState(savedInstanceState);
                counter = savedInstanceState.GetInt("counter");
                nameText.Text = savedInstanceState.GetString("name");
                surnameText.Text = savedInstanceState.GetString("surname");
                urlTextBox.Text = savedInstanceState.GetString("imageURL");
                localPath = savedInstanceState.GetString("pathToImage");
                if (string.IsNullOrEmpty(localPath))
                {
                    Console.WriteLine("No image file to load");
                }
                else {
                    restoreFromDisc(localPath);
                }
                Console.WriteLine("State loaded");
            } catch (Exception e) {
                Console.WriteLine("Something went wrong");
            }
            
        }
    }
}
