using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Node s�n�f�, yol bulma algoritmas�nda kullan�lan temel bir veri yap�s�d�r.
public class Node
{
    public Vector2 position; // D���m�n bulundu�u pozisyon.
    public Vector2 parent;   // Bu d���me ula�mak i�in ge�ilen �nceki d���m�n pozisyonu.

    // Node s�n�f�n�n yap�c� metodu (constructor).
    public Node(Vector2 _position, Vector2 _parent)
    {
        position = _position; // D���m�n pozisyonu atan�r.
        parent = _parent;     // D���m�n ebeveyni atan�r.
    }
}
