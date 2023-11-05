using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Deformacao_Bra2 : MonoBehaviour
    {
        public Text valorDeformacao_Bra2;

        private mainSerial serialController;

        private void Start()
        {

            serialController = FindObjectOfType<mainSerial>();


            StartCoroutine(UpdateDeformacaoText());
        }

        private IEnumerator UpdateDeformacaoText()
        {
            while (true)
            {
                if (serialController != null)
                {

                    float lastDeformacao_Bra2 = serialController.GetLastDeformacao_Bra2();

                    valorDeformacao_Bra2.text = lastDeformacao_Bra2.ToString("F2") + " m";

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
