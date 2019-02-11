using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public Rect rect;
    private Vector2 offset;
    public ConnectionPointType type;
    public bool isActivated = false;
    public Connection parentConnection;

    public Node node;

    public GUIStyle style;

    public Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint(Node node, Vector2 offset, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.offset = offset;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void Draw()
    {
        if (!isActivated)
            return;

        rect.y = node.rect.y + offset.y;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 12f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 12f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }

    public void DeleteParentConnexion()
    {
        if (node.connections != null && node.connections.Contains(parentConnection))
            node.connections.Remove(parentConnection);
    }
}