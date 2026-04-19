using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LAMENT
{
    [SerializeField]
    public class GutShopEntry
    {
        [SerializeField] private GutData gut;
        [SerializeField] private int price = 1;

        public GutData Gut => gut;
        public int Price => price;
    }

    [CreateAssetMenu(fileName = "GUtShopCatalog", menuName = "ScriptableObjects/GutShop/Catalog")]
    public class GutShopCatalog : ScriptableObject
    {
        //[SerializeField] private string 
    }

}
