using UnityEngine;
using UnityEditor;

namespace RotaryHeart.Lib.SerializableDictionary
{
    public class SupportWindow : EditorWindow
    {
        int ToolBarIndex;

        private GUIContent assetName;
        private GUIContent support;
        private GUIContent contact;
        private GUIContent review;

        private GUIStyle labelStyle;
        private GUIStyle PublisherNameStyle;
        private GUIStyle ToolBarStyle;
        private GUIStyle GreyText;
        private GUIStyle ReviewBanner;

        [MenuItem("Tools/Rotary Heart/Serializable Dictionary/About")]
        public static void ShowWindow()
        {
            SupportWindow myWindow = ScriptableObject.CreateInstance<SupportWindow>();
            myWindow.ShowUtility();
            myWindow.titleContent = new GUIContent("About");

            myWindow.assetName = myWindow.IconContent("<size=20><b><color=#AAAAAA> Serializable Dictionary</color></b></size>", "", "");
            myWindow.support = myWindow.IconContent("<size=12><b> Support</b></size>\n <size=9>Get help and talk \n with others.</size>", "_Help", "");
            myWindow.contact = myWindow.IconContent("<size=12><b> Contact</b></size>\n <size=9>Reach out and \n get help.</size>", "console.infoicon", "");
            myWindow.review = myWindow.IconContent("<size=11><color=white> Please consider leaving a review.</color></size>", "Favorite Icon", "");

            myWindow.labelStyle = new GUIStyle(EditorStyles.label);
            myWindow.labelStyle.richText = true;

            myWindow.PublisherNameStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            myWindow.ToolBarStyle = new GUIStyle("LargeButtonMid")
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            myWindow.GreyText = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };
            myWindow.ReviewBanner = new GUIStyle("TL SelectionButton")
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
        }

        void OnGUI()
        {
            maxSize = minSize = new Vector2(300, 400);

            EditorGUILayout.Space();
            GUILayout.Label(assetName, PublisherNameStyle);
            EditorGUILayout.Space();

            GUIContent[] toolbarOptions = new GUIContent[2];
            toolbarOptions[0] = support;
            toolbarOptions[1] = contact;

            ToolBarIndex = GUILayout.Toolbar(ToolBarIndex, toolbarOptions, ToolBarStyle, GUILayout.Height(50));

            switch (ToolBarIndex)
            {
                case 0:
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Support Forum"))
                        Application.OpenURL("https://forum.unity.com/threads/released-serializable-dictionary.518178/");

                    EditorGUILayout.LabelField("Talk with others.", GreyText);

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Wiki"))
                        Application.OpenURL("https://www.rotaryheart.com/Wiki.html");

                    EditorGUILayout.LabelField("Detailed code documentation.", GreyText);
                    break;

                case 1:
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Email"))
                        Application.OpenURL("mailto:ma.rotaryheart@gmail.com?");

                    EditorGUILayout.LabelField("Get in touch.", GreyText);
                    break;
                default: break;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(review, ReviewBanner, GUILayout.Height(30)))
                Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/account/downloads/search=Serialized%20Dictionary");
        }

        GUIContent IconContent(string text, string icon, string tooltip)
        {
            GUIContent content;

            if (string.IsNullOrEmpty(icon))
            {
                content = new GUIContent();
            }
            else
            {
                content = EditorGUIUtility.IconContent(icon);
            }

            content.text = text;
            content.tooltip = tooltip;
            return content;
        }

    }
}