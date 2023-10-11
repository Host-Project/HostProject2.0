using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Scenario
{
    public interface IInfluencer
    {
        public void Play();
        public int GetInfluenceLevel();
    }
}