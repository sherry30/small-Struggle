using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monologue : MonoBehaviour
{
    [SerializeField]
    [TextArea(3, 10)]
    public string[] sentences;
}
