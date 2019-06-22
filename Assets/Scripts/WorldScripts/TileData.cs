using UnityEngine;
using Assets.Scripts.Utility;

namespace Assets.Scripts.WorldScripts
{

    /// <summary>
    /// Holds data for each tile. Heightmap, buildings in json,
    /// tile position and 4 textures.
    /// </summary>
    public class TileData
    {

        private float[,] m_Heightmap = new float[256, 256];
        private Vector2i m_TilePos;
        private Texture2D[] m_Textures = new Texture2D[4];
        private JSONObject m_Buildings;

        public TileData(Vector2i pos)
        {
            m_TilePos = pos;
        }

        public JSONObject Buildings
        {
            set; get;
        }

        public Texture2D[] Textures
        {
            set
            {
                if (value == null)
                    m_Textures = new Texture2D[4];

                m_Textures[0] = value[0];
                m_Textures[1] = value[1];
                m_Textures[2] = value[2];
                m_Textures[3] = value[3];
            }

            get
            {
                return m_Textures;
            }
        }

        public float[,] Heightmap
        {
            get
            {
                return m_Heightmap;
            }
        }

        public Vector2i Position
        {
            set
            {
                m_TilePos = value;
            }

            get
            {
                return m_TilePos;
            }
        }

    }
}
