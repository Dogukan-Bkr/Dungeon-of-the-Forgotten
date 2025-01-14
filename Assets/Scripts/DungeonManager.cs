using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli kütüphane.

// Zindan türlerini tanýmlayan enum. Üç farklý zindan türü bulunur: Caverns, Rooms ve Winding.
public enum DungeonType { Caverns, Rooms, Winding }

// DungeonManager sýnýfý, procedural (algoritmik) zindan oluþturmayý ve yönetmeyi saðlar.
public class DungeonManager : MonoBehaviour
{
    // Rastgele yerleþtirilecek eþyalar, düþmanlar ve yuvarlatýlmýþ kenar prefab'larý.
    public GameObject[] randomItems, randomEnemies, roundedEdges;

    // Zemin, duvar, karot ve çýkýþ kapýsý prefab'larý.
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;

    // Zindanýn oluþturulmasýnda kullanýlacak parametreler.
    [Range(50, 5000)] public int totalFloorCount; // Toplam zemin sayýsý.
    [Range(0, 100)] public int itemSpawnPercent;  // Rastgele eþya yüzdesi.
    [Range(0, 100)] public int enemySpawnPercent; // Rastgele düþman yüzdesi.
    [Range(0, 100)] public int windingHallPercent; // Dolambaçlý koridor yüzdesi.
    public bool useRoundedEdges; // Yuvarlatýlmýþ kenarlarýn kullanýlýp kullanýlmayacaðý.
    public DungeonType dungeonType; // Zindan türü seçimi.

    // Zindanýn sýnýrlarýný belirleyen deðiþkenler.
    [HideInInspector] public float minX, maxX, minY, maxY;

    // Zemin pozisyonlarýný ve katman maskelerini tutar.
    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;
    Vector2 hitSize; // Çarpýþma kutusunun boyutu.

    // Zindanýn oluþturulmasýný baþlatan baþlangýç metodu.
    void Start()
    {
        hitSize = Vector2.one * 0.8f; // Çarpýþma kutusunun boyutunu belirler.
        floorMask = LayerMask.GetMask("Floor"); // Zemin katmaný maskesi.
        wallMask = LayerMask.GetMask("Wall");   // Duvar katmaný maskesi.

        // Sahnedeki tüm TileSpawner nesnelerini bulur.
        TileSpawner[] spawners = FindObjectsOfType<TileSpawner>();

        // Seçilen zindan türüne göre uygun algoritmayý çaðýrýr.
        switch (dungeonType)
        {
            case DungeonType.Caverns: RandomWalker(); break; // Caverns türü için rastgele yürüyüþ algoritmasý.
            case DungeonType.Rooms: RoomWalker(); break;     // Rooms türü için oda tabanlý algoritma.
            case DungeonType.Winding: WindingWalker(); break; // Winding türü için dolambaçlý yollar.
        }
    }

    // Update metodu, editör modunda sahneyi yeniden yüklemek için kullanýlýr.
    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden yükler.
        }
    }

    // Rastgele yönlerde zemin oluþturan algoritma.
    void RandomWalker()
    {
        Vector3 curPos = Vector3.zero; // Baþlangýç pozisyonu.
        floorList.Add(curPos);         // Ýlk pozisyon zemin listesine eklenir.
        while (floorList.Count < totalFloorCount) // Belirlenen zemin sayýsýna ulaþýlana kadar devam eder.
        {
            curPos += RandomDirection(); // Rastgele bir yön seçilir.
            if (!InFloorList(curPos))    // Eðer seçilen pozisyon zemin listesinde yoksa.
            {
                floorList.Add(curPos);   // Pozisyon listeye eklenir.
            }
        }
        StartCoroutine(DelayProgress()); // Zemin ve duvarlarýn oluþturulmasýný geciktirir.
    }

    // Oda tabanlý zindan oluþturan algoritma.
    void RoomWalker()
    {
        Vector3 curPos = Vector3.zero; // Baþlangýç pozisyonu.
        floorList.Add(curPos);
        while (floorList.Count < totalFloorCount)
        {
            curPos = TakeAHike(curPos); // Rastgele bir yönde yürüyüþ yapar.
            RandomRoom(curPos);         // Bulunan pozisyonda bir oda oluþturur.
        }
        StartCoroutine(DelayProgress());
    }

    // Dolambaçlý yollar oluþturan algoritma.
    void WindingWalker()
    {
        Vector3 curPos = Vector3.zero; // Baþlangýç pozisyonu.
        floorList.Add(curPos);
        while (floorList.Count < totalFloorCount)
        {
            curPos = TakeAHike(curPos); // Rastgele bir yönde yürüyüþ yapar.
            int rool = Random.Range(0, 100);
            if (rool > windingHallPercent) // Belirli bir yüzde ile oda oluþturur.
            {
                RandomRoom(curPos);
            }
        }
        StartCoroutine(DelayProgress());
    }

    // Rastgele bir yönde yürüyüþ yapar ve yeni pozisyonlar ekler.
    Vector3 TakeAHike(Vector3 myPos)
    {
        Vector3 walkDir = RandomDirection(); // Rastgele bir yön seçilir.
        int walkLength = Random.Range(9, 18); // Yürüyüþ uzunluðu belirlenir.
        for (int i = 0; i < walkLength; i++)
        {
            if (!InFloorList(myPos + walkDir))
            {
                floorList.Add(myPos + walkDir); // Yeni pozisyon zemin listesine eklenir.
            }
            myPos += walkDir; // Pozisyon güncellenir.
        }
        return myPos;
    }

    // Rastgele boyutlarda bir oda oluþturur.
    void RandomRoom(Vector3 myPos)
    {
        int width = Random.Range(1, 5);  // Odanýn geniþliði.
        int height = Random.Range(1, 5); // Odanýn yüksekliði.

        for (int w = -width; w <= height; w++)
        {
            for (int h = -height; h <= width; h++)
            {
                Vector3 offset = new Vector3(w, h, 0); // Her bir pozisyon için ofset belirlenir.
                if (!InFloorList(myPos + offset)) // Eðer pozisyon zemin listesinde yoksa.
                {
                    floorList.Add(myPos + offset); // Listeye eklenir.
                }
            }
        }
    }

    // Pozisyonun zemin listesinde olup olmadýðýný kontrol eder.
    bool InFloorList(Vector3 myPos)
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            if (Vector3.Equals(myPos, floorList[i])) // Eðer pozisyon listede varsa.
            {
                return true; // Pozisyon bulunmuþtur.
            }
        }
        return false; // Pozisyon bulunamamýþtýr.
    }

    // Rastgele bir yön döndürür (yukarý, sað, aþaðý, sol).
    Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1: return Vector3.up;    // Yukarý yön.
            case 2: return Vector3.right; // Sað yön.
            case 3: return Vector3.down;  // Aþaðý yön.
            case 4: return Vector3.left;  // Sol yön.
        }
        return Vector3.zero; // Varsayýlan yön.
    }

    // Zemin ve duvarlarýn oluþturulmasýný geciktirir.
    IEnumerator DelayProgress()
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject; // Zemin oluþturulur.
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform); // Zemin zindan yöneticisine baðlanýr.
        }
        while (FindObjectsOfType<TileSpawner>().Length > 0) // Tüm spawner'lar tamamlanana kadar bekler.
        {
            yield return null;
        }
        yield return StartCoroutine(InsetExitDoorway()); // Çýkýþ kapýsýný oluþturur.

        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask); // Zemin kontrolü.
                if (hitFloor)
                {
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1]))
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                        RandomItems(hitFloor, hitTop, hitRight, hitLeft, hitBottom); // Rastgele eþya ekler.
                        RandomEnemies(hitFloor, hitTop, hitRight, hitLeft, hitBottom); // Rastgele düþman ekler.
                    }
                }
                RoundedEdges(x, y); // Yuvarlatýlmýþ kenarlarý ekler.
            }
        }
    }

    // Yuvarlatýlmýþ kenarlarý tamamlayan metot.
    void RoundedEdges(int x, int y)
    {
        if (useRoundedEdges) // Eðer yuvarlatýlmýþ kenarlar kullanýlýyorsa.
        {
            Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, wallMask); // Duvar kontrolü.
            if (hitWall)
            {
                Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                int bitVal = 0; // Kenar tipi için bit deðeri hesaplanýr.
                if (!hitTop) { bitVal += 1; }
                if (!hitRight) { bitVal += 2; }
                if (!hitBottom) { bitVal += 4; }
                if (!hitLeft) { bitVal += 8; }
                if (bitVal > 0)
                {
                    GameObject goEdge = Instantiate(roundedEdges[bitVal], new Vector2(x, y), Quaternion.identity) as GameObject; // Yuvarlatýlmýþ kenar oluþturulur.
                    goEdge.name = roundedEdges[bitVal].name;
                    goEdge.transform.SetParent(hitWall.transform); // Kenar duvara baðlanýr.
                }
            }
        }
    }

    // Rastgele düþman ekler.
    void RandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitLeft, Collider2D hitBottom)
    {
        if (!hitTop && !hitRight && !hitLeft && !hitBottom) // Eðer tüm yönler açýk ise.
        {
            int roll = Random.Range(1, 101); // Rastgele sayý oluþturulur.
            if (roll <= enemySpawnPercent) // Eðer sayý düþman yüzdesine eþit veya küçükse.
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length); // Rastgele düþman seçilir.
                GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goEnemy.name = randomEnemies[enemyIndex].name; // Düþman adý atanýr.
                goEnemy.transform.SetParent(hitFloor.transform); // Düþman zemine baðlanýr.
            }
        }
    }

    // Rastgele eþya ekler.
    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitLeft, Collider2D hitBottom)
    {
        if ((hitTop || hitRight || hitLeft || hitBottom) && !(hitTop && hitBottom) && !(hitLeft && hitRight)) // Belirli bir koþulda eþya eklenir.
        {
            int roll = Random.Range(1, 101); // Rastgele sayý oluþturulur.
            if (roll <= itemSpawnPercent) // Eðer sayý eþya yüzdesine eþit veya küçükse.
            {
                int itemIndex = Random.Range(0, randomItems.Length); // Rastgele eþya seçilir.
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goItem.name = randomItems[itemIndex].name; // Eþya adý atanýr.
                goItem.transform.SetParent(hitFloor.transform); // Eþya zemine baðlanýr.
            }
        }
    }

    // Çýkýþ kapýsýný oluþturur.
    void ExitDoorway()
    {
        Vector3 doorPos = floorList[floorList.Count - 1]; // Çýkýþ kapýsý için son zemin pozisyonu seçilir.
        GameObject goDoor = Instantiate(exitPrefab, doorPos, Quaternion.identity) as GameObject;
        goDoor.name = exitPrefab.name; // Kapý adý atanýr.
        goDoor.transform.SetParent(transform); // Kapý zindan yöneticisine baðlanýr.
    }

    // Çýkýþ kapýsýný yerleþtiren coroutine.
    IEnumerator InsetExitDoorway()
    {
        Vector3 walkDir = RandomDirection(); // Rastgele bir yön seçilir.
        bool isExitPlaced = CheckExitCondition(floorList[floorList.Count - 1]); // Çýkýþ kapýsý yerleþtirme koþulu kontrol edilir.
        while (!isExitPlaced) // Çýkýþ kapýsý yerleþene kadar devam eder.
        {
            Vector3 curPos = WalkStraight(walkDir); // Rastgele bir yönde ilerler.
            yield return null; // Bir sonraki kareye kadar bekler.
            isExitPlaced = CheckExitCondition(curPos); // Yeni pozisyonda koþulu kontrol eder.
        }
        yield return null;
    }

    // Çýkýþ kapýsýný yerleþtirmek için koþullarý kontrol eder.
    bool CheckExitCondition(Vector2 curPos)
    {
        int numWalls = 0;

        // Dört yöndeki duvar sayýsýný kontrol eder.
        if (Physics2D.OverlapBox(curPos + Vector2.up, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.right, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.down, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.left, hitSize, 0, wallMask)) { numWalls++; }

        if (numWalls == 3) // Eðer üç tarafý duvarla çevrili ise.
        {
            ExitDoorway(); // Çýkýþ kapýsý oluþturulur.
            return true;
        }
        return false; // Aksi halde false döner.
    }

    // Rastgele bir yönde düz ilerler.
    Vector3 WalkStraight(Vector3 walkDir)
    {
        Vector3 myPos = floorList[floorList.Count - 1] + walkDir; // Yeni pozisyon hesaplanýr.

        if (InFloorList(myPos)) // Eðer pozisyon zemin listesinde varsa.
        {
            floorList.Remove(myPos); // Pozisyon listeden çýkarýlýr.
        }
        else
        {
            Collider2D hitWall = Physics2D.OverlapBox(myPos, hitSize, 0, wallMask); // Duvar kontrolü yapýlýr.
            if (hitWall)
            {
                DestroyImmediate(hitWall.gameObject); // Duvar anýnda yok edilir.
            }
            GameObject goTile = Instantiate(tilePrefab, myPos, Quaternion.identity, transform) as GameObject; // Yeni zemin oluþturulur.
            goTile.name = tilePrefab.name;
        }
        floorList.Add(myPos); // Yeni pozisyon zemin listesine eklenir.
        return myPos; // Yeni pozisyon döndürülür.
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden yükler.
    }
}
