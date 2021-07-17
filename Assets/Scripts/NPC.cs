using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPC : MonoBehaviour
{
    public string Name;

    [SerializeField]
    public Dialogue dialogue;
    public Dialogue answer;
    public List<int> dialogueSwitch;
    public List<int> answerSwitch;
    


    public void TrigerDialogue()
    {
        DialogueManager.Instance.startDialogue(dialogue, Name, answer, dialogueSwitch, answerSwitch);
    }
    
}
