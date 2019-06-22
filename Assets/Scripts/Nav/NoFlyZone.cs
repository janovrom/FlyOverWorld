using Assets.Scripts.Models;
using Assets.Scripts.Network.Command;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Represents no-fly zone in scene. No-fly zones are red semi-transparent cylinders, since nothing
    /// else is supported by simulator. No-fly zone is specified by its center, radius and height.
    /// </summary>
    class NoFlyZone : Zone
    {

        private float m_Radius;
        private float m_Height;
        private Vector3 m_Center;
        private MeshRenderer m_Renderer;

        void Start()
        {
            m_Renderer = GetComponent<MeshRenderer>();
        }

        public Vector3 Center
        {
            get
            {
                return m_Center;
            }

            set
            {
                m_Center = value;
            }
        }

        public float Height
        {
            get
            {
                return m_Height;
            }

            set
            {
                m_Height = value;
            }
        }

        public float Radius
        {
            get
            {
                return m_Radius;
            }

            set
            {
                m_Radius = value;
            }
        }

        public enum ZoneType : byte
        {
            CYLINDER = 0
        }

        protected override Color GetColor()
        {
            return Constants.COLOR_RED;
        }

        /// <summary>
        /// If player entered no-fly zone, disable its rendering, since outline would
        /// create black hole.
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == Constants.TAG_PLAYER)
            {
                m_Renderer.enabled = false;
            }
        }

        /// <summary>
        /// Check if UAV's center is currently in No-fly zone and if so, inform
        /// user as warning in logger.
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.tag == Constants.TAG_UAV)
            {
                if (gameObject.GetComponent<MeshCollider>().bounds.Contains(collider.transform.position))
                {
                    ZoneManager.Instance.OnDroneEnter(gameObject.name, collider.gameObject.name);
                }
            }
        }

        /// <summary>
        /// If UAV left no-fly zone inform user using message.
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == Constants.TAG_UAV)
            {
                Gui.GuiManager.Log("Drone " + collider.gameObject.name + " left no-flight zone " + gameObject.name + ".");
                ZoneManager.Instance.OnDroneExit(gameObject.name, collider.gameObject.name);
            }
            else if (collider.gameObject.tag == Constants.TAG_PLAYER)
            {
                m_Renderer.enabled = true;
            }
        }

        /// <summary>
        /// Creates new no fly zone based on parameters given. Currently only
        /// cylinder NFZ can be created.
        /// </summary>
        /// <param name="type">only cylinder NFZ can be created</param>
        /// <param name="center">center of NFZ</param>
        /// <param name="position">position of NFZ</param>
        /// <param name="zoneName">name of NFZ</param>
        /// <param name="values">parameters specific to each type: cylinder has radius and height</param>
        /// <returns>Returns created NFZ.</returns>
        public static GameObject Initialize(ZoneType type, Vector3 center, Vector3 position, string zoneName, params float[] values)
        {
            GameObject o;
            switch (type)
            {
                case ZoneType.CYLINDER:
                    o = GameObject.Instantiate(ZoneManager.Instance.CylinderPrefab);
                    o.layer = LayerMask.NameToLayer(Constants.LAYER_PICKABLE);
                    NoFlyZone zone = o.AddComponent<NoFlyZone>();
                    zone.m_Center = center;
                    zone.m_Radius = values[0];
                    zone.m_Height = values[1] / 2.0f;
                    o.name = zoneName;
                    // values = [radius, height(both up and down)]
                    o.transform.localPosition = position;
                    // Scale is diameter!
                    o.transform.localScale = new Vector3(zone.Radius * 2.0f, zone.Height, zone.Radius * 2.0f);
                    // Create billboard for NFZ
                    //zone.AddBillboard();
                    zone.AddLabel();
                    DrawLines dl = FindObjectOfType<DrawLines>();
                    zone.m_LineId = dl.RegisterLine();
                    dl.SetLineColor(zone.m_LineId, zone.GetColor());
                    dl.UpdateLine(zone.m_LineId, new List<Vector3>() { position, new Vector3(position.x, 0, position.z) });
                    break;
                default:
                    Debug.LogError("Undefined type of No-fly zone");
                    return null;
            }
            o.isStatic = true;
            return o;
        }

        public override string[] Values()
        {
            return new string[] { "Center: " + Center, "Radius: " +  Radius, "Height: " + Height };
        }

        public override bool IsDeletable()
        {
            return true;
        }

        [System.Obsolete("No longer uses world billboards. Use AddLabel() for screen space label box.", true)]
        public override void AddBillboard()
        {
            Gui.GuiManager.Instance.AddBillboard(this, 5.0f, name, Color.red,
                new UnityEngine.Events.UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(this); }));
        }

        protected override void AddLabel()
        {
            m_Label = Instantiate(Gui.LabelManager.Instance.LabelPrefab);
            m_Label.Init(this, Utility.Constants.COLOR_RED, Utility.Constants.COLOR_DARK);
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
            if (Network.SimulatorClient.Instance)
            {
                // Remove no fly zone
                Command cmd = new CommandNoFlyZone(Commands.CommandRemoveNoFlyZone, Command.BROADCAST_ADDRESS, name);
                Network.SimulatorClient.Instance.Send(cmd);
            }
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
