using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
  [Header("Sprites")]
  [SerializeField]
  private Sprite[] myImages;  //images taken initially

  [Header("Slot Images")]
  [SerializeField]
  private List<SlotImage> images;     //class to store total images
  [SerializeField]
  private List<SlotImage> Tempimages;     //class to store the result matrix
  [SerializeField] Sprite[] TurboToggleSprites;

  [Header("Slots Transforms")]
  [SerializeField]
  private Transform[] Slot_Transform;
  private Dictionary<int, string> x_string = new Dictionary<int, string>();
  private Dictionary<int, string> y_string = new Dictionary<int, string>();

  [Header("Buttons")]
  [SerializeField]
  private Button SlotStart_Button;
  [SerializeField]
  private Button AutoSpin_Button;
  [SerializeField] private Button AutoSpinStop_Button;
  [SerializeField] private Button Maxbet_button;
  [SerializeField] private Button Turbo_Button;
  [SerializeField] private Button StopSpin_Button;

  [Header("Animated Sprites")]
  [SerializeField]
  private Sprite[] Coin_Sprite;
  [SerializeField]
  private Sprite[] Frog_Sprite;
  [SerializeField]
  private Sprite[] Turtle_Sprite;
  [SerializeField]
  private Sprite[] Cap_Sprite;
  [SerializeField]
  private Sprite[] Fish_Sprite;
  [SerializeField]
  private Sprite[] Ten_Sprite;
  [SerializeField]
  private Sprite[] A_Sprite;
  [SerializeField]
  private Sprite[] J_Sprite;
  [SerializeField]
  private Sprite[] K_Sprite;
  [SerializeField]
  private Sprite[] Q_Sprite;
  [SerializeField]
  private Sprite[] Scatter_Sprite;

  [Header("Miscellaneous UI")]
  [SerializeField]
  private TMP_Text Balance_text;
  [SerializeField]
  private TMP_Text TotalBet_text;
  [SerializeField]
  private Button MaxBet_Button;
  [SerializeField]
  private Button BetPlus_Button;
  [SerializeField]
  private Button BetMinus_Button;
  [SerializeField]
  private TMP_Text TotalWin_text;
  [SerializeField]
  private TMP_Text BetPerLine_text;
  [SerializeField] private TMP_Text Total_lines;

  [Header("Audio Management")]
  [SerializeField] private AudioController audioController;
  int tweenHeight = 0;  //calculate the height at which tweening is done

  [SerializeField]
  private GameObject Image_Prefab;    //icons prefab

  [SerializeField]
  private PayoutCalculation PayCalculator;

  private List<Tweener> alltweens = new List<Tweener>();
  private Tweener WinTween = null;
  private Tween BalanceTween;
  [SerializeField]
  private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 
  [SerializeField]
  private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
  private int numberOfSlots = 5;          //number of columns
  [SerializeField] private SocketIOManager SocketManager;
  [SerializeField] private UIManager uiManager;
  private Coroutine AutoSpinRoutine = null;
  private Coroutine tweenroutine = null;
  private bool IsAutoSpin = false;
  private bool IsSpinning = false;
  private bool CheckSpinAudio = false;
  [SerializeField]
  private int spacefactor;
  private int BetCounter = 0;
  internal bool CheckPopups;
  private double currentBalance = 0;
  private double currentTotalBet = 0;
  private bool StopSpinToggle;
  private float SpinDelay = 0.2f;
  private bool IsTurboOn;

  private void Start()
  {
    IsAutoSpin = false;
    if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
    if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

    if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
    if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

    if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
    if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
    if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
    if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

    if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
    if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

    if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
    if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

    if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
    if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });

    tweenHeight = (16 * IconSizeFactor) - 280;
  }

  void TurboToggle()
  {
    audioController.PlayButtonAudio();
    if (IsTurboOn)
    {
      IsTurboOn = false;
      Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
      Turbo_Button.image.sprite = TurboToggleSprites[0];
    }
    else
    {
      IsTurboOn = true;
      Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
    }
  }

  private void AutoSpin()
  {
    if (!IsAutoSpin)
    {
      IsAutoSpin = true;
      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        AutoSpinRoutine = null;
      }
      AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
    }
  }

  private void StopAutoSpin()
  {
    if (IsAutoSpin)
    {
      IsAutoSpin = false;
      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
      StartCoroutine(StopAutoSpinCoroutine());
    }
  }

  private IEnumerator AutoSpinCoroutine()
  {
    while (IsAutoSpin)
    {
      StartSlots(IsAutoSpin);
      yield return tweenroutine;
      yield return new WaitForSeconds(SpinDelay);
    }
  }


  private IEnumerator StopAutoSpinCoroutine()
  {
    yield return new WaitUntil(() => !IsSpinning);
    ToggleButtonGrp(true);
    if (AutoSpinRoutine != null || tweenroutine != null)
    {
      StopCoroutine(AutoSpinRoutine);
      StopCoroutine(tweenroutine);
      tweenroutine = null;
      AutoSpinRoutine = null;
      StopCoroutine(StopAutoSpinCoroutine());
    }
  }

  void OnBetOne(bool IncDec)
  {
    if (audioController) audioController.PlayButtonAudio();

    if (BetCounter < SocketManager.initData.gameData.bets.Count - 1)
    {
      BetCounter++;
    }
    else
    {
      BetCounter = 0;
    }
    Debug.Log("Index:" + BetCounter);

    if (TotalBet_text) TotalBet_text.text = SocketManager.initData.gameData.bets[BetCounter].ToString();
    if (BetPerLine_text) BetPerLine_text.text = SocketManager.initData.gameData.bets[BetCounter].ToString();
  }

  private void ChangeBet(bool IncDec)
  {
    if (audioController) audioController.PlayButtonAudio();

    if (IncDec)
    {
      if (BetCounter < SocketManager.initData.gameData.bets.Count - 1)
      {
        BetCounter++;
      }
      else
      {
        BetCounter = 0;
      }
    }
    else
    {
      if (BetCounter > 0)
      {
        BetCounter--;
      }
      else
      {
        BetCounter = SocketManager.initData.gameData.bets.Count - 1;
      }
    }

    if (BetPerLine_text) BetPerLine_text.text = SocketManager.initData.gameData.bets[BetCounter].ToString();
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count).ToString();
    currentTotalBet = SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count;

  }

  private void MaxBet()
  {
    if (audioController) audioController.PlayButtonAudio();
    BetCounter = SocketManager.initData.gameData.bets.Count - 1;
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count).ToString();
    if (BetPerLine_text) BetPerLine_text.text = SocketManager.initData.gameData.bets[BetCounter].ToString();
    currentTotalBet = SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count;

  }

  //Fetch Lines from backend
  internal void FetchLines(string LineVal, int count)
  {
    y_string.Add(count + 1, LineVal);
  }

  //Generate Static Lines from button hovers
  internal void GenerateStaticLine(TMP_Text LineID_Text)
  {
    DestroyStaticLine();
    int LineID = 1;
    try
    {
      LineID = int.Parse(LineID_Text.text);
    }
    catch (Exception e)
    {
      Debug.Log("Exception while parsing " + e.Message);
    }
    List<int> x_points = null;
    List<int> y_points = null;
    x_points = x_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
    y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
    PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
  }

  //Destroy Static Lines from button hovers
  internal void DestroyStaticLine()
  {
    PayCalculator.ResetStaticLine();
  }

  internal void SetInitialUI()
  {
    shuffleInitialMatrix();
    BetCounter = 0;
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count).ToString();
    if (BetPerLine_text) BetPerLine_text.text = SocketManager.initData.gameData.bets[BetCounter].ToString();
    if (TotalWin_text) TotalWin_text.text = "0.000";
    if (Balance_text) Balance_text.text = SocketManager.playerData.balance.ToString("f3");
    if (Total_lines) Total_lines.text = SocketManager.initData.gameData.lines.Count.ToString();
    currentBalance = SocketManager.playerData.balance;
    currentTotalBet = SocketManager.initData.gameData.bets[BetCounter] * SocketManager.initData.gameData.lines.Count;
    CompareBalance();
    uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
  }

  //function to populate animation sprites accordingly
  private void PopulateAnimationSprites(ImageAnimation animScript, int val)
  {
    animScript.textureArray.Clear();
    animScript.textureArray.TrimExcess();
    switch (val)
    {
      case 8:
        for (int i = 0; i < Coin_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Coin_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 9:
        for (int i = 0; i < Frog_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Frog_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 6:
        for (int i = 0; i < Turtle_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Turtle_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 7:
        for (int i = 0; i < Cap_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Cap_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 5:
        for (int i = 0; i < Fish_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Fish_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 4:
        for (int i = 0; i < Ten_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Ten_Sprite[i]);
        }
        animScript.AnimationSpeed = 29f;
        break;
      case 0:
        for (int i = 0; i < A_Sprite.Length; i++)
        {
          animScript.textureArray.Add(A_Sprite[i]);
        }
        animScript.AnimationSpeed = 29f;
        break;
      case 3:
        for (int i = 0; i < J_Sprite.Length; i++)
        {
          animScript.textureArray.Add(J_Sprite[i]);
        }
        animScript.AnimationSpeed = 30f;
        break;
      case 1:
        for (int i = 0; i < K_Sprite.Length; i++)
        {
          animScript.textureArray.Add(K_Sprite[i]);
        }
        animScript.AnimationSpeed = 29f;
        break;
      case 2:
        for (int i = 0; i < Q_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Q_Sprite[i]);
        }
        animScript.AnimationSpeed = 29f;
        break;

      case 10:
        for (int i = 0; i < Scatter_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Scatter_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;

    }
  }

  //starts the spin process
  private void StartSlots(bool autoSpin = false)
  {
    WinningTextAnimationToggle(false);
    TotalWin_text.text = "0.000";
    if (!autoSpin)
    {
      if (audioController) audioController.PlayButtonAudio("spin");

      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        StopCoroutine(tweenroutine);
        tweenroutine = null;
        AutoSpinRoutine = null;
      }
    }
    if (TempList.Count > 0)
    {
      StopGameAnimation();
    }
    PayCalculator.ResetLines();
    tweenroutine = StartCoroutine(TweenRoutine());
  }


  private void BalanceDeduction()
  {
    double bet = 0;
    double balance = 0;

    try
    {
      bet = double.Parse(TotalBet_text.text);
    }
    catch (Exception e)
    {
      Debug.Log("Error while conversion " + e.Message);
    }

    try
    {
      balance = double.Parse(Balance_text.text);
    }
    catch (Exception e)
    {
      Debug.Log("Error while conversion " + e.Message);
    }
    double initAmount = balance;
    balance = balance - (bet);

    BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
    {
      if (Balance_text) Balance_text.text = initAmount.ToString("f3");
    });

  }

  private void WinningTextAnimationToggle(bool IsStart)
  {
    if (IsStart)
    {
      WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
    }
    else
    {
      WinTween.Kill();
      TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }
  }

  //manage the Routine for spinning of the slots
  private IEnumerator TweenRoutine()
  {
    yield return new WaitForSeconds(0.1f);
    if (currentBalance < currentTotalBet)
    {
      CompareBalance();
      StopAutoSpin();
      yield return new WaitForSeconds(1f);
      ToggleButtonGrp(true);
      yield break;
    }
    IsSpinning = true;
    if (audioController) audioController.PlaySpinAudio();
    CheckSpinAudio = true;
    ToggleButtonGrp(false);

    if (!IsTurboOn && !IsAutoSpin)
    {
      StopSpin_Button.gameObject.SetActive(true);
    }
    for (int i = 0; i < numberOfSlots; i++)
    {
      InitializeTweening(Slot_Transform[i]);
      yield return new WaitForSeconds(0.1f);
    }

    BalanceDeduction();

    SocketManager.AccumulateResult(BetCounter);
    yield return new WaitUntil(() => SocketManager.isResultdone);

    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 5; j++)
      {
        int resultNum = int.Parse(SocketManager.resultData.matrix[i][j]);
        // print("resultNum: "+resultNum); 
        // print("image loc: " +j + " " + i);
        Tempimages[j].slotImages[i].sprite = myImages[resultNum];
        PopulateAnimationSprites(Tempimages[j].slotImages[i].GetComponent<ImageAnimation>(), resultNum);
      }
    }
    if (IsTurboOn)
    {
      StopSpinToggle = true;
    }
    else
    {
      for (int i = 0; i < 5; i++)
      {
        yield return new WaitForSeconds(0.1f);
        if (StopSpinToggle)
        {
          break;
        }
      }
      StopSpin_Button.gameObject.SetActive(false);
    }

    for (int i = 0; i < numberOfSlots; i++)
    {
      yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
    }
    StopSpinToggle = false;

    yield return alltweens[^1].WaitForCompletion();
    KillAllTweens();
    currentBalance = SocketManager.playerData.balance;
    if (SocketManager.resultData.payload.winAmount > 0)
    {
      SpinDelay = 1.2f;
    }
    else
    {
      SpinDelay = 0.2f;
    }

    if (audioController) audioController.StopSpinAudio();

    if (SocketManager.resultData.payload.winAmount > 0)
    {
      if (audioController) audioController.PlayWLAudio("win");
    }
    CheckWinLines(SocketManager.resultData.payload.wins);
    if (SocketManager.resultData.scatter.amount > 0)
    {
      PlaySpecialSymbolAnimations(10);
    }
    CheckPopups = true;
    CheckWinPopups();

    yield return new WaitUntil(() => !CheckPopups);

    if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("f3");
    BalanceTween?.Kill();
    if (Balance_text) Balance_text.text = SocketManager.playerData.balance.ToString("f3");

    if (!IsAutoSpin)
    {
      ToggleButtonGrp(true);
      IsSpinning = false;
    }
    else
    {
      IsSpinning = false;
      yield return new WaitForSeconds(0.1f);
    }
  }

  internal void CheckWinPopups()
  {
    if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 10 && SocketManager.resultData.payload.winAmount < currentTotalBet * 15)
    {
      uiManager.PopulateWin(1, SocketManager.resultData.payload.winAmount);
    }
    else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 15 && SocketManager.resultData.payload.winAmount < currentTotalBet * 20)
    {
      uiManager.PopulateWin(2, SocketManager.resultData.payload.winAmount);
    }
    else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 20)
    {
      uiManager.PopulateWin(3, SocketManager.resultData.payload.winAmount);
    }
    else
    {
      CheckPopups = false;
    }
  }

  void PlaySpecialSymbolAnimations(int id)
  {
    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 5; j++)
      {
        int resultNum = int.Parse(SocketManager.resultData.matrix[i][j]);
        if (resultNum == id)
        {
          StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
        }
      }
    }
  }

  private void shuffleInitialMatrix()
  {
    for (int i = 0; i < Tempimages.Count; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
        Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
      }
    }
  }

  private void OnApplicationFocus(bool focus)
  {
    audioController.CheckFocusFunction(focus, CheckSpinAudio);
  }

  void ToggleButtonGrp(bool toggle)
  {
    if (SlotStart_Button) SlotStart_Button.interactable = toggle;
    if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
    if (Maxbet_button) Maxbet_button.interactable = toggle;
    if (BetMinus_Button) BetMinus_Button.interactable = toggle;
    if (BetPlus_Button) BetPlus_Button.interactable = toggle;
  }

  private void CompareBalance()
  {
    if (currentBalance < currentTotalBet)
    {
      uiManager.LowBalPopup();
    }
  }

  //start the icons animation
  private void StartGameAnimation(GameObject animObjects)
  {
    int i = animObjects.transform.childCount;

    if (i > 0)
    {
      ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
      animObjects.transform.GetChild(0).gameObject.SetActive(true);

      temp.StartAnimation();

      TempList.Add(temp);
    }
    else
    {
      animObjects.GetComponent<ImageAnimation>().StartAnimation();
    }
  }

  //stop the icons animation
  private void StopGameAnimation()
  {
    for (int i = 0; i < TempList.Count; i++)
    {
      TempList[i].StopAnimation();
      if (TempList[i].transform.childCount > 0)
        TempList[i].transform.GetChild(0).gameObject.SetActive(false);
    }
    TempList.Clear();
    TempList.TrimExcess();
  }

  //generate the payout lines generated 
  private void CheckWinLines(List<Win> wins)
  {
    if (wins.Count <= 0)
    {
      if (audioController) audioController.StopWLAudio();
      return;
    }

    WinningTextAnimationToggle(true);

    List<KeyValuePair<int, int>> coords = new();
    for (int j = 0; j < wins.Count; j++)
    {
      for (int k = 0; k < wins[j].positions.Count; k++)
      {
        int rowIndex = SocketManager.initData.gameData.lines[wins[j].line][k];
        int columnIndex = k;
        coords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
      }
    }

    foreach (var coord in coords)
    {
      int rowIndex = coord.Key;
      int columnIndex = coord.Value;
      StartGameAnimation(Tempimages[columnIndex].slotImages[rowIndex].gameObject);
    }
    CheckSpinAudio = false;
  }

  #region TweeningCode
  private void InitializeTweening(Transform slotTransform)
  {
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
    Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
    tweener.Play();
    alltweens.Add(tweener);
  }



  private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
  {
    alltweens[index].Pause();
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
    int tweenpos = (reqpos * (IconSizeFactor + spacefactor)) - (IconSizeFactor + (2 * spacefactor));
    alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (spacefactor > 0 ? spacefactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
    if (!isStop)
    {
      yield return new WaitForSeconds(0.2f);
    }
    else
    {
      yield return null;
    }
  }


  private void KillAllTweens()
  {
    for (int i = 0; i < numberOfSlots; i++)
    {
      alltweens[i].Kill();
    }
    alltweens.Clear();

  }
  #endregion

}

[Serializable]
public class SlotImage
{
  public List<Image> slotImages = new List<Image>(10);
}

