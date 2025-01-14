using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TileSpawner sýnýfý, her bir zemin parçasýnýn (tile) oluþturulmasý ve duvarlarýn eklenmesi için kullanýlýr.
public class TileSpawner : MonoBehaviour
{
    // Zindan yöneticisi, zemini ve duvarlarý oluþtururken referans alýnýr.
    DungeonManager dungeonManager;

    // Awake, sahne yüklendiðinde ilk çalýþan metottur.
    private void Awake()
    {
        // DungeonManager bileþenini sahnedeki herhangi bir objede bulur.
        dungeonManager = FindAnyObjectByType<DungeonManager>();

        // Zemin (floorPrefab) oluþturulur ve pozisyonu ayarlanýr.
        GameObject goFloor = Instantiate(dungeonManager.floorPrefab, transform.position, Quaternion.identity) as GameObject;
        goFloor.name = dungeonManager.floorPrefab.name; // Zemin objesine isim verilir.
        goFloor.transform.SetParent(dungeonManager.transform); // Zindan yöneticisine baðlý hale getirilir.

        // X eksenindeki maksimum ve minimum pozisyon deðerleri kontrol edilir ve güncellenir.
        if (transform.position.x > dungeonManager.maxX)
        {
            dungeonManager.maxX = transform.position.x;
        }
        if (transform.position.x < dungeonManager.minX)
        {
            dungeonManager.minX = transform.position.x;
        }
        // Y eksenindeki maksimum ve minimum pozisyon deðerleri kontrol edilir ve güncellenir.
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
        // Duvar ve zemin katmanlarýný kapsayan bir maske tanýmlanýr.
        LayerMask envMask = LayerMask.GetMask("Wall", "Floor");

        // Her bir zemin parçasýnýn çevresindeki 3x3 alan kontrol edilir.
        Vector2 hitSize = Vector2.one * 0.8f;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Çevredeki hedef pozisyon belirlenir.
                Vector2 targetPos = new Vector2(transform.position.x + x, transform.position.y + y);

                // Hedef pozisyonda bir obje olup olmadýðý kontrol edilir.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, envMask);

                // Eðer hedef pozisyonda bir obje yoksa, bir duvar oluþturulur.
                if (!hit)
                {
                    GameObject goWall = Instantiate(dungeonManager.wallPrefab, targetPos, Quaternion.identity) as GameObject;
                    goWall.name = dungeonManager.wallPrefab.name; // Duvar objesine isim verilir.
                    goWall.transform.SetParent(dungeonManager.transform); // Zindan yöneticisine baðlanýr.
                }
            }
        }

        // Ýþlem tamamlandýðýnda bu obje sahneden silinir.
        Destroy(gameObject);
    }

    // OnDrawGizmos, sahne editöründe görsel rehberlik için bir küp çizer.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white; // Beyaz renk seçilir.
        Gizmos.DrawCube(transform.position, Vector3.one); // Küp çizimi yapýlýr.
    }
}
