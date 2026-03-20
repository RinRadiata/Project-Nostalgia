using UnityEngine;

namespace CHARACTERS
{
    public class Character_Model3D : Character
    {
        private Transform modelContainer;
        public Transform model { get; private set; }

        public Character_Model3D(string name, CharacterConfigData config, GameObject prefab,string rootAssetsFolder) : base(name, config, prefab)
        {
            Debug.Log($"Created a Model3D character name: '{name}'");
        }

        //public override void OnReceiveCastingExpression(int layer, string expression)
        //{
        //    SetExpression(expression);
        //}
    }
}
