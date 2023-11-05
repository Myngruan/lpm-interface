using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Aceleracao_Y : MonoBehaviour
    {
        public Text valorAceleracao_Y;

        private mainSerial serialController;

        private void Start()
        {

            serialController = FindObjectOfType<mainSerial>();
            StartCoroutine(UpdateAceleracaoText());
        }

        private IEnumerator UpdateAceleracaoText()
        {
            while (true)
            {
                if (serialController != null)
                {
                    float lastAceleracao_Y = serialController.GetLastAceleracao_Y();

                    valorAceleracao_Y.text = lastAceleracao_Y.ToString("F2") + " m/s^2";

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
