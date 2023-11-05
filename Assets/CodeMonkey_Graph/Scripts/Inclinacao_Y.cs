using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Inclinacao_Y : MonoBehaviour
    {
        public Text valorInclinacao_Y;

        private mainSerial serialController;

        private void Start()
        {

            serialController = FindObjectOfType<mainSerial>();

            StartCoroutine(UpdateInclinacaoText());
        }

        private IEnumerator UpdateInclinacaoText()
        {
            while (true)
            {
                if (serialController != null)
                {
                    float lastInclinacao_Y= serialController.GetLastInclinacao_Y();

                    valorInclinacao_Y.text = lastInclinacao_Y.ToString("F2") + " graus";

                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
