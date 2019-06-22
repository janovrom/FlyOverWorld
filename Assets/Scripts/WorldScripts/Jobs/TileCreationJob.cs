using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.WorldScripts.Jobs
{

    /// <summary>
    /// Creates tile from given list of 9 heightmaps. Interpolates values
    /// on border.
    /// </summary>
    class TileCreationJob : ThreadedJob
    {

        private TileCache m_TileCache;
        private WorldTile m_Tile;
        private World m_World;
        /// <summary>
        /// Offset to move each terrain tile.
        /// </summary>
        private Vector2i[] m_Offsets = new Vector2i[4];
        /// <summary>
        /// List of 9 heightmaps.
        /// </summary>
        List<float[,]> m_Heightmaps;
        private int m_TileCreatedIdx = 0;


        public TileCreationJob(TileCache tileCache, WorldTile tile, World w, List<float[,]> hms)
        {
            m_TileCache = tileCache;
            m_Tile = tile;
            m_World = w;
            m_Heightmaps = hms;
        }

        /// <summary>
        /// Interpolate values on borders for center tile.
        /// </summary>
        protected override void ThreadFunction()
        {
            //Debug.Log("Generating values in thread");
            Vector2i offset = new Vector2i(m_Tile.Data.Position.x, m_Tile.Data.Position.y);
            offset.x -= m_World.WorldCenter.x;
            offset.y -= m_World.WorldCenter.y;

            float[,] hm = new float[257, 257];
            // bottom-left corner
            hm[0, 0] = MathExt.Bilerp(m_Heightmaps[0][255,255], m_Heightmaps[1][0,255], m_Heightmaps[3][255,0], m_Heightmaps[4][0,0], 0.5f, 0.5f);
            // bottom-right corner
            hm[256, 0] = MathExt.Bilerp(m_Heightmaps[1][255,255], m_Heightmaps[2][0,255], m_Heightmaps[4][255,0], m_Heightmaps[5][0,0], 0.5f, 0.5f);
            // top-left corner
            hm[0, 256] = MathExt.Bilerp(m_Heightmaps[3][255,255], m_Heightmaps[4][0,255], m_Heightmaps[6][255,0], m_Heightmaps[7][0,0], 0.5f, 0.5f);
            // top-right corner
            hm[256, 256] = MathExt.Bilerp(m_Heightmaps[4][255,255], m_Heightmaps[5][0,255], m_Heightmaps[7][255,0], m_Heightmaps[8][0,0], 0.5f, 0.5f);

            // fill sides
            for (int i = 0; i < 255; ++i)
            {
                // bottom line
                hm[i + 1, 0] = MathExt.Bilerp(m_Heightmaps[1][i,255], m_Heightmaps[1][i+1,255], m_Heightmaps[4][i,0], m_Heightmaps[4][i+1,0], 0.5f, 0.5f);
                // top line
                hm[i + 1, 256] = MathExt.Bilerp(m_Heightmaps[4][i,255], m_Heightmaps[4][i+1,255], m_Heightmaps[7][i,0], m_Heightmaps[7][i+1,0], 0.5f, 0.5f);
                // left line
                hm[0, i + 1] = MathExt.Bilerp(m_Heightmaps[3][255,i], m_Heightmaps[3][255,i+1], m_Heightmaps[4][0,i], m_Heightmaps[4][0,i+1], 0.5f, 0.5f);
                // right line
                hm[256, i + 1] = MathExt.Bilerp(m_Heightmaps[4][255,i], m_Heightmaps[4][255,i+1], m_Heightmaps[5][0,i], m_Heightmaps[5][0,i+1], 0.5f, 0.5f);
            }

            // fill rest
            for (int i = 1; i < 256; ++i)
            {
                for (int j = 1; j < 256; ++j)
                {
                    hm[i, j] = MathExt.Bilerp(m_Heightmaps[4][i-1,j-1], m_Heightmaps[4][i-1,j], m_Heightmaps[4][i,j-1], m_Heightmaps[4][i,j], 0.5f, 0.5f);
                }
            }

            m_Heightmaps = new List<float[,]>();
            // Split elevation data to 4
            #region divide elevation
            int idx = 0;
            for (int i = 0; i <= 1; ++i)
            {
                for (int j = 0; j <= 1; ++j)
                {
                    float[,] Heightmap = new float[129, 129];
                    // copy borders to make sure they are the same
                    int startX = i * 128;
                    int startY = j * 128;
                    //float t = 128.0f / 256.0f;
                    for (int x = 0; x < 129; ++x)
                    {
                        for (int y = 0; y < 129; ++y)
                        {
                            Heightmap[x, y] = hm[startX + x, startY + y];
                        }
                    }
                    //Debug.Log("Starting subdivision.");

                    Vector2i newOffset = new Vector2i(offset.x * 2 + j, offset.y * 2 - i);
                    m_Offsets[idx] = newOffset;
                    m_Heightmaps.Add(Heightmap);
                    ++idx;
                    //Debug.Log("Generated cleaned heightmap " + idx);
                }
            }
            #endregion

            #region supersample for elevation - UNUSED
            /*
            // super sample the matrix with fixed border and middle lines
            int idx = 0;
            for (int i = 0; i <= 1; ++i)
            {
                for (int j = 0; j <= 1; ++j)
                {
                    float[,] Heightmap = new float[257, 257];
                    // copy borders to make sure they are the same
                    int startX = i * 128;
                    int startY = j * 128;
                    float t = 128.0f / 256.0f;
                    for (int x = 0; x < 257; ++x)
                    {
                        for (int y = 0; y < 257; ++y)
                        {
                            if ((x & 0x1) != 1 && (y & 0x1) != 1)
                            {
                                Heightmap[x, y] = hm[startX + x / 2, startY + y / 2];
                            }
                            else
                            {
                                // one index is odd
                                int xx = startX + x / 2;
                                int xx1 = startX + (x + 1) / 2;
                                int yy = startY + y / 2;
                                int yy1 = startY + (y + 1) / 2;
                                Heightmap[x, y] = MathExt.Bilerp(hm[xx, yy], hm[xx1,yy], hm[xx,yy1], hm[xx1,yy1], t, t);
                            }
                        }
                    }
                    //Debug.Log("Starting subdivision.");

                    Vector2i newOffset = new Vector2i(offset.x * 2 + j, offset.y * 2 - i);
                    m_Offsets[idx] = newOffset;
                    m_Heightmaps.Add(Heightmap);
                    ++idx;
                    //Debug.Log("Generated cleaned heightmap " + idx);
                }
            }
            */
            #endregion
        }

        /// <summary>
        /// Creates four terrain tiles, but creation is splitted in 4 frames.
        /// Afterwards creates now jobs for buildings and city wall.
        /// </summary>
        /// <returns></returns>
        protected override bool OnFinished()
        {
            if (m_Tile == null || m_Tile.Destroyed)
            {
                //m_Tile.Clean();
                return true;
            }

            //Debug.Log("Creating tiles in OnFinished.");
            // This is executed by the Unity main thread when the job is finished
            float size = m_World.MetersPerPixel * World.TileSize;
            Vector2 offset = new Vector2(m_Tile.Data.Position.x, m_Tile.Data.Position.y);
            offset.x -= m_World.WorldCenter.x;
            offset.y -= m_World.WorldCenter.y;
            m_Tile.transform.position = new Vector3(size * offset.x, 0, -size * offset.y);

            // Temporary for now - create 4 tiles with higher zoom
            if (m_TileCreatedIdx < 4)
            {
                if (m_Tile.Data.Textures[m_TileCreatedIdx] == null)
                {
                    // some error in texture creation
                    m_Tile.Data.Textures[m_TileCreatedIdx] = new Texture2D(256, 256, TextureFormat.RGB24, false);
                }
                if (Settings.USE_UNITY_TERRAIN)
                {
                    m_Tile.CreateTerrain(m_Tile.Data.Textures[m_TileCreatedIdx], m_Heightmaps[m_TileCreatedIdx], size / 2.0f, m_Offsets[m_TileCreatedIdx], Settings.SUBDIVISION_DEPTH, m_World);
                }
                else
                {
                    m_Tile.CreateTerrain(m_Tile.Data.Textures[m_TileCreatedIdx], m_Heightmaps[m_TileCreatedIdx], size / 2.0f, m_Offsets[m_TileCreatedIdx], m_World);
                }
                m_Tile.name = m_Tile.Data.Position.ToString();
                m_TileCreatedIdx++;
                return false;
            }
            // Request building creation on ThreadPool
            //if (m_Tile.Data.Buildings.str != null && m_Tile.Data.Buildings.str.Length > 0)
            //{
                BuildingCreationJob job = new BuildingCreationJob(m_TileCache, m_Tile, m_World, true, "buildings");
                BuildingCreationJob jobWalls = new BuildingCreationJob(m_TileCache, m_Tile, m_World, false, "landuse");
                m_TileCache.StartJob(job);
                m_TileCache.StartJob(jobWalls);
            //}
            // Tile finally created
            return true;
        }

    }
}
