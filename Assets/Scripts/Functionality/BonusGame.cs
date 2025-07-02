using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using System.Net.Sockets;


public class BonusGame : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private int IconSizeFactor = 100;

    private int numberOfSlots = 3;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField] private SlotBehaviour slotBehaviour;

    [SerializeField] private int stopIndex;
    [SerializeField] private Sprite exitSprite;
    [SerializeField] private GameObject bonusGame;
    [SerializeField] private List<Transform> OuterReelSlots;
    [SerializeField] private GameObject OuterSlotItemPrefab;

    [SerializeField] private List<OuterReelItem> Outer_Reel_All_Item;
    [SerializeField] private GameObject lighting;

    [SerializeField] private GameManager m_GameManager;

    [Header("Data Update Section")]
    [SerializeField] private TMP_Text m_Lives;
    [SerializeField] private TMP_Text m_Amount;
    [SerializeField] private TMP_Text m_TotalWonAmount;

    [SerializeField] private List<int> animIndex = new List<int>();
    [SerializeField] private int m_SpaceFactor;

    [Header("Outer Reel References Section")]
    [SerializeField] private List<OuterReelItem> m_FruitCockTail;
    [SerializeField] private List<OuterReelItem> m_WaterMelon;
    [SerializeField] private List<OuterReelItem> m_Peer;
    [SerializeField] private List<OuterReelItem> m_Coconut;
    [SerializeField] private List<OuterReelItem> m_Pineapple;
    [SerializeField] private List<OuterReelItem> m_Orange;
    [SerializeField] private List<OuterReelItem> m_Cherry;
    [SerializeField] private List<OuterReelItem> m_Exit;

    [SerializeField] private GameObject m_StopGameobject;
    [SerializeField] internal GameObject m_BonusWonPopup;


    private bool ison = true;
    public List<int> resultnum = new List<int>();
    private int Lives = 0;
    private int N_SpinCount_Begin = 0;
    private int N_SpinCount = 0;
    private Coroutine m_Spinning;
    private Coroutine tweenroutine;
    private bool IsSpinning;

    private void Start()
    {
        InvokeRepeating("ToggleOnOff", 0.1f, 0.2f);
        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }


    internal void StartBonusGame()
    {
        Debug.Log("startbonusGame");
        m_GameManager.m_Bonus_Start_Object.gameObject.SetActive(false);
        m_GameManager.m_AudioController.m_BG_Audio.Stop();
        m_GameManager.m_AudioController.m_Bonus_BG_Audio.Play();
        bonusGame.SetActive(true);
        Debug.Log(IsSpinning);
        if (!IsSpinning)
        {
            IsSpinning = true;

            if (m_Spinning != null)
            {
                StopCoroutine(m_Spinning);
                m_Spinning = null;
            }
            m_Spinning = StartCoroutine(AutoSpinCoroutine());
        }
    }

   
    
    internal void StartBonus(int m_count)                                          //change it here ashu
    {
       N_SpinCount = m_count;
       m_StopGameobject = null;
       N_SpinCount_Begin = 0;
        Lives = 1;
       m_TotalWonAmount.text = SocketManager.ResultData.bonus.amount.ToString();
    }


    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {

        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }

        PayCalculator.ResetLines();

        tweenroutine = StartCoroutine(TweenRoutine());
    }

    private void StopAutoSpin()
    {
        //if (audioController) audioController.PlaySpinButtonAudio();

        if (IsSpinning)
        {
            IsSpinning = false;
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        yield return new WaitForSeconds(0.4f);

        for(int i = 0; i < N_SpinCount; i ++)
        {
            StartSlots(IsSpinning);
            yield return tweenroutine;
        }

        yield return new WaitForSeconds(0.2f);

        bonusGame.SetActive(false);
        m_GameManager.m_AudioController.m_Bonus_Audio.Play();
        // m_GameManager.m_PushObject(m_BonusWonPopup);
        slotBehaviour.uiManager.MainPopup_Object.SetActive(true);
        m_BonusWonPopup.gameObject.SetActive(true);
        IsSpinning = false;
        Debug.Log("bonusENd");
        yield return new WaitForSeconds(2f);
        slotBehaviour.uiManager.MainPopup_Object.SetActive(false);
        m_BonusWonPopup.gameObject.SetActive(false) ;
        Reset();
        Debug.Log(slotBehaviour.WasAutoSpinOn);
        if (!slotBehaviour.WasAutoSpinOn)
        {
            Debug.Log("asautospinwasfalse");
            slotBehaviour.ToggleButtonGrp(true);
        }
        else
        {
            Debug.Log("invokeautospinBonus");
            Invoke("callAutoSpinAgain", 2f);
        }

    }

    void callAutoSpinAgain()
    {
        slotBehaviour.callAutoSpinAgain();
    }


    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        if (m_Spinning != null || tweenroutine != null)
        {
            try
            {
                StopCoroutine(m_Spinning);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                m_Spinning = null;
                StopCoroutine(StopAutoSpinCoroutine());
            }
            catch (Exception e)
            {
                Debug.Log("Error Occured..." + string.Concat("<color=red><b>", e, "</b></color>"));
            }
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        IsSpinning = true;
        StopInnerAnimations();
        m_Lives.text = Lives.ToString();

        m_GameManager.m_AudioController.m_Spin_Audio.Play();

        Coroutine moveSelector = StartCoroutine(ToggleSelectorAnimation(0.02f, 2));

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0f);
        }

        yield return new WaitForSeconds(0.5f);
        m_StopGameobject = PopulateOuterMatrix(SocketManager.ResultData.bonus.result.outerElement[N_SpinCount_Begin]);                          //changeit here ashu

        PopulateInnerMatrix();

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }

        yield return moveSelector;

        m_GameManager.m_AudioController.m_Spin_Audio.Stop();

        m_Lives.text = Lives.ToString();

       // Debug.Log(N_SpinCount_Begin - 1);
        //if(m_ReceivedStructure.outerRingSymbol[N_SpinCount_Begin - 1] != 7)
        //{
        //    PlayInnerAnimations();
        //}
        PlayInnerAnimations();

        CheckPayoutLineBackend(resultnum);

        m_Amount.text = (SocketManager.ResultData.bonus.result.winAmount[N_SpinCount_Begin - 1]).ToString();                             // change it here ashu

        yield return new WaitForSeconds(3f);
        KillAllTweens();
    }


    private void CheckPayoutLineBackend(List<int> numbers)
    {

        for (int i = 0; i < animIndex.Count; i++)
        {
            StartGameAnimation(Tempimages[animIndex[i]].slotImages[0].gameObject);
        }

    }

    private void StartGameAnimation(GameObject animObjects)
    {

        Tweener tweener = animObjects.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        alltweens.Add(tweener);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }

    void ToggleOnOff()
    {
        ison = !ison;
        lighting.SetActive(ison);

    }

    private void Reset()
    {
        StopGameAnimation();
        ResetHighlights();
       
        m_GameManager.m_AudioController.m_Bonus_BG_Audio.Stop();
        m_GameManager.m_AudioController.m_BG_Audio.Play();
    }

    private void ResetHighlights()
    {
        foreach (OuterReelItem item in Outer_Reel_All_Item)
        {
            item.selector.SetActive(false);
        }
    }

    private void PopulateInnerMatrix()                                                // change it here ashu
    {
       for(int i = 0; i < Tempimages.Count; i++)
       {
           Tempimages[i].slotImages[0].transform.GetChild(0).GetComponent<Image>().sprite = myImages[SocketManager.ResultData.bonus.result.innerElements[N_SpinCount_Begin][i]];
           Tempimages[i].slotImages[0].GetComponent<OuterReelItem>().image.GetComponent<ImageAnimation>().textureArray = GetSpriteList(SocketManager.ResultData.bonus.result.innerElements[N_SpinCount_Begin][i]).ToList();
       }
       Debug.Log(N_SpinCount_Begin);
       if(N_SpinCount_Begin < N_SpinCount)
       {
           N_SpinCount_Begin++;
       }
    }

    private Sprite[] GetSpriteList(int m_value)
    {
        switch (m_value)
        {
            case 0://FRUITCOCKTAIL
                return slotBehaviour.Juice_Sprite;
            case 1://WATERMELON
                return slotBehaviour.Watermelon_Sprite;
            case 2://PEER
                return slotBehaviour.Pear_Sprite;
            case 3://COCONUT
                return slotBehaviour.Coconut_Sprite;
            case 4://PINEAPPLE
                return slotBehaviour.Pineapple_Sprite;
            case 5://ORANGE
                return slotBehaviour.Orange_Sprite;
            case 6://CHERRY
                return slotBehaviour.Cherry_Sprite;
            case 7://EXIT
                break;
        }
        return null;
    }

    private void PlayInnerAnimations()
    {
        for(int i = 0; i < Tempimages.Count; i++)
        {
            Tempimages[i].slotImages[0].GetComponent<OuterReelItem>().image.GetComponent<ImageAnimation>().StartAnimation();
        }
    }

    private void StopInnerAnimations()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            Tempimages[i].slotImages[0].GetComponent<OuterReelItem>().image.GetComponent<ImageAnimation>().StopAnimation();
            Tempimages[i].slotImages[0].GetComponent<OuterReelItem>().image.GetComponent<ImageAnimation>().textureArray.Clear();
            Tempimages[i].slotImages[0].GetComponent<OuterReelItem>().image.GetComponent<ImageAnimation>().textureArray.TrimExcess();
        }
    }

    private GameObject PopulateOuterMatrix(int m_value)
    {
        int random_index;
        switch (m_value)
        {
            case 0://FRUITCOCKTAIL
                random_index = UnityEngine.Random.Range(0, m_FruitCockTail.Count);
                return m_FruitCockTail[random_index].gameObject;
            case 1://WATERMELON
                random_index = UnityEngine.Random.Range(0, m_WaterMelon.Count);
                return m_WaterMelon[random_index].gameObject;
            case 2://PEER
                random_index = UnityEngine.Random.Range(0, m_Peer.Count);
                return m_Peer[random_index].gameObject;
            case 3://COCONUT
                random_index = UnityEngine.Random.Range(0, m_Coconut.Count);
                return m_Coconut[random_index].gameObject;
            case 4://PINEAPPLE
                random_index = UnityEngine.Random.Range(0, m_Pineapple.Count);
                return m_Pineapple[random_index].gameObject;
            case 5://ORANGE
                random_index = UnityEngine.Random.Range(0, m_Orange.Count);
                return m_Orange[random_index].gameObject;
            case 6://CHERRY
                random_index = UnityEngine.Random.Range(0, m_Cherry.Count);
                return m_Cherry[random_index].gameObject;
            case 7://EXIT
                random_index = UnityEngine.Random.Range(0, m_Exit.Count);
                Lives--;
                return m_Exit[random_index].gameObject;
        }

        return null;
    }

    #region [[===TWEENING CODE===]]

    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
       
    }

    //HACK: Killing all initialized tweens to stop tweens.
    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }

    //HACK: This coroutine is used to run the border of the boxes one after another.
    IEnumerator ToggleSelectorAnimation(float delay, int noOfRotation = 1)
    {
        int count = 0;
        ResetHighlights();
        for (int j = 0; j < noOfRotation + 1; j++)
        {
            for(int i = Outer_Reel_All_Item.Count - 1; i >= 0; i--)
            {
                Outer_Reel_All_Item[i].selector.SetActive(true);
                if (j == noOfRotation && Outer_Reel_All_Item[i].gameObject == m_StopGameobject)
                {
                    Outer_Reel_All_Item[i].selector.SetActive(true);
                    slotBehaviour.CheckPopups = false;
                    yield return new WaitForSeconds(0.2f);
                    yield break;
                }
                if (j < noOfRotation)
                {
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    //count = 0;
                    //if(count < 4)
                    //{
                    //    count++;
                    //    yield return new WaitForSeconds(delay * 2f);
                    //}
                    //else
                    //{
                    //    yield return new WaitForSeconds(delay * 4f);
                    //}
                    yield return new WaitForSeconds(delay);
                }
                Outer_Reel_All_Item[i].selector.SetActive(false);
            }
        }
        yield return new WaitForSeconds(0.1f);
    }
    #endregion
}