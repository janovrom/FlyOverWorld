using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Class logging information from other components. It has pending messages front,
    /// to allow loggin from another threads such as network thread. Messages are added
    /// to waiting and each update they are checked and, if any, created. Messages can
    /// be plain informations (green), warnings (orange) and errors (red). Errors can
    /// be shown indefinetly, only to disappear when clicked on "cross" on right top.
    /// </summary>
    public class Logger : MonoBehaviour
    {

        private const float m_ImmediateDestruction = -1000.0f;
        private const float m_Destruction = -1.0f;
        public RectTransform PrefabPanel;
        /// <summary>
        /// Instantiated messages currently being displayed.
        /// </summary>
        private List<Message> m_Messages = new List<Message>();
        /// <summary>
        /// List of messages marked to be removed.
        /// </summary>
        private List<Message> m_MessagesToRemove = new List<Message>();
        /// <summary>
        /// Messages waiting to be created and shown, since message
        /// can be created outside unity main/render thread.
        /// </summary>
        private List<Message> m_WaitingMessages = new List<Message>();
        private object m_Lock = new object();


        /// <summary>
        /// Adds new message to waiting front if not already added.
        /// </summary>
        /// <param name="message">message to add</param>
        /// <param name="color">color of message</param>
        /// <param name="timeS">time how long message is shown</param>
        private void Add(Message message, Color color, float timeS)
        {
            message.m_Color = color;
            message.m_TimeS = timeS;
            lock (m_Lock)
            {
                if (!m_WaitingMessages.Contains(message) && !m_Messages.Contains(message))
                    m_WaitingMessages.Add(message);
            }
        }

        /// <summary>
        /// Coroutine to wait for specified time and then remove 
        /// message from log.
        /// </summary>
        /// <param name="timeS">time how long to wait</param>
        /// <param name="m">message</param>
        /// <returns></returns>
        private IEnumerator RemoveAfterTime(float timeS, Message m)
        {
            yield return new WaitForSeconds(timeS);
            Remove(m);
        }

        /// <summary>
        /// Removes message and destroys its game object.
        /// </summary>
        /// <param name="m">message to remove</param>
        private void Remove(Message m)
        {
            if (m.m_GameObject != null)
                DestroyMessage(m.m_GameObject);
            m.m_GameObject = null;
        }

        /// <summary>
        /// Adds error to waiting list. Priority can be undefined time, which
        /// means forever.
        /// </summary>
        /// <param name="message">message to add</param>
        /// <param name="priority">priority of error, default is standard error</param>
        public void LogError(Message message, int priority = 0)
        {
            if (priority == Message.TopPriority)
                Add(message, Constants.COLOR_RED, Settings.TIME_UNDEFINED);
            else
                Add(message, Constants.COLOR_RED, Settings.TIME_ERROR);
        }

        /// <summary>
        /// Adds warning to waiting list
        /// </summary>
        /// <param name="message">message to add</param>
        public void LogWarning(Message message)
        {
            Add(message, Constants.COLOR_ORANGE, Settings.TIME_WARNING);
        }

        /// <summary>
        /// Adds regular message to waiting list.
        /// </summary>
        /// <param name="message">message to add</param>
        public void Log(Message message)
        {
            Add(message, Constants.COLOR_GREEN, Settings.TIME_MESSAGE);
        }

        /// <summary>
        /// Sets time of message to destruction time, since messages are removed based
        /// on their remaining time. Such message is destroyed after 1 sec.
        /// </summary>
        /// <param name="message">message to remove</param>
        public void RemoveMessage(Message message)
        {
            // We are removing based on time, this will force the message to be removed next update
            // If message is not contained in list, it is not even displayed, so it doesn't matter.
            message.m_TimeS = m_Destruction;
        }

        /// <summary>
        /// Sets time of message to immediate destruction time, since messages are removed based
        /// on their remaining time.
        /// </summary>
        /// <param name="message">message to remove immediately</param>
        public void RemoveMessageImmediate(Message message)
        {
            // We are removing based on time, this will force the message to be removed next update
            // If message is not contained in list, it is not even displayed, so it doesn't matter.
            message.m_TimeS = m_ImmediateDestruction;
        }

        /// <summary>
        /// Destroys game object of message and all other messages are shifted
        /// to fill the empty space.
        /// </summary>
        /// <param name="o">game object of message to destroy</param>
        public void DestroyMessage(GameObject o)
        {
            if (o == null)
                return;
            int i = 0;
            for (; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).gameObject == o)
                {
                    break;
                }
            }

            for (; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                if (child.position.y > 52)
                    child.position = new Vector3(child.position.x, child.position.y - 52, child.position.z);
            }
            DestroyImmediate(o);
        }

        /// <summary>
        /// Checks for waiting messages and creates them. Also updates times how long
        /// are messages shown and if this time is already below 0, destroys them.
        /// </summary>
        void Update()
        {
            lock(m_Lock)
            {
                for (int i = 0; i < m_WaitingMessages.Count; ++i)
                {
                    RectTransform rect = GameObject.Instantiate(PrefabPanel);
                    Message m = m_WaitingMessages[i];
                    // Add close action to button
                    rect.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { RemoveMessageImmediate(m); });
                    // Add message
                    rect.transform.GetChild(1).GetComponent<Text>().text = m.m_Message;
                    rect.GetComponent<Image>().color = m.m_Color;
                    rect.transform.SetParent(transform);
                    rect.transform.SetAsLastSibling();
                    rect.localPosition = new Vector3(0.0f, (transform.childCount - 1) * 52.0f, 0.0f);
                    m.m_GameObject = rect.gameObject;
                    m_Messages.Add(m);
                }
                m_WaitingMessages.Clear();
            }

            for (int i = 0; i < m_Messages.Count; ++i)
            {
                m_Messages[i].m_TimeS -= Time.deltaTime;
                if (m_Messages[i].m_TimeS <= 0.0f)
                {
                    m_MessagesToRemove.Add(m_Messages[i]);
                    m_Messages.RemoveAt(i);
                    --i;
                }
            }

            foreach (Message m in m_MessagesToRemove)
            {
                if (m.m_TimeS <= m_ImmediateDestruction)
                    Remove(m);
                else
                    StartCoroutine(RemoveAfterTime(1.0f, m));
            }
            m_MessagesToRemove.Clear();
        }

        /// <summary>
        /// Message class that is used by logger to display information. Message can be created
        /// outside logger, so that the message is always shown onnly once and when it is already 
        /// displayed, its time on screen is renewed.
        /// </summary>
        public class Message
        {

            /// <summary>
            /// TopPriority errors can be only removed by user, otherwise they are being shown indefinitely.
            /// </summary>
            public const int TopPriority = 10;
            /// <summary>
            /// Counter for assigned ids. Id is always unique for displayed messages.
            /// Message can be deleted or restarted using this id.
            /// </summary>
            private static long ID_COUNTER = 0;

            /// <summary>
            /// Id is always unique for displayed messages.
            /// Message can be deleted or restarted using this id.
            /// </summary>
            internal long m_Id;
            internal Color m_Color;
            internal string m_Message;
            internal float m_TimeS;
            internal GameObject m_GameObject;

            public Message(string message)
            {
                this.m_Message = message;
                m_Id = ID_COUNTER++;
            }

            internal Message(string message, Color color, float timeS)
            {
                this.m_Message = message;
                this.m_Color = color;
                this.m_TimeS = timeS;
                m_Id = ID_COUNTER++;
            }

            public override int GetHashCode()
            {
                // Since maximum zoom is 15, it is enough to make it to 1 integer - always unique
                return m_Id.GetHashCode();
            }

            public static bool operator ==(Message obj1, Message obj2)
            {
                if (object.ReferenceEquals(obj1, null) || object.ReferenceEquals(obj2, null))
                    return false;

                return obj1.m_Id == obj2.m_Id;
            }

            public static bool operator !=(Message obj1, Message obj2)
            {
                return !(obj1 == obj2);
            }

            public override bool Equals(object obj)
            {
                var other = obj as Message;
                if (other == null)
                    return false;

                return this.m_Id == other.m_Id;
            }

            public override string ToString()
            {
                return "ID(" + m_Id +")::" + m_Message;
            }

        }

    }
}
