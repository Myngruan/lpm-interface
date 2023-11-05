using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace grafico
{
    public interface IDataReceiver
    {
        void ReceiveDeformacao_Bra1(float deformacao_bra1);
        void ReceiveDeformacao_Bra2(float deformacao_bra2);
        void ReceiveInclinacao_X(float inclinacao_X);
        void ReceiveInclinacao_Y(float inclinacao_Y);
        void ReceiveInclinacao_Z(float inclinacao_Z);

        void ReceiveAceleracao_X(float aceleracao_X);
        void ReceiveAceleracao_Y(float aceleracao_Y);
        void ReceiveAceleracao_Z(float aceleracao_Z);
        void ReceivePressao(float pressao);
    }
}

