using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace grafico
{
    public class btGravar : MonoBehaviour
    {
        private mainSerial serialController;
        private string logFilePath;
        private StreamWriter logStreamWriter;

        private void Start()
        {
            serialController = FindObjectOfType<mainSerial>();
            if (serialController == null)
            {
                Debug.LogError("mainSerial não encontrado na cena!");
                return;
            }

            logFilePath = Application.dataPath + "/serial_data.txt";
            logStreamWriter = File.AppendText(logFilePath);

            if (logStreamWriter == null)
            {
                Debug.LogError("Não foi possível abrir o arquivo de log.");
                return;
            }

            serialController.OnDataReceived += HandleDataReceive1;
            serialController.OnDataReceived += HandleDataReceive2;
            serialController.OnDataReceived += HandleDataReceive3;
        }

        private void HandleDataReceive1(float pressao, float deformacao_bra1, float deformacao_bra2)
        {
            string logEntry = $"Pressão: {pressao}, deformacao_bra1: {deformacao_bra1}, deformacao_bra2: {deformacao_bra2}";
            logStreamWriter.WriteLine(logEntry);
            logStreamWriter.Flush();
        }

        private void HandleDataReceive2(float inclinacao_X, float inclinacao_Y, float inclinacao_Z)
        {
            string logEntry = $"inclinacao_X: {inclinacao_X}, inclinacao_Y: {inclinacao_Y}, inclinacao_Z: {inclinacao_Z}";
            logStreamWriter.WriteLine(logEntry);
            logStreamWriter.Flush();
        }

        private void HandleDataReceive3(float aceleracao_X, float aceleracao_Y, float aceleracao_Z)
        {
            string logEntry = $"aceleracao_X: {aceleracao_X}, aceleracao_Y: {aceleracao_Y}, aceleracao_Z: {aceleracao_Z}";
            logStreamWriter.WriteLine(logEntry);
            logStreamWriter.Flush();
        }

        private void OnDestroy()
        {
            if (logStreamWriter != null)
            {
                logStreamWriter.Close();
                logStreamWriter.Dispose();
            }
        }
    }
}
