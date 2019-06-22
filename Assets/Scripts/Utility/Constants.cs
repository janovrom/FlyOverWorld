using UnityEngine;

namespace Assets.Scripts.Utility
{

    /// <summary>
    /// Contains some unchanging constants for simple access and  
    /// easy refactor.
    /// </summary>
    public static class Constants
    {

        // Namings for used layers
        public const string LAYER_TERRAIN = "Terrain";
        public const string LAYER_PICKABLE = "Pickable";
        public const string LAYER_EDITABLE = "Editable";
        public const string LAYER_OUTLINED = "Outlined";
        public const string LAYER_BUILDINGS = "Buildings";

        // Naming for canvas fields to display statistics about drone
        public const string TEXT_KEYS = "Keys";
        public const string TEXT_VALUES = "Values";

        // Naming for buttons for camera swiching (displayed only if drone selected)
        public const string BUTTON_GIMBAL = "Gimbal Camera";
        public const string BUTTON_COMPASS = "Compass Camera";

        // Naming for JSON keys
        public const string JSON_KEY_KIND = "kind";
        public const string JSON_KEY_WALL = "city_wall";
        public const string JSON_KEY_PROP = "properties";
        public const string JSON_KEY_GEO = "geometry";
        public const string JSON_KEY_HEIGHT = "height";
        public const string JSON_KEY_COOR = "coordinates";
        public const string JSON_KEY_POLYGON = "Polygon";
        public const string JSON_KEY_MULTI_POLYGON = "MultiPolygon";
        public const string JSON_KEY_BUILDINGS = "features";
        public const string JSON_KEY_TYPE = "type";

        // Naming for tags
        public const string TAG_NO_FLIGHT_ZONE = "NoFlightZone";
        public const string TAG_UAV = "UAV";
        public const string TAG_GROUND_TARGET = "GroundTarget";
        public const string TAG_SURVEILLANCE_AREA = "SurveillanceArea";
        public const string TAG_BLOCK_MOUSE = "BlockMouse";
        public const string TAG_PLAYER = "Player";

        // Physics costants
        public const float METERS_PER_SEC_2_MILES_PER_HOUR = 3.6f / 1.60934f;
        public const float MILES_PER_HOUR_2_METERS_PER_SEC = 1.0f / METERS_PER_SEC_2_MILES_PER_HOUR;

        // Setting keys for PlayerPrefs
        public const string TERRAIN_DISTANCE = "TerrainDistanceM";
        public const string DATA_DISTANCE = "DataDistanceM";
        public const string SCALING_ENABLED = "ScalingEnabled";
        public const string FLY_PATH_SEGMENT_LIMIT = "FlyPathSegmentLimit";
        public const string CLEAR_CACHE = "ClearCache";
        public const string CACHE_FILES = "CacheFiles";
        public const string DISPLAY_BUILDINGS = "DisplayBuildings";
        public const string USE_GAME_UI = "UseGameUI";
        public const string TEXT_COLOR_R = "TextColorR";
        public const string TEXT_COLOR_G = "TextColorG";
        public const string TEXT_COLOR_B = "TextColorB";
        public const string BACKGROUND_COLOR_R = "BackgroundColorR";
        public const string BACKGROUND_COLOR_G = "BackgroundColorG";
        public const string BACKGROUND_COLOR_B = "BackgroundColorB";

        // String constants - keys for api keys
        public const string GOOGLE_API_KEY_KEY = "GOOGLE_KEY";
        public const string MAPZEN_API_KEY_KEY = "MAPZEN_KEY";
        public const string MAPBOX_API_KEY_KEY = "MAPBOX_KEY";
        public const string NEXTZEN_API_KEY_KEY = "NEXTZEN_KEY";
        public const string GPS_KEY = "GPS";
        public const string IP_KEY = "IP";

        // Some contants used in creation of world
        public const float MAXIMUM_HEIGHT = 10000.0f;

        // Constants for use of naming
        public const string HELI = "Heli";
        public const string PLANE = "Plane";
        public const string ROVER = "Rover";


        // Colors
        public static readonly Color COLOR_RED = new Color(183.0f / 255.0f, 20.0f / 255.0f, 0.0f);
        public static readonly Color COLOR_GREEN = new Color(73.0f / 255.0f, 183.0f / 255.0f, 0.0f);
        public static readonly Color COLOR_BLUE = new Color(17.0f / 255.0f, 81.0f / 255.0f, 135.0f / 255.0f);
        public static readonly Color COLOR_YELLOW = new Color(255.0f / 255.0f, 234.0f / 255.0f, 25.0f / 255.0f);
        public static readonly Color COLOR_YELLOWISH = new Color(1.0f, 213.0f / 255.0f, 62.0f / 255.0f);
        public static readonly Color COLOR_BLAND_GREEN = new Color(62.0f / 255.0f, 1.0f, 103.0f / 255.0f);
        public static readonly Color COLOR_BRIGHT_GREEN = new Color(103.0f / 255.0f, 1.0f, 62.0f / 255.0f);
        public static readonly Color COLOR_PURPLE = new Color(213.0f / 255.0f, 62.0f / 255.0f, 1.0f);
        public static readonly Color COLOR_PINKISH = new Color(1.0f, 62.0f / 255.0f, 103.0f / 255.0f);
        public static readonly Color COLOR_ORANGE = new Color(238.0f / 255.0f, 156.0f / 255.0f, 24.0f / 255.0f);
        public static readonly Color COLOR_DARK = new Color(38.0f / 255.0f, 38.0f / 255.0f, 38.0f / 255.0f, 1.0f);
        public static readonly Color COLOR_LIGHT = new Color(213.0f / 255.0f, 213.0f / 255.0f, 213.0f / 255.0f, 1.0f);
        public static readonly Color COLOR_OUTLINE_DARK = new Color(1.0f, 103.0f / 255.0f, 62.0f / 255.0f, 0.0f);
        public static readonly Color COLOR_OUTLINE_LIGHT = new Color(62.0f / 255.0f, 103.0f / 255.0f, 1.0f, 0.0f);
        public static readonly Color FLY_PATH_COLOR_FUTURE = new Color(0.2f, 0.2f, 0.8f, 0.65f);
        public static readonly Color FLY_PATH_COLOR_PAST = new Color(0.8f, 0.2f, 0.2f, 0.65f);

    }
}
