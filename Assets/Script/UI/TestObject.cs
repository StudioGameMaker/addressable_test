using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class TestObject: MonoBehaviour
    {
        public Button _btnDelete;


        private void Awake()
        {
            _btnDelete.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}