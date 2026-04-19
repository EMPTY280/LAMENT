using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAMENT
{
    [Serializable]
    public class GutShopItem
    {
       [SerializeField] private GutData gut;
       [SerializeField] private int price = 100;

       public GutData Gut => gut;
       public int Price => price;
    }

    [CreateAssetMenu(fileName = "GutShopData", menuName = "ScriptableObjects/Gut Shop Data")]
    public class GutShopData : ScriptableObject
    {
        [SerializeField] private GutShopItem[] items;
        public GutShopItem[] Items => items;
    }

}
