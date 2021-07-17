using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class DialogueManager : MonoBehaviour
{

    public static DialogueManager Instance;
    public string Name;
    public Queue<string> sentences;
    public Queue<string> answers;
    public GameObject NPCUI;
    public GameObject PlayerUI;
    //public Button continueButton;

    private TextMeshProUGUI NameUI;
    private TextMeshProUGUI ConvUI;
    public bool inMonologue=false;

    [Range(1,50)]
    public  float speed=5f;
    private float textDisplay=0f;

    public bool inDialogue = false;

    public playerController pc;

    //for answering
    public int dialogueNum=0;
    public int answerNum=0;
    List<int> dialogueSwitch;
    List<int> answerSwitch;
    public bool answering = false;

    public bool talking = false;
    public bool completeDialogue = false;


    private void Awake()
    {

        //NameUI = NPCUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ConvUI = NPCUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }


    public void startDialogue(Dialogue d, string Name, Dialogue answers, List<int> dialogueSwitch, List<int> answerSwitch)
    {
        //asigning values
        this.dialogueSwitch = dialogueSwitch;
        this.answerSwitch = answerSwitch;

        inDialogue = true;
        if (sentences == null)
            sentences = new Queue<string>();
        if (this.answers == null)
            this.answers = new Queue<string>();
        sentences.Clear();
        this.answers.Clear();
        answerNum = 0;
        dialogueNum = 0;
        answering = false;
        this.Name = Name;
        foreach (string s in d.sentences)
        {
            sentences.Enqueue(s);
        }

        foreach(string s in answers.sentences)
        {
            this.answers.Enqueue(s);
        }

        string text;
        checkSwitch();
        if (answering)
        {
            text = this.answers.Dequeue();
            answerNum++;
            //NameUI.text = pc.Name;
        }
        else
        {
            text = sentences.Dequeue();
            dialogueNum++;
            //NameUI.text = Name;
        }
        //dialogueUI.SetActive(true);
        
        
        StartCoroutine(displayText(text));
    }

    public void nextDialogue()
    {
        if (talking)
        {
            StopAllCoroutines();
            SoundManager.stopTalk();
        }
        checkSwitch();

        if ( !answering && sentences.Count <= 0)
        {
            //dialogueUI.SetActive(false);
            inDialogue = false;
            turnOffUI();
            return;
        }
        else if(answering && answers.Count <= 0)
        {
            //dialogueUI.SetActive(false);
            turnOffUI();
            inDialogue = false;
            answering = false;

            return;
        }
        string text;
        if (!answering)
        {
            text = sentences.Dequeue();
            dialogueNum++;
            //NameUI.text = this.Name;
        }
        else
        {
            text = answers.Dequeue();
            answerNum++;
            //NameUI.text = pc.Name;
        }
        StartCoroutine(displayText(text));
        

    }

    public IEnumerator displayText(string text, bool bigDialogue=false)
    {
        bool scroll = false;
        bool scroll2 = false;
        if (GameState.Instance.level >= 3  && bigDialogue)
            scroll = true;
        if (GameState.Instance.level == 4 && bigDialogue)
            scroll2 = true;
        talking = true;
        if(!bigDialogue)
            SoundManager.playClip("talk");
        bool playerTalk = false;
        if (answering)
        {
            playerTalk = true;
            pc.GetComponent<Animator>().SetTrigger("talk");
        }
        ConvUI.text = text[0].ToString();
        textDisplay = 0f;
        int threshold = 1;
        for(int i = 1; i < text.Length ; i++)
        {
            if (completeDialogue)
            {
                ConvUI.text = text;
                completeDialogue = false;
                break;
            }
            while (textDisplay<threshold)
            {
                textDisplay += speed * Time.deltaTime;
                yield return null;
            }
            threshold++;
            bool wait = false;
            if (bigDialogue && text[i] == '.')
                wait = true;
            string textToDisplay = "";

            
            for(int j=0;j<=i;j++)
            {

                //scrolling for level 3 and 4
                if (scroll || scroll2)
                {
                    if (GameState.Instance.level == 3 && scroll)
                    {
                        if (text[j] == '.')
                        {
                            if (text[j -1] == '"')
                            {
                                pc.bigDialogue.GetComponent<Animator>().Play("move");
                                scroll = false;
                                yield return new WaitForSeconds(2);
                            }
                        }
                    }

                    else
                    {
                        if (text[j] == '.' && scroll)
                        {
                            if (text[j - 1] == 'X')
                            {
                                pc.bigDialogue.GetComponent<Animator>().Play("move");
                                scroll = false;
                                yield return new WaitForSeconds(2);
                            }
                        }
                        else if (!scroll && scroll2)
                        {
                            if (text[j] == '.')
                            {
                                if(text[j-1]=='y' && text[j - 2] == 'l' && text[j - 3] == 'r')
                                {
                                    pc.bigDialogue.GetComponent<Animator>().Play("move2");
                                    scroll2 = false;
                                    yield return new WaitForSeconds(2);
                                }
                            }
                        }
                    }
                }


                textToDisplay += text[j].ToString();
            }
            ConvUI.text = textToDisplay;
            if (wait)
                yield return new WaitForSeconds(1.7f);
            yield return null;
        }
        if (playerTalk)
        {
            //Debug.Log("Stopped");
            pc.GetComponent<Animator>().SetTrigger("stopTalk");
        }
        SoundManager.stopTalk();
        talking = false;
    }

    public void checkSwitch()
    {
        if(!answering)
        {
            if (dialogueSwitch.Contains(dialogueNum))
            {
                answering = true;
            }
        }
        else if (answerSwitch.Contains(answerNum))
        {
            answering = false;
        }

        if (!answering)
        {
            NPCUI.SetActive(true);
            PlayerUI.SetActive(false);
            ConvUI = NPCUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        else
        {
            NPCUI.SetActive(false);
            PlayerUI.SetActive(true);
            ConvUI = PlayerUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            if (!pc.GetComponent<CharacterController2D>().m_FacingRight)
            {
                flipDialogueBox();
            }
        }
    }

    public void turnOffUI()
    {
        NPCUI.SetActive(false);
        PlayerUI.SetActive(false);
    }

    public void flipDialogueBox()
    {
        Vector3 dialogueScale = PlayerUI.transform.localScale;
        dialogueScale.x *= -1;
        PlayerUI.transform.GetChild(0).transform.localScale = dialogueScale;
    }

    public void playMonologue(string[] sentences, bool bigDialogue=false) {
        inMonologue = true;
        if (this.sentences == null)
            this.sentences = new Queue<string>();
        this.sentences.Clear();

        foreach (string s in sentences)
        {
            this.sentences.Enqueue(s);
        }
        
        if (bigDialogue)
        {
            pc.bigDialogue.SetActive(true);
            ConvUI = pc.bigDialogue.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            speed = 10;
        }
        else
        {
            PlayerUI.SetActive(true);
            ConvUI = PlayerUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        string text;

        text = this.sentences.Dequeue();
        StartCoroutine(displayText(text, bigDialogue));
    }

    public void nextMonolgue()
    {
        if (talking)
        {
            StopAllCoroutines();
            SoundManager.stopTalk();
        }
        if (sentences.Count <= 0)
        {
            //dialogueUI.SetActive(false);
            inMonologue = false;
            turnOffUI();
            return;
        }
        string text;
        text = sentences.Dequeue();

        StartCoroutine(displayText(text));

    }

    public void dialogueComplete()
    {
        completeDialogue = true;

    }




    }
