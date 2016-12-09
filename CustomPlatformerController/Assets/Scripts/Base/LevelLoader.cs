﻿using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Base.Exceptions;

namespace Assets.Scripts.Base
{
    public abstract class LevelLoader
    {
        private static List<Scene> AllScenes = new List<Scene>();
        private static Playthrough currentPlayThrough;
        private static SaveSystem saveSystem;

        static LevelLoader()
        {
            saveSystem = new SaveSystem();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                Scene s = SceneManager.GetSceneByBuildIndex(i);
                if (s.name != null)
                    AllScenes.Add(s);
            }
        }

        public const int DEFAULT_LEVEL_AMOUNT = 5;
        public const string RANDOMLEVEL_NAME = "level";
        public const string OPTIONSCENE_NAME = "options";
        public const string MAINMENUSCENE_NAME = "mainmenu";
        public const string TUTORIALSCENE_NAME = "tutoral";
        public const string SAVE_NAME = "Playthrough.dat";

        /// <summary>
        /// Loads the first scene in the builder
        /// </summary>
        public static void LoadMainMenu()
        {
            SceneManager.LoadScene(FindLevel(MAINMENUSCENE_NAME));
        }

        /// <summary>
        /// Loads the tutorial in the builder
        /// </summary>
        public static void LoadTutorial()
        {
            SceneManager.LoadScene(FindLevel(TUTORIALSCENE_NAME));
        }

        /// <summary>
        /// Loads the option screen in the builder
        /// </summary>
        public static void LoadOptions()
        {
            SceneManager.LoadScene(FindLevel(OPTIONSCENE_NAME));
        }

        /// <summary>
        /// Alternative for LoadRandomLevelSet(bool, true, DEFAULT_LEVEL_AMOUNT, true);
        /// Creates a new playthrough
        /// </summary>
        /// <param name="instantplay">Whether to launch a level from the set</param>
        public static void NewPlayThrough(bool instantplay)
        {
            currentPlayThrough = new Playthrough();
            LoadRandomLevelSet(instantplay, true, DEFAULT_LEVEL_AMOUNT, true);
        }

        /// <summary>
        /// Loads a playthrough from file, or creates a new one from scratch
        /// </summary>
        public static void LoadPlayThrough(bool instantplay)
        {
            //Prevent duplicate load in SaveSystem instance
            saveSystem.Clear();
            saveSystem.Load(SAVE_NAME);

            currentPlayThrough = saveSystem.GetObject<Playthrough>();
            if (currentPlayThrough == null)
            {
                saveSystem.Clear();
                NewPlayThrough(instantplay);
            }
        }

        /// <summary>
        /// Saves the playthrough to a file
        /// </summary>
        public static void SavePlayThrough()
        {
            //Prevent saving more than just the playthrough
            saveSystem.Clear();
            saveSystem.Add(currentPlayThrough);
            saveSystem.Save(SAVE_NAME);
            saveSystem.Clear();
        }

        /// <summary>
        /// Loads a random set of levels
        /// </summary>
        /// <param name="instantplay">Whether to launch a level from the set</param>
        /// <param name="newsetIfIsRequired">Whether to generate a new levelset if no set was generated before</param>
        /// <param name="newset">Whether to force a new levelset</param>
        /// <param name="amount">The amount of levels to generate, defaults to DEFAULT_LEVEL_AMOUNT</param>
        public static void LoadRandomLevelSet(bool instantplay, bool newsetIfIsRequired = true, int amount = DEFAULT_LEVEL_AMOUNT, bool newset = false)
        {
            if (currentPlayThrough.LacksLevels && newsetIfIsRequired || newset)
            {
                List<int> selectable = FindLevels(RANDOMLEVEL_NAME);

                if (selectable.Count < amount)
                {
                    throw new NotEnoughLevelsException();
                }

                //Generate new set
                for (int i = 0; i < amount; i++)
                {
                    int index = Random.Range(0, selectable.Count - 1);

                    //Pick a random one
                    int pick = selectable[index];

                    //Remove from the selectable list
                    currentPlayThrough.Levels.Add(pick);
                    selectable.RemoveAt(index);
                }
            }
            else
            {
                if (currentPlayThrough.LacksLevels)
                {
                    throw new NoLevelsLoadedException();
                }
            }
            if (instantplay)
            {
                LoadNextLevel();
            }
        }

        /// <summary>
        /// Whether the playthrough is able to continue after this level or not
        /// </summary>
        public static bool HasMoreLevels
        {
            get
            {
                return !currentPlayThrough.NoMoreLevels;
            }
        }

        /// <summary>
        /// Loads the current level. Also known as a reset, or reload
        /// </summary>
        public static void LoadCurrentLevel()
        {
            LoadRandomLevelSet(false);
            SceneManager.LoadScene(currentPlayThrough.CurrentLevel);
        }

        /// <summary>
        /// Loads the next level from the set
        /// Generates a new set if no set was present
        /// </summary>
        public static void LoadNextLevel()
        {
            LoadRandomLevelSet(false);
            SceneManager.LoadScene(currentPlayThrough.NextLevel());
            SavePlayThrough();
        }

        /// <summary>
        /// Loads a random playable level
        /// </summary>
        public static void LoadRandomLevel()
        {
            List<int> selectable = FindLevels(RANDOMLEVEL_NAME);
            SceneManager.LoadScene(selectable[Random.Range(0, selectable.Count - 1)]);
        }

        /// <summary>
        /// Finds a level by its name, works safe
        /// </summary>
        /// <param name="contains"></param>
        /// <returns></returns>
        private static int FindLevel(string contains, bool checkForDuplicates = true)
        {
            bool found = false;
            int c = -1;
            foreach (Scene s in AllScenes)
            {
                c++;
                if (s.name.ToLower().Contains(contains))
                {
                    if (checkForDuplicates && found)
                    {
                        //If more than one level was found
                        throw new InvalidLevelIdentifierException();
                    }
                    else
                    {
                        found = true;
                    }
                }
            }
            if (!found)
            {
                throw new LevelNotFoundException();
            }
            else
            {
                return c;
            }
        }

        /// <summary>
        /// Finds and returns all levels containing this name
        /// </summary>
        /// <param name="contains"></param>
        /// <returns></returns>
        private static List<int> FindLevels(string contains)
        {
            List<int> ret = new List<int>();
            int c = -1;
            foreach (Scene s in AllScenes)
            {
                c++;
                if (s.name.ToLower().Contains(contains))
                {
                    ret.Add(c);
                }
            }
            if (ret.Count == 0)
            {
                throw new LevelNotFoundException();
            }
            else
            {
                return ret;
            }
        }
    }
}