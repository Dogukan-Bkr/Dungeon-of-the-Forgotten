using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyController s�n�f�, d��man davran��lar�n� kontrol eder.
public class EnemyController : MonoBehaviour
{
    PlayerController player; // Oyuncunun referans�.
    public Vector2 patrolInterval; // Devriye s�ras�nda bekleme s�resi aral���.
    public Vector2 dmgRange; // Sald�r� s�ras�nda yap�lacak hasar aral���.
    LayerMask obstacleMask, walkableMask; // Engel ve y�r�nebilir alan maskeleri.
    Vector2 curPos; // D��man�n mevcut pozisyonu.
    List<Vector2> availableMovementList = new List<Vector2>(); // Mevcut hareket y�nleri.
    List<Node> nodesList = new List<Node>(); // Yol bulma d���mleri.
    bool isMoving; // D��man�n hareket edip etmedi�ini kontrol eder.
    public float alertRange; // Oyuncunun fark edilece�i mesafe.
    public float chaseSpeed; // Takip h�z�.

    // Ba�lang�� metodu, d��man davran��lar�n� ba�lat�r.
    void Start()
    {
        player = FindObjectOfType<PlayerController>(); // Oyuncu referans�n� bulur.
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player"); // Engel maskesi.
        walkableMask = LayerMask.GetMask("Wall", "Enemy"); // Y�r�nebilir alan maskesi.
        curPos = transform.position; // Ba�lang�� pozisyonu.
        StartCoroutine(Movement()); // D��man hareketini ba�lat�r.
    }

    // Devriye yapma metodu.
    void Patrol()
    {
        availableMovementList.Clear(); // Mevcut hareket listesi temizlenir.
        Vector2 size = Vector2.one * 0.8f; // �arp��ma kutusu boyutu.

        // Yukar�, sa�, a�a�� ve sol y�nlerde hareket kontrol� yap�l�r.
        Collider2D hitUp = Physics2D.OverlapBox(curPos + Vector2.up, size, 0, obstacleMask);
        if (!hitUp) { availableMovementList.Add(Vector2.up); } // E�er yukar�da engel yoksa hareket eklenir.

        Collider2D hitRight = Physics2D.OverlapBox(curPos + Vector2.right, size, 0, obstacleMask);
        if (!hitRight) { availableMovementList.Add(Vector2.right); }

        Collider2D hitDown = Physics2D.OverlapBox(curPos + Vector2.down, size, 0, obstacleMask);
        if (!hitDown) { availableMovementList.Add(Vector2.down); }

        Collider2D hitLeft = Physics2D.OverlapBox(curPos + Vector2.left, size, 0, obstacleMask);
        if (!hitLeft) { availableMovementList.Add(Vector2.left); }

        // Mevcut y�nlerden rastgele birini se�erek hareket eder.
        if (availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            curPos += availableMovementList[randomIndex];
        }

        // P�r�zs�z hareket sa�lar.
        StartCoroutine(SmoothMove(Random.Range(patrolInterval.x, patrolInterval.y)));
    }

    // D��man�n sald�r� yapmas�n� sa�layan metot.
    void Attack()
    {
        int roll = Random.Range(0, 100); // Sald�r� �ans� i�in rastgele bir say�.
        if (roll > 50) // %50 �ansla sald�r�r.
        {
            float dmgAmount = Mathf.Ceil(Random.Range(dmgRange.x, dmgRange.y)); // Hasar miktar�.
            Debug.Log(name + " attacked and hit for " + dmgAmount + " points of damage"); // Konsola hasar bilgisi yazd�r�l�r.
        }
        else
        {
            Debug.Log(name + " attacked and missed"); // Sald�r� ba�ar�s�z olursa.
        }
    }

    // Yol bulma algoritmas�nda d���m kontrol� yapan metot.
    void CheckNode(Vector2 chkPoint, Vector2 parent)
    {
        Vector2 size = Vector2.one * 0.5f; // �arp��ma kutusu boyutu.
        Collider2D hit = Physics2D.OverlapBox(chkPoint, size, 0, walkableMask);
        if (!hit)
        {
            nodesList.Add(new Node(chkPoint, parent)); // E�er pozisyonda engel yoksa d���m eklenir.
        }
    }

    // Oyuncuya do�ru bir sonraki ad�m� bulur.
    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        int listIndex = 0; // D���m listesi indeksi.
        Vector2 myPos = startPos; // Ba�lang�� pozisyonu.
        nodesList.Clear(); // Mevcut d���m listesi temizlenir.
        nodesList.Add(new Node(startPos, targetPos)); // Ba�lang�� d���m� eklenir.

        // Hedefe ula�ana veya d���m listesi t�kenene kadar devam eder.
        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0)
        {
            // Kom�u d���mleri kontrol eder.
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);

            listIndex++;
            if (listIndex < nodesList.Count)
            {
                myPos = nodesList[listIndex].position; // Yeni pozisyona ilerler.
            }
        }

        // E�er hedefe ula��lm��sa, yolun bir ad�m�n� d�nd�r�r.
        if (myPos == targetPos)
        {
            nodesList.Reverse(); // D���m listesi ters �evrilir.
            for (int i = 0; i < nodesList.Count; i++)
            {
                if (myPos == nodesList[i].position)
                {
                    if (nodesList[i].parent == startPos)
                    {
                        return myPos; // Bir sonraki ad�m� d�nd�r�r.
                    }
                    myPos = nodesList[i].parent; // Bir �nceki d���me geri d�ner.
                }
            }
        }
        return startPos; // E�er yol bulunamazsa ba�lang�� pozisyonunu d�nd�r�r.
    }

    // P�r�zs�z hareket sa�layan coroutine.
    IEnumerator SmoothMove(float speed)
    {
        isMoving = true; // Hareket durumu aktif edilir.

        // Hedef pozisyona ula�ana kadar hareket eder.
        while (Vector2.Distance(transform.position, curPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, curPos, 5f * Time.deltaTime); // Pozisyon g�ncellenir.
            yield return null; // Bir sonraki kareye kadar bekler.
        }

        transform.position = curPos; // Son pozisyon ayarlan�r.
        yield return new WaitForSeconds(speed); // Hareket aral��� kadar bekler.

        isMoving = false; // Hareket durumu kapat�l�r.
    }

    // D��man�n hareketlerini y�neten coroutine.
    IEnumerator Movement()
    {
        while (true) // S�rekli devam eden d�ng�.
        {
            yield return new WaitForSeconds(0.1f); // Her ad�m aras�nda k�sa bir bekleme.

            if (!isMoving) // E�er d��man hareket etmiyorsa:
            {
                float dist = Vector2.Distance(transform.position, player.transform.position); // Oyuncuya olan mesafeyi hesaplar.

                if (dist <= alertRange) // E�er oyuncu belirli bir mesafedeyse:
                {
                    if (dist <= 1.1f) // Oyuncu �ok yak�nsa sald�r�r.
                    {
                        Attack();
                        yield return new WaitForSeconds(Random.Range(0.5f, 1.15f)); // Sald�r� aral��� kadar bekler.
                    }
                    else // Oyuncu mesafedeyse onu takip eder.
                    {
                        Vector2 newPos = FindNextStep(transform.position, player.transform.position);
                        if (newPos != curPos)
                        {
                            curPos = newPos; // Yeni hedef pozisyon belirlenir.
                            StartCoroutine(SmoothMove(chaseSpeed));
                        }
                        else
                        {
                            Patrol(); // E�er oyuncuya ula�amazsa devriye yapar.
                        }
                    }
                }
                else
                {
                    Patrol(); // Oyuncu uzaktaysa devriye yapar.
                }
            }
        }
    }
}
