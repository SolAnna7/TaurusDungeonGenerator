using System;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    public class RoomConnector : MonoBehaviour
    {
        public Vector2 Size = new Vector2(1, 2);
        public string Type = "";

        public GameObject OpenReplacement;
        public GameObject ClosedReplacement;

        public ConnectionState State { get; private set; } = ConnectionState.UNINITIATED;
        public RoomConnector ConnectedRoom { get; private set; } = null;

        public TaurusDungeonGenerator.Component.Room GetParentRoom()
        {
            var parentRoom = GetComponentInParent<TaurusDungeonGenerator.Component.Room>();
            if (parentRoom == null)
            {
                throw new Exception($"Parent room not found! (in {gameObject.name})");
            }

            return parentRoom;
        }

        private void OnDrawGizmos()
        {
            Color drawColor;

            Vector2 halfSize = Size * 0.5f;

            switch (State)
            {
                case ConnectionState.UNINITIATED:
                    drawColor = Color.HSVToRGB((Size.x * 227 + Size.y * 491) % 100 / 100.0f, 1, 1);
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
            float lineLength = Mathf.Min(Size.x, Size.y);
            Gizmos.DrawLine(transform.position + transform.right * halfSize.x, transform.position + transform.forward * lineLength);
            Gizmos.DrawLine(transform.position - transform.right * halfSize.x, transform.position + transform.forward * lineLength);

//            Gizmos.color = drawColor;
            Vector3 topLeft = transform.position - (transform.right * halfSize.x) + (transform.up * Size.y);
            Vector3 topRight = transform.position + (transform.right * halfSize.x) + (transform.up * Size.y);
            Vector3 bottomLeft = transform.position - (transform.right * halfSize.x);
            Vector3 bottomRight = transform.position + (transform.right * halfSize.x);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);


            Gizmos.DrawLine(topLeft, transform.position + transform.up * (Size.y + lineLength / 4));
            Gizmos.DrawLine(topRight, transform.position + transform.up * (Size.y + lineLength / 4));
        }

        public static void Connect(RoomConnector c1, RoomConnector c2)
        {
            c1.ConnectedRoom = c2;
            c2.ConnectedRoom = c1;
            c1.State = ConnectionState.CONNECTED;
            c2.State = ConnectionState.CONNECTED;
        }

        public void Close()
        {
            ConnectedRoom = null;
            State = ConnectionState.CLOSED;
        }


        public enum ConnectionState
        {
            UNINITIATED,
            CLOSED,
            CONNECTED
        }
    }
}