using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedTextTyper : MonoBehaviour {
    [SerializeField] private float m_TypeRate;
    
    private Text text;
    private Coroutine currentTypeAnimation;
    private string currentTypeText;
    private bool skipAnimation;

    private void Awake() {
        text = GetComponent<Text>();
    }

    IEnumerator TypeAnimation() {
        text.text = string.Empty;
        float t = 0;
        int currentChar = 0;
        while (currentChar < currentTypeText.Length) {
            t += Time.deltaTime * m_TypeRate;
            while (currentChar < t && currentChar < currentTypeText.Length) {
                text.text = text.text + currentTypeText[currentChar];
                currentChar++;
            }

            yield return new WaitForEndOfFrame();
        }

        currentTypeAnimation = null;
    }

    public void StartTypeAnimation(string text) {
        if (currentTypeAnimation != null) {
            StopCoroutine(currentTypeAnimation);
        }
        
        currentTypeText = text;
        skipAnimation = false;
        currentTypeAnimation = StartCoroutine(TypeAnimation());
    }

    [ContextMenu("Test Type Animation")]
    public void TestTypeAnimation() {
        StartTypeAnimation("THIS IS A TEST");
    }
}
