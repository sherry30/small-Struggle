using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class playerController : MonoBehaviour
{
    public CharacterController2D controller;

    public string Name;
    public Animator animator;
    public float runSpeed = 40f;
    public float jumpTime;
    

    public float horizontalMove = 0f;

    private bool idle = true;

    
    private bool walking = false;

    

    public int  playerID=-1;
    public  int team = -1;
    //for netyowkring
    public bool attack1 = false;

    public bool inNPCRange = false;
    public NPC NPCinRange;
    public bool inDialogue=false;


    public GameObject Camera;
    public Vector3 cameraOffset;
    public Rigidbody2D rb;

    public float maxSlopeX, maxSlopeY;
    public bool cameraFollow=true;
    private Vector2 maxVelocity;
    public bool inMonologue=false;
    public bool cantWalk = false;
    public Animator extraAnimation;
    public bool finalSequenceRange = false;
    private GameObject currentMonologue;
    [SerializeField]
    public Monologue thoughts;
    private bool thinking = false;
    public bool finalSequenceStarted = false;
    private bool interacted = false;

    public GameObject disc;

    public GameObject bigDialogue;

    public Vector3 finalLocation;
    public GameObject GameEnd;
    public void Awake()
    {
        extraAnimation.Play("FadeIn");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GameObject temp = GameObject.Find("Main Camera");
        Vector2 maxVelocity= new Vector2(maxSlopeX, maxSlopeY);
        finalSequenceStarted = false;

        if (temp != null)
        {
            //temp.SetActive(false);
        }
        

    }
    // Update is called once per frame
    void Update()
    {


        //limiting max speed

        /*        if ((rb.velocity.x) > maxVelocity.x)
                    rb.velocity = new Vector2(maxSlopeX, rb.velocity.y);
                else if((rb.velocity.x) < -maxVelocity.x)
                    rb.velocity = new Vector2(-maxSlopeX, rb.velocity.y);*/

        if (finalSequenceRange)
        {
            if (Input.GetButtonDown("Interact"))
            {
                finalSequenceRange = false;
                StartCoroutine(finalSequence());
                return;
            }
        }
        if (cameraFollow)

        //following camera
            Camera.transform.position = transform.position + cameraOffset ;
        else
        {
            if (transform.position.x >= Camera.transform.position.x - cameraOffset.x)
                cameraFollow = true;
        }

        
        if (inDialogue || inMonologue || thinking)
        {
            
            if (Input.GetButtonDown("Interact"))
            {
                if (DialogueManager.Instance.talking && !thinking)
                {
                    DialogueManager.Instance.dialogueComplete();
                    return;
                }
                if (DialogueManager.Instance.inMonologue)
                {
                    DialogueManager.Instance.nextMonolgue();
                    if (!DialogueManager.Instance.inMonologue)
                    {
                        inMonologue = false;
                        cantWalk = false;
                        GetComponent<CharacterController2D>().cantWalk = false;
                        if(!thinking)
                            Destroy(currentMonologue);
                        thinking = false;
                    }
                }
                else
                {
                    DialogueManager.Instance.nextDialogue();
                    if (!DialogueManager.Instance.inDialogue)
                    {
                        inDialogue = false;
                        //StartCoroutine(getDisc());
                    }
                }
            }

            return;
        }
        if (inNPCRange && !inDialogue)
        {
            if (Input.GetButtonDown("Interact"))
            {
                inDialogue = true;
                NPCinRange.TrigerDialogue();
                extraAnimation.Play("closeInteract");
                interacted = true;
            }
        }
/*        if (cantWalk)
        {
            horizontalMove = 0;
            rb.velocity = Vector2.zero;
            walking = false;
            idle = false;
            animator.SetFloat("horizontalMove", Mathf.Abs(horizontalMove));
            idleFix();
            //return;
        }*/
        
        if (idle || walking || controller.isInAir())
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");
            if (cantWalk)
                horizontalMove = 0;
            if (controller.isGrounded())
                animator.SetFloat("horizontalMove", Mathf.Abs(horizontalMove));
        }

        float move = horizontalMove * runSpeed * Time.fixedDeltaTime;

        controller.Move(move, false, false, cantWalk);



    }

    void FixedUpdate(){
        //setting states
        if (animator.GetFloat("horizontalMove") == 0)
        {
            
            walking = false;
        }
        else
        {
            walking = true;
            idle = false;
        }
       

        /*if (!photonView.IsMine)
        {
            //smoothMovement();
            idleFix();
            return;
        }*/
        // Move the character by finding the target velocity

       

        //playing walk animation
        /*if(horizontalMove!=0 && jumped == false && attack == false && walking ==false){
            animator.Play("walk");
            walking = true;
        }
        else if(walking && horizontalMove ==  0){
            walking= false;
        }*/
        

        //go to idle if attack animation is not playing and player is not moving
        idleFix();

        
        //go to idle when done doing attack 1
        /*if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("hotman_attack1")){
            animator.Play("hotman_idle1");
            attack = false;
        }*/
    }

    bool AnimatorIsPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length >
        animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    bool AnimatorIsPlaying(string name)
    {
        return AnimatorIsPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public void idleFix()
    {
        //go to idle if attack animation is not playing and player is not moving
        if (!walking && !idle && !controller.isInAir())
        {
            animator.Play("idle");
            animator.Update(0);
            idle = true;

        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (finalSequenceStarted)
        {
            return;
        }

        if (collision.CompareTag("NPC"))
        {
            inNPCRange = true;

            NPCinRange = collision.gameObject.GetComponent<NPC>();
            if(!interacted)
                extraAnimation.Play("InteractAnimation");
        }


        //handling slope
        if (collision.CompareTag("slopeStart"))
        {
            collision.gameObject.transform.parent.GetChild(0).gameObject.SetActive(true);
        }
        if (collision.CompareTag("slopeEnd"))
        {
            collision.gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
        }
        if (collision.CompareTag("slopeMid"))
        {
            maxSlopeX = rb.velocity.x;
            maxSlopeY = rb.velocity.y;
        }

        if (collision.CompareTag("CameraSwitch"))
        {
            if(cameraFollow)
                switchCameraFollow();
        }

        if (collision.CompareTag("monologue"))
        {
            inMonologue = true;
            DialogueManager.Instance.playMonologue(collision.GetComponent<Monologue>().sentences);
            cantWalk = true;
            GetComponent<CharacterController2D>().cantWalk = true;

            horizontalMove = 0;
            rb.velocity = Vector2.zero;
            walking = false;
            idle = false;
            animator.SetFloat("horizontalMove", Mathf.Abs(horizontalMove));
            idleFix();

            currentMonologue = collision.gameObject;
        }

        if (collision.CompareTag("musicStarter"))
        {
            if (!finalSequenceStarted)
            {
                finalSequenceRange = true;
                extraAnimation.Play("sitAnimation");
                finalSequenceStarted = true;
            }

        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("slopeMid"))
        {
            Vector2 temp = new Vector2(maxSlopeX, maxSlopeY);
            if(Mathf.Abs(rb.velocity.x)>temp.x || Mathf.Abs(rb.velocity.y)>temp.y)
                rb.velocity = new Vector2(maxSlopeX, maxSlopeY);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            inNPCRange = false;
            if(!interacted)
                extraAnimation.Play("closeInteract");
        }
    }

    public void switchCameraFollow()
    {
        cameraFollow = !cameraFollow;
    }

    public IEnumerator finalSequence()
    {
        

        extraAnimation.Play("sitCloseInteraction");
        yield return new WaitForSeconds(1.5f);
        transform.position = finalLocation;
        extraAnimation.Play("FadeIn");
        yield return new WaitForSeconds(1.5f);
        //if(GameState.Instance.level<4)
        BackgroundSoundManager.playBackgroundSong(GameState.Instance.naemOfSongs[GameState.Instance.level - 1]);
        float startT = Time.time;
        startThought();
        yield return new WaitForSeconds(1f);
        startT -= Time.time;
        yield return new WaitForSeconds(Mathf.Max(GameState.Instance.timeOfBackgroundSong[GameState.Instance.level - 1]-startT,0));
        if (GameState.Instance.level >= 4)
            bigDialogue.SetActive(false);
        extraAnimation.Play("FadeOut");
        yield return new WaitForSeconds(1.5f);
        GameState.Instance.levleUp();
        if (GameState.Instance.level >= 5)
        {
            


            GameEnd.SetActive(true);


            yield break;
        }
        
        SceneManager.LoadScene(GameState.Instance.level - 1);

    }

    public void startThought()
    {
        thinking = true;
        DialogueManager.Instance.playMonologue(thoughts.sentences, true);
    }

    public IEnumerator getDisc()
    {
        Animator anim = disc.GetComponent<Animator>();
        anim.SetTrigger("getDisc");
        cantWalk = true;
        GetComponent<CharacterController2D>().cantWalk = true;

        horizontalMove = 0;
        rb.velocity = Vector2.zero;
        walking = false;
        idle = false;
        animator.SetFloat("horizontalMove", Mathf.Abs(horizontalMove));
        idleFix();
        yield return new WaitForSeconds(3);

        cantWalk = false;
        GetComponent<CharacterController2D>().cantWalk = false;


    }

    public IEnumerator fadeText()
    {
        while (bigDialogue.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().alpha != 0)
        {
            bigDialogue.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().alpha -= 1;
            yield return null;
        }
    }

}
