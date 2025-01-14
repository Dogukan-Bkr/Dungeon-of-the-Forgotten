using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TileSpawner s�n�f�, her bir zemin par�as�n�n (tile) olu�turulmas� ve duvarlar�n eklenmesi i�in kullan�l�r.
public class TileSpawner : MonoBehaviour
{
    // Zindan y�neticisi, zemini ve duvarlar� olu�tururken referans al�n�r.
    DungeonManager dungeonManager;

    // Awake, sahne y�klendi�inde ilk �al��an metottur.
    private void Awake()
    {
        // DungeonManager bile�enini sahnedeki herhangi bir objede bulur.
        dungeonManager = FindAnyObjectByType<DungeonManager>();

        // Zemin (floorPrefab) olu�turulur ve pozisyonu ayarlan�r.
        GameObject goFloor = Instantiate(dungeonManager.floorPrefab, transform.position, Quaternion.identity) as GameObject;
        goFloor.name = dungeonManager.floorPrefab.name; // Zemin objesine isim verilir.
        goFloor.transform.SetParent(dungeonManager.transform); // Zindan y�neticisine ba�l� hale getirilir.

        // X eksenindeki maksimum ve minimum pozisyon de�erleri kontrol edilir ve g�ncellenir.
        if (transform.position.x > dungeonManager.maxX)
        {
            dungeonManager.maxX = transform.position.x;
        }
        if (transform.position.x < dungeonManager.minX)
        {
            dungeonManager.minX = transform.position.x;
        }
        // Y eksenindeki maksimum ve minimum pozisyon de�erleri kontrol edilir ve g�ncellenir.
        if (transform.position.y > dungeonManager.maxY)
        {
            dungeonManager.maxY = transform.position.y;
        }
        if (transform.position.y < dungeonManager.minY)
        {
            dungeonManager.minY = transform.position.y;
        }
    }

    void Start()
    {
        // Duvar ve zemin katmanlar�n� kapsayan bir maske tan�mlan�r.
        LayerMask envMask = LayerMask.GetMask("Wall", "Floor");

        // Her bir zemin par�as�n�n �evresindeki 3x3 alan kontrol edilir.
        Vector2 hitSize = Vector2.one * 0.8f;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // �evredeki hedef pozisyon belirlenir.
                Vector2 targetPos = new Vector2(transform.position.x + x, transform.position.y + y);

                // Hedef pozisyonda bir obje olup olmad��� kontrol edilir.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, envMask);

                // E�er hedef pozisyonda bir obje yoksa, bir duvar olu�turulur.
                if (!hit)
                {
                    GameObject goWall = Instantiate(dungeonManager.wallPrefab, targetPos, Quaternion.identity) as GameObject;
                    goWall.name = dungeonManager.wallPrefab.name; // Duvar objesine isim verilir.
                    goWall.transform.SetParent(dungeonManager.transform); // Zindan y�neticisine ba�lan�r.
                }
            }
        }

        // ��lem tamamland���nda bu obje sahneden silinir.
        Destroy(gameObject);
    }

    // OnDrawGizmos, sahne edit�r�nde g�rsel rehberlik i�in bir k�p �izer.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white; // Beyaz renk se�ilir.
        Gizmos.DrawCube(transform.position, Vector3.one); // K�p �izimi yap�l�r.
    }
}
