using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Button btnWin;
    [SerializeField] private Button btnLose;
    [SerializeField] private Button btnTimeAttack;
    [SerializeField] private GameObject UIWinGame;
    [SerializeField] private GameObject UILoseGame;
    [SerializeField] private Text timer;

    private void Start() {
        btnWin.onClick.AddListener(() => {
            TileManager.Instance.AutoWin();
        });

        btnLose.onClick.AddListener(() => {
            TileManager.Instance.AutoLose();
        });
        btnTimeAttack.onClick.AddListener(() =>
        {
            timer.gameObject.SetActive(true);
            TileManager.Instance.StartTimeAttackMode();
        });
    }

    public void ShowUIWin() {
        UIWinGame.SetActive(true);
    }

    public void ShowUILose() {
        UILoseGame.SetActive(true);
    }

    public void CountDownTime(int value)
    {
        timer.text = "Time: " + value.ToString(); 
    }
}
