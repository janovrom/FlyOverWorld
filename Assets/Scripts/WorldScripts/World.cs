/**
 *  Powered by <a href="https://mapzen.com">Mapzen</a> and
 *  <a href="https://www.mapbox.com/">Mapbox</a>.
    Contains information from ©<a href="https://openstreetmap.org/copyright">OSM</a>, 
    which is made available here under the Open Database License (ODbL).

    Mapbox, Mapzen, © OpenStreetMap contributors, Who’s On First, Natural Earth, and openstreetmapdata.com
 */


using UnityEngine;
using Assets.Scripts.WorldScripts;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// One instance of world.
/// </summary>
public class World : MonoBehaviour {

    // My backup
    //public const string MapzenAPIKey = "?api_key=mapzen-7dy9fLa";
    //public const string GoogleAPIKey = "&key=AIzaSyCdbvGNPqVgEYCNDY3FDtfaTuSh98KuLuE";
    //public static readonly string MapboxAPIKey = "?access_token=pk.eyJ1IjoiamFub3Zyb20iLCJhIjoiY2oyZGVtN3FyMDAzNzMzbzVmdTE3OGZnMCJ9.rBv_qZhMNRLn44Vb0PWGCg";
    // Api keys for access to web services
    public static string MapzenAPIKey;
    public static string GoogleAPIKey;
    public static string MapboxAPIKey;
    public static string NextzenAPIKey;

    // Default size of terrain and image tiles
    public const int TileSize = 256;
    public float MetersPerPixel;
    /// <summary>
    /// Default zoom level.
    /// </summary>
    public int zoom = 15;
    /// <summary>
    /// Starting position of world.
    /// </summary>
    public Vector2 WorldStart;
    /// <summary>
    /// World center in Mercator projection.
    /// </summary>
    public Vector2i WorldCenter;
    // Prefabs
    public Material buildingMaterial;
    public Material TerrainMaterial;
    public GameObject TerrainPrefab;
    public Texture2D MissingTexture;
    private TileCache m_TileCache;
    public string path = "C:\\Users\\roman\\OneDrive\\DP\\Unity\\FlyOverWorld\\dron_logs\\kuklik";
    /// <summary>
    /// Starting latitude and longitude.
    /// </summary>
    public Vector2 latLon = new Vector2(49.6209556586354f, 16.1247969873662f);
    /// <summary>
    /// Current tile position of user.
    /// </summary>
    private Vector2i m_CurrentTilePos = new Vector2i(-100, -100);
    // Front and check list for created tiles.
    private List<Vector2i> m_TileFront = new List<Vector2i>();
    private HashSet<Vector2i> m_UsedInFront = new HashSet<Vector2i>();

    /// <summary>
    /// Loads keys from world.txt. If it doesn't exist, some arbitrary
    /// string is used, which effectively disables downloading.
    /// </summary>
    private void LoadKeys()
    {
        // Load panels
        FileStream f;
        if (!File.Exists(Settings.WORLD_SETTINGS_FILE_PATH))
        {
            MapzenAPIKey = "?api_key=mapzen-xxxxxxx";
            GoogleAPIKey = "&key=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            MapboxAPIKey = "?access_token=your-access-token";
            return;
        }
        else
        {
            f = File.OpenRead(Settings.WORLD_SETTINGS_FILE_PATH);
        }

        StreamReader sr = new StreamReader(f);
        string line = null;
        while ((line = sr.ReadLine()) != null)
        {
            // split on whitespace
            string[] split = line.Split(' ');
            if (split.Length == 2)
            {
                if (split[0].Equals(Constants.GOOGLE_API_KEY_KEY))
                {
                    GoogleAPIKey = split[1];
                }
                else if (split[0].Equals(Constants.MAPZEN_API_KEY_KEY))
                {
                    MapzenAPIKey = split[1];
                }
                else if (split[0].Equals(Constants.MAPBOX_API_KEY_KEY))
                {
                    MapboxAPIKey = split[1];
                }
                else if (split[0].Equals(Constants.NEXTZEN_API_KEY_KEY))
                {
                    NextzenAPIKey = split[1];
                }
            }
        } 
        /// Set some default values
        if (MapzenAPIKey == null)
        {
            MapzenAPIKey = "?api_key=mapzen-xxxxxxx";
        }

        if (GoogleAPIKey == null)
        {
            GoogleAPIKey = "&key=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        }

        if (MapboxAPIKey == null)
        {
            MapboxAPIKey = "?access_token=your-access-token";
        }

        if (NextzenAPIKey == null)
        {
            NextzenAPIKey = "?api_key=mapzen-xxxxxxx";
        }

    }

    /// <summary>
    /// Load gps position from world.txt. Also create initial terrain circle.
    /// Initializes values as resolution for tiles, world center and its position.
    /// </summary>
    void Start () {
        LoadKeys();
        // Get latitude and longitude from file
        FileStream f;
        if (File.Exists(Settings.WORLD_SETTINGS_FILE_PATH))
        {
            f = File.OpenRead(Settings.WORLD_SETTINGS_FILE_PATH);
            StreamReader sr = new StreamReader(f);
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                // split on whitespace
                string[] split = line.Split(' ');
                if (split.Length >= 3)
                {
                    if (split[0].Equals(Constants.GPS_KEY))
                    {
                        float.TryParse(split[1], out latLon.x);
                        float.TryParse(split[2], out latLon.y);
                    }
                }
            }
        }
        // Get TileCache
        m_TileCache = gameObject.AddComponent<TileCache>();
        m_TileCache.Init(this);
        // Get resolution and position
        Vector2 world = GM.Project(latLon, TileSize);
        MetersPerPixel = GM.MetersPerPixel(latLon.x, zoom);
        Debug.Log(MetersPerPixel * TileSize);
        float scale = (float)(1 << zoom) / (float)TileSize;
        // Floor down the coordinates to compute tile position
        WorldCenter = new Vector2i((int)(world.x * scale), (int)(world.y * scale));
        Vector2 tile00LatLon = GM.TileToWorldPos(WorldCenter, zoom);
        WorldStart = GM.Project(tile00LatLon, TileSize) * (1 << zoom) * MetersPerPixel;
        WorldStart.y = -WorldStart.y;
        // Position camera
        FindObjectOfType<Assets.Scripts.Cameras.PickerCamera>().gameObject.transform.position = new Vector3(MetersPerPixel * 128.0f, 0.0f, MetersPerPixel * 128.0f);

        PositionMoved(new Vector3());
    }

    /// <summary>
    /// If tile is in range and wasn't created create the tile.
    /// </summary>
    /// <param name="tilePos">tile position</param>
    public void CreateTile(Vector2i tilePos)
    {
        float x = (m_CurrentTilePos.x - tilePos.x) * MetersPerPixel * TileSize;
        float y = (m_CurrentTilePos.y - tilePos.y) * MetersPerPixel * TileSize;
        y *= y;
        x *= x;
        if (x + y < Settings.TERRAIN_DISTANCE_M * Settings.TERRAIN_DISTANCE_M)
        {
            // Check if wasn't created in this run
            if (!m_UsedInFront.Contains(tilePos))
            {
                m_UsedInFront.Add(tilePos);
                m_TileFront.Add(tilePos);
                m_TileCache.CreateTile(tilePos, zoom);
            }
        }
        
    }

    /// <summary>
    /// Creates tiles around current position in breadth first manner.
    /// </summary>
    private void GenerateTiles()
    {
        // Number of tiles on each side
        int count = Mathf.CeilToInt(Settings.TERRAIN_DISTANCE_M / MetersPerPixel / TileSize);

        m_TileFront.Add(m_CurrentTilePos);
        CreateTile(m_CurrentTilePos);
        while (m_TileFront.Count > 0)
        {
            Vector2i pos = m_TileFront[0];
            m_TileFront.RemoveAt(0);

            Vector2i L = new Vector2i(pos.x - 1, pos.y);
            Vector2i U = new Vector2i(pos.x, pos.y + 1);
            Vector2i R = new Vector2i(pos.x + 1, pos.y);
            Vector2i B = new Vector2i(pos.x, pos.y - 1);
            Vector2i LU = new Vector2i(pos.x - 1, pos.y + 1);
            Vector2i RU = new Vector2i(pos.x + 1, pos.y + 1);
            Vector2i RB = new Vector2i(pos.x + 1, pos.y - 1);
            Vector2i LB = new Vector2i(pos.x - 1, pos.y - 1);

            CreateTile(L);
            CreateTile(U);
            CreateTile(R);
            CreateTile(B);
            CreateTile(LU);
            CreateTile(RU);
            CreateTile(RB);
            CreateTile(LB);
        }

        m_UsedInFront.Clear();

        // clean cache
        m_TileCache.RemoveTerrains(m_CurrentTilePos, Settings.DATA_DISTANCE_M * Settings.DATA_DISTANCE_M, MetersPerPixel * TileSize);
        //m_TileCache.RemoveData(m_CurrentTilePos, Settings.DATA_DISTANCE_M * Settings.DATA_DISTANCE_M, MetersPerPixel * TileSize);
    }

    /// <summary>
    /// Assigns new current position and creates additional tiles around it.
    /// </summary>
    /// <param name="newPosition"></param>
    public void PositionMoved(Vector3 newPosition)
    {
        // Update only if we moved by more than half the terrain size
        Vector2i camPos = GetPositionInMercator(newPosition);
        float x = Math.Abs(m_CurrentTilePos.x - camPos.x) * MetersPerPixel * TileSize;
        float y = Math.Abs(m_CurrentTilePos.y - camPos.y) * MetersPerPixel * TileSize;
        if (camPos != m_CurrentTilePos /*&& x * x + y * y > (Settings.TERRAIN_DISTANCE_M * Settings.TERRAIN_DISTANCE_M) / 4.0f*/)
        {
            // Current position changed, update surroundings
            m_CurrentTilePos = camPos;
            GenerateTiles();
        }

        // Update my information with position above ground
        Ray ray = new Ray();
        ray.origin = new Vector3(newPosition.x, 10000.0f, newPosition.z);
        // Terrain should be below - for Earth at least
        ray.direction = Vector3.down;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
        {
            newPosition.y = newPosition.y - hit.point.y;
        }
        Assets.Scripts.Gui.GuiManager.Instance.PositionInWorldChanged(newPosition, GetGpsFromWorld(newPosition));
    }

    public Vector2 GetGpsFromWorld(Vector3 worldPos)
    {
        Vector2 pos = worldPos.xz();
        pos += WorldStart;
        pos.y = -pos.y + TileSize * MetersPerPixel;
        pos /= MetersPerPixel * (1 << zoom);
        pos = GM.Unproject(pos, TileSize);

        return pos;
    }

    public Vector2i GetPositionInMercator(Vector3 worldPos)
    {
        Vector2 pos = worldPos.xz();
        pos += WorldStart;
        pos.y = -pos.y + TileSize * MetersPerPixel;
        pos /= MetersPerPixel * TileSize;
        Vector2i tilePos = new Vector2i((int) pos.x, (int)pos.y);
        return tilePos;
    }

    public Vector2 GetPositionInWorld(Vector2 latLon)
    {
        Vector2 world = GM.Project(latLon, TileSize) * (1 << zoom) * MetersPerPixel;
        world.y = -world.y + TileSize * MetersPerPixel;
        return world - WorldStart;
    }

    public static float Color2HeightMapzen(Color rgb)
    {
        float height = (rgb.r * 256.0f + rgb.g + rgb.b / 256.0f) * 255.0f - 32768;
        return height / 8900.0f;
    }

}
