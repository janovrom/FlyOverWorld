using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.WorldScripts.Jobs
{

    /// <summary>
    /// Threaded job, which on separate thread creates vertices and indices
    /// for mesh and then in Unity Update assigns these two to mesh.
    /// </summary>
    class BuildingCreationJob : ThreadedJob
    {

        private TileCache m_TileCache;
        private WorldTile m_Tile;
        private World m_World;
        /// <summary>
        /// List of buildings for this tile.
        /// </summary>
        private List<BuildingHolder> m_Buildings = new List<BuildingHolder>();
        /// <summary>
        /// Type of building - building or landuse wall.
        /// </summary>
        private string m_Type;
        /// <summary>
        /// Whether object is solid.
        /// </summary>
        private bool m_IsVolume;
        /// <summary>
        /// Takes from JSON needed property, that differs for building and landuse.
        /// </summary>
        private Func<JSONObject, bool> m_SelectCondition;


        public BuildingCreationJob(TileCache tileCache, WorldTile tile, World w, bool isVolume, string type)
        {
            m_TileCache = tileCache;
            m_Tile = tile;
            m_World = w;
            m_IsVolume = isVolume;
            m_Type = type;
            if (type == "landuse")
            {
                m_SelectCondition = (x => x[Constants.JSON_KEY_GEO][Constants.JSON_KEY_TYPE].str == Constants.JSON_KEY_POLYGON
                && x[Constants.JSON_KEY_PROP][Constants.JSON_KEY_KIND].str == Constants.JSON_KEY_WALL);
            }
            else
            {
                m_SelectCondition = (x => x[Constants.JSON_KEY_GEO][Constants.JSON_KEY_TYPE].str == Constants.JSON_KEY_POLYGON || x[Constants.JSON_KEY_GEO][Constants.JSON_KEY_TYPE].str == Constants.JSON_KEY_MULTI_POLYGON);
            }
        }

        protected override void ThreadFunction()
        {
            float size = m_World.MetersPerPixel * World.TileSize;
            if (!m_Tile.Data.Buildings.HasField(m_Type))
            {
                return;
            }
            JSONObject mapData = m_Tile.Data.Buildings[m_Type];

            if (!mapData.HasField(Constants.JSON_KEY_BUILDINGS))
            {
                return;
            }

            // For each json object that represent building, create it
            foreach (JSONObject geo in mapData[Constants.JSON_KEY_BUILDINGS].list.Where(m_SelectCondition))
            {
                if (geo[Constants.JSON_KEY_GEO][Constants.JSON_KEY_TYPE].str == Constants.JSON_KEY_POLYGON)
                {
                    HandlePolygon(geo, geo[Constants.JSON_KEY_GEO][Constants.JSON_KEY_COOR][0].list, size);
                }
                else if(geo[Constants.JSON_KEY_GEO][Constants.JSON_KEY_TYPE].str == Constants.JSON_KEY_MULTI_POLYGON)
                {
                    for (int i = 0; i < geo[Constants.JSON_KEY_GEO][Constants.JSON_KEY_COOR][0].list.Count; i++)
                    {
                        HandlePolygon(geo, geo[Constants.JSON_KEY_GEO][Constants.JSON_KEY_COOR][0].list[i].list, size);
                    }
                }
            }
        }

        private void HandlePolygon(JSONObject geo, List<JSONObject> corners, float size)
        {
            List<Vector3> l = new List<Vector3>();
            // Add vertices
            for (int i = 0; i < corners.Count - 1; i++)
            {
                JSONObject c = corners[i];

                Vector2 bm = GM.Project(new Vector2(c[1].f, c[0].f), World.TileSize) * (1 << m_World.zoom) * m_World.MetersPerPixel;
                Vector2 pm = new Vector2(bm.x - m_World.WorldStart.x, -bm.y - m_World.WorldStart.y + size);
                l.Add(pm.ToVector3xz());
            }

            try
            {
                // Find center and position all vertices around it
                Vector3 center = l.Aggregate((acc, cur) => acc + cur) / l.Count;

                float height = 15.0f;
                if (geo[Constants.JSON_KEY_PROP].HasField(Constants.JSON_KEY_HEIGHT))
                {
                    height = geo[Constants.JSON_KEY_PROP][Constants.JSON_KEY_HEIGHT].f;
                }
                Vector3 flatCenter = new Vector3(center.x, 0.0f, center.z);
                for (int i = 0; i < l.Count; i++)
                {
                    l[i] = l[i] - flatCenter;
                }
                // Create building
                BuildingHolder bh = new BuildingHolder(center, l, height, m_IsVolume);
                bh.Initialize();
                m_Buildings.Add(bh);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        protected override bool OnFinished()
        {
            // This is executed by the Unity main thread when the job is finished

            // If tile was already destroyed, don't create buildings.
            if (m_Tile == null)
                return true;

            foreach (BuildingHolder bh in m_Buildings)
            {
                // Create Mesh for each building
                GameObject o = bh.CreateModel(m_Tile, m_Type);
                if (o == null)
                {
                    Debug.LogError("Building really couldn't be positioned");
                    //m_TileCache.UnpositionedBuildings.Add(bh);
                }
                else
                {
                    o.AddComponent<MeshCollider>();
                }
            }

            // Building creation is for now quite straightforward, no need to break after each building
            return true;

        }

    }
}
