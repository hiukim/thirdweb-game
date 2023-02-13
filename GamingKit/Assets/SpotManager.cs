using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpotManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nPlayerText;

    [SerializeField]
    private TMP_Text valText;

    [SerializeField]
    private GameObject meIndicator;

    private bool isSelected;
    private int pos;

    private GameManager gameManager;

    public void Start()
    {
        this.SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        meIndicator.SetActive(this.isSelected);
    }

    public void SetNPlayerText(int nPlayer)
    {
        nPlayerText.text = nPlayer.ToString();
    }

    public void SetValText(int val)
    {
        valText.text = "x" + val.ToString();
    }

    public void SetPos(int pos)
    {
        this.pos = pos;
    }

    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void OnClick()
    {
        Debug.Log("Onclick: " + this.pos);
        this.gameManager.SpotClick(this.pos);
    }
}
