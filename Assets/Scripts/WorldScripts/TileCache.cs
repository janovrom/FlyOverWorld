using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Utility;
using System.Diagnostics;
using System.IO;
using Assets.Scripts.WorldScripts.Jobs;

namespace Assets.Scripts.WorldScripts
{

    /// <summary>
    /// Structure that holds downloaded tiles and data and tiles requested or being downloaded.
    /// </summary>
    class TileCache : MonoBehaviour
    {

        private int m_RunningCoroutines = 0;
        private byte[] m_MissingElevation = new byte[World.TileSize * World.TileSize * 4];

        /// <summary>
        /// Contains tiles data, which were already used and terrain created.
        /// </summary>
        private Dictionary<Vector2i, WorldTile> m_CreatedTiles = new Dictionary<Vector2i, WorldTile>();

        /// <summary>
        /// Tiles requested for creation. Can be in creation, but not created.
        /// </summary>
        private HashSet<Vector2i> m_RequestedTiles = new HashSet<Vector2i>();

        /// <summary>
        /// Contains data, which were already fully downloaded.
        /// </summary>
        private Dictionary<Vector2i, TileData> m_DownloadedData = new Dictionary<Vector2i, TileData>();

        /// <summary>
        /// Data, which are being downloaded. Not thread safe, thus can be only used in coroutines
        /// and Unity main thread.
        /// </summary>
        private Dictionary<Vector2i, TileData> m_DataBeingDownloaded = new Dictionary<Vector2i, TileData>();

        /// <summary>
        /// Jobs currently waiing or being processed in ThreadPool. If finished ThreadedJob.OnFinished()
        /// is called on Unity main thread.
        /// </summary>
        private List<ThreadedJob> m_WaitingThreadedJob = new List<ThreadedJob>();

        /// <summary>
        /// Contains buildings, that weren't yet processed or weren't able to be positioned on terrain.
        /// </summary>
        private List<Models.BuildingHolder> m_UnpositionedBuildings = new List<Models.BuildingHolder>();
        /// <summary>
        /// Stopwatch to measure time in update, to stop from another task to not make 
        /// processor to work too much.
        /// </summary>
        private Stopwatch m_StopWatch = new Stopwatch();
        private World m_World;
        /// <summary>
        /// First terrain circle loaded.
        /// </summary>
        private bool m_TerrainLoaded = false;
        /// <summary>
        /// First terrain tile loaded.
        /// </summary>
        private bool m_FirstTerrainChunkLoaded = false;

        //public List<Models.BuildingHolder> UnpositionedBuildings
        //{
        //    get
        //    {
        //        return m_UnpositionedBuildings;
        //    }
        //}

        /// <summary>
        /// Initialize missing elevation data to 0.
        /// </summary>
        /// <param name="w"></param>
        internal void Init(World w)
        {
            m_World = w;
            for (int i = 0; i < m_MissingElevation.Length; ++i)
            {
                m_MissingElevation[i] = 0;
            }
        }

        /// <summary>
        /// Called when tile was downloaded in PopulateTileJob. Data
        /// are marked as downloaded.
        /// </summary>
        /// <param name="tilePos">position of tile</param>
        /// <param name="data">data of tile</param>
        public void TilePopulated(Vector2i tilePos, TileData data)
        {
            // We finished downloading, move data to finished ones
            m_DataBeingDownloaded.Remove(tilePos);
            m_DownloadedData.Add(tilePos, data);
        }


        /// <summary>
        /// Coroutine that downloads data using WWW.
        /// </summary>
        /// <param name="tilePos">positon of tile</param>
        /// <param name="zoom">zoom level</param>
        /// <param name="data">data of tile</param>
        /// <returns></returns>
        private IEnumerator PopulateData(Vector2i tilePos, int zoom, TileData data)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            //TileData data = new TileData(tilePos);
            //// Add data to inform, we started downloading
            //m_DataBeingDownloaded.Add(tilePos, data);

            // Get data
            string tileurl = tilePos.x + "/" + tilePos.y;
            string buildingUrl = "https://a.tile.nextzen.org/tilezen/vector/v1/all/" + zoom + "/";
            string eleUrl = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/" + zoom + "/";
            // Define file paths
            string buildingPath = Settings.CACHE_PATH + "/landuse_" + tilePos.x + "_" + tilePos.y;
            string elePath = Settings.CACHE_PATH + "/ele_" + tilePos.x + "_" + tilePos.y;
            string texture1Path = Settings.CACHE_PATH + "/texture1_" + tilePos.x + "_" + tilePos.y;
            string texture0Path = Settings.CACHE_PATH + "/texture0_" + tilePos.x + "_" + tilePos.y;
            string texture2Path = Settings.CACHE_PATH + "/texture2_" + tilePos.x + "_" + tilePos.y;
            string texture3Path = Settings.CACHE_PATH + "/texture3_" + tilePos.x + "_" + tilePos.y;

            // Download elevation data
            byte[] rgbData = null;
            if (File.Exists(elePath))
            {
                rgbData = File.ReadAllBytes(elePath);
            }
            else
            {
                WWW wwwEle = new WWW(eleUrl + tileurl + ".png" + World.MapzenAPIKey);
                yield return wwwEle;
                if (!string.IsNullOrEmpty(wwwEle.error))
                {
                    rgbData = m_MissingElevation;
                }
                else
                {
                    rgbData = wwwEle.texture.GetRawTextureData();
                    File.WriteAllBytes(elePath, rgbData);
                }
                wwwEle.Dispose();
                wwwEle = null;
            }
            //yield return null;
            // Copy texture to float heightmap and inverse
            for (int xx = 0; xx < 256; ++xx)
            {
                for (int yy = 0; yy < 256; ++yy)
                {
                    // a, r, g, b - in raw data it is already inverted
                    float r = rgbData[(xx * 256 + yy) * 4 + 1];
                    float g = rgbData[(xx * 256 + yy) * 4 + 2];
                    float b = rgbData[(xx * 256 + yy) * 4 + 3];
                    float height = (r * 256.0f + g + b / 256.0f) - 32768;
                    data.Heightmap[xx, yy] = height / 8900.0f;
                }
            }
            // We have what we need to create another tiles: heightmap
            // Mark the data as downloaded, though it doesn't mean it is finished
            m_DownloadedData.Add(tilePos, data);

            data.Textures[0] = new Texture2D(256, 256, TextureFormat.RGB24, false);
            data.Textures[1] = new Texture2D(256, 256, TextureFormat.RGB24, false);
            data.Textures[2] = new Texture2D(256, 256, TextureFormat.RGB24, false);
            data.Textures[3] = new Texture2D(256, 256, TextureFormat.RGB24, false);

            // Download buildings
            if (File.Exists(buildingPath))
            {
                data.Buildings = new JSONObject(File.ReadAllText(buildingPath));
            }
            else
            {
                WWW www = new WWW(buildingUrl + tileurl + ".json" + World.NextzenAPIKey);
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    data.Buildings = new JSONObject();
                }
                else
                {
                    data.Buildings = new JSONObject(www.text);
                    File.WriteAllText(buildingPath, www.text);
                }
            }
            //yield return null;
            // Download 4 textures
            int x = tilePos.x * 2;
            int y = tilePos.y * 2;
            string wwwTexture0S = "https://api.mapbox.com/v4/mapbox.satellite/" + (zoom + 1) + "/" + x + "/" + y + ".jpg" + World.MapboxAPIKey;
            string wwwTexture1S = "https://api.mapbox.com/v4/mapbox.satellite/" + (zoom + 1) + "/" + (x + 1) + "/" + y + ".jpg" + World.MapboxAPIKey;
            string wwwTexture2S = "https://api.mapbox.com/v4/mapbox.satellite/" + (zoom + 1) + "/" + x + "/" + (y + 1) + ".jpg" + World.MapboxAPIKey;
            string wwwTexture3S = "https://api.mapbox.com/v4/mapbox.satellite/" + (zoom + 1) + "/" + (x + 1) + "/" + (y + 1) + ".jpg" + World.MapboxAPIKey;

            //string wwwTexture0S = "https://khms0.googleapis.com/kh?v=709&hl=cs&&x=" + x + "&y=" + y + "&z=" + (zoom + 1) + World.GoogleAPIKey;
            //string wwwTexture1S = "https://khms1.googleapis.com/kh?v=709&hl=cs&&x=" + (x + 1) + "&y=" + y + "&z=" + (zoom + 1) + World.GoogleAPIKey;
            //string wwwTexture2S = "https://khms2.googleapis.com/kh?v=709&hl=cs&&x=" + x + "&y=" + (y + 1) + "&z=" + (zoom + 1) + World.GoogleAPIKey;
            //string wwwTexture3S = "https://khms3.googleapis.com/kh?v=709&hl=cs&&x=" + (x + 1) + "&y=" + (y + 1) + "&z=" + (zoom + 1) + World.GoogleAPIKey;

            byte[] data0, data1, data2, data3;
            // Tex0
            if (File.Exists(texture0Path))
            {
                data0 = File.ReadAllBytes(texture0Path);
            }
            else
            {
                WWW wwwTexture0 = new WWW(wwwTexture0S);
                yield return wwwTexture0;
                if (!string.IsNullOrEmpty(wwwTexture0.error))
                {
                    data0 = m_World.MissingTexture.GetRawTextureData();
                }
                else
                {
                    data0 = wwwTexture0.texture.GetRawTextureData();
                    File.WriteAllBytes(texture0Path, data0);
                }
                wwwTexture0.Dispose();
                wwwTexture0 = null;
            }
            //yield return null;
            // Tex1
            if (File.Exists(texture1Path))
            {
                data1 = File.ReadAllBytes(texture1Path);
            }
            else
            {
                WWW wwwTexture1 = new WWW(wwwTexture1S);
                yield return wwwTexture1;
                if (!string.IsNullOrEmpty(wwwTexture1.error))
                {
                    data1 = m_World.MissingTexture.GetRawTextureData();
                }
                else
                {
                    data1 = wwwTexture1.texture.GetRawTextureData();
                    File.WriteAllBytes(texture1Path, data1);
                }
                wwwTexture1.Dispose();
                wwwTexture1 = null;
            }
            //yield return null;
            // Tex2
            if (File.Exists(texture2Path))
            {
                data2 = File.ReadAllBytes(texture2Path);
            }
            else
            {
                WWW wwwTexture2 = new WWW(wwwTexture2S);
                yield return wwwTexture2;
                if (!string.IsNullOrEmpty(wwwTexture2.error))
                {
                    data2 = m_World.MissingTexture.GetRawTextureData();
                }
                else
                {
                    data2 = wwwTexture2.texture.GetRawTextureData();
                    File.WriteAllBytes(texture2Path, data2);
                }
                wwwTexture2.Dispose();
                wwwTexture2 = null;
            }
            //yield return null;
            // Tex3
            if (File.Exists(texture3Path))
            {
                data3 = File.ReadAllBytes(texture3Path);
            }
            else
            {
                WWW wwwTexture3 = new WWW(wwwTexture3S);
                yield return wwwTexture3;
                if (!string.IsNullOrEmpty(wwwTexture3.error))
                {
                    data3 = m_World.MissingTexture.GetRawTextureData();
                }
                else
                {
                    data3 = wwwTexture3.texture.GetRawTextureData();
                    File.WriteAllBytes(texture3Path, data3);
                }
                wwwTexture3.Dispose();
                wwwTexture3 = null;
            }
            //yield return null;

            data.Textures[0].LoadRawTextureData(data2);
            data.Textures[1].LoadRawTextureData(data3);
            data.Textures[2].LoadRawTextureData(data0);
            data.Textures[3].LoadRawTextureData(data1);

            // We finished downloading, move data to finished ones
            m_DataBeingDownloaded.Remove(tilePos);
            //Populate another data, if any
            if (m_DataBeingDownloaded.Count > 0)
            {
                KeyValuePair<Vector2i, TileData> first = m_DataBeingDownloaded.ElementAt(0);
                Vector2i pos = first.Key;
                //m_Tiles.RemoveAt(0);
                StartCoroutine(PopulateData(pos, zoom, first.Value));
            }
            m_RunningCoroutines--;
            Test.Log("Populate=" + s.ElapsedMilliseconds);
        }

        /// <summary>
        /// Wait until all nine tile datas are available.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator WaitForData(Vector2i tilePos, System.Action<bool> callback)
        {
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    Vector2i pos = new Vector2i(tilePos.x + i, tilePos.y + j);
                    while (!m_DownloadedData.ContainsKey(pos))
                    {
                        // wait until download is finished
                        yield return null;
                    }
                }
            }

            while (m_DataBeingDownloaded.ContainsKey(tilePos))
                yield return null;

            bool success = true;
            // Check again if contains all required data (could be deleted), if not, report as failure
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    Vector2i pos = new Vector2i(tilePos.x + i, tilePos.y + j);
                    success &= m_DownloadedData.ContainsKey(pos);
                }
            }
            // Check if tile was deleted based on distance
            success &= m_RequestedTiles.Contains(tilePos);
            // Check if tile was not created.
            success &= !m_CreatedTiles.ContainsKey(tilePos);

            // report success
            callback(success);
        }

        /// <summary>
        /// If tile is not being downloaded or already created, 9 surrounding tiles
        /// are requested and tile is marked for creation. Otherwise returns false.
        /// That tile is marked for creation doesn't necessarily means, it will
        /// be created.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="zoom"></param>
        /// <returns>Returns true if tile is marked for creation.</returns>
        public bool CreateTile(Vector2i tilePos, int zoom)
        {
            // If tile already created or being created, don't request it again.
            if (!CanCreate(tilePos))
                return false;
            else
                m_RequestedTiles.Add(tilePos);

            // Request all data around my tile
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    Vector2i pos = new Vector2i(tilePos.x + i, tilePos.y + j);
                    // Each request will stop sometime, we do not care when
                    RequestTileData(pos, zoom);
                }
            }

            m_RunningCoroutines++;
            // Wait to download all data (if not already done so)
            StartCoroutine(WaitForData(tilePos, (success) =>
            {

                if (success)
                {
                    // We have all data
                    TileData data;
                    ///*
                    // * Only two cases can happen:
                    // * 1. Tile data was destroyed in another coroutine.
                    // * 2. Tile has successfully downloaded data.
                    // */
                    if (!m_DownloadedData.TryGetValue(tilePos, out data))
                    {
                        m_RequestedTiles.Remove(tilePos);
                        return;
                    }

                    // Create the TILE!
                    List<float[,]> hms = new List<float[,]>();
                    for (int i = -1; i <= 1; ++i)
                    {
                        for (int j = -1; j <= 1; ++j)
                        {
                            TileData d;
                            Vector2i pos = new Vector2i(tilePos.x + i, tilePos.y - j);
                            m_DownloadedData.TryGetValue(pos, out d);

                            // Get all the data
                            hms.Add(d.Heightmap);
                        }
                    }
                    // All data successfully acquired
                    // But tile could be already destroyed
                    if (m_RequestedTiles.Contains(tilePos))
                    {
                        // Do we still want to create the tile?
                        //WorldTile tile = GameObject.Instantiate(m_World.TilePrefab);
                        //tile.name = "tile" + tilePos.x + "" + tilePos.y;
                        GameObject go = new GameObject("tile" + tilePos.x + "" + tilePos.y);
                        WorldTile tile = go.AddComponent<WorldTile>();
                        tile.transform.parent = m_World.transform;
                        tile.Data = data;
                        m_CreatedTiles.Add(tilePos, tile);
                        m_RequestedTiles.Remove(tilePos);
                        TileCreationJob job = new TileCreationJob(this, tile, m_World, hms);
                        StartJob(job);
                    }
                } // else: Some data were deleted. Don't care. If we are close enough. Another request will be done.
                m_RunningCoroutines--;
            }));

            return true;
        }

        /// <summary>
        /// Removes terrain and terrain data that are beyond some distance+little offset to
        /// prevent deletion of tiles which are on border of terrain.
        /// </summary>
        /// <param name="currentTilePos">current position in world</param>
        /// <param name="terrainDistanceSq">squared border distance</param>
        /// <param name="unitDistance">distance for one tile</param>
        internal void RemoveTerrains(Vector2i currentTilePos, float terrainDistanceSq, float unitDistance)
        {
            List<Vector2i> toRemove = new List<Vector2i>();
            // Find tiles to remove in requested tiles
            foreach (Vector2i keyPos in m_RequestedTiles)
            {
                float x = (keyPos.x - currentTilePos.x) * unitDistance;
                float y = (keyPos.y - currentTilePos.y) * unitDistance;
                y *= y;
                x *= x;
                // 1000 is for filler, since we create in circle, so tiles that were created are not deleted
                if (x + y > terrainDistanceSq + 1000.0f)
                {
                    
                    toRemove.Add(keyPos);
                }
            }

            // Remove tiles
            foreach (Vector2i keyPos in toRemove)
            {
                m_RequestedTiles.Remove(keyPos);
            }
            toRemove.Clear();

            // Find tiles to remove in created tiles
            foreach (Vector2i keyPos in m_CreatedTiles.Keys)
            {
                float x = (keyPos.x - currentTilePos.x) * unitDistance;
                float y = (keyPos.y - currentTilePos.y) * unitDistance;
                y *= y;
                x *= x;
                if (x + y > terrainDistanceSq + 1000.0f)
                {

                    toRemove.Add(keyPos);
                }
            }

            // Remove tiles
            foreach (Vector2i keyPos in toRemove)
            {
                WorldTile tile;
                // Tile could be only partly created.
                if (m_CreatedTiles.TryGetValue(keyPos, out tile))
                {
                    tile.Clean();
                    m_CreatedTiles.Remove(keyPos);
                    m_DownloadedData.Remove(keyPos);
                }
            }

        }

        /// <summary>
        /// Starts threaded job and adds it to end of waiting list.
        /// </summary>
        /// <param name="job">job to start</param>
        public void StartJob(ThreadedJob job)
        {
            job.Start();
            //StartCoroutine(job.WaitFor());
            m_WaitingThreadedJob.Add(job);
        }

        /// <summary>
        /// Takes waiting jobs and checks if they are finished. It stops if 
        /// limit of 10 jobs is reached or it took more than 15 milliseconds.
        /// </summary>
        void Update()
        {
            m_StopWatch.Reset();
            m_StopWatch.Start();
            for (int i = 0; i < Math.Min(m_WaitingThreadedJob.Count, 10); ++i)
            {
                // Check if thread is finished, if so, do final touches on Unity main thread
                if (m_WaitingThreadedJob[i].Update())
                {
                    // First chunk was not yet loaded
                    if (!m_FirstTerrainChunkLoaded && m_WaitingThreadedJob[i] is TileCreationJob)
                    {
                        Gui.GuiManager.Instance.FirstChunkLoaded();
                        m_FirstTerrainChunkLoaded = true;
                        //UnityEditor.EditorApplication.isPaused = true;
                    }
                    m_WaitingThreadedJob.RemoveAt(i);
                    --i;
                }

                if (m_StopWatch.ElapsedMilliseconds > 15)
                {
                    m_StopWatch.Stop();
                    break;
                }
            }

            // If at least one tile created, no work in progress and no tile requested, we finished terrain generation
            if (m_RequestedTiles.Count == 0 && m_WaitingThreadedJob.Count == 0 && m_CreatedTiles.Count > 0)
            {

                if (!m_TerrainLoaded)
                {
                    Gui.GuiManager.Instance.LoadingTerrainFinished();
                    if (Settings.TEST_LOG)
                        PopulateTileJob.Log();
                    m_TerrainLoaded = true;
                }
            }
        }

        /// <summary>
        /// If tile can be created - if not requested and neither created.
        /// </summary>
        /// <param name="tilePos">position of tile</param>
        /// <returns>Returns true iff tile can be created.</returns>
        public bool CanCreate(Vector2i tilePos)
        {
            return !m_RequestedTiles.Contains(tilePos) && !m_CreatedTiles.ContainsKey(tilePos);
        }

        /// <summary>
        /// If data for tile not downloaded or not already downloading,
        /// download them from web in coroutine or in PopulateTileJob.
        /// Has to be called from Unity main thread.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="zoom"></param>
        public void RequestTileData(Vector2i tilePos, int zoom)
        {
            //Debug.Log("Requested data for tile: " + tilePos);
            bool downloading = m_DataBeingDownloaded.ContainsKey(tilePos);
            bool downloaded = m_DownloadedData.ContainsKey(tilePos);
            if (!downloaded && !downloading)
            {
                // We don't have any data requested yet, download them
                TileData data = new TileData(tilePos);
                // Add data to inform, we started downloading
                m_DataBeingDownloaded.Add(tilePos, data);
                if (Settings.USE_COROUTINE_DOWNLOAD)
                {
                    if (m_DataBeingDownloaded.Count == 1)
                    {
                        m_RunningCoroutines++;
                        StartCoroutine(PopulateData(tilePos, zoom, data));
                    }
                }
                else
                {
                    StartJob(new PopulateTileJob(tilePos, data, zoom, this));
                }
            }
        }

        /// <summary>
        /// If set so, clears all cached files after exiting program.
        /// </summary>
        void OnApplicationQuit()
        {
            if (Settings.CLEAR_CACHE_AFTER)
            {
                if (Directory.Exists(Settings.CACHE_PATH))
                {
                    foreach (string path in Directory.GetFiles(Settings.CACHE_PATH))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

    }
}
