using System;
using UnityEngine;

namespace BOO.UI
{
    public class CreditsUI : MonoBehaviour
    {
        public event Action BackClicked;

        public void OnClickBackButton()
        {
            BackClicked?.Invoke();
        }
    }
}