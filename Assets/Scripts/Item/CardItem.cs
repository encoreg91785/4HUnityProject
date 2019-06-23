using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
    [SerializeField]
    Text cardIdText, cardName;
    [SerializeField]
    Toggle receiveNow;
    public Card card;

    public void SetCard(Card card)
    {
        this.card = card;
        var cardData = Main.GetInstance().GetCardData(card.cardid);
        cardIdText.text = cardData.id;
        cardName.text = cardData.name;
        receiveNow.isOn = true;
    }

    public bool ReceiveNow()
    {
        return receiveNow.isOn;
    }
}
