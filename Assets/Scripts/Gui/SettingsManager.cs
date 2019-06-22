using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Handles input from settings window. Caching files, model scaling,
    /// displaying buildings, terrain size, color theme and terrain size.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {

        public Toggle ScalingEnabled;
        public Toggle ClearCache;
        public Toggle CacheFiles;
        public Toggle DisplayBuildings;
        public Toggle UseGameUi;
        public Dropdown TerrainSize;
        public Dropdown Theme;


        /// <summary>
        /// Called when settings are displayed. Values are set according to
        /// each respective saved settings.
        /// </summary>
        public void Show()
        {
            ScalingEnabled.isOn = Settings.SCALING_ENABLED;
            ClearCache.isOn = Settings.CLEAR_CACHE_AFTER;
            CacheFiles.isOn = Settings.CACHE_FILES;
            DisplayBuildings.isOn = Settings.DISPLAY_BUILDINGS;
            UseGameUi.isOn = Settings.USE_GAME_UI;
            if (Settings.TERRAIN_DISTANCE_M > 5000.0f)
                TerrainSize.value = 2;
            else if (Settings.TERRAIN_DISTANCE_M > 3000.0f)
                TerrainSize.value = 1;
            else
                TerrainSize.value = 0;

            if (Settings.BACKGROUND_COLOR.EqualsRGB(Constants.COLOR_DARK))
            {
                Theme.value = 0;
            }
            else if(Settings.BACKGROUND_COLOR.EqualsRGB(Constants.COLOR_LIGHT))
            {
                Theme.value = 1;
            }

            gameObject.SetActive(!gameObject.activeInHierarchy);
        }

        /// <summary>
        /// Saves currently selected values in settings window and saves
        /// it to playerprefs. Closes the window.
        /// </summary>
        public void Save()
        {
            Settings.SCALING_ENABLED = ScalingEnabled.isOn;
            Settings.CLEAR_CACHE_AFTER = ClearCache.isOn;
            Settings.CACHE_FILES = CacheFiles.isOn;
            Settings.DISPLAY_BUILDINGS = DisplayBuildings.isOn;
            Settings.USE_GAME_UI = UseGameUi.isOn;
            switch (TerrainSize.value)
            {
                case 0:
                    Settings.TERRAIN_DISTANCE_M = 1500.0f;
                    Settings.DATA_DISTANCE_M = 4500.0f;
                    break;
                case 1:
                    Settings.TERRAIN_DISTANCE_M = 3500.0f;
                    Settings.DATA_DISTANCE_M = 6500.0f;
                    break;
                case 2:
                    Settings.TERRAIN_DISTANCE_M = 5500.0f;
                    Settings.DATA_DISTANCE_M = 8500.0f;
                    break;
                default:
                    Settings.TERRAIN_DISTANCE_M = 1500.0f;
                    Settings.DATA_DISTANCE_M = 4500.0f;
                    break;
            }
            switch (Theme.value)
            {
                case 0: // Dark theme
                    Settings.TEXT_COLOR = Constants.COLOR_LIGHT;
                    Settings.BACKGROUND_COLOR = Constants.COLOR_DARK;
                    break;
                case 1: // Light theme
                    Settings.BACKGROUND_COLOR = Constants.COLOR_LIGHT;
                    Settings.TEXT_COLOR = Constants.COLOR_DARK;
                    break;
                default: // Dark theme
                    Settings.TEXT_COLOR = Constants.COLOR_LIGHT;
                    Settings.BACKGROUND_COLOR = Constants.COLOR_DARK;
                    break;
            }
            foreach (ColorChangableImage cci in FindObjectsOfType<ColorChangableImage>())
                cci.ChangeColor();
            foreach (ColorChangableText cct in FindObjectsOfType<ColorChangableText>())
                cct.ChangeColor();
            foreach (Effects.BlinkOutline bo in FindObjectsOfType<Effects.BlinkOutline>())
            {
                bo.IsDarkTheme = Theme.value != 1;
                bo.ChangeColor();
            }
            Close();
        }

        /// <summary>
        ///  Close the settings window without any change
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Close/save settings by pressng Esc/Enter.
        /// </summary>
        void Update()
        {
            // Enable enter confirm
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Save();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }
        }

    }
}
