using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingAnvil : MonoBehaviour
{
    [SerializeField] private List<CraftingRecipeSO> craftingRecipeSOList;
    [SerializeField] private BoxCollider placeItemsAreaBoxCollider;
    [SerializeField] private Transform itemSpawnPoint;

    [SerializeField] private TextMeshPro text;

    [SerializeField] private Transform vfxSpawnItem;

    private CraftingRecipeSO craftingRecipeSO;

    int i = 0;

    private void Awake()
    {
        craftingRecipeSO = craftingRecipeSOList[i];
        text.text = "The Recipe Was Changed to " + craftingRecipeSOList[0] + ". It is crafted with: " + craftingRecipeSOList[0].inputItemSOList[0] + craftingRecipeSOList[0].inputItemSOList[1];
    }

    public void Craft()
    {
        Debug.Log("craft");
        Collider[] colliderArray = Physics.OverlapBox(transform.position + placeItemsAreaBoxCollider.center, placeItemsAreaBoxCollider.size, placeItemsAreaBoxCollider.transform.rotation);

        List<ItemSO> inputItemList = new List<ItemSO>(craftingRecipeSO.inputItemSOList);
        List<GameObject> consumeItemGameObjectList = new List<GameObject>();

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out ItemSOHolder itemSOHolder))
            {
                if (inputItemList.Contains(itemSOHolder.itemSO))
                {
                    inputItemList.Remove(itemSOHolder.itemSO);
                    consumeItemGameObjectList.Add(collider.gameObject);
                }
            }
        }

        if (inputItemList.Count == 0)
        {
            Debug.Log("yay workie");
            Transform spawnedItemTransform = Instantiate(craftingRecipeSO.outputItemSO.prefab, itemSpawnPoint.position, itemSpawnPoint.rotation);

           // Instantiate(vfxSpawnItem, itemSpawnPoint.position, itemSpawnPoint.rotation);

            foreach (GameObject consumeItemGameObject in consumeItemGameObjectList)
            {
                Destroy(consumeItemGameObject);
            }
        } 




    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.O) && i >= 0)
        {
            string result = "";
            craftingRecipeSO = craftingRecipeSOList[0];
            for (int j = 0; j < craftingRecipeSOList[0].inputItemSOList.Count; j++)
            {
                result += craftingRecipeSOList[0].inputItemSOList[j];
            }
            text.text = "The Recipe Was Changed to " + craftingRecipeSOList[0] + ". It is crafted with: " + result;
            Debug.Log("Recipe Changed to " + craftingRecipeSOList[0]);
            i = 0;
            
        }

        if (Input.GetKeyDown(KeyCode.P) && i+1 < craftingRecipeSOList.Count)
        {

            string result = "";
            i++;
            craftingRecipeSO = craftingRecipeSOList[i];
            for(int j=0; j < craftingRecipeSOList[i].inputItemSOList.Count; j++)
            {
                result += craftingRecipeSOList[i].inputItemSOList[j];
            }
            text.text = "The Recipe Was Changed to " + craftingRecipeSOList[i] + "It is crafted with: " + result;
            Debug.Log("Recipe Changed to " + craftingRecipeSOList[i]);
            Debug.Log(i);
        }

    }
}
