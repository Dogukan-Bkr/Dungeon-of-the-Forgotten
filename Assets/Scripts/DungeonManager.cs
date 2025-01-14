using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne y�netimi i�in gerekli k�t�phane.

// Zindan t�rlerini tan�mlayan enum. �� farkl� zindan t�r� bulunur: Caverns, Rooms ve Winding.
public enum DungeonType { Caverns, Rooms, Winding }

// DungeonManager s�n�f�, procedural (algoritmik) zindan olu�turmay� ve y�netmeyi sa�lar.
public class DungeonManager : MonoBehaviour
{
    // Rastgele yerle�tirilecek e�yalar, d��manlar ve yuvarlat�lm�� kenar prefab'lar�.
    public GameObject[] randomItems, randomEnemies, roundedEdges;

    // Zemin, duvar, karot ve ��k�� kap�s� prefab'lar�.
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;

    // Zindan�n olu�turulmas�nda kullan�lacak parametreler.
    [Range(50, 5000)] public int totalFloorCount; // Toplam zemin say�s�.
    [Range(0, 100)] public int itemSpawnPercent;  // Rastgele e�ya y�zdesi.
    [Range(0, 100)] public int enemySpawnPercent; // Rastgele d��man y�zdesi.
    [Range(0, 100)] public int windingHallPercent; // Dolamba�l� koridor y�zdesi.
    public bool useRoundedEdges; // Yuvarlat�lm�� kenarlar�n kullan�l�p kullan�lmayaca��.
    public DungeonType dungeonType; // Zindan t�r� se�imi.

    // Zindan�n s�n�rlar�n� belirleyen de�i�kenler.
    [HideInInspector] public float minX, maxX, minY, maxY;

    // Zemin pozisyonlar�n� ve katman maskelerini tutar.
    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;
    Vector2 hitSize; // �arp��ma kutusunun boyutu.

    // Zindan�n olu�turulmas�n� ba�latan ba�lang�� metodu.
    void Start()
    {
        hitSize = Vector2.one * 0.8f; // �arp��ma kutusunun boyutunu belirler.
        floorMask = LayerMask.GetMask("Floor"); // Zemin katman� maskesi.
        wallMask = LayerMask.GetMask("Wall");   // Duvar katman� maskesi.

        // Sahnedeki t�m TileSpawner nesnelerini bulur.
        TileSpawner[] spawners = FindObjectsOfType<TileSpawner>();

        // Se�ilen zindan t�r�ne g�re uygun algoritmay� �a��r�r.
        switch (dungeonType)
        {
            case DungeonType.Caverns: RandomWalker(); break; // Caverns t�r� i�in rastgele y�r�y�� algoritmas�.
            case DungeonType.Rooms: RoomWalker(); break;     // Rooms t�r� i�in oda tabanl� algoritma.
            case DungeonType.Winding: WindingWalker(); break; // Winding t�r� i�in dolamba�l� yollar.
        }
    }

    // Update metodu, edit�r modunda sahneyi yeniden y�klemek i�in kullan�l�r.
    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden y�kler.
        }
    }

    // Rastgele y�nlerde zemin olu�turan algoritma.
    void RandomWalker()
    {
        Vector3 curPos = Vector3.zero; // Ba�lang�� pozisyonu.
        floorList.Add(curPos);         // �lk pozisyon zemin listesine eklenir.
        while (floorList.Count < totalFloorCount) // Belirlenen zemin say�s�na ula��lana kadar devam eder.
        {
            curPos += RandomDirection(); // Rastgele bir y�n se�ilir.
            if (!InFloorList(curPos))    // E�er se�ilen pozisyon zemin listesinde yoksa.
            {
                floorList.Add(curPos);   // Pozisyon listeye eklenir.
            }
        }
        StartCoroutine(DelayProgress()); // Zemin ve duvarlar�n olu�turulmas�n� geciktirir.
    }

    // Oda tabanl� zindan olu�turan algoritma.
    void RoomWalker()
    {
        Vector3 curPos = Vector3.zero; // Ba�lang�� pozisyonu.
        floorList.Add(curPos);
        while (floorList.Count < totalFloorCount)
        {
            curPos = TakeAHike(curPos); // Rastgele bir y�nde y�r�y�� yapar.
            RandomRoom(curPos);         // Bulunan pozisyonda bir oda olu�turur.
        }
        StartCoroutine(DelayProgress());
    }

    // Dolamba�l� yollar olu�turan algoritma.
    void WindingWalker()
    {
        Vector3 curPos = Vector3.zero; // Ba�lang�� pozisyonu.
        floorList.Add(curPos);
        while (floorList.Count < totalFloorCount)
        {
            curPos = TakeAHike(curPos); // Rastgele bir y�nde y�r�y�� yapar.
            int rool = Random.Range(0, 100);
            if (rool > windingHallPercent) // Belirli bir y�zde ile oda olu�turur.
            {
                RandomRoom(curPos);
            }
        }
        StartCoroutine(DelayProgress());
    }

    // Rastgele bir y�nde y�r�y�� yapar ve yeni pozisyonlar ekler.
    Vector3 TakeAHike(Vector3 myPos)
    {
        Vector3 walkDir = RandomDirection(); // Rastgele bir y�n se�ilir.
        int walkLength = Random.Range(9, 18); // Y�r�y�� uzunlu�u belirlenir.
        for (int i = 0; i < walkLength; i++)
        {
            if (!InFloorList(myPos + walkDir))
            {
                floorList.Add(myPos + walkDir); // Yeni pozisyon zemin listesine eklenir.
            }
            myPos += walkDir; // Pozisyon g�ncellenir.
        }
        return myPos;
    }

    // Rastgele boyutlarda bir oda olu�turur.
    void RandomRoom(Vector3 myPos)
    {
        int width = Random.Range(1, 5);  // Odan�n geni�li�i.
        int height = Random.Range(1, 5); // Odan�n y�ksekli�i.

        for (int w = -width; w <= height; w++)
        {
            for (int h = -height; h <= width; h++)
            {
                Vector3 offset = new Vector3(w, h, 0); // Her bir pozisyon i�in ofset belirlenir.
                if (!InFloorList(myPos + offset)) // E�er pozisyon zemin listesinde yoksa.
                {
                    floorList.Add(myPos + offset); // Listeye eklenir.
                }
            }
        }
    }

    // Pozisyonun zemin listesinde olup olmad���n� kontrol eder.
    bool InFloorList(Vector3 myPos)
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            if (Vector3.Equals(myPos, floorList[i])) // E�er pozisyon listede varsa.
            {
                return true; // Pozisyon bulunmu�tur.
            }
        }
        return false; // Pozisyon bulunamam��t�r.
    }

    // Rastgele bir y�n d�nd�r�r (yukar�, sa�, a�a��, sol).
    Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1: return Vector3.up;    // Yukar� y�n.
            case 2: return Vector3.right; // Sa� y�n.
            case 3: return Vector3.down;  // A�a�� y�n.
            case 4: return Vector3.left;  // Sol y�n.
        }
        return Vector3.zero; // Varsay�lan y�n.
    }

    // Zemin ve duvarlar�n olu�turulmas�n� geciktirir.
    IEnumerator DelayProgress()
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject; // Zemin olu�turulur.
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform); // Zemin zindan y�neticisine ba�lan�r.
        }
        while (FindObjectsOfType<TileSpawner>().Length > 0) // T�m spawner'lar tamamlanana kadar bekler.
        {
            yield return null;
        }
        yield return StartCoroutine(InsetExitDoorway()); // ��k�� kap�s�n� olu�turur.

        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask); // Zemin kontrol�.
                if (hitFloor)
                {
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1]))
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                        RandomItems(hitFloor, hitTop, hitRight, hitLeft, hitBottom); // Rastgele e�ya ekler.
                        RandomEnemies(hitFloor, hitTop, hitRight, hitLeft, hitBottom); // Rastgele d��man ekler.
                    }
                }
                RoundedEdges(x, y); // Yuvarlat�lm�� kenarlar� ekler.
            }
        }
    }

    // Yuvarlat�lm�� kenarlar� tamamlayan metot.
    void RoundedEdges(int x, int y)
    {
        if (useRoundedEdges) // E�er yuvarlat�lm�� kenarlar kullan�l�yorsa.
        {
            Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, wallMask); // Duvar kontrol�.
            if (hitWall)
            {
                Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                int bitVal = 0; // Kenar tipi i�in bit de�eri hesaplan�r.
                if (!hitTop) { bitVal += 1; }
                if (!hitRight) { bitVal += 2; }
                if (!hitBottom) { bitVal += 4; }
                if (!hitLeft) { bitVal += 8; }
                if (bitVal > 0)
                {
                    GameObject goEdge = Instantiate(roundedEdges[bitVal], new Vector2(x, y), Quaternion.identity) as GameObject; // Yuvarlat�lm�� kenar olu�turulur.
                    goEdge.name = roundedEdges[bitVal].name;
                    goEdge.transform.SetParent(hitWall.transform); // Kenar duvara ba�lan�r.
                }
            }
        }
    }

    // Rastgele d��man ekler.
    void RandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitLeft, Collider2D hitBottom)
    {
        if (!hitTop && !hitRight && !hitLeft && !hitBottom) // E�er t�m y�nler a��k ise.
        {
            int roll = Random.Range(1, 101); // Rastgele say� olu�turulur.
            if (roll <= enemySpawnPercent) // E�er say� d��man y�zdesine e�it veya k���kse.
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length); // Rastgele d��man se�ilir.
                GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goEnemy.name = randomEnemies[enemyIndex].name; // D��man ad� atan�r.
                goEnemy.transform.SetParent(hitFloor.transform); // D��man zemine ba�lan�r.
            }
        }
    }

    // Rastgele e�ya ekler.
    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitLeft, Collider2D hitBottom)
    {
        if ((hitTop || hitRight || hitLeft || hitBottom) && !(hitTop && hitBottom) && !(hitLeft && hitRight)) // Belirli bir ko�ulda e�ya eklenir.
        {
            int roll = Random.Range(1, 101); // Rastgele say� olu�turulur.
            if (roll <= itemSpawnPercent) // E�er say� e�ya y�zdesine e�it veya k���kse.
            {
                int itemIndex = Random.Range(0, randomItems.Length); // Rastgele e�ya se�ilir.
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goItem.name = randomItems[itemIndex].name; // E�ya ad� atan�r.
                goItem.transform.SetParent(hitFloor.transform); // E�ya zemine ba�lan�r.
            }
        }
    }

    // ��k�� kap�s�n� olu�turur.
    void ExitDoorway()
    {
        Vector3 doorPos = floorList[floorList.Count - 1]; // ��k�� kap�s� i�in son zemin pozisyonu se�ilir.
        GameObject goDoor = Instantiate(exitPrefab, doorPos, Quaternion.identity) as GameObject;
        goDoor.name = exitPrefab.name; // Kap� ad� atan�r.
        goDoor.transform.SetParent(transform); // Kap� zindan y�neticisine ba�lan�r.
    }

    // ��k�� kap�s�n� yerle�tiren coroutine.
    IEnumerator InsetExitDoorway()
    {
        Vector3 walkDir = RandomDirection(); // Rastgele bir y�n se�ilir.
        bool isExitPlaced = CheckExitCondition(floorList[floorList.Count - 1]); // ��k�� kap�s� yerle�tirme ko�ulu kontrol edilir.
        while (!isExitPlaced) // ��k�� kap�s� yerle�ene kadar devam eder.
        {
            Vector3 curPos = WalkStraight(walkDir); // Rastgele bir y�nde ilerler.
            yield return null; // Bir sonraki kareye kadar bekler.
            isExitPlaced = CheckExitCondition(curPos); // Yeni pozisyonda ko�ulu kontrol eder.
        }
        yield return null;
    }

    // ��k�� kap�s�n� yerle�tirmek i�in ko�ullar� kontrol eder.
    bool CheckExitCondition(Vector2 curPos)
    {
        int numWalls = 0;

        // D�rt y�ndeki duvar say�s�n� kontrol eder.
        if (Physics2D.OverlapBox(curPos + Vector2.up, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.right, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.down, hitSize, 0, wallMask)) { numWalls++; }
        if (Physics2D.OverlapBox(curPos + Vector2.left, hitSize, 0, wallMask)) { numWalls++; }

        if (numWalls == 3) // E�er �� taraf� duvarla �evrili ise.
        {
            ExitDoorway(); // ��k�� kap�s� olu�turulur.
            return true;
        }
        return false; // Aksi halde false d�ner.
    }

    // Rastgele bir y�nde d�z ilerler.
    Vector3 WalkStraight(Vector3 walkDir)
    {
        Vector3 myPos = floorList[floorList.Count - 1] + walkDir; // Yeni pozisyon hesaplan�r.

        if (InFloorList(myPos)) // E�er pozisyon zemin listesinde varsa.
        {
            floorList.Remove(myPos); // Pozisyon listeden ��kar�l�r.
        }
        else
        {
            Collider2D hitWall = Physics2D.OverlapBox(myPos, hitSize, 0, wallMask); // Duvar kontrol� yap�l�r.
            if (hitWall)
            {
                DestroyImmediate(hitWall.gameObject); // Duvar an�nda yok edilir.
            }
            GameObject goTile = Instantiate(tilePrefab, myPos, Quaternion.identity, transform) as GameObject; // Yeni zemin olu�turulur.
            goTile.name = tilePrefab.name;
        }
        floorList.Add(myPos); // Yeni pozisyon zemin listesine eklenir.
        return myPos; // Yeni pozisyon d�nd�r�l�r.
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden y�kler.
    }
}
