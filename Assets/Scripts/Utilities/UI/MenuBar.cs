using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities
{

    public class MenuBar : MonoBehaviour
    {
		public RectTransform buttonsContainer;
		public GameObject buttonPrefab;

		public Color DefaultMenuEntryTextColor => this.buttonPrefab.GetComponentInChildren<Text>().color;

        public IEnumerable<MenuBarEntry> MenuBarEntries
		{
			get
			{
				for (int i = 0; i < this.buttonsContainer.transform.childCount; i++)
				{
					if (this.buttonsContainer.transform.GetChild(i).TryGetComponent<MenuBarEntry>(out var entry))
						yield return entry;
				}
			}
		}


        public MenuBarEntry RegisterMenuEntry(string entryName, int sortPriority, System.Action clickAction)
		{
			GameObject buttonGo = Object.Instantiate(this.buttonPrefab);

			buttonGo.name = entryName;

			buttonGo.GetComponentInChildren<Text>().text = entryName;

			buttonGo.transform.SetParent(this.buttonsContainer.transform, false);
			
			buttonGo.GetComponentOrThrow<Button>().onClick.AddListener(() => clickAction());

			var entry = buttonGo.GetComponentOrThrow<MenuBarEntry>();

			entry.sortPriority = sortPriority;

			// sort entries

			var list = this.MenuBarEntries.ToList();
			list.Sort((a, b) => a.sortPriority.CompareTo(b.sortPriority));

			for (int i = 0; i < list.Count; i++)
			{
				list[i].transform.SetSiblingIndex(i);
			}

			return entry;
		}

		public void SetEntryColor(MenuBarEntry entry, Color color)
		{
			entry.GetComponentInChildren<Text>().color = color;
		}

	}

}
