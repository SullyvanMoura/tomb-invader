using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public GameObject wakeUpButton;
    public AudioSource audioSource;
    private Queue<string> sentences;
    public Animator animator;

    private void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue (Dialogue dialogue)
    {

        Debug.Log("Dialog started!");
        
        animator.SetBool("IsOpen", true);

        
        nameText.text = dialogue.name;

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        audioSource.Stop();
        wakeUpButton.SetActive(false);
        DisplayNextSentence();
    }


    public void DisplayNextSentence ()
    {
        if (sentences.Count == 0)
    	{
            Debug.Log("Finalizando");
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        {
            dialogText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogText.text += letter;
                yield return null;
            }
        }
    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        SceneManager.LoadScene(0);
    }
}
