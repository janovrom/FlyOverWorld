using System.IO;
using UnityEngine;

namespace Assets.Scripts.Utility
{

    /// <summary>
    /// Nice class for user settings or settings changeable by some configuration
    /// or through command line.
    /// </summary>
    public class Settings : MonoBehaviour
    {
        // Unchangeable settings
#if UNITY_ANDROID
        public static string GUI_PANELS_FILE_PATH;
        public static string WORLD_SETTINGS_FILE_PATH;
        public static string CACHE_PATH;
#else
        public static readonly string GUI_PANELS_FILE_PATH = "panels.txt";
        public static readonly string WORLD_SETTINGS_FILE_PATH = "world.txt";
        public static readonly string CACHE_PATH = Path.GetFullPath(".") + "/Cache";
#endif
        public static readonly float FLY_PATH_WIDTH = 0.5f;
        public static readonly float FLY_PATH_HEIGHT = 0.1f;
        public static readonly float BILLBOARD_LABEL_LINE_START_WIDTH = 8.0f;
        public static readonly float BILLBOARD_LABEL_LINE_END_WIDTH = 1.5f;
        public static readonly float TIME_ERROR = 15.0f;
        public static readonly float TIME_WARNING = 10.0f;
        public static readonly float TIME_MESSAGE = 5.0f;
        public static readonly float TIME_UNDEFINED = float.PositiveInfinity;
        public static readonly bool SCALING_MIN_MAX_SIZE = true;
        public static readonly float CREATION_ALTITUDE_SENSITIVITY = 80.0f;
        public static readonly float CREATION_RESIZE_SENSITIVITY = 30.0f;
        public static readonly int SUBDIVISION_DEPTH = 1;

        // User definable settings
        public static float TERRAIN_DISTANCE_M
        {
            get
            {
                return m_TerrainDistanceM;
            }

            set
            {
                PlayerPrefs.SetFloat(Constants.TERRAIN_DISTANCE, value);
                m_TerrainDistanceM = value;
            }
        }

        public static float DATA_DISTANCE_M
        {
            get
            {
                return m_DataDistanceM;
            }

            set
            {
                PlayerPrefs.SetFloat(Constants.DATA_DISTANCE, value);
                m_DataDistanceM = value;
            }
        }

        public static bool SCALING_ENABLED
        {
            get
            {
                return m_ScalingEnabled;
            }

            set
            {
                PlayerPrefs.SetInt(Constants.SCALING_ENABLED, value ? 1 : 0);
                m_ScalingEnabled = value; 
            }
        }

        public static Color TEXT_COLOR
        {
            get
            {
                return m_TextColor;
            }

            set
            {
                m_TextColor = value;
                PlayerPrefs.SetFloat(Constants.TEXT_COLOR_R, m_TextColor.r);
                PlayerPrefs.SetFloat(Constants.TEXT_COLOR_G, m_TextColor.g);
                PlayerPrefs.SetFloat(Constants.TEXT_COLOR_B, m_TextColor.b);
            }
        }

        public static Color BACKGROUND_COLOR
        {
            get
            {
                return m_BackgroundColor;
            }

            set
            {
                m_BackgroundColor = value;
                PlayerPrefs.SetFloat(Constants.BACKGROUND_COLOR_R, m_BackgroundColor.r);
                PlayerPrefs.SetFloat(Constants.BACKGROUND_COLOR_G, m_BackgroundColor.g);
                PlayerPrefs.SetFloat(Constants.BACKGROUND_COLOR_B, m_BackgroundColor.b);
            }
        }

        public static int FLY_PATH_SEGMENT_LIMIT
        {
            get
            {
                return m_FlyPathSegmentLimit;
            }

            set
            {
                PlayerPrefs.SetInt(Constants.FLY_PATH_SEGMENT_LIMIT, value);
                m_FlyPathSegmentLimit = value;
            }
        }

        public static bool CLEAR_CACHE_AFTER
        {
            get
            {
                return m_ClearCache;
            }

            set
            {
                PlayerPrefs.SetInt(Constants.CLEAR_CACHE, value ? 1 : 0);
                m_ClearCache = value;
            }
        }

        public static bool CACHE_FILES
        {
            get
            {
#if UNITY_ANDROID
                return false;
#else
                return m_CacheFiles;
#endif
            }

            set
            {
                PlayerPrefs.SetInt(Constants.CACHE_FILES, value ? 1 : 0);
                m_CacheFiles = value;
            }
        }
        public static bool DISPLAY_BUILDINGS
        {
            get
            {
                return m_DisplayBuildings;
            }

            set
            {
                PlayerPrefs.SetInt(Constants.DISPLAY_BUILDINGS, value ? 1 : 0);
                m_DisplayBuildings = value;
                if (m_DisplayBuildings)
                {
                    Camera.main.cullingMask = Camera.main.cullingMask | LayerMask.GetMask(Constants.LAYER_BUILDINGS);
                }
                else
                {
                    Camera.main.cullingMask = Camera.main.cullingMask & (~LayerMask.GetMask(Constants.LAYER_BUILDINGS));
                }
            }
        }
        public static bool USE_GAME_UI
        {
            get
            {
                return m_UseGameUI;
            }

            set
            {
                PlayerPrefs.SetInt(Constants.USE_GAME_UI, value ? 1 : 0);
                m_UseGameUI = value;
                Gui.GuiManager.Instance.UseGameUI(m_UseGameUI);
            }
        }

        // Default values for player settings
        private static float m_TerrainDistanceM = 3000.0f;
        private static float m_DataDistanceM = 5000.0f;
        private static bool m_ScalingEnabled = true;
        private static int m_FlyPathSegmentLimit = 100;
        private static bool m_ClearCache = true;
        private static bool m_CacheFiles = true;
        private static bool m_DisplayBuildings = true;
        private static bool m_UseGameUI = false;
        private static Color m_TextColor = Constants.COLOR_LIGHT;
        private static Color m_BackgroundColor = Constants.COLOR_DARK;

        // Settings that can be changed using command line arguments
        public static bool TEST_LOG;
        public static bool USE_UNITY_TERRAIN;
        public static bool USE_COROUTINE_DOWNLOAD;
        public static int TEST_MESH_SIZE;

        void Awake()
        {
#if UNITY_ANDROID
            // Add paths for android, for windows they already exist
            GUI_PANELS_FILE_PATH = Application.persistentDataPath + "/panels.txt";
            WORLD_SETTINGS_FILE_PATH = Application.persistentDataPath + "/world.txt";
            CACHE_PATH = Application.persistentDataPath + "/Cache";
#endif

            if (!Directory.Exists(CACHE_PATH))
                Directory.CreateDirectory(CACHE_PATH);

            TERRAIN_DISTANCE_M = PlayerPrefs.GetFloat(Constants.TERRAIN_DISTANCE, m_TerrainDistanceM);
            DATA_DISTANCE_M = PlayerPrefs.GetFloat(Constants.DATA_DISTANCE, m_DataDistanceM);
            SCALING_ENABLED = PlayerPrefs.GetInt(Constants.SCALING_ENABLED, (m_ScalingEnabled ? 1:0)) == 1;
            FLY_PATH_SEGMENT_LIMIT = PlayerPrefs.GetInt(Constants.FLY_PATH_SEGMENT_LIMIT, m_FlyPathSegmentLimit);
            CLEAR_CACHE_AFTER = PlayerPrefs.GetInt(Constants.CLEAR_CACHE, m_ClearCache ? 1 : 0) == 1;
            CACHE_FILES = PlayerPrefs.GetInt(Constants.CACHE_FILES, m_CacheFiles ? 1 : 0) == 1;
            DISPLAY_BUILDINGS = PlayerPrefs.GetInt(Constants.DISPLAY_BUILDINGS, m_DisplayBuildings ? 1 : 0) == 1;
            USE_GAME_UI = PlayerPrefs.GetInt(Constants.USE_GAME_UI, m_UseGameUI ? 1 : 0) == 1;
            float r = PlayerPrefs.GetFloat(Constants.TEXT_COLOR_R, m_TextColor.r);
            float g = PlayerPrefs.GetFloat(Constants.TEXT_COLOR_G, m_TextColor.g);
            float b = PlayerPrefs.GetFloat(Constants.TEXT_COLOR_B, m_TextColor.b);
            TEXT_COLOR = new Color(r, g, b, 1.0f);
            r = PlayerPrefs.GetFloat(Constants.BACKGROUND_COLOR_R, m_BackgroundColor.r);
            g = PlayerPrefs.GetFloat(Constants.BACKGROUND_COLOR_G, m_BackgroundColor.g);
            b = PlayerPrefs.GetFloat(Constants.BACKGROUND_COLOR_B, m_BackgroundColor.b);
            BACKGROUND_COLOR = new Color(r, g, b, 1.0f);
            TEST_MESH_SIZE = 17;
            TEST_LOG = false;
            USE_UNITY_TERRAIN = false;
            USE_COROUTINE_DOWNLOAD = false;
#if UNITY_ANDROID
            // Choose medium terrain and disable caching
            Settings.TERRAIN_DISTANCE_M = 1500.0f;
            Settings.DATA_DISTANCE_M = 4500.0f;
            CLEAR_CACHE_AFTER = false;
            CACHE_FILES = false;
            return;
#endif

            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "+test")
                {
                    TEST_LOG = true;
                }
                else if (args[i] == "+mesh-size")
                {
                    int.TryParse(args[i + 1], out TEST_MESH_SIZE);
                }
                else if (args[i] == "+unity-terrain")
                {
                    USE_UNITY_TERRAIN = true;
                }
                else if (args[i] == "+unity-coroutine")
                {
                    USE_COROUTINE_DOWNLOAD = true;
                }
                else if (args[i] == "+terrain-small")
                {
                    Settings.TERRAIN_DISTANCE_M = 1500.0f;
                    Settings.DATA_DISTANCE_M = 4500.0f;
                }
                else if (args[i] == "+terrain-medium")
                {
                    Settings.TERRAIN_DISTANCE_M = 3500.0f;
                    Settings.DATA_DISTANCE_M = 6500.0f;
                }
                else if (args[i] == "+terrain-large")
                {
                    Settings.TERRAIN_DISTANCE_M = 5500.0f;
                    Settings.DATA_DISTANCE_M = 8500.0f;
                }
            }
            if (TEST_LOG)
            {
                string logName = "";
                if (USE_UNITY_TERRAIN)
                {
                    logName += ("-terrain");
                }
                else
                {
                    logName += ("-mesh-size"+ TEST_MESH_SIZE.ToString());
                }

                if (USE_COROUTINE_DOWNLOAD)
                {
                    logName += ("-coroutine");
                }
                else
                {
                    logName += ("-tilejob");
                }

                if (Settings.TERRAIN_DISTANCE_M > 5000.0f)
                    logName += "-big";
                else if (Settings.TERRAIN_DISTANCE_M > 3000.0f)
                    logName += "-medium";
                else
                    logName += "-small";

                Test.StartLog(logName);
            }
        }

    }
}
