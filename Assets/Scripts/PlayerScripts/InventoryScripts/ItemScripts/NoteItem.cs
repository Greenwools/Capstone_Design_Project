using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Note", menuName = "Inventory/Note Item")]
public class NoteItem : Item
{
    [TextArea(10, 15)]
    public List<string> pages;

    public override void Use()
    {
        NoteUI.Instance.ShowNote(pages);
    }
}
