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
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button StopSpin_Button;
    [SerializeField]
    private Button Turbo_Button;
    [SerializeField]
    private Button SlotStop_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button LinePlus_Button;
    [SerializeField]
    private Button LineMinus_Button;

    [Header("Animated Sprites")]
    //[SerializeField]
    //private Sprite[] Bonus_Sprite;
    [SerializeField]
    internal Sprite[] Apple_sprite;
    [SerializeField]
    internal Sprite[] Cherry_Sprite;
    [SerializeField]
    internal Sprite[] Coconut_Sprite;
    [SerializeField]
    internal Sprite[] Jelly_Sprite;
    [SerializeField]
    internal Sprite[] Juice_Sprite;
    [SerializeField]
    internal Sprite[] Lemon_Sprite;
    [SerializeField]
    internal Sprite[] Orange_Sprite;
    [SerializeField]
    internal Sprite[] Pear_Sprite;
    [SerializeField]
    internal Sprite[] Pineapple_Sprite;
    [SerializeField]
    internal Sprite[] Strawberry_Sprite;
    [SerializeField]
    internal Sprite[] Watermelon_Sprite;
    [SerializeField]
    internal Sprite[] Wild_Sprite;


    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text Lines_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;

    [Header("paylines ")]
    [SerializeField] private List<TMP_Text> StaticLine_Texts;
    [SerializeField] private List<GameObject> StaticLine_Objects;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField] private PayoutCalculation PayCalculator;
    [SerializeField] private BonusGame bonusManager;
    [SerializeField] internal SocketIOManager SocketManager;
    [SerializeField] internal UIManager uiManager;
    [SerializeField] private GameManager m_GameManager;

    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField] private List<TMP_Text> m_BonusPayText = new List<TMP_Text>();

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField] private GameObject charecter_happy;
    [SerializeField] private GameObject charecter_idle;

    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine = null;
    Coroutine FreeSpinRoutine = null;
    bool IsAutoSpin = false;
    bool IsSpinning = false;
    bool IsFreeSpin = false;
    bool IsTurboOn;
    bool StopSpinToggle;
    internal bool WasAutoSpinOn;


    internal bool CheckPopups = false;
    internal int BetCounter = 0;

    static private int Lines = 20;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    private float SpinDelay = 0.2f;

    [SerializeField]
    Sprite[] TurboToggleSprites;


    private void Start()
    {
        IsAutoSpin = false;
        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); m_GameManager.m_AudioController.m_Spin_Button_Clicked.Play(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); m_GameManager.m_AudioController.m_Click_Audio.Play(); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        if(StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { m_GameManager.m_AudioController.m_Spin_Button_Clicked.Play(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });

        if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ChangeBet(true); m_GameManager.m_AudioController.m_Click_Audio.Play(); });
        if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ChangeBet(false); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate { AutoSpin(); m_GameManager.m_AudioController.m_Spin_Button_Clicked.Play(); });


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate { StopAutoSpin(); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }

    void TurboToggle()
    {
        m_GameManager.m_AudioController.m_Click_Audio.Play();
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprites[0];
            Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            Turbo_Button.image.color = new Color(1, 1, 1, 1);
        }
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

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f3");
        });
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

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private void StopAutoSpin()
    {
        
        if (WasAutoSpinOn)
        {
            WasAutoSpinOn = false;
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private void StopAutoSpinLowBalance()
    {

        
            WasAutoSpinOn = false;
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        

    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        if (!WasAutoSpinOn)
        {
            ToggleButtonGrp(true);
        }
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        //StaticLine_Texts[count].text = (count + 1).ToString();
        //StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(int LineNo)
    {
        DestroyStaticLine();
        int LineID = 1;
        LineID = LineNo;
        //try
        //{
        //    LineID = int.Parse(LineID_Text.text);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("Exception while parsing " + e.Message);
        //}
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();

        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

    private void MaxBet()
    {
        if (audioController.m_Player_Listener.enabled) audioController.m_Click_Audio.Play();

        BetCounter = SocketManager.InitialData.bets.Count - 1;
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
        if (TotalBet_text) TotalBet_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (!AutoSpinStop_Button.gameObject.activeSelf)
        {
            if (audioController.m_Player_Listener.enabled) audioController.m_Click_Audio.Play();
            if (IncDec)
            {
                BetCounter++;
                if (BetCounter > SocketManager.InitialData.bets.Count - 1)
                {
                    BetCounter = 0;
                }
            }
            else
            {
                BetCounter--;
                if (BetCounter < 0)
                {
                    BetCounter = SocketManager.InitialData.bets.Count - 1;
                }
            }
            currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
            if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * Lines).ToString("f3");
            if (Lines_text) Lines_text.text = (SocketManager.InitialData.bets[BetCounter]).ToString();
           
        }
    }


    // just for testing purposes delete on production
    bool isStart =false;
    private void Update()
    {
#if !UNITY_WEBGL && UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        {
            if (isStart)
            {
                StopSpinToggle = true;
                StopSpin_Button.gameObject.SetActive(false);
                isStart = true;
            }
            else
            {
                StartSlots();
                isStart = true;
            }
        }
#endif
    }

    internal void SetInitialUI()
    {
        Debug.Log("TTTNNNTNTNNTNTNTTNNTTN");
        BetCounter = 0;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * Lines).ToString("f3");
        if (Lines_text) Lines_text.text = (SocketManager.InitialData.bets[BetCounter]).ToString();
        if (TotalWin_text) TotalWin_text.text = 0.ToString("f3");
        if (Balance_text) Balance_text.text = (SocketManager.PlayerData.balance).ToString("f3");
        uiManager.InitialiseUIData( SocketManager.UIData.paylines);
        //bonusManager.PopulateBonusPaytable(SocketManager.bonusdata);
        currentBalance = SocketManager.PlayerData.balance;
        currentTotalBet = double.Parse(TotalBet_text.text);
        for (int i = 0; i < m_BonusPayText.Count; i++)
        {
            m_BonusPayText[i].text = SocketManager.bonusdata[i].ToString() + " x";
        }
        CompareBalance();
    }

    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        Debug.Log("layout_Reset");
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();

        switch (val)
        {
            case 0:
                for (int i = 0; i < Apple_sprite.Length; i++)
                {
                    animScript.textureArray.Add(Apple_sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 1:
                for (int i = 0; i < Cherry_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Cherry_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 2:
                for (int i = 0; i < Watermelon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Watermelon_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;
            case 3:
                for (int i = 0; i < Lemon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Lemon_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 4:
                for (int i = 0; i < Orange_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Orange_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 5:
                for (int i = 0; i < Pear_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pear_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 6:
                for (int i = 0; i < Pineapple_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pineapple_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 7:
                for (int i = 0; i < Strawberry_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Strawberry_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 8:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;
            case 9:
                for (int i = 0; i < Juice_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Juice_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 11:
                for (int i = 0; i < Jelly_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Jelly_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
        }
    }

    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        TotalWin_text.text = 0.ToString("f3");
        if (audioController.m_Player_Listener.enabled) audioController.m_Spin_Audio.Play();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        Debug.Log("autoSpinOn");
        
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        IsSpinning = true;

        ToggleButtonGrp(false);
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }

        if (IsFreeSpin)
        {

            uiManager.updateFreespinInfo();
            uiManager.currentSpin--;
        }
        else
        {
            BalanceDeduction();
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

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
        balance = balance - bet;

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        if (IsAutoSpin)
        {
            WasAutoSpinOn = true;
        }


        //HACK: Image Populate Loop For Testing
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int resultNum = int.Parse(SocketManager.ResultData.matrix[i][j]);
                // print("resultNum: " + resultNum);
                // print("image loc: " + j + " " + i);
                PopulateAnimationSprites(Tempimages[j].slotImages[i].GetComponent<ImageAnimation>(), resultNum);
                Tempimages[j].slotImages[i].sprite = myImages[resultNum];
            }
        }

        if (IsTurboOn || IsFreeSpin)
        {
            StopSpinToggle = true;
            yield return new WaitForSeconds(0.1f);
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

        yield return new WaitForSeconds(0.3f);

        m_GameManager.m_AudioController.m_Spin_Audio.Stop();

        //TODO: To Be Uncommented When Move To Publishing
        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            List<int> winLine = new();
            foreach (var item in SocketManager.ResultData.payload.wins)
            {
                winLine.Add(item.line);
            }
            CheckPayoutLineBackend(winLine);
        }
        CheckForFeaturesAnimation();

        currentBalance = SocketManager.PlayerData.balance;
        KillAllTweens();
        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            SpinDelay = 2f;
        }
        else
        {
            SpinDelay = 0.5f;
        }

        if (TotalWin_text) TotalWin_text.text = SocketManager.ResultData.payload.winAmount.ToString("f3");

        if (Balance_text) Balance_text.text = SocketManager.PlayerData.balance.ToString("f3");


        CheckPopups = true;
        if (SocketManager.ResultData.scatter.amount > 0)
        {
            uiManager.PopulateScatterWin(SocketManager.ResultData.scatter.amount);
            yield return new WaitForSeconds(2.3f);
        }
        

        if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 15)
        {
            uiManager.PopulateWin(3, (double)SocketManager.ResultData.payload.winAmount);
        }
        else
        {

            CheckBonusGame();
        }
       
        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            
            if (!SocketManager.ResultData.bonus.isTriggered)
            {
                ToggleButtonGrp(true);
            }
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(SpinDelay);
            IsSpinning = false;
        }


        // if (SocketManager.ResultData.freeSpin.isFreeSpin && !IsFreeSpin)
        // {

        //    uiManager.StartFreeSpins((int)SocketManager.ResultData.freeSpin.count);
        //    yield break;
        // }
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            if (IsAutoSpin)
            {
                StopAutoSpinLowBalance();
            }
            uiManager.LowBalPopup();
   
        }
  
    }

    internal void CallCloseSocket()
    {
       StartCoroutine(SocketManager.CloseSocket());
    }

    internal void shuffleInitialMatrix()
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

    internal void CheckBonusGame()
    {
       if (SocketManager.ResultData.bonus.isTriggered)
       {
           if (WasAutoSpinOn)
           {
              IsAutoSpin = false;
              StopCoroutine(AutoSpinCoroutine());                
           }

            
           DOVirtual.DelayedCall(1f, () =>
           {
               uiManager.ResetPopups();
               m_GameManager.m_AudioController.m_Bonus_Audio.Play();
               uiManager.MainPopup_Object.SetActive(true);
               m_GameManager.m_Bonus_Start_Object.SetActive(true);
           });
           
           m_GameManager.m_PushObject(m_GameManager.m_Bonus_Start_Object);
           bonusManager.StartBonus(SocketManager.ResultData.bonus.result.winAmount.Count);
           Invoke("startbonusautomatically", 2f);

       }
       else
       {
            
          
               CheckPopups = false;
            
       }

    //    if (SocketManager.ResultData.freeSpin.isFreeSpin )
    //    {
    //        if (IsAutoSpin)
    //        {
    //            StopAutoSpin();
    //        }
    //    }
    }

    
    internal void startbonusautomatically()
    {
        bonusManager.StartBonusGame();
    }

    internal void callAutoSpinAgain()
    {
        Debug.Log(AutoSpinStop_Button.gameObject.activeSelf);
        if (AutoSpinStop_Button.gameObject.activeSelf)
        {
            
            AutoSpin();
        }
    }



    internal void ToggleButtonGrp(bool toggle)
    {
       
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if(!IsSpinning || !IsAutoSpin || !IsFreeSpin)
        {
            if (LinePlus_Button) LinePlus_Button.interactable = true;
            if (LineMinus_Button) LineMinus_Button.interactable = true;
            if (BetMinus_Button) BetMinus_Button.interactable = true;
            if (BetPlus_Button) BetPlus_Button.interactable = true;
        }
        else
        {
            if (LinePlus_Button) LinePlus_Button.interactable = toggle;
            if (LineMinus_Button) LineMinus_Button.interactable = toggle;
            if (BetMinus_Button) BetMinus_Button.interactable = toggle;
            if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        }

    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }
            

          
    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, double jackpot = 0)
    {
        List<int> y_points = null;
        if (LineId.Count > 0)
        {
            if (jackpot <= 0)
            {
                if (audioController.m_Player_Listener.enabled) audioController.m_Win_Audio.Play();
            }

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            if (jackpot > 0)
            {
               // if (audioController.m_Player_Listener.enabled) audioController.m_Win_Audio.Play();
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                List<KeyValuePair<int, int>> coords = new();
                for (int j = 0; j < LineId.Count; j++)
                {
                    for (int k = 0; k < SocketManager.ResultData.payload.wins[j].positions.Count; k++)
                    {
                        int rowIndex = SocketManager.InitialData.lines[LineId[j]][k];
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
            }
          //  WinningsAnim(true);               //change it here ashu
        }
        else
        {

            if (audioController.m_Player_Listener.enabled) audioController.m_LooseAudio.Play();
        }
        
    }
    private void CheckForFeaturesAnimation()
    {
        bool playJackpot = false;
        bool playScatter = false;
        bool playBonus = false;
        bool playFreespin = false;
        if (SocketManager.ResultData.scatter.amount > 0)
        {
            playScatter = true;
        }
        if (SocketManager.ResultData.bonus.amount > 0)
        {
            playBonus = true;
        }
        PlayFeatureAnimation(playJackpot, playScatter, playBonus, playFreespin);
    }
    private void PlayFeatureAnimation(bool jackpot = false, bool scatter = false, bool bonus = false, bool freeSpin = false)
    {
        for (int i = 0; i < SocketManager.ResultData.matrix.Count; i++)
        {
            for (int j = 0; j < SocketManager.ResultData.matrix[i].Count; j++)
            {

                if (int.TryParse(SocketManager.ResultData.matrix[i][j], out int parsedNumber))
                {
                    if (jackpot && parsedNumber == 12)
                    {
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                    if (scatter && parsedNumber == 11)
                    {
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                    if (bonus && parsedNumber == 18)
                    {
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                    if (freeSpin && parsedNumber == 13)
                    {
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                }

            }
        }
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
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
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

    // private void WinningsAnim(bool IsStart)                           //change it here ashu
    // {
    //    if (IsStart)
    //    {
    //        WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
    //        foreach (GameObject e in FireAnim_Objects)
    //        {
    //            e.SetActive(true);
    //        }
    //    }
    //    else
    //    {
    //        WinTween.Kill();
    //        TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    //        foreach (GameObject e in FireAnim_Objects)
    //        {
    //            e.SetActive(false);
    //        }
    //    }
    // }
    #endregion
  

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(13);
}

