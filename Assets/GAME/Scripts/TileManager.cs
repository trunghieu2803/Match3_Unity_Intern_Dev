using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : Singleton<TileManager> {
    [SerializeField] private int boundSizeX, boundSizeY;
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Transform tileHolder;
    Camera m_cam;
    private Tile[,] tiles;
    private bool[,] tileStates;
    Vector3 origin;
    public List<Transform> holderSlots;
    private List<Tile> selectedTiles = new List<Tile>();
    private bool m_gameOver;
    private bool m_WinGame;
    private bool isAuto;
    int count = 0;
    private bool isTimeAttackMode;
    private float timeRemaining;

    private void Start() {
        tiles = new Tile[boundSizeX, boundSizeY];
        tileStates = new bool[boundSizeX, boundSizeY];
        origin = new Vector3(-boundSizeX * 0.5f + 0.5f, -boundSizeY * 0.5f + 0.5f, 0f);
        m_cam = Camera.main;
        CreateBoard();
        CreateBoardTileHolder();
        Fill();
    }

    public void StartTimeAttackMode()
    {
        isTimeAttackMode = true;
        timeRemaining = 60f;
        StartCoroutine(TimeAttackTimer());
    }

    private IEnumerator TimeAttackTimer()
    {
        while (timeRemaining > 0 && isTimeAttackMode)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
            UIManager.Instance.CountDownTime((int)timeRemaining);
        }
        if (!IsBoardEmpty() && isTimeAttackMode)
        {
            m_gameOver = true;
            UIManager.Instance.ShowUILose();
            tileHolder.gameObject.SetActive(false);
        }
    }

    private void CreateBoard() {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_TILE_BACKGROUND);
        for (int x = 0; x < boundSizeX; x++) {
            for (int y = 0; y < boundSizeY; y++) {
                GameObject go = Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(this.transform);
                go.name = "BG " + x + "_" + y;
                Tile tile = go.GetComponent<Tile>();
                tile.Setup(x, y);
                tiles[x, y] = tile;
                tiles[x, y].setPos(go.transform.position);
            }
        }
    }

    private void Fill() {
        int titalTile = (boundSizeX * boundSizeY) / 3;
        Stack<int> exitNumber = new Stack<int>();

        for (int i = 0; i < titalTile; i++) {
            exitNumber.Push(i);
        }

        int index = 0;
        int t = 0;
        while (exitNumber.Count != 0) {
            int temp = exitNumber.Pop();
            if (index >= prefabs.Length)
                t = Random.Range(0, prefabs.Length);

            for (int i = 0; i < 3; i++) {
                int x = Random.Range(0, boundSizeX);
                int y = Random.Range(0, boundSizeY);
                while (tileStates[x, y]) {
                    x = Random.Range(0, boundSizeX);
                    y = Random.Range(0, boundSizeY);
                }

                GameObject g = Instantiate(index >= prefabs.Length ? prefabs[t] : prefabs[index]);
                g.transform.position = origin + new Vector3(x, y, 0);
                g.transform.SetParent(tiles[x, y].transform);
                g.name = "fish " + x + " " + y;
                tileStates[x, y] = true;
                tiles[x, y].SetID(index >= prefabs.Length ? t : index);
            }
            index++;
        }
    }

    private void CreateBoardTileHolder() {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_TILE_BACKGROUND);
        for (int i = 0; i < 5; i++) {
            GameObject go = Instantiate(prefabBG);
            go.GetComponent<Collider2D>().enabled = false;
            go.transform.SetParent(tileHolder);
            go.transform.localPosition = new Vector3(i + origin.x - 0.5f, 0, 0);
            holderSlots.Add(go.transform);
        }
    }

    private void Update() {
        if (m_WinGame) return;
        if (m_gameOver) return;
        if (isAuto) return;

        if (Input.GetMouseButtonDown(0)) {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null) {
                Tile selectedTile = hit.transform.GetComponent<Tile>();
                if (selectedTiles.Count < holderSlots.Count  && !isTimeAttackMode)
                {
                    SelectTile(selectedTile);
                }
                else
                {
                    if(tileStates[selectedTile.BoardX, selectedTile.BoardY])
                    {
                        SelectTile(selectedTile);
                    }
                    else
                    {
                        ReturnTileToBoard(selectedTile);
                    }
                }
            }
        }

        if (tiles.Length == count) {
            UIManager.Instance.ShowUIWin();
            m_WinGame = true;
            tileHolder.gameObject.SetActive(false);
        }
    }

    private void InsertTileIntoHolder(Tile newTile) {
        int insertIndex = selectedTiles.Count;
        for (int i = selectedTiles.Count - 1; i >= 0; i--) {
            if (selectedTiles[i].ID == newTile.ID) {
                insertIndex = i + 1;
                break;
            }
        }
        selectedTiles.Insert(insertIndex, newTile);
    }

    private void UpdateHolderPositions() {
        for (int i = 0; i < selectedTiles.Count; i++) {
            selectedTiles[i].transform.DOMove(holderSlots[i].position, 0.5f).SetEase(Ease.Linear);
        }
    }

    private void SelectTile(Tile tile)
    {
        if (selectedTiles.Count < holderSlots.Count)
        {
            count++;
            if(!isTimeAttackMode)
                tile.GetComponent<Collider2D>().enabled = false;
            Vector3 pos = tile.transform.position;
            pos.z = -5;
            tile.transform.position = pos;
            InsertTileIntoHolder(tile);
            UpdateHolderPositions();
            tileStates[tile.BoardX, tile.BoardY] = false;
            Check(selectedTiles);
            if (selectedTiles.Count == holderSlots.Count && !isTimeAttackMode)
            {
                if (!CanMatch())
                {
                    m_gameOver = true;
                    UIManager.Instance.ShowUILose();
                    tileHolder.gameObject.SetActive(false);
                }
            }
        }

        if (tiles.Length == count)
        {
            UIManager.Instance.ShowUIWin();
            m_WinGame = true;
            tileHolder.gameObject.SetActive(false);
        }
    }
    private void ReturnTileToBoard(Tile tile)
    {
        count--;
        selectedTiles.Remove(tile);
        tile.transform.DOMove(tiles[tile.BoardX, tile.BoardY].pos, 0.5f).SetEase(Ease.Linear);
        tile.GetComponent<Collider2D>().enabled = true;
        tileStates[tile.BoardX, tile.BoardY] = true;
        UpdateHolderPositions();
    }

    public void Check(List<Tile> selectedTiles) {
        var groups = selectedTiles.GroupBy(tile => tile.ID)
                                 .Where(group => group.Count() >= 3)
                                 .ToList();

        foreach (var group in groups) {
            List<Tile> tilesToRemove = group.Take(3).ToList();

            foreach (var tile in tilesToRemove) {
                tile.transform.DOScale(Vector3.zero, 0.5f)
                              .SetEase(Ease.InBack)
                              .OnComplete(() => {
                                  selectedTiles.Remove(tile);
                                  Destroy(tile.gameObject);
                              });
            }
        }

        DOVirtual.DelayedCall(0.5f, () => {
            UpdateHolderPositions();
        });
    }


    private bool CanMatch() {
        var groups = selectedTiles.GroupBy(tile => tile.ID).Where(group => group.Count() >= 3).ToList();
        return groups.Count > 0;
    }
    #region Win
    public void AutoWin() {
        isAuto = true;
        StartCoroutine(AutoWinCoroutine());
    }

    private IEnumerator AutoWinCoroutine() {
        while (!IsBoardEmpty()) {
            Tile firstTile = FindTileOnBoard();
            if (firstTile == null) break;
            int targetID = firstTile.ID;
            SelectTile(firstTile);

            for (int i = 0; i < 2; i++) {
                Tile nextTile = FindTileWithID(targetID);
                if (nextTile != null) {
                    yield return new WaitForSeconds(0.5f);
                    SelectTile(nextTile);
                }
                else {
                    Debug.LogError("Không đủ ô với ID " + targetID);
                    yield break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }


    private Tile FindTileWithID(int id) {
        for (int x = 0; x < boundSizeX; x++) {
            for (int y = 0; y < boundSizeY; y++) {
                if (tileStates[x, y] && tiles[x, y].ID == id && tiles[x,y].GetComponent<Collider2D>().enabled) {
                    return tiles[x, y];
                }
            }
        }
        return null;
    }

    private Tile FindTileOnBoard() {
        input:
        int x = Random.Range(0, boundSizeX);
        int y = Random.Range(0, boundSizeY);
        if (!IsBoardEmpty())
        {
            if (tileStates[x, y] && tiles[x, y].GetComponent<Collider2D>().enabled)
            {
                return tiles[x, y];
            }
            goto input;
        }
      
        return null;
    }

    private bool IsBoardEmpty() {
        for (int x = 0; x < boundSizeX; x++) {
            for (int y = 0; y < boundSizeY; y++) {
                if (tileStates[x, y]) {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region AutoLose
    public void AutoLose() {
        isAuto = true;
        StartCoroutine(AutoLoseCoroutine());
    }
    
    private IEnumerator AutoLoseCoroutine() {
        while (!IsBoardEmpty()) {
            Tile firstTile = FindTileOnBoard();
            if (firstTile == null) break;
            int targetID = firstTile.ID;
            SelectTile(firstTile);

            for (int i = 0; i < 2; i++) {
                Tile nextTile = FindTileNotWithID(targetID);
                if (nextTile != null) {
                    yield return new WaitForSeconds(0.5f);
                    SelectTile(nextTile);
                }
                else {
                    Debug.LogError("Không đủ ô với ID " + targetID);
                    yield break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private Tile FindTileNotWithID(int id) {
        for (int x = 0; x < boundSizeX; x++) {
            for (int y = 0; y < boundSizeY; y++) {
                if (tileStates[x, y] && tiles[x, y].ID != id && tiles[x, y].GetComponent<Collider2D>().enabled) {
                    return tiles[x, y];
                }
            }
        }
        return null;
    }
    #endregion

    
}