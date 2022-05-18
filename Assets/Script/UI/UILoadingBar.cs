using System;
using UnityEngine;

namespace Game
{
    public class UILoadingBar: MonoBehaviour
    {
        public RectTransform body;
        public RectTransform bar;

        private Vector2 _size;
        private float _value = 0;

        public float Value
        {
            get => _value;
            set => SetValue(value);
        }
        
        
        
        private void Awake()
        {
            _size = body.sizeDelta;
            Value = 0;
        }


        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="progress"></param>
        public void SetValue(float progress)
        {
            _value = Mathf.Clamp01(progress);
            var w = _size.x * _value;
            var h = _size.y;
            bar.sizeDelta = new Vector2(w, h);
        }
        
        
    }
}