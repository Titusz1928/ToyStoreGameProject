using UnityEngine;
using UnityEngine.UI;

public enum CardType { Teddy, Doll, Clown, Blocks, JackBox, BlocksB, Train, Car }
public enum CardVariant { toy, box }

public class Card : MonoBehaviour
{
    public CardType type;
    public CardVariant variant;
    public int slotIndex;

    private GridManager gridManager;

    public void Initialize(CardType t, CardVariant v, Sprite cardSprite, int index, GridManager gm)
    {
        type = t;
        variant = v;
        slotIndex = index;
        gridManager = gm;
        GetComponent<Image>().sprite = cardSprite;
    }

    public void OnClick()
    {
        //Debug.Log($"Clicked on card: {type} - {variant}");
        gridManager.HandleCardClick(this);
    }
}
