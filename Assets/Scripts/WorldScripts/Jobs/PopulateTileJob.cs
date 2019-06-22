using Assets.Scripts.Utility;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldScripts.Jobs
{

    /// <summary>
    /// Downloads one height map, 4 satellite images and vector data for buildings
    /// and for landuse. Download can be saved to file if specified in settings.
    /// It uses WebClient and async task.
    /// </summary>
    class PopulateTileJob : ThreadedJob
    {

        private Vector2i m_Pos;
        private TileData m_Data;
        private int m_Zoom;
        // Paths to cache
        string buildingPath;
        string elePath;
        string texture0Path;
        string texture1Path;
        string texture2Path;
        string texture3Path;
        private TileCache m_Cache;
        // Ids for each respective download
        private const int Elevation = 0;
        private const int Buildings = 1;
        private const int Texture0 = 2;
        private const int Texture1 = 3;
        private const int Texture2 = 4;
        private const int Texture3 = 5;
        // Loaded bytes
        private byte[] m_ElevationBytes;
        private string m_BuildingsString;
        private byte[] m_Texture0Bytes;
        private byte[] m_Texture1Bytes;
        private byte[] m_Texture2Bytes;
        private byte[] m_Texture3Bytes;
        /// <summary>
        /// Number of finished downloads.
        /// </summary>
        int m_Finished = 0;
        /// <summary>
        /// Lock to increase finished count.
        /// </summary>
        object m_Lock = new object();

        public PopulateTileJob(Vector2i tilePos, TileData data, int zoom, TileCache cache)
        {
            m_Pos = tilePos;
            m_Data = data;
            m_Zoom = zoom;
            m_Cache = cache;
        }

        /// <summary>
        /// Certificate validation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }

        /// <summary>
        /// Next download was finished, increment counter in lock.
        /// </summary>
        internal void Next()
        {
            lock (m_Lock)
            {
                ++m_Finished;
            }
        }

        /// <summary>
        /// Returns true iff all 6 downloads are finished.
        /// </summary>
        /// <returns>Returns true iff all 6 downloads are finished.</returns>
        internal bool Finished()
        {
            return m_Finished == 6;
        }

        /// <summary>
        /// Number of finished downloads.
        /// </summary>
        internal int Count
        {
            get
            {
                return m_Finished;
            }
        }

        /// <summary>
        /// Download arguments, which contains id for download task to identify what we are
        /// getting and path to file.
        /// </summary>
        private class DownloadArg
        {

            internal int Id;
            internal string Path;

            internal DownloadArg(int no, string path)
            {
                Id = no;
                Path = path;
            }

        }

        /// <summary>
        /// If file for given path already exists, load it (done on Unity main thread).
        /// If not download in async task.
        /// </summary>
        /// <param name="url">url from which it is downloaded</param>
        /// <param name="path">path to where store the file</param>
        /// <param name="id">id of task</param>
        private void Download(string url, string path, int id)
        {
            // Search for file in cache
            if (!File.Exists(path))
            {
                // Download it
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                WebClient client = new WebClient();
                //client.UseDefaultCredentials = true;
                client.Headers.Add("User-Agent: Other");
                if (Settings.CACHE_FILES)
                {
                    client.DownloadFileAsync(new Uri(@url), path, new DownloadArg(id, path));
                    client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFileCompleted);
                }
                else
                {
                    client.DownloadDataAsync(new Uri(@url), new DownloadArg(id, path));
                    client.DownloadDataCompleted += DownloadCompleted;
                }
            }
            else
            {
                // Load it on Unity thread and mark it as finished
                HandleFileLoading(path, id);
                Next();
            }
        }

        /// <summary>
        /// Loads file from cache. If file doesn't exist, array filled
        /// with zeros is assigned. Assignment is based on given task id.
        /// </summary>
        /// <param name="path">path to cache</param>
        /// <param name="id">id of given task</param>
        private void HandleFileLoading(string path, int id)
        {
            byte[] bytes;
            if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }
            else
            {
                bytes = new byte[256*256*4];
            }
            // Assign to each respective byte array, except building, it's string.
            switch (id)
            {
                case Elevation:
                    m_ElevationBytes = bytes;
                    break;
                case Buildings:
                    if (File.Exists(path))
                    {
                        m_BuildingsString = File.ReadAllText(path);
                    }
                    else
                    {
                        m_BuildingsString = "";
                    }
                    break;
                case Texture0:
                    m_Texture0Bytes = bytes;
                    break;
                case Texture1:
                    m_Texture1Bytes = bytes;
                    break;
                case Texture2:
                    m_Texture2Bytes = bytes;
                    break;
                case Texture3:
                    m_Texture3Bytes = bytes;
                    break;
                default:
                    Debug.LogError("Unrecognized argument for downloading");
                    break;
            }
        }

        /// <summary>
        /// Callback to handle completed download without saving to file.
        /// Assigns output to each respective byte arrays (string for buildings).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DownloadArg as event data</param>
        void DownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                switch (((DownloadArg)e.UserState).Id)
                {
                    case Elevation:
                        m_ElevationBytes = e.Result;
                        break;
                    case Buildings:
                        m_BuildingsString = Encoding.ASCII.GetString(e.Result);
                        break;
                    case Texture0:
                        m_Texture0Bytes = e.Result;
                        break;
                    case Texture1:
                        m_Texture1Bytes = e.Result;
                        break;
                    case Texture2:
                        m_Texture2Bytes = e.Result;
                        break;
                    case Texture3:
                        m_Texture3Bytes = e.Result;
                        break;
                    default:
                        Debug.LogError("Unrecognized argument for downloading");
                        break;
                }
            }
            else
            {
                UnityEngine.Debug.Log(e.Error);
            }
            Next();
        }

        /// <summary>
        /// Callback to handle completed download with saving to file.
        /// Data is then loaded from file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DownloadArg as event data</param>
        void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            if (e.Error == null)
            {
            }
            else
            {
                UnityEngine.Debug.Log(e.Error);
            }
            HandleFileLoading(((DownloadArg)e.UserState).Path, ((DownloadArg)e.UserState).Id);
            Next();
        }

        /// <summary>
        /// No action is done in start. It's immediately marked as finished,
        /// since all tasks are done in unity thread or new async task
        /// is created when needed.
        /// </summary>
        public override void Start()
        {
            
            IsDone = true;
        }

        /// <summary>
        /// Parses height from color. Values from Mapbox heightmap.
        /// Height = -10000 + ((r * 256 * 256 + g * 256 + b) * 0.1).
        /// </summary>
        /// <param name="r">red channel</param>
        /// <param name="g">green channel</param>
        /// <param name="b">blue channel</param>
        /// <returns></returns>
        private float GetHeightFromColor(float r, float g, float b)
        {
            return (float)(-10000 + ((r * 256 * 256 + g * 256 + b) * 0.1));
        }

        // Just for testing
        private static System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        private static int count = 0;
        /// <summary>
        /// On which stage of finishing we currently are.
        /// </summary>
        int step = 0;

        protected override bool OnFinished()
        {
            switch (step)
            {
                case 0:
                    // Initialize paths and start download or immediately load from file
                    if (!s.IsRunning)
                        s.Start();
                    string tileurl = m_Pos.x + "/" + m_Pos.y;
                    string buildingUrl = "https://a.tile.nextzen.org/tilezen/vector/v1/all/" + m_Zoom + "/" + tileurl + ".json" + World.NextzenAPIKey;
                    //string eleUrl = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/" + m_Zoom + "/" + tileurl + ".png" + World.MapzenAPIKey;
                    string eleUrl = "https://api.mapbox.com/v4/mapbox.terrain-rgb/" + m_Zoom + "/" + tileurl + ".pngraw" + World.MapboxAPIKey;
                    // Download 4 textures
                    int x = m_Pos.x * 2;
                    int y = m_Pos.y * 2;

                    string wwwTexture0 = "https://api.mapbox.com/v4/mapbox.satellite/" + (m_Zoom + 1) + "/" + x + "/" + y + ".jpg" + World.MapboxAPIKey;
                    string wwwTexture1 = "https://api.mapbox.com/v4/mapbox.satellite/" + (m_Zoom + 1) + "/" + (x + 1) + "/" + y + ".jpg" + World.MapboxAPIKey;
                    string wwwTexture2 = "https://api.mapbox.com/v4/mapbox.satellite/" + (m_Zoom + 1) + "/" + x + "/" + (y + 1) + ".jpg" + World.MapboxAPIKey;
                    string wwwTexture3 = "https://api.mapbox.com/v4/mapbox.satellite/" + (m_Zoom + 1) + "/" + (x + 1) + "/" + (y + 1) + ".jpg" + World.MapboxAPIKey;

                    //string wwwTexture0 = "https://mt0.google.com/vt/lyrs=s&x=" + x + "&y=" + y + "&z=" + (m_Zoom + 1) + World.GoogleAPIKey;
                    //string wwwTexture1 = "https://mt1.google.com/vt/lyrs=s&x=" + (x + 1) + "&y=" + y + "&z=" + (m_Zoom + 1) + World.GoogleAPIKey;
                    //string wwwTexture2 = "https://mt2.google.com/vt/lyrs=s&x=" + x + "&y=" + (y + 1) + "&z=" + (m_Zoom + 1) + World.GoogleAPIKey;
                    //string wwwTexture3 = "https://mt3.google.com/vt/lyrs=s&x=" + (x + 1) + "&y=" + (y + 1) + "&z=" + (m_Zoom + 1) + World.GoogleAPIKey;

                    buildingPath = Settings.CACHE_PATH + "/landuse_" + m_Pos.x + "_" + m_Pos.y + ".json";
                    elePath = Settings.CACHE_PATH + "/ele_" + m_Pos.x + "_" + m_Pos.y + ".png";
                    texture0Path = Settings.CACHE_PATH + "/texture0_" + m_Pos.x + "_" + m_Pos.y + ".jpg";
                    texture1Path = Settings.CACHE_PATH + "/texture1_" + m_Pos.x + "_" + m_Pos.y + ".jpg";
                    texture2Path = Settings.CACHE_PATH + "/texture2_" + m_Pos.x + "_" + m_Pos.y + ".jpg";
                    texture3Path = Settings.CACHE_PATH + "/texture3_" + m_Pos.x + "_" + m_Pos.y + ".jpg";
                    Download(eleUrl, elePath, Elevation);
                    Download(buildingUrl, buildingPath, Buildings);
                    Download(wwwTexture0, texture0Path, Texture0);
                    Download(wwwTexture1, texture1Path, Texture1);
                    Download(wwwTexture2, texture2Path, Texture2);
                    Download(wwwTexture3, texture3Path, Texture3);

                    ++step;
                    return false;
                case 1:
                    // Wait for finishing of async tasks
                    if (!Finished())
                    {
                        return false;
                    }
                    ++step;
                    return false;
                case 2:
                    // Load heightmap from texture data
                    Texture2D eleTex = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    //Debug.Log("Len=" + m_ElevationBytes.Length);
                    eleTex.LoadImage(m_ElevationBytes);
                    //System.Diagnostics.Stopwatch ss = new System.Diagnostics.Stopwatch();
                    //ss.Start();
                    byte[] rgbData = eleTex.GetRawTextureData();
                    if (rgbData.Length != 256 * 256 * 4)
                        rgbData = new byte[256 * 256 * 4];
                    for (int xx = 0; xx < 256; ++xx)
                    {
                        for (int yy = 0; yy < 256; ++yy)
                        {
                            // a, r, g, b - in raw data it is already inverted
                            float r = rgbData[(xx * 256 + yy) * 4 + 1];
                            float g = rgbData[(xx * 256 + yy) * 4 + 2];
                            float b = rgbData[(xx * 256 + yy) * 4 + 3];
                            float height = GetHeightFromColor(r, g, b);
                            height = Mathf.Max(height, 0.0f);
                            //float height = (r * 256.0f + g + b / 256.0f) - 32768;
                            m_Data.Heightmap[xx, yy] = height / Constants.MAXIMUM_HEIGHT;
                        }
                    }
                    //Debug.Log(ss.ElapsedMilliseconds);
                    // Parse json
                    m_Data.Buildings = new JSONObject(m_BuildingsString);
                    ++step;
                    return false;
                // Load each texture in new frame
                case 3:
                    m_Data.Textures[0] = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    m_Data.Textures[0].LoadImage(m_Texture2Bytes);
                    m_Data.Textures[0].wrapMode = TextureWrapMode.Clamp;
                    ++step;
                    return false;
                case 4:
                    m_Data.Textures[1] = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    m_Data.Textures[1].LoadImage(m_Texture3Bytes);
                    m_Data.Textures[1].wrapMode = TextureWrapMode.Clamp; ++step;
                    return false;
                case 5:
                    m_Data.Textures[2] = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    m_Data.Textures[2].LoadImage(m_Texture0Bytes);
                    m_Data.Textures[2].wrapMode = TextureWrapMode.Clamp; ++step;
                    return false;
                case 6:
                    m_Data.Textures[3] = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    m_Data.Textures[3].LoadImage(m_Texture1Bytes);
                    m_Data.Textures[3].wrapMode = TextureWrapMode.Clamp; ++step;
                    break;

            }
            // Downloading finished, inform cache
            m_Cache.TilePopulated(m_Pos, m_Data);
            s.Stop();
            ++count;
            return true;
        }

        public static void Log()
        {
            Test.Log("Populate=" + s.ElapsedMilliseconds);
            Test.Log("Count=" + count);
        }

    }
}
