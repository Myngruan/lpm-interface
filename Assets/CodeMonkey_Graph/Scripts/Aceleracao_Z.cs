using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Aceleracao_Z : MonoBehaviour
    {
        public Text valorAceleracao_Z;

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
                    float lastAceleracao_Z = serialController.GetLastAceleracao_X();

                    valorAceleracao_Z.text = lastAceleracao_Z.ToString("F2") + " m/s^2";

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
