using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Target waypoint represents another zone in scene - Waypoints.
    /// Waypoints are yellow semitransparent spheres. It also contains
    /// corresponding CartesianWaypoint, which holds all information
    /// about its whereabouts.
    /// </summary>
    class TargetWaypoint : Zone
    {

        private CartesianWaypoint m_Waypoint;
        private MeshRenderer m_Renderer;


        void Start()
        {
            m_Renderer = GetComponent<MeshRenderer>();
        }

        protected override Color GetColor()
        {
            return Utility.Constants.COLOR_YELLOW;
        }

        /// <summary>
        /// Given name and CartesianWaypoint, create new waypoint in scene.
        /// Given position is different than position in CartesianWaypoint,
        /// since CartesianWaypoint position is relative to ground.
        /// </summary>
        /// <param name="zoneName"></param>
        /// <param name="w"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static GameObject Initialize(string zoneName, CartesianWaypoint w, Vector3 position)
        {
            GameObject o = GameObject.Instantiate(ZoneManager.Instance.WaypointsPrefab);
            TargetWaypoint waypoint = o.AddComponent<TargetWaypoint>();
            waypoint.m_Waypoint = w;
            o.name = zoneName;
            o.layer = LayerMask.NameToLayer(Utility.Constants.LAYER_PICKABLE);
            // Position has to be separate since CartesianWaypoint is adjusted as altitude above ground
            o.transform.position = position;
            o.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
            // Create billboard for area
            //waypoint.AddBillboard();
            waypoint.AddLabel();
            o.isStatic = true;
            DrawLines dl = FindObjectOfType<DrawLines>();
            waypoint.m_LineId = dl.RegisterLine();
            dl.SetLineColor(waypoint.m_LineId, waypoint.GetColor());
            dl.UpdateLine(waypoint.m_LineId, new List<Vector3>() { position, new Vector3(position.x, 0, position.z) });
            return o;
        }

        [Obsolete("No longer uses world billboards. Use AddLabel() for screen space label box.", true)]
        public override void AddBillboard()
        {
            Gui.GuiManager.Instance.AddBillboard(this, 5.0f, name, Color.yellow,
                new UnityEngine.Events.UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(this); }));
        }

        protected override void AddLabel()
        {
            m_Label = Instantiate(Gui.LabelManager.Instance.LabelPrefab);
            m_Label.Init(this, Utility.Constants.COLOR_YELLOW, Utility.Constants.COLOR_DARK);
        }

        public override bool IsDeletable()
        {
            return true;
        }

        public override string[] Values()
        {
            return new string[] { "Position: " + transform.position,
                "Altitude (AGL): " + Mathf.RoundToInt(m_Waypoint.Position.y)
            };
        }

        public CartesianWaypoint CartesianWaypoint
        {
            get
            {
                return m_Waypoint;
            }
        }

        public override void CompassSelected()
        {
            m_Renderer.enabled = false;
        }

        public override void CompassDeselected()
        {
            m_Renderer.enabled = true;
        }

        void OnDestroy()
        {
            if (ZoneManager.Instance)
                ZoneManager.Instance.RemoveZone(this);
            //RemoveBillboard();
            RemoveLabel();
            DrawLines dl = FindObjectOfType<DrawLines>();
            if (dl)
                dl.RemoveLine(m_LineId);
        }

    }
}
