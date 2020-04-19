using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities
{

    public class MenuBar : MonoBehaviour
    {
		MenuBarEntry m_rootMenuEntry = new MenuBarEntry();

		public RectTransform buttonsContainer;
		public GameObject buttonPrefab;

		public Color DefaultMenuEntryTextColor => this.buttonPrefab.GetComponentInChildren<Text>().color;



		public void RegisterMenuEntry(MenuBarEntry menuEntry)
		{
			int indexOfMenuEntry = m_rootMenuEntry.AddChild(menuEntry);

			GameObject buttonGo = Object.Instantiate(this.buttonPrefab);

			buttonGo.name = menuEntry.name;

			buttonGo.GetComponentInChildren<Text>().text = menuEntry.name;

			buttonGo.transform.SetParent(this.buttonsContainer.transform, false);
			buttonGo.transform.SetSiblingIndex(indexOfMenuEntry);

			buttonGo.GetComponent<Button>().onClick.AddListener(() => menuEntry.clickAction());

		}

		public Button GetMenuEntryButton(MenuBarEntry entry)
		{
			Transform child = this.buttonsContainer.transform.Find(entry.name);
			return child != null ? child.GetComponent<Button>() : null;
		}

		public void SetEntryColor(MenuBarEntry entry, Color color)
		{
			var button = GetMenuEntryButton(entry);
			if (button != null)
				button.GetComponentInChildren<Text>().color = color;
		}

	}

}
