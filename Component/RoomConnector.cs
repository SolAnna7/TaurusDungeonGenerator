using System;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    /// <summary>
    /// An 
    /// </summary>
    public class RoomConnector : MonoBehaviour
    {
        [Tooltip("Connects only with the same type and size")]
        public Vector2 size = new Vector2(1, 2);

        [Tooltip("Connects only with the same type and size")]
        public string type = "";

        [Tooltip("Created if the connection is open")]
        public GameObject openPrefab;

        [Tooltip("Created if the connection is closed")]
        public GameObject closedPrefab;

        [Tooltip("Remove the original object after building")]
        public bool removeOriginal;

        /// <summary>
        /// The actual state of the connection
        /// </summary>
        public ConnectionState State { get; private set; } = ConnectionState.UNINITIATED;

        /// <summary>
        /// The other room connector connected to this (if the ConnectionState is CONNECTED)
        /// </summary>
        public RoomConnector ConnectedRoomConnector { get; private set; } = null;

        /// <summary>
        /// Gets the parent room of the connection
        /// </summary>
        public Room ParentRoom => GetComponentInParent<Room>() ?? throw new Exception($"Parent room not found! (in {gameObject.name})");

        /// <summary>
        /// Connects 2 RoomConnector to each other
        /// </summary>
        public static void Connect(RoomConnector c1, RoomConnector c2)
        {
            c1.ConnectedRoomConnector = c2;
            c2.ConnectedRoomConnector = c1;
            c1.State = ConnectionState.CONNECTED;
            c2.State = ConnectionState.CONNECTED;
        }

        /// <summary>
        /// Closes the connection, sets its state to CLOSED
        /// </summary>
        public void Close()
        {
            ConnectedRoomConnector = null;
            State = ConnectionState.CLOSED;
        }

        /// <summary>
        /// The possible states of a RoomConnector
        /// </summary>
        public enum ConnectionState
        {
            /// <summary>
            /// The Connector is neither closed, nor connected. Default state.
            /// </summary>
            UNINITIATED,
            /// <summary>
            /// Tho Connector is not connected to another room, closed
            /// </summary>
            CLOSED,
            /// <summary>
            /// Tho Connector is connected to another room
            /// </summary>
            CONNECTED
        }

        private void OnDrawGizmos()
        {
            Color drawColor;

            Vector2 halfSize = size * 0.5f;

            switch (State)
            {
                case ConnectionState.UNINITIATED:
                    drawColor = Color.HSVToRGB((size.x * 227 + size.y * 491) % 100 / 100.0f, 1, 1);
                    break;
                case ConnectionState.CONNECTED:
                    drawColor = Color.green;
                    break;
                case ConnectionState.CLOSED:
                    drawColor = Color.black;
                    break;
                default: throw new Exception($"Unhandled state {State}");
            }


            Gizmos.color = drawColor;
            float lineLength = Mathf.Min(size.x, size.y);
            Gizmos.DrawLine(transform.position + transform.right * halfSize.x, transform.position + transform.forward * lineLength);
            Gizmos.DrawLine(transform.position - transform.right * halfSize.x, transform.position + transform.forward * lineLength);

//            Gizmos.color = drawColor;
            Vector3 topLeft = transform.position - (transform.right * halfSize.x) + (transform.up * size.y);
            Vector3 topRight = transform.position + (transform.right * halfSize.x) + (transform.up * size.y);
            Vector3 bottomLeft = transform.position - (transform.right * halfSize.x);
            Vector3 bottomRight = transform.position + (transform.right * halfSize.x);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);


            Gizmos.DrawLine(topLeft, transform.position + transform.up * (size.y + lineLength / 4));
            Gizmos.DrawLine(topRight, transform.position + transform.up * (size.y + lineLength / 4));
        }
    }
}