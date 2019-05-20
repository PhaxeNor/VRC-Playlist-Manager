using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRCSDK2;


namespace VRCPrefabs.PlaylistManager
{
    public class PlaylistManagerWindow : EditorWindow
    {
        private List<PlaylistItem> playlists;
        private int currentPlaylist;

        private PlaylistItem selectedPlaylist;

        private List<GameObject> videoPlayers;
        private List<string> videoPlayerOptions;
        private int videoPlayerSelected = 0;
        private bool selectFirst;

        private string defaultNewFileName = "New-Playlist";

        public PlaylistManagerWindow()
        {
            playlists = new List<PlaylistItem>();
        }

        private void OnEnable()
        {
            openPlaylists();
            currentPlaylist = 0;

            selectedPlaylist = playlists.Count > 0 ? playlists[0] : null;
            selectFirst = true;

            boxStyle = new GUIStyle
            {
                contentOffset = new Vector2(10, 0)
            };
            boxStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            boxStyle.fontSize = 12;
            boxStyle.alignment = TextAnchor.MiddleLeft;

            boxBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            boxBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            boxBgSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;


            videoPlayers = new List<GameObject>();
            videoPlayerOptions = new List<string>();

            getVideoPlayers();
        }

        private void OnDisable()
        {
            // Make sure we empty the list for memory sake
            playlists = null;
            videoPlayers = null;
        }

        private void getVideoPlayers()
        {
            VRC_SyncVideoPlayer[] foundVideoSync = GameObject.FindObjectsOfType<VRC_SyncVideoPlayer>();

            foreach (VRC_SyncVideoPlayer f in foundVideoSync)
            {
                addPlayerToList(f.gameObject, "SyncVideoPlayer");
            }

            VRC_SyncVideoStream[] foundVideoStream = FindObjectsOfType<VRC_SyncVideoStream>();

            foreach (VRC_SyncVideoStream f in foundVideoStream)
            {
                addPlayerToList(f.gameObject, "SyncVideoStream");
            }
        }

        private void addPlayerToList(GameObject g, string nameSufix)
        {
            try
            {
                videoPlayers.Add(g);
                videoPlayerOptions.Add(g.name + " <" + nameSufix + ">");
            }
            catch (NullReferenceException ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private Rect leftPanel;
        private Rect rightPanel;
        private Rect menuBar;
        private Rect menuLeftBox;

        private readonly float menuBarHeight = 20f;

        private readonly GUIStyle resizerStyle;

        private GUIStyle boxStyle;

        private Texture2D boxBgOdd;
        private Texture2D boxBgEven;
        private Texture2D boxBgSelected;


        private Vector2 upperPanelScroll;
        private readonly string notification = "Playlist Applied";

        public void OnGUI()
        {
            DrawLeftMenu();
            DrawRightMenu();
            DrawMenuBar();
        }

        private void DrawLeftMenu()
        {
            leftPanel = new Rect(0, menuBarHeight - 2, 200, position.height - menuBarHeight);

            GUILayout.BeginArea(leftPanel);
            upperPanelScroll = GUILayout.BeginScrollView(upperPanelScroll);

            for (int i = 0; i < playlists.Count; i++)
            {
                playlists[i].isSelected = i == currentPlaylist ? true : false;

                if (DrawBox(playlists[i].Name, i % 2 == 0, playlists[i].isSelected))
                {
                    if (selectedPlaylist != null)
                    {
                        selectedPlaylist.isSelected = false;
                    }

                    GUI.FocusControl(null);
                    playlists[i].isSelected = true;
                    selectedPlaylist = playlists[i];
                    currentPlaylist = i;
                    GUI.changed = true;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private Vector2 textScroll;

        private void DrawRightMenu()
        {
            rightPanel = new Rect(200, 0, position.width - 200, position.height - 1);

            GUILayout.BeginArea(rightPanel, EditorStyles.helpBox);

            GUILayout.Space(menuBarHeight);
            if (selectedPlaylist != null)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(150));
                {
                    EditorGUILayout.LabelField("Name", new GUIStyle(GUI.skin.label), GUILayout.Width(50));
                    selectedPlaylist.Name = EditorGUILayout.TextField(selectedPlaylist.Name, new GUIStyle(GUI.skin.textArea), GUILayout.Width(position.width - 290));


                    if (GUILayout.Button("X"))
                    {
                        destroyPlaylist();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUIStyle areaStyle = new GUIStyle(GUI.skin.textArea)
                {
                    wordWrap = true
                };
                float width = position.width - 35;

                EditorGUILayout.LabelField("Links - One per line", new GUIStyle(GUI.skin.label));

                textScroll = EditorGUILayout.BeginScrollView(textScroll);

                selectedPlaylist.Contents = EditorGUILayout.TextArea(selectedPlaylist.Contents, areaStyle, GUILayout.Height(position.height - 110));
                EditorGUILayout.EndScrollView();


                videoPlayerSelected = EditorGUILayout.Popup("Video Player", videoPlayerSelected, videoPlayerOptions.ToArray());

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Save"))
                {
                    savePlaylist();
                }

                if (GUILayout.Button("Apply"))
                {
                    applyPlaylist();
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private bool DrawBox(string content, bool isOdd, bool isSelected)
        {
            if (isSelected)
            {
                boxStyle.normal.background = boxBgSelected;
            }
            else
            {
                if (isOdd)
                {
                    boxStyle.normal.background = boxBgOdd;
                }
                else
                {
                    boxStyle.normal.background = boxBgEven;
                }
            }
            return GUILayout.Button(new GUIContent(content), boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));
        }


        private void DrawMenuBar()
        {
            menuBar = new Rect(0, 0, position.width, menuBarHeight);

            GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Create"), EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                createPlaylist();
            }
            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("Refresh"), EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                openPlaylists();
            }

            if (GUILayout.Button(new GUIContent("Help"), EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GenericMenu menu = new GenericMenu();

                AddMenuItemForHelp(menu, "Twitter", "https://twitter.com/PhaxeNor");
                AddMenuItemForHelp(menu, "Github", "https://github.com/PhaxeNor/VRC-Playlist-Manager");

                Rect btnRect = GUILayoutUtility.GetLastRect();
                menu.DropDown(new Rect(110, btnRect.y + 8, 10, 10));

            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void AddMenuItemForHelp(GenericMenu menu, string menuPath, string url)
        {
            menu.AddItem(new GUIContent(menuPath), false, OnMenuClicked, url);
        }

        private void OnMenuClicked(object url)
        {
            Application.OpenURL(url.ToString());
        }

        [MenuItem("VRC Prefabs/Playlist Manager")]
        public static void ShowPlaylistManagerWindow()
        {
            PlaylistManagerWindow window = GetWindow<PlaylistManagerWindow>();

            window.titleContent = new GUIContent("Playlists");

            window.minSize = new Vector2(600, 300);

            window.Show();
        }

        private void openPlaylists()
        {
            playlists.Clear();
            MonoScript script = MonoScript.FromScriptableObject(this);
            string path = AssetDatabase.GetAssetPath(script);

            FileInfo dir = new FileInfo(path + "/../../Playlists/");
            FileInfo[] files = dir.Directory.GetFiles("*.txt");

            foreach (FileInfo f in files)
            {
                PlaylistItem item = new PlaylistItem
                {
                    Name = f.Name.Replace("-", " ").Replace("_", " ").Replace(".txt", ""),
                    Slug = f.Name,
                    Path = f.FullName,

                    File = f,

                    Contents = File.ReadAllText(f.FullName),

                    isSelected = false
                };

                playlists.Add(item);
            }
        }

        private void savePlaylist()
        {
            if (selectedPlaylist.Name == "")
            {
                selectedPlaylist.Name = defaultNewFileName.Replace("-", " ");
            }

            string tempName = selectedPlaylist.Name.Replace(" ", "-");

            if (File.Exists(selectedPlaylist.File.Directory.FullName + "\\" + tempName + ".txt"))
            {
                ShowNotification(new GUIContent("Playlist \"" + tempName.Replace("-", " ") + "\" already exists"));
                return;
            }
            File.WriteAllText(selectedPlaylist.File.FullName, selectedPlaylist.Contents);

            playlists[currentPlaylist].File.MoveTo(playlists[currentPlaylist].File.Directory.FullName + "\\" + tempName + ".txt");
            AssetDatabase.Refresh();
            openPlaylists();
        }

        private void createPlaylist()
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string path = AssetDatabase.GetAssetPath(script);

            FileInfo dir = new FileInfo(path + "/../../Playlists/");

            string filePath = dir.Directory.FullName + "\\";

            string fileName = defaultNewFileName;

            GUI.FocusControl(null);

            if (File.Exists(filePath + fileName + ".txt"))
            {
                ShowNotification(new GUIContent("Playlist \"" + fileName.Replace("-", " ") + "\" already exists"));
                return;
            }
            try
            {
                FileStream fs = File.Create(filePath + fileName + ".txt");
                fs.Close();

                PlaylistItem temp = new PlaylistItem
                {
                    Name = fileName.Replace("-", " "),
                    Slug = fileName,
                    Path = filePath + fileName + ".txt",
                    isSelected = false,
                    File = new FileInfo(filePath + fileName + ".txt"),
                    Contents = "",
                };

                playlists.Add(temp);

                GUI.changed = true;
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not create playlist file. " + ex.Message);
            }
        }

        private void destroyPlaylist()
        {
            if (EditorUtility.DisplayDialog("Destroy Playlist?", "Are you sure you want to destroy this playlist?", "Yes", "No"))
            {
                if (selectedPlaylist != null)
                {
                    selectedPlaylist.isSelected = false;
                }

                PlaylistItem temp = selectedPlaylist;
                GUI.FocusControl(null);

                temp.File.Delete();
                playlists.Remove(temp);
                currentPlaylist = 0;


                GUI.changed = true;

                AssetDatabase.Refresh();

                openPlaylists();

                if (playlists.Count > 0)
                {
                    selectedPlaylist = playlists[0];
                }
            }
        }

        private void applyPlaylist()
        {
            savePlaylist();

            try
            {
                if (videoPlayers[videoPlayerSelected].GetComponent<VRC_SyncVideoPlayer>())
                {
                    VRC_SyncVideoPlayer videoPlayer = videoPlayers[videoPlayerSelected].GetComponent<VRC_SyncVideoPlayer>();

                    videoPlayer.Videos = null;

                    List<VRC_SyncVideoPlayer.VideoEntry> videos = new List<VRC_SyncVideoPlayer.VideoEntry>();

                    string[] links = selectedPlaylist.Contents.Split(new[] { '\r', '\n' });

                    foreach (string link in links)
                    {
                        VRC_SyncVideoPlayer.VideoEntry tmp = new VRC_SyncVideoPlayer.VideoEntry
                        {
                            Source = UnityEngine.Video.VideoSource.Url,
                            PlaybackSpeed = 1.0f,
                            AspectRatio = UnityEngine.Video.VideoAspectRatio.FitInside,
                            URL = link
                        };

                        videos.Add(tmp);
                    }

                    videoPlayer.Videos = videos.ToArray();
                }

                if (videoPlayers[videoPlayerSelected].GetComponent<VRC_SyncVideoStream>())
                {
                    VRC_SyncVideoStream videoPlayer = videoPlayers[videoPlayerSelected].GetComponent<VRC_SyncVideoStream>();

                    videoPlayer.Videos = null;

                    List<VRC_SyncVideoStream.VideoEntry> videos = new List<VRC_SyncVideoStream.VideoEntry>();

                    string[] links = selectedPlaylist.Contents.Split(new[] { '\r', '\n' });

                    foreach (string link in links)
                    {
                        VRC_SyncVideoStream.VideoEntry tmp = new VRC_SyncVideoStream.VideoEntry
                        {
                            Source = UnityEngine.Video.VideoSource.Url,
                            PlaybackSpeed = 1.0f,
                            URL = link
                        };
                        videos.Add(tmp);
                    }

                    videoPlayer.Videos = videos.ToArray();
                }

                ShowNotification(new GUIContent(notification));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                ShowNotification(new GUIContent("Could not create playlist"));
            }
        }
    }
}