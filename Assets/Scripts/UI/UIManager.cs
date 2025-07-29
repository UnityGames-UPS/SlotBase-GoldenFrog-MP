using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
  [SerializeField]
  private Button About_Button;

  [Header("Popus UI")]
  [SerializeField]
  private GameObject MainPopup_Object;

  [Header("About Popup")]
  [SerializeField]
  private GameObject AboutPopup_Object;
  [SerializeField]
  private Button AboutExit_Button;

  [Header("Paytable Texts")]
  [SerializeField] private TMP_Text[] SymbolsText;
  [SerializeField] private TMP_Text Scatter_Text;
  [SerializeField] private TMP_Text Wild_Text;

  [Header("Settings Popup")]
  [SerializeField] private GameObject SettingsPopup_Object;
  [SerializeField] private Button Settings_Button;
  [SerializeField] private Button SettingsExit_Button;
  [SerializeField] private Button SoundOn_Button;
  [SerializeField] private Button SoundOff_Button;
  [SerializeField] private Button MusicOn_Button;
  [SerializeField] private Button MusicOff_Button;

  [Header("all Win Popup")]
  [SerializeField]
  private Sprite BigWin_Sprite;
  [SerializeField]
  private Sprite HugeWin_Sprite;
  [SerializeField]
  private Sprite MegaWin_Sprite;
  [SerializeField]
  private Image Win_Image;
  [SerializeField]
  private GameObject WinPopup_Object;
  [SerializeField]
  private TMP_Text Win_Text;

  [Header("jackpot Win Popup")]
  [SerializeField] private TMP_Text jackpot_Text;
  [SerializeField] private GameObject jackpot_Object;

  [Header("low balance popup")]
  [SerializeField] private GameObject LowBalancePopup_Object;
  [SerializeField] private Button Close_Button;

  [Header("Scripts")]
  [SerializeField] private AudioController audioController;
  [SerializeField] private SlotBehaviour slotManager;
  [SerializeField] private SocketIOManager socketManager;

  [Header("Reconnection popup")]
  [SerializeField] private GameObject ReconnectPopup_Object;

  [Header("disconnection popup")]
  [SerializeField] private Button CloseDisconnect_Button;
  [SerializeField] private GameObject DisconnectPopup_Object;

  [Header("Quit Popup")]
  [SerializeField] private GameObject QuitPopupObject;
  [SerializeField] private Button yes_Button;
  [SerializeField] private Button GameExit_Button;
  [SerializeField] private Button no_Button;

  [Header("Splash Screen")]
  [SerializeField] private GameObject spalsh_screen;
  [SerializeField] private Image progressbar;
  [SerializeField] private TMP_Text loadingText;
  [SerializeField]
  private Button QuitSplash_button;

  [Header("AnotherDevice Popup")]
  [SerializeField] private Button CloseAD_Button;
  [SerializeField] private GameObject ADPopup_Object;

  [Header("Pagination")]
  int CurrentIndex = 0;
  [SerializeField] private GameObject[] paytableList;
  [SerializeField] private Button RightBtn;
  [SerializeField] private Button LeftBtn;

  private bool isMusic = true;
  private bool isSound = true;
  private bool isExit = false;
  private Tween WinPopupTextTween;
  private Tween ClosePopupTween;
  [SerializeField] private Button SkipWinAnimation;
  [SerializeField] private Button SkipJackpotAnimation;


  private void Start()
  {
    if (yes_Button) yes_Button.onClick.RemoveAllListeners();
    if (yes_Button) yes_Button.onClick.AddListener(CallOnExitFunction);

    if (no_Button) no_Button.onClick.RemoveAllListeners();
    if (no_Button) no_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopupObject); } });

    if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
    if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopupObject); });

    if (About_Button) About_Button.onClick.RemoveAllListeners();
    if (About_Button) About_Button.onClick.AddListener(delegate { OpenPopup(AboutPopup_Object); });

    if (AboutExit_Button) AboutExit_Button.onClick.RemoveAllListeners();
    if (AboutExit_Button) AboutExit_Button.onClick.AddListener(delegate { ClosePopup(AboutPopup_Object); });

    if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
    if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

    if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
    if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

    if (MusicOn_Button) MusicOn_Button.onClick.RemoveAllListeners();
    if (MusicOn_Button) MusicOn_Button.onClick.AddListener(ToggleMusic);

    if (MusicOff_Button) MusicOff_Button.onClick.RemoveAllListeners();
    if (MusicOff_Button) MusicOff_Button.onClick.AddListener(ToggleMusic);

    if (SoundOn_Button) SoundOn_Button.onClick.RemoveAllListeners();
    if (SoundOn_Button) SoundOn_Button.onClick.AddListener(ToggleSound);

    if (SoundOff_Button) SoundOff_Button.onClick.RemoveAllListeners();
    if (SoundOff_Button) SoundOff_Button.onClick.AddListener(ToggleSound);

    if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
    if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(SkipWin);

    if (SkipJackpotAnimation) SkipJackpotAnimation.onClick.RemoveAllListeners();
    if (SkipJackpotAnimation) SkipJackpotAnimation.onClick.AddListener(SkipWin);

    paytableList[CurrentIndex = 0].SetActive(true);

    if (LeftBtn) LeftBtn.onClick.RemoveAllListeners();
    if (LeftBtn) LeftBtn.onClick.AddListener(delegate { Slide(-1); });

    if (RightBtn) RightBtn.onClick.RemoveAllListeners();
    if (RightBtn) RightBtn.onClick.AddListener(delegate { Slide(+1); });

    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); });

    if (Close_Button) Close_Button.onClick.AddListener(delegate { ClosePopup(LowBalancePopup_Object); });

    if (QuitSplash_button) QuitSplash_button.onClick.RemoveAllListeners();
    if (QuitSplash_button) QuitSplash_button.onClick.AddListener(delegate { OpenPopup(QuitPopupObject); });

    isMusic = false;
    isSound = false;
    ToggleMusic();
    ToggleSound();
  }

  void SkipWin()
  {
    Debug.Log("Skip win called");
    if (ClosePopupTween != null)
    {
      ClosePopupTween.Kill();
      ClosePopupTween = null;
    }
    if (WinPopupTextTween != null)
    {
      WinPopupTextTween.Kill();
      WinPopupTextTween = null;
    }
    ClosePopup(WinPopup_Object);
    ClosePopup(jackpot_Object);
    slotManager.CheckPopups = false;
  }

  internal void PopulateWin(int value, double amount)
  {
    switch (value)
    {
      case 1:
        if (Win_Image) Win_Image.sprite = BigWin_Sprite;
        break;
      case 2:
        if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
        break;
      case 3:
        if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
        break;

    }

    if (value == 4)
      StartPopupAnim(amount, true);
    else
      StartPopupAnim(amount, false);

  }

  private void StartPopupAnim(double amount, bool jackpot = false)
  {
    double initAmount = 0;
    if (jackpot)
    {
      if (jackpot_Object) jackpot_Object.SetActive(true);
    }
    else
    {
      if (WinPopup_Object) WinPopup_Object.SetActive(true);

    }

    if (MainPopup_Object) MainPopup_Object.SetActive(true);

    WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, amount, 5f).OnUpdate(() =>
    {
      if (jackpot)
      {
        if (jackpot_Text) jackpot_Text.text = initAmount.ToString("F3");
      }
      else
      {
        if (Win_Text) Win_Text.text = initAmount.ToString("F3");

      }
    });

    ClosePopupTween = DOVirtual.DelayedCall(6f, () =>
    {
      if (jackpot)
      {
        ClosePopup(jackpot_Object);

      }
      else
      {
        ClosePopup(WinPopup_Object);
      }
      slotManager.CheckPopups = false;
    });
  }

  internal void LowBalPopup()
  {

    OpenPopup(LowBalancePopup_Object);
  }

  internal void InitialiseUIData(Paylines symbolsText)
  {
    PopulateSymbolsPayout(symbolsText);
  }

  internal void ADfunction()
  {
    OpenPopup(ADPopup_Object);
  }

  private void PopulateSymbolsPayout(Paylines paylines)
  {
    for (int i = 0; i < SymbolsText.Length; i++)
    {
      string text = null;
      if (paylines.symbols[i].multiplier[0] != 0)
      {
        text += "5x - " + paylines.symbols[i].multiplier[0] + "x";
      }
      if (paylines.symbols[i].multiplier[1] != 0)
      {
        text += "\n4x - " + paylines.symbols[i].multiplier[1] + "x";
      }
      if (paylines.symbols[i].multiplier[2] != 0)
      {
        text += "\n3x - " + paylines.symbols[i].multiplier[2] + "x";
      }
      if (SymbolsText[i]) SymbolsText[i].text = text;
    }

    for (int i = 0; i < paylines.symbols.Count; i++)
    {

      if (paylines.symbols[i].name.ToUpper() == "SCATTER")
      {
        if (Scatter_Text) Scatter_Text.text = paylines.symbols[i].description.ToString();
      }

      if (paylines.symbols[i].name.ToUpper() == "WILD")
      {
        if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
      }
    }
  }

  private void CallOnExitFunction()
  {
    if (!isExit)
    {
      isExit = true;
      audioController.PlayButtonAudio();
      StartCoroutine(socketManager.CloseSocket());
    }
  }

  private void OpenPopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(true);
    if (MainPopup_Object) MainPopup_Object.SetActive(true);
    paytableList[CurrentIndex = 0].SetActive(true);
  }

  private void ClosePopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(false);
    if (MainPopup_Object) MainPopup_Object.SetActive(false);
    paytableList[CurrentIndex].SetActive(false);
  }

  private void Slide(int direction)
  {
    if (audioController) audioController.PlayButtonAudio();

    if (CurrentIndex < paytableList.Length - 1 && direction > 0)
    {
      // Move to the next item
      paytableList[CurrentIndex].SetActive(false);
      paytableList[CurrentIndex + 1].SetActive(true);

      CurrentIndex++;
    }
    else if (CurrentIndex >= 1 && direction < 0)
    {
      // Move to the previous item
      paytableList[CurrentIndex].SetActive(false);
      paytableList[CurrentIndex - 1].SetActive(true);

      CurrentIndex--;
    }
    if (CurrentIndex == paytableList.Length - 1)
    {
      RightBtn.interactable = false;
    }
    else
    {
      RightBtn.interactable = true;

    }
    if (CurrentIndex == 0)
    {
      LeftBtn.interactable = false;
    }
    else
    {
      LeftBtn.interactable = true;
    }

  }

  internal void CheckAndClosePopups()
  {
    if (ReconnectPopup_Object.activeInHierarchy)
    {
      ClosePopup(ReconnectPopup_Object);
    }
    if (DisconnectPopup_Object.activeInHierarchy)
    {
      ClosePopup(DisconnectPopup_Object);
    }
  }

  internal void DisconnectionPopup()
  {
    if (!isExit)
    {
      OpenPopup(DisconnectPopup_Object);
    }
  }

  internal void ReconnectionPopup()
  {
    OpenPopup(ReconnectPopup_Object);
  }

  private void ToggleMusic()
  {
    isMusic = !isMusic;
    if (isMusic)
    {
      if (MusicOn_Button) MusicOn_Button.interactable = false;
      if (MusicOff_Button) MusicOff_Button.interactable = true;
      audioController.ToggleMute(false, "bg");
    }
    else
    {
      if (MusicOn_Button) MusicOn_Button.interactable = true;
      if (MusicOff_Button) MusicOff_Button.interactable = false;
      audioController.ToggleMute(true, "bg");
    }
  }

  private void ToggleSound()
  {
    isSound = !isSound;
    if (isSound)
    {
      if (SoundOn_Button) SoundOn_Button.interactable = false;
      if (SoundOff_Button) SoundOff_Button.interactable = true;
      if (audioController) audioController.ToggleMute(false, "button");
      if (audioController) audioController.ToggleMute(false, "wl");
    }
    else
    {
      if (SoundOn_Button) SoundOn_Button.interactable = true;
      if (SoundOff_Button) SoundOff_Button.interactable = false;
      if (audioController) audioController.ToggleMute(true, "button");
      if (audioController) audioController.ToggleMute(true, "wl");
    }
  }
}
