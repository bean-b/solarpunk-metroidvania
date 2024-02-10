using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility 
{
    public static bool IsWithinBounds(Vector2 position, Vector2 bottomLeft, Vector2 topRight)
    {
        return position.x >= bottomLeft.x && position.x <= topRight.x &&
               position.y >= bottomLeft.y && position.y <= topRight.y;
    }
}
