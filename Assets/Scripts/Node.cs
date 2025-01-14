using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Node sýnýfý, yol bulma algoritmasýnda kullanýlan temel bir veri yapýsýdýr.
public class Node
{
    public Vector2 position; // Düðümün bulunduðu pozisyon.
    public Vector2 parent;   // Bu düðüme ulaþmak için geçilen önceki düðümün pozisyonu.

    // Node sýnýfýnýn yapýcý metodu (constructor).
    public Node(Vector2 _position, Vector2 _parent)
    {
        position = _position; // Düðümün pozisyonu atanýr.
        parent = _parent;     // Düðümün ebeveyni atanýr.
    }
}
